using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingPaymentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class minorbugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalaryPaymentId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SalaryPaymentId",
                table: "Transactions",
                column: "SalaryPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SalaryPayments_SalaryPaymentId",
                table: "Transactions",
                column: "SalaryPaymentId",
                principalTable: "SalaryPayments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SalaryPayments_SalaryPaymentId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SalaryPaymentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SalaryPaymentId",
                table: "Transactions");
        }
    }
}
