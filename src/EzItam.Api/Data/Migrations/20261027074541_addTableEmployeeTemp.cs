using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EzItam.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class addTableEmployeeTemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Employees",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeTemps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrgCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EmployeeNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeptCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DeptName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeptLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TitleCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TitleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PositionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsResigned = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    DateValue1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue3 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue4 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue5 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue6 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue7 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue8 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateValue9 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTemps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTemps_OrgCode_EmployeeNo",
                table: "EmployeeTemps",
                columns: new[] { "OrgCode", "EmployeeNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeTemps");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Employees");
        }
    }
}
