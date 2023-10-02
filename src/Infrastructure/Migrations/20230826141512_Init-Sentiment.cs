using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BanHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSentiment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EFChatSentiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sentiment = table.Column<float>(type: "real", nullable: false),
                    ChatCount = table.Column<int>(type: "integer", nullable: false),
                    LastChatCalculated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFChatSentiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFChatSentiments_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_EFChatSentiments_PlayerId",
                table: "EFChatSentiments",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EFChatSentiments");

            migrationBuilder.UpdateData(
                table: "EFAliases",
                keyColumn: "Id",
                keyValue: -1,
                column: "Created",
                value: new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3823), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFChats",
                keyColumn: "Id",
                keyValue: -1,
                column: "Submitted",
                value: new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4081), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "EFCommunities",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "ApiKey", "CommunityGuid", "Created", "HeartBeat", "Socials" },
                values: new object[] { new Guid("a2214d5b-c8f6-4922-86eb-f6898164de25"), new Guid("1f0e5209-a1ec-47a8-92f3-05fdd94c06ee"), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4059), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4058), new TimeSpan(0, 0, 0, 0, 0)), new Dictionary<string, string> { ["YouTube"] = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", ["Another YouTube"] = "https://www.youtube.com/watch?v=sFce1pBvSd4" } });

            migrationBuilder.UpdateData(
                table: "EFPenalties",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "PenaltyGuid", "Submitted" },
                values: new object[] { new Guid("3145e870-265a-4dc2-953c-5952d824e25f"), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4078), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFPlayers",
                keyColumn: "Id",
                keyValue: -1,
                columns: new[] { "Created", "Heartbeat" },
                values: new object[] { new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3828), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3828), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "EFServers",
                keyColumn: "Id",
                keyValue: -1,
                column: "Updated",
                value: new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4087), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
