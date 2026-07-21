using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditRisk.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Decision",
                table: "LoanApplications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Decision",
                table: "LoanApplications");
        }
    }
}
