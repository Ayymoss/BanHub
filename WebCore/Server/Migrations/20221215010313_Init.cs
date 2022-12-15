using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalBan.WebCore.Server.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstanceGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    InstanceIp = table.Column<string>(type: "TEXT", nullable: false),
                    InstanceName = table.Column<string>(type: "TEXT", nullable: true),
                    ApiKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileGuid = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileGame = table.Column<string>(type: "TEXT", nullable: false),
                    ProfileIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    Reputation = table.Column<int>(type: "INTEGER", nullable: false),
                    LastConnected = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Infractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InfractionType = table.Column<int>(type: "INTEGER", nullable: false),
                    InfractionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    InfractionScope = table.Column<int>(type: "INTEGER", nullable: false),
                    InfractionGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Submitted = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AdminGuid = table.Column<string>(type: "TEXT", nullable: false),
                    AdminUserName = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Infractions_Instances_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Infractions_Profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileMetas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Changed = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileMetas_Profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_ServerId",
                table: "Infractions",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_UserId",
                table: "Infractions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileMetas_UserId",
                table: "ProfileMetas",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Infractions");

            migrationBuilder.DropTable(
                name: "ProfileMetas");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
