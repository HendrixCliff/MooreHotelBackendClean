using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MooreHotelAndSuites.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupReferenceToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "GroupReference",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupReference",
                table: "Bookings");

           
        }
    }
}
