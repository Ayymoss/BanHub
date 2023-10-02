using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BanHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitServersMaxPlayersColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                table: "EFServers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "EFAliases",
                keyColumn: "Id",
                keyValue: -1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6450), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFChats",
                keyColumn: "Id",
                keyValue: -1,
                column: "Submitted",
                value: new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6745), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFCommunities",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "ApiKey", "CommunityGuid", "Created", "HeartBeat", "Socials" },
                values: new object[] { new Guid("aa85fe95-cc9b-46eb-b3c8-04661f8c9f2c"), new Guid("353fd886-7ea1-4067-825c-191d087f55eb"), new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6723), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6721), new TimeSpan(0, 0, 0, 0, 0)), new Dictionary<string, string> { ["YouTube"] = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", ["Another YouTube"] = "https://www.youtube.com/watch?v=sFce1pBvSd4" } });

            migrationBuilder.UpdateData(
                table: "EFPenalties",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PenaltyGuid", "Submitted" },
                values: new object[] { new Guid("f83d05b3-e49f-4416-8e49-63e145e1beb4"), new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6740), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFPlayers",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Created", "Heartbeat" },
                values: new object[] { new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6457), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6456), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFServers",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "MaxPlayers", "Updated" },
                values: new object[] { 0, new DateTimeOffset(new DateTime(2023, 9, 1, 20, 23, 26, 385, DateTimeKind.Unspecified).AddTicks(6754), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                table: "EFServers");

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
    }
}
