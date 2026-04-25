using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlumbingAIS.Backend.Migrations
{
    public partial class FixLocationStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Locations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Locations");
        }
    }
}