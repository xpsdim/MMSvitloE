using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMSvitloE.Migrations
{
    public partial class Event_Index_ByDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Events_DateUtc",
                table: "Events",
                column: "DateUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_DateUtc",
                table: "Events");
        }
    }
}
