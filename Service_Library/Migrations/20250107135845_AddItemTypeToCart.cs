using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTypeToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "ShoppingCartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "ShoppingCartItems");
        }
    }
}
