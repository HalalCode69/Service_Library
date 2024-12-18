using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountFieldsToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDate",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "Books",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBuyPrice",
                table: "Books",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEndDate",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "PreviousBuyPrice",
                table: "Books");
        }
    }
}
