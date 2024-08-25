using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project_OCS.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreProductImagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoreProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreProductImages_StoreProducts_ItemId",
                        column: x => x.ItemId,
                        principalTable: "StoreProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreProductImages_ItemId",
                table: "StoreProductImages",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreProductImages");
        }
    }
}
