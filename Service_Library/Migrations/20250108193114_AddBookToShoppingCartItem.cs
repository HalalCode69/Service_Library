using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddBookToShoppingCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_BookId",
                table: "ShoppingCartItems",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCartItems_Books_BookId",
                table: "ShoppingCartItems",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCartItems_Books_BookId",
                table: "ShoppingCartItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCartItems_BookId",
                table: "ShoppingCartItems");
        }
    }
}
