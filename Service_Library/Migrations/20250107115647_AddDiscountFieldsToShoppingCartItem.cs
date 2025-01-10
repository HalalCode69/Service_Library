using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountFieldsToShoppingCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDate",
                table: "ShoppingCartItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "ShoppingCartItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEndDate",
                table: "ShoppingCartItems");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "ShoppingCartItems");
        }
    }
}
