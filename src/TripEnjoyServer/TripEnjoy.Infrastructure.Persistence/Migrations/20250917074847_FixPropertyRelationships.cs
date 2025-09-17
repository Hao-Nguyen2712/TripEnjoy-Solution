using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripEnjoy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPropertyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Partners_PartnerId1",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertyTypes_PropertyTypeId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_PartnerId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_PropertyTypeId1",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PartnerId1",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PropertyTypeId1",
                table: "Properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PartnerId1",
                table: "Properties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyTypeId1",
                table: "Properties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PartnerId1",
                table: "Properties",
                column: "PartnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyTypeId1",
                table: "Properties",
                column: "PropertyTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Partners_PartnerId1",
                table: "Properties",
                column: "PartnerId1",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertyTypes_PropertyTypeId1",
                table: "Properties",
                column: "PropertyTypeId1",
                principalTable: "PropertyTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
