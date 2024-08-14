using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project_OCS.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberOfAdsAllowedToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfAdsAllowed",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfAdsAllowed",
                table: "AspNetUsers");
        }
    }
}
