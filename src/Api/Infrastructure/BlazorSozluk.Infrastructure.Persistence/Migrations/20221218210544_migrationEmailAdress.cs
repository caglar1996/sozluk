using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorSozluk.Infrastructure.Persistence.Migrations
{
    public partial class migrationEmailAdress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAdress",
                schema: "dbo",
                table: "user",
                newName: "EmailAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                schema: "dbo",
                table: "user",
                newName: "EmailAdress");
        }
    }
}
