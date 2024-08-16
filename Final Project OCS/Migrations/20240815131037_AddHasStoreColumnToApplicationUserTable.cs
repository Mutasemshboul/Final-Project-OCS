using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project_OCS.Migrations
{
    /// <inheritdoc />
    public partial class AddHasStoreColumnToApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasStore",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasStore",
                table: "AspNetUsers");
        }
    }
}
