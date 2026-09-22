using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechGearAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixWarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWatches_Auctions_AuctionId",
                table: "AuctionWatches");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWatches_Users_UserId",
                table: "AuctionWatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_Auctions_AuctionId",
                table: "CreditTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_Users_UserId",
                table: "CreditTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Auctions_AuctionId",
                table: "SuspiciousActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Users_BidderId",
                table: "SuspiciousActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Users_SellerId",
                table: "SuspiciousActivities");

            migrationBuilder.DropColumn(
                name: "LoginTime",
                table: "UserDeviceLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWatches_Auctions_AuctionId",
                table: "AuctionWatches",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWatches_Users_UserId",
                table: "AuctionWatches",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_Auctions_AuctionId",
                table: "CreditTransactions",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_Users_UserId",
                table: "CreditTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Auctions_AuctionId",
                table: "SuspiciousActivities",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Users_BidderId",
                table: "SuspiciousActivities",
                column: "BidderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Users_SellerId",
                table: "SuspiciousActivities",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWatches_Auctions_AuctionId",
                table: "AuctionWatches");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionWatches_Users_UserId",
                table: "AuctionWatches");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_Auctions_AuctionId",
                table: "CreditTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_Users_UserId",
                table: "CreditTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Auctions_AuctionId",
                table: "SuspiciousActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Users_BidderId",
                table: "SuspiciousActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_SuspiciousActivities_Users_SellerId",
                table: "SuspiciousActivities");

            migrationBuilder.AddColumn<DateTime>(
                name: "LoginTime",
                table: "UserDeviceLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWatches_Auctions_AuctionId",
                table: "AuctionWatches",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionWatches_Users_UserId",
                table: "AuctionWatches",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_Auctions_AuctionId",
                table: "CreditTransactions",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_Users_UserId",
                table: "CreditTransactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Auctions_AuctionId",
                table: "SuspiciousActivities",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Users_BidderId",
                table: "SuspiciousActivities",
                column: "BidderId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SuspiciousActivities_Users_SellerId",
                table: "SuspiciousActivities",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
