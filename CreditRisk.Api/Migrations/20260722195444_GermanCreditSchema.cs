using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditRisk.Api.Migrations
{
    /// <inheritdoc />
    public partial class GermanCreditSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_Applicants_ApplicantId",
                table: "LoanApplications");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_ApplicantId",
                table: "LoanApplications");

            migrationBuilder.RenameColumn(
                name: "LoanTermMonths",
                table: "LoanApplications",
                newName: "DurationMonths");

            migrationBuilder.RenameColumn(
                name: "LoanAmount",
                table: "LoanApplications",
                newName: "Savings");

            migrationBuilder.RenameColumn(
                name: "ApplicantId",
                table: "LoanApplications",
                newName: "Age");

            migrationBuilder.AddColumn<string>(
                name: "CheckingStatus",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditAmount",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CreditHistory",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Employment",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Housing",
                table: "LoanApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckingStatus",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CreditAmount",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "CreditHistory",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "Employment",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "Housing",
                table: "LoanApplications");

            migrationBuilder.RenameColumn(
                name: "Savings",
                table: "LoanApplications",
                newName: "LoanAmount");

            migrationBuilder.RenameColumn(
                name: "DurationMonths",
                table: "LoanApplications",
                newName: "LoanTermMonths");

            migrationBuilder.RenameColumn(
                name: "Age",
                table: "LoanApplications",
                newName: "ApplicantId");

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreditScore = table.Column<int>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_ApplicantId",
                table: "LoanApplications",
                column: "ApplicantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_Applicants_ApplicantId",
                table: "LoanApplications",
                column: "ApplicantId",
                principalTable: "Applicants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
