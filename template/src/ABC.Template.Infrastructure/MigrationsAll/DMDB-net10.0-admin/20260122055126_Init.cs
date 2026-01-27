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
                name: "dept",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Remark = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ParentId = table.Column<long>(type: "BIGINT", nullable: false),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dept", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    RealName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "INT", nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "BIT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    LastLoginTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: true),
                    UpdateTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    IsDeleted = table.Column<bool>(type: "BIT", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    Gender = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    Age = table.Column<int>(type: "INT", nullable: false),
                    BirthDate = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                    PermissionCode = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PermissionDescription = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permission_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_dept",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "BIGINT", nullable: false),
                    DeptId = table.Column<long>(type: "BIGINT", nullable: false),
                    DeptName = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_dept", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_dept_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_refresh_token",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT", nullable: false),
                    UserId = table.Column<long>(type: "BIGINT", nullable: false),
                    Token = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    ExpiresTime = table.Column<DateTimeOffset>(type: "DATETIME WITH TIME ZONE", nullable: false),
                    IsUsed = table.Column<bool>(type: "BIT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "BIT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_refresh_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_refresh_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "BIGINT", nullable: false),
                    RoleId = table.Column<Guid>(type: "CHAR(36)", nullable: false),
                    RoleName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_role_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_dept_IsDeleted",
                table: "dept",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_dept_ParentId",
                table: "dept",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_Status",
                table: "dept",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_role_Name",
                table: "role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_user_Name",
                table: "user",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_DeptId",
                table: "user_dept",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_UserId",
                table: "user_dept",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_token_UserId",
                table: "user_refresh_token",
                column: "UserId");
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
                name: "dept");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "user_dept");

            migrationBuilder.DropTable(
                name: "user_refresh_token");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
