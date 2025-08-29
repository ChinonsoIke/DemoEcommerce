using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbedding2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Embedding",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Categories");
        }
    }
}
