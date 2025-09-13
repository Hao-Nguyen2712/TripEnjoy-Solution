using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripEnjoy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlackListToken : Migration
    {
        /// <summary>
        /// Applies the migration by creating the BlackListTokens table for storing blacklisted tokens associated with accounts.
        /// </summary>
        /// <remarks>
        /// The table includes:
        /// - Id: primary key (uniqueidentifier)
        /// - AccountId: foreign key to Accounts.Id with cascade delete
        /// - Token: nvarchar(500), not nullable
        /// - Expiration: datetime2, not nullable
        /// - CreatedAt: datetime2, not nullable
        /// An index IX_BlackListTokens_AccountId is created on AccountId to support account-based lookups.
        /// </remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlackListTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlackListTokens_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlackListTokens_AccountId",
                table: "BlackListTokens",
                column: "AccountId");
        }

        /// <summary>
        /// Reverts the migration by dropping the "BlackListTokens" table.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackListTokens");
        }
    }
}
