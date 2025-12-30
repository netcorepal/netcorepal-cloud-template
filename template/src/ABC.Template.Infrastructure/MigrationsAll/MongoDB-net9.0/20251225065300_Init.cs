using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MongoDB EF Core provider automatically creates collections
            // No explicit schema creation is needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // MongoDB collections can be dropped if needed
        }
    }
}
