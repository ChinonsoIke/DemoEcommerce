using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Embedded",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Embedded",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Embedded",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedded",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Embedded",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Embedded",
                table: "Categories");
        }
    }
}
