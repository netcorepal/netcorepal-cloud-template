using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABC.Template.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CAPLock",
                columns: table => new
                {
                    Key = table.Column<string>(type: "NVARCHAR2(128)", maxLength: 128, nullable: false),
                    Instance = table.Column<string>(type: "NVARCHAR2(256)", maxLength: 256, nullable: true),
                    LastLockTime = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPLock", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CAPPublishedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT", nullable: false),
                    Version = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "INT", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "NVARCHAR2(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPPublishedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAPReceivedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT", nullable: false),
                    Version = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(400)", maxLength: 400, nullable: false),
                    Group = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "INT", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPReceivedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deliverrecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                    OrderId = table.Column<Guid>(type: "CHAR(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deliverrecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                    Paid = table.Column<bool>(type: "BIT", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Count = table.Column<int>(type: "INT", nullable: false),
                    RowVersion = table.Column<int>(type: "INT", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAPLock");

            migrationBuilder.DropTable(
                name: "CAPPublishedMessage");

            migrationBuilder.DropTable(
                name: "CAPReceivedMessage");

            migrationBuilder.DropTable(
                name: "deliverrecord");

            migrationBuilder.DropTable(
                name: "order");
        }
    }
}
