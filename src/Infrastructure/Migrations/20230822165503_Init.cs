using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BanHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:hstore", ",,");

            migrationBuilder.CreateTable(
                name: "EFCommunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommunityGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityIp = table.Column<string>(type: "text", nullable: false),
                    CommunityPort = table.Column<int>(type: "integer", nullable: false),
                    CommunityIpFriendly = table.Column<string>(type: "text", nullable: true),
                    CommunityName = table.Column<string>(type: "text", nullable: false),
                    HeartBeat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    About = table.Column<string>(type: "text", nullable: true),
                    Socials = table.Column<Dictionary<string, string>>(type: "hstore", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFCommunities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EFPlayers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identity = table.Column<string>(type: "text", nullable: false),
                    Heartbeat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TotalConnections = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WebRole = table.Column<int>(type: "integer", nullable: false),
                    CommunityRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EFServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerId = table.Column<string>(type: "text", nullable: false),
                    ServerName = table.Column<string>(type: "text", nullable: false),
                    ServerIp = table.Column<string>(type: "text", nullable: false),
                    ServerPort = table.Column<int>(type: "integer", nullable: false),
                    ServerGame = table.Column<int>(type: "integer", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CommunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFServers_EFCommunities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "EFCommunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFAliases_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFAuthTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFAuthTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFAuthTokens_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NoteGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    IssuerId = table.Column<int>(type: "integer", nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFNotes_EFPlayers_IssuerId",
                        column: x => x.IssuerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFNotes_EFPlayers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFPenalties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PenaltyType = table.Column<int>(type: "integer", nullable: false),
                    PenaltyStatus = table.Column<int>(type: "integer", nullable: false),
                    PenaltyScope = table.Column<int>(type: "integer", nullable: false),
                    PenaltyGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Submitted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Expiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Automated = table.Column<bool>(type: "boolean", nullable: false),
                    Evidence = table.Column<string>(type: "text", nullable: true),
                    IssuerId = table.Column<int>(type: "integer", nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    CommunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFPenalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFPenalties_EFCommunities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "EFCommunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFPenalties_EFPlayers_IssuerId",
                        column: x => x.IssuerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFPenalties_EFPlayers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFTomatoCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tomatoes = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFTomatoCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFTomatoCounters_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Submitted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    ServerId = table.Column<int>(type: "integer", nullable: false),
                    CommunityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFChats_EFCommunities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "EFCommunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFChats_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFChats_EFServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "EFServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFServerConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Connected = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    ServerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFServerConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFServerConnections_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFServerConnections_EFServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "EFServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFCurrentAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    AliasId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFCurrentAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFCurrentAliases_EFAliases_AliasId",
                        column: x => x.AliasId,
                        principalTable: "EFAliases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFCurrentAliases_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EFPenaltyIdentifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identity = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    Expiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PenaltyId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EFPenaltyIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EFPenaltyIdentifiers_EFPenalties_PenaltyId",
                        column: x => x.PenaltyId,
                        principalTable: "EFPenalties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EFPenaltyIdentifiers_EFPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "EFPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EFCommunities",
                columns: new[] { "Id", "About", "Active", "ApiKey", "CommunityGuid", "CommunityIp", "CommunityIpFriendly", "CommunityName", "CommunityPort", "Created", "HeartBeat", "Socials" },
                values: new object[] { -1, "Some description about the instance.", true, new Guid("a2214d5b-c8f6-4922-86eb-f6898164de25"), new Guid("1f0e5209-a1ec-47a8-92f3-05fdd94c06ee"), "123.123.123.123", "zombo.com", "Seed Instance", 1624, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4059), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4058), new TimeSpan(0, 0, 0, 0, 0)), new Dictionary<string, string> { ["YouTube"] = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", ["Another YouTube"] = "https://www.youtube.com/watch?v=sFce1pBvSd4" } });

            migrationBuilder.InsertData(
                table: "EFPlayers",
                columns: new[] { "Id", "CommunityRole", "Created", "Heartbeat", "Identity", "PlayTime", "TotalConnections", "WebRole" },
                values: new object[] { -1, 10, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3828), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3828), new TimeSpan(0, 0, 0, 0, 0)), "0:UKN", new TimeSpan(0, 0, 0, 0, 0), 0, 10 });

            migrationBuilder.InsertData(
                table: "EFAliases",
                columns: new[] { "Id", "Created", "IpAddress", "PlayerId", "UserName" },
                values: new object[] { -1, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(3823), new TimeSpan(0, 0, 0, 0, 0)), "0.0.0.0", -1, "IW4MAdmin" });

            migrationBuilder.InsertData(
                table: "EFPenalties",
                columns: new[] { "Id", "Automated", "CommunityId", "Evidence", "Expiration", "IssuerId", "PenaltyGuid", "PenaltyScope", "PenaltyStatus", "PenaltyType", "Reason", "RecipientId", "Submitted" },
                values: new object[] { -1, true, -1, "WePNs-G7puA", null, -1, new Guid("3145e870-265a-4dc2-953c-5952d824e25f"), 20, 10, 8, "Seed Infraction", -1, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4078), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "EFServers",
                columns: new[] { "Id", "CommunityId", "ServerGame", "ServerId", "ServerIp", "ServerName", "ServerPort", "Updated" },
                values: new object[] { -1, -1, 0, "123.123.123.123:123", "123.123.123.123", "Shef", 123, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4087), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "EFChats",
                columns: new[] { "Id", "CommunityId", "Message", "PlayerId", "ServerId", "Submitted" },
                values: new object[] { -1, -1, "Seed Chat", -1, -1, new DateTimeOffset(new DateTime(2023, 8, 22, 16, 55, 3, 406, DateTimeKind.Unspecified).AddTicks(4081), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "EFCurrentAliases",
                columns: new[] { "Id", "AliasId", "PlayerId" },
                values: new object[] { -1, -1, -1 });

            migrationBuilder.CreateIndex(
                name: "IX_EFAliases_PlayerId",
                table: "EFAliases",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFAuthTokens_PlayerId",
                table: "EFAuthTokens",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFChats_CommunityId",
                table: "EFChats",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_EFChats_PlayerId",
                table: "EFChats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFChats_ServerId",
                table: "EFChats",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFCurrentAliases_AliasId",
                table: "EFCurrentAliases",
                column: "AliasId");

            migrationBuilder.CreateIndex(
                name: "IX_EFCurrentAliases_PlayerId",
                table: "EFCurrentAliases",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EFNotes_IssuerId",
                table: "EFNotes",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFNotes_RecipientId",
                table: "EFNotes",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_EFPenalties_CommunityId",
                table: "EFPenalties",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_EFPenalties_IssuerId",
                table: "EFPenalties",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFPenalties_RecipientId",
                table: "EFPenalties",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_EFPenaltyIdentifiers_PenaltyId",
                table: "EFPenaltyIdentifiers",
                column: "PenaltyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EFPenaltyIdentifiers_PlayerId",
                table: "EFPenaltyIdentifiers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFServerConnections_PlayerId",
                table: "EFServerConnections",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFServerConnections_ServerId",
                table: "EFServerConnections",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_EFServers_CommunityId",
                table: "EFServers",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_EFTomatoCounters_PlayerId",
                table: "EFTomatoCounters",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EFAuthTokens");

            migrationBuilder.DropTable(
                name: "EFChats");

            migrationBuilder.DropTable(
                name: "EFCurrentAliases");

            migrationBuilder.DropTable(
                name: "EFNotes");

            migrationBuilder.DropTable(
                name: "EFPenaltyIdentifiers");

            migrationBuilder.DropTable(
                name: "EFServerConnections");

            migrationBuilder.DropTable(
                name: "EFTomatoCounters");

            migrationBuilder.DropTable(
                name: "EFAliases");

            migrationBuilder.DropTable(
                name: "EFPenalties");

            migrationBuilder.DropTable(
                name: "EFServers");

            migrationBuilder.DropTable(
                name: "EFPlayers");

            migrationBuilder.DropTable(
                name: "EFCommunities");
        }
    }
}
