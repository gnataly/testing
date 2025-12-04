using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreCenter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedTwoFactorAttempts",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangedAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingTwoFactorChallengeId",
                table: "Accounts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingTwoFactorCodeHash",
                table: "Accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingTwoFactorExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingUnlockCodeExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingUnlockCodeHash",
                table: "Accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "FailedTwoFactorAttempts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastPasswordChangedAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PendingTwoFactorChallengeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PendingTwoFactorCodeHash",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PendingTwoFactorExpiresAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PendingUnlockCodeExpiresAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PendingUnlockCodeHash",
                table: "Accounts");
        }
    }
}
