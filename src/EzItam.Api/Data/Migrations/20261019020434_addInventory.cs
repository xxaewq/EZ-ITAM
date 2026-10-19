using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzItam.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class addInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    DateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InventoryStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    IdAsset = table.Column<int>(type: "int", nullable: false),
                    IdLocation = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IdEmployee = table.Column<int>(type: "int", nullable: false),
                    IdLocationIV = table.Column<int>(type: "int", nullable: false),
                    AssetStatusIV = table.Column<int>(type: "int", nullable: false),
                    IdEmployeeIV = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ScanCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Assets_IdAsset",
                        column: x => x.IdAsset,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Employees_IdEmployee",
                        column: x => x.IdEmployee,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Employees_IdEmployeeIV",
                        column: x => x.IdEmployeeIV,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Locations_IdLocation",
                        column: x => x.IdLocation,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryDetails_Locations_IdLocationIV",
                        column: x => x.IdLocationIV,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_Name",
                table: "Inventories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_IdAsset",
                table: "InventoryDetails",
                column: "IdAsset");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_IdEmployee",
                table: "InventoryDetails",
                column: "IdEmployee");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_IdEmployeeIV",
                table: "InventoryDetails",
                column: "IdEmployeeIV");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_IdLocation",
                table: "InventoryDetails",
                column: "IdLocation");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_IdLocationIV",
                table: "InventoryDetails",
                column: "IdLocationIV");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_InventoryId_IdAsset",
                table: "InventoryDetails",
                columns: new[] { "InventoryId", "IdAsset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDetails_InventoryId_ScanCount",
                table: "InventoryDetails",
                columns: new[] { "InventoryId", "ScanCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryDetails");

            migrationBuilder.DropTable(
                name: "Inventories");
        }
    }
}
