using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitIdentityColumnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EFPenaltyIdentifiers_EFPlayers_PlayerId",
                table: "EFPenaltyIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_EFPenaltyIdentifiers_PlayerId",
                table: "EFPenaltyIdentifiers");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "EFPenaltyIdentifiers");

            migrationBuilder.UpdateData(
                table: "EFAliases",
                keyColumn: "Id",
                keyValue: -1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(8973), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFChats",
                keyColumn: "Id",
                keyValue: -1,
                column: "Submitted",
                value: new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(9217), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFCommunities",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "ApiKey", "CommunityGuid", "Created", "HeartBeat", "Socials" },
                values: new object[] { new Guid("06c7b389-ffe2-496f-b895-d0276698f5b4"), new Guid("754e8caf-477c-49d5-be47-7662088eef0c"), new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(9194), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(9193), new TimeSpan(0, 0, 0, 0, 0)), new Dictionary<string, string> { ["YouTube"] = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", ["Another YouTube"] = "https://www.youtube.com/watch?v=sFce1pBvSd4" } });

            migrationBuilder.UpdateData(
                table: "EFPenalties",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PenaltyGuid", "Submitted" },
                values: new object[] { new Guid("4d863dab-97bd-4850-8f05-63cdf0bb0e8a"), new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(9210), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFPlayers",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Created", "Heartbeat" },
                values: new object[] { new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(8978), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(8977), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFServers",
                keyColumn: "Id",
                keyValue: -1,
                column: "Updated",
                value: new DateTimeOffset(new DateTime(2023, 8, 27, 19, 45, 11, 912, DateTimeKind.Unspecified).AddTicks(9224), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "EFPenaltyIdentifiers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "EFAliases",
                keyColumn: "Id",
                keyValue: -1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(905), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFChats",
                keyColumn: "Id",
                keyValue: -1,
                column: "Submitted",
                value: new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(1141), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFCommunities",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "ApiKey", "CommunityGuid", "Created", "HeartBeat", "Socials" },
                values: new object[] { new Guid("51fa2c41-fdbe-46c0-a4b7-733b45b49f3d"), new Guid("c522baa6-ffcb-4ac2-9ec5-a958d2016962"), new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(1119), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(1117), new TimeSpan(0, 0, 0, 0, 0)), new Dictionary<string, string> { ["YouTube"] = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", ["Another YouTube"] = "https://www.youtube.com/watch?v=sFce1pBvSd4" } });

            migrationBuilder.UpdateData(
                table: "EFPenalties",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PenaltyGuid", "Submitted" },
                values: new object[] { new Guid("80148588-fafa-4389-9f1a-5f24ae349d4f"), new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(1134), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFPlayers",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Created", "Heartbeat" },
                values: new object[] { new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(911), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(911), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFServers",
                keyColumn: "Id",
                keyValue: -1,
                column: "Updated",
                value: new DateTimeOffset(new DateTime(2023, 8, 26, 14, 15, 12, 606, DateTimeKind.Unspecified).AddTicks(1150), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_EFPenaltyIdentifiers_PlayerId",
                table: "EFPenaltyIdentifiers",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_EFPenaltyIdentifiers_EFPlayers_PlayerId",
                table: "EFPenaltyIdentifiers",
                column: "PlayerId",
                principalTable: "EFPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
