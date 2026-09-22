using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechGearAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReportedAsBuyerCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportedAsSellerCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalAuctionsCreated",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalAuctionsWon",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReportedAsBuyerCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReportedAsSellerCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalAuctionsCreated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalAuctionsWon",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Users");
        }
    }
}
