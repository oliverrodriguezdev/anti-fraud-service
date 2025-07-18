using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AntiFraudService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "transactions",
                newName: "Value");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceAccountId",
                table: "transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TargetAccountId",
                table: "transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TransferTypeId",
                table: "transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceAccountId",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "TargetAccountId",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "TransferTypeId",
                table: "transactions");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "transactions",
                newName: "Amount");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
