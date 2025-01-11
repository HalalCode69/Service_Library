using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddIsWebsiteRatingToUserRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRatings_Books_BookId",
                table: "UserRatings");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "UserRatings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "UserRatings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsWebsiteRating",
                table: "UserRatings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRatings_Books_BookId",
                table: "UserRatings",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRatings_Books_BookId",
                table: "UserRatings");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "UserRatings");

            migrationBuilder.DropColumn(
                name: "IsWebsiteRating",
                table: "UserRatings");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "UserRatings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRatings_Books_BookId",
                table: "UserRatings",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
