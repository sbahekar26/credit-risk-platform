# Credit Risk Assessment Platform

A full-stack .NET application that evaluates loan applications and predicts credit risk using a machine learning model trained on real lending data. Applications are submitted through a web UI, scored by the model, and stored with their decisions — Approve, Review, or Decline — which are surfaced on a live dashboard. A natural-language assistant page lets you ask questions about the stored applications.

Built in C#/.NET 10 as a hands-on project to learn the full Microsoft stack properly: every layer — front end, API, data, model, and retrieval — written and debugged by hand rather than scaffolded from a tutorial.

## What it does

A loan officer fills in an application (checking account status, credit history, loan amount, employment, and so on). The form validates required fields and ranges client-side; the API re-validates the same DataAnnotations server-side and rejects bad input with a 400 and the specific error messages. The API maps valid inputs to the feature set of an ML.NET model trained on the UCI German Credit dataset, gets a default-risk probability back, converts it to a decision, and stores the full application with its outcome in PostgreSQL. The UI shows a dashboard with approval statistics, a filterable list of all applications, and a detail view for each one with the coded features decoded into readable labels.

An Ask Assistant page lets you ask questions in plain English — "how many applications were declined", "what's the highest loan amount", "tell me about the riskiest applicants" — and get an answer back, either from a direct database query or from a local LLM reasoning over the closest matching applications.

## Architecture

The solution is split into separate projects with a shared core:

- `CreditRisk.Core` — the domain: the loan application model with its DataAnnotations validation attributes, decision enum, decision thresholds, feature-label decoding, the embedding entity, and the ML data contracts shared between training and serving.
- `CreditRisk.Api` — ASP.NET Core minimal API. Validates and receives applications, runs the ML prediction, persists everything with Entity Framework Core, embeds each application into pgvector on submission, and answers `/api/ask` through a hybrid router: `ApplicationStatsService` handles structured/aggregate questions with direct EF Core queries, and anything it doesn't recognize falls back to pgvector similarity search plus a local LLM via Semantic Kernel.
- `CreditRisk.Web` — Blazor Server front end: dashboard, applications list, application detail, submission form (with `DataAnnotationsValidator`), and the Ask Assistant page. Hand-styled CSS, no component library.
- `CreditRisk.ModelTrainer` — a console app that trains the model: loads the German Credit dataset, one-hot encodes categorical features, trains a FastTree binary classifier, evaluates it on a held-out test set, and saves the model file the API loads.
- `CreditRisk.Tests` — xUnit tests: unit tests for pure logic (feature-label decoding, decision thresholds) and integration tests that spin up the real API with `WebApplicationFactory` against an in-memory database.
- `CreditRisk.Console` — an early scratch console app from before the project had a real API or database. It's out of date with the current domain model and isn't part of the solution file; kept around but not maintained.

The database is PostgreSQL with the pgvector extension, running in Docker. Both the API and the web app are containerized with their own Dockerfiles, orchestrated together with Docker Compose. Ollama runs on the host and serves both the chat model (`llama3.2`) and the embedding model (`nomic-embed-text`).

## The ML story

This is the part of the project I learned the most from.

The first training run reported 98.6% accuracy — which on this dataset is not a good result, it is a wrong one. The F1 score was only 40%, and that mismatch pointed to data leakage. The cause was mundane: the raw data file uses multiple spaces between columns, the loader split on single spaces, and the resulting column shift pushed the label into the feature vector. The model was reading the answer.

After normalizing the file, accuracy dropped to an honest ~73%. From there, switching the trainer from logistic regression to a gradient-boosted tree (FastTree) — chosen after diagnosing that class imbalance was suppressing detection of bad credit risks — brought AUC from 0.65 to 0.78 and more than doubled F1. Those numbers hold on a held-out 20% test set.

The model is served through a single scoring interface in the API, which originally held a hand-written rule engine. Swapping rules for the trained model required no changes anywhere else in the stack.

## The RAG assistant

Every application gets embedded on submission: the API builds a short text summary of the application (name, age, loan purpose, checking status, credit history, employment, decision) and sends it to Ollama's `nomic-embed-text` model, storing the resulting vector in a pgvector column alongside the application.

Questions sent to `/api/ask` go through a hybrid router:

1. `ApplicationStatsService` first checks the question against a set of keyword patterns — counts by decision, highest/lowest loan amount, newest application, average loan amount. If it matches, it runs a direct EF Core query and returns the answer with `source: "database"`.
2. If nothing matches, the question itself is embedded, compared against stored application vectors by cosine distance, and the three closest applications are stuffed into a prompt sent to `llama3.2` through Semantic Kernel. That answer comes back with `source: "rag"`.

Everything runs locally through Ollama — no paid API involved. The router is plain keyword matching, not LLM function-calling, so it's brittle: rephrase a question slightly and it can miss a pattern it should have matched and fall through to RAG instead. Good enough for a demo, not something I'd trust in front of real users without hardening.

## Running it locally

Prerequisites: .NET 10 SDK, Docker, and [Ollama](https://ollama.com) with two models pulled:

```bash
ollama pull llama3.2
ollama pull nomic-embed-text
```

Start Ollama (if it isn't already running as a service):

```bash
ollama serve
```

Then start the whole stack — PostgreSQL, the API, and the web app:

```bash
docker compose up --build
```

The API applies EF Core migrations on startup, so the database schema initializes itself on first run; there's no separate migration step. Containers reach Ollama on the host via `host.docker.internal`.

- Web: http://localhost:5272
- API: http://localhost:5260

Data persists in a named Docker volume (`pgdata`) across restarts — stopping and restarting the stack doesn't lose applications.

Connection string and Ollama URL are both read from environment variables (`ConnectionStrings__Default`, `Ollama:Url`) with `localhost` defaults baked in, so the same code runs either through Compose (container-to-container hostnames) or with `dotnet run` against a locally-installed Postgres and Ollama.

To retrain the model, run the `CreditRisk.ModelTrainer` project against the dataset file (see the training source for the expected file) and copy the resulting `credit-model.zip` into `CreditRisk.Api`.

## Things that tripped me up

- The data leakage above — the single most valuable bug of the project. A too-good metric is a symptom, not a success.
- PostgreSQL rejects local-kind DateTimes: `DateTime.Now` fails against `timestamp with time zone`, `DateTime.UtcNow` is required. Found this the hard way migrating off SQLite.
- ML.NET models that use custom mappings record which assembly the mapping code lives in. If that code sits in the training project, the API cannot load the model. The fix was moving the mapping into the shared core library and retraining.
- SQLite in a container is a dead end — the file does not travel with the image and the container filesystem is ephemeral. That constraint is what motivated the move to PostgreSQL as a separate persistent container.
- Docker requires the file to be named `Dockerfile`, and `DockerFile` fails with an unhelpful error about a 2-byte file.
- pgvector's `Vector` column type has nothing to map to under EF Core's in-memory test provider, so the integration tests crashed on model build until `OnModelCreating` branched on `Database.IsNpgsql()` — real Postgres gets the vector column and extension, the in-memory provider just ignores the embeddings table entirely.
- Early on, the API and web app had connection strings and the Ollama URL hardcoded to `localhost`, which works with `dotnet run` but breaks the moment either service moves into its own container — `localhost` inside a container means the container, not the host or its sibling. Externalizing all of that to configuration (with `localhost` defaults preserved for the no-Docker path) is what made Compose actually work.

## Workflow

Every feature was built on its own branch and merged through a pull request — domain modeling, the rule engine, the API, persistence, the ML pipeline, the UI pages, security fixes, the database migration, the RAG assistant, and the Docker Compose setup each have their own PR history.

## Roadmap

- Structured logging — right now the API relies on default console output, no request/error correlation
- Azure deployment is a deliberate non-goal for now — keeping everything local avoids cloud billing while this stays a learning project
