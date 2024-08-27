using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project_OCS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedColumnToStoreAndStoreCategoryAndContactUsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StoreCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContactUs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StoreCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContactUs");
        }
    }
}
