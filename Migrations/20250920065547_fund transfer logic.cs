using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingPaymentsAPI.Migrations
{
    /// <inheritdoc />
    public partial class fundtransferlogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumberEncrypted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Beneficiaries");

            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "Employees",
                newName: "Balance");

            migrationBuilder.RenameColumn(
                name: "PAN",
                table: "Employees",
                newName: "AccountNumber");

            migrationBuilder.RenameColumn(
                name: "BankName",
                table: "Beneficiaries",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "AccountNumberEncrypted",
                table: "Beneficiaries",
                newName: "AccountNumber");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Employees",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Clients",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Beneficiaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Beneficiaries",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "AccountBalance",
                table: "Banks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "AccountBalance",
                table: "Banks");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Employees",
                newName: "Salary");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "Employees",
                newName: "PAN");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Beneficiaries",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "Beneficiaries",
                newName: "AccountNumberEncrypted");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumberEncrypted",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
