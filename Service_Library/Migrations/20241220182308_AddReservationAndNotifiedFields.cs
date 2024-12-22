using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service_Library.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationAndNotifiedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Notified",
                table: "WaitingList",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationExpiry",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedForUserId",
                table: "Books",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notified",
                table: "WaitingList");

            migrationBuilder.DropColumn(
                name: "ReservationExpiry",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "ReservedForUserId",
                table: "Books");
        }
    }
}
