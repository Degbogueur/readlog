using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Readlog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixReadingListItemForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingListItems_BookId",
                table: "ReadingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ReadingListItems_ReadingListId",
                table: "ReadingListItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReadingListId",
                table: "ReadingListItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingListItems_ReadingListId_BookId",
                table: "ReadingListItems",
                columns: new[] { "ReadingListId", "BookId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingListItems_ReadingListId_BookId",
                table: "ReadingListItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReadingListId",
                table: "ReadingListItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingListItems_BookId",
                table: "ReadingListItems",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingListItems_ReadingListId",
                table: "ReadingListItems",
                column: "ReadingListId");
        }
    }
}
