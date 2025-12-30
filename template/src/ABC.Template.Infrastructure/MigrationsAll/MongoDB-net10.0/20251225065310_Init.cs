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
            // MongoDB EF Core provider automatically creates collections based on entity configurations
            // Entity configurations in ApplicationDbContext define the collection and document structure
            // No SQL DDL statements are needed for schema creation
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // MongoDB collections can be dropped if needed
        }
    }
}
