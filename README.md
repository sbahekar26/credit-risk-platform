# Credit Risk Assessment Platform

A full-stack .NET application that evaluates loan applications and predicts credit risk using a machine learning model trained on real lending data. Applications are submitted through a web UI, scored by the model, and stored with their decisions — Approve, Review, or Decline — which are surfaced on a live dashboard.

Built in C#/.NET 10 as a hands-on project to learn the full Microsoft stack properly: every layer — front end, API, data, and model — written and debugged by hand rather than scaffolded from a tutorial.

## What it does

A loan officer fills in an application (checking account status, credit history, loan amount, employment, and so on). The API maps those inputs to the feature set of an ML.NET model trained on the UCI German Credit dataset, gets a default-risk probability back, converts it to a decision, and stores the full application with its outcome in PostgreSQL. The UI shows a dashboard with approval statistics, a filterable list of all applications, and a detail view for each one with the coded features decoded into readable labels.

## Architecture

The solution is split into separate projects with a shared core:

- `CreditRisk.Core` — the domain: the loan application model, decision enum, feature-label decoding, and the ML data contracts shared between training and serving.
- `CreditRisk.Api` — ASP.NET Core Web API. Receives applications, runs the ML prediction, persists everything with Entity Framework Core, and serves the data back.
- `CreditRisk.Web` — Blazor Server front end: dashboard, applications list, application detail, and the submission form. Hand-styled CSS, no component library.
- `CreditRisk.ModelTrainer` — a console app that trains the model: loads the German Credit dataset, one-hot encodes categorical features, trains a FastTree binary classifier, evaluates it on a held-out test set, and saves the model file the API loads.

The database is PostgreSQL (with the pgvector extension enabled for planned retrieval features), running in Docker. The API is containerized with a multi-stage Dockerfile.

## The ML story

This is the part of the project I learned the most from.

The first training run reported 98.6% accuracy — which on this dataset is not a good result, it is a wrong one. The F1 score was only 40%, and that mismatch pointed to data leakage. The cause was mundane: the raw data file uses multiple spaces between columns, the loader split on single spaces, and the resulting column shift pushed the label into the feature vector. The model was reading the answer.

After normalizing the file, accuracy dropped to an honest ~73%. From there, switching the trainer from logistic regression to a gradient-boosted tree (FastTree) — chosen after diagnosing that class imbalance was suppressing detection of bad credit risks — brought AUC from 0.65 to 0.78 and more than doubled F1. Those numbers hold on a held-out 20% test set.

The model is served through a single scoring interface in the API, which originally held a hand-written rule engine. Swapping rules for the trained model required no changes anywhere else in the stack.

## Running it locally

Prerequisites: .NET 10 SDK, Docker.

Start the database:

```bash
docker run --name creditrisk-db \
  -e POSTGRES_PASSWORD=devpassword \
  -e POSTGRES_DB=creditrisk \
  -p 5432:5432 \
  -d pgvector/pgvector:pg17
```

Apply the schema and start the API:

```bash
cd CreditRisk.Api
dotnet ef database update
dotnet run
```

Start the web app in a second terminal:

```bash
cd CreditRisk.Web
dotnet run
```

Open the web app's URL (shown in its startup output), submit an application from the New Application page, and watch the decision appear. To retrain the model, run the `CreditRisk.ModelTrainer` project against the dataset file (see the training source for the expected file) and copy the resulting `credit-model.zip` into `CreditRisk.Api`.

## Things that tripped me up

- The data leakage above — the single most valuable bug of the project. A too-good metric is a symptom, not a success.
- PostgreSQL rejects local-kind DateTimes: `DateTime.Now` fails against `timestamp with time zone`, `DateTime.UtcNow` is required. Found this the hard way migrating off SQLite.
- ML.NET models that use custom mappings record which assembly the mapping code lives in. If that code sits in the training project, the API cannot load the model. The fix was moving the mapping into the shared core library and retraining.
- SQLite in a container is a dead end — the file does not travel with the image and the container filesystem is ephemeral. That constraint is what motivated the move to PostgreSQL as a separate persistent container.
- Docker requires the file to be named `Dockerfile`, and `DockerFile` fails with an unhelpful error about a 2-byte file.

## Workflow

Every feature was built on its own branch and merged through a pull request — domain modeling, the rule engine, the API, persistence, the ML pipeline, the UI pages, security fixes, and the database migration each have their own PR history.

## Roadmap

- Retrieval-augmented (RAG) natural-language querying over decisions, using pgvector and a local LLM via Semantic Kernel
- Azure deployment
- Input validation and richer model diagnostics on the detail view