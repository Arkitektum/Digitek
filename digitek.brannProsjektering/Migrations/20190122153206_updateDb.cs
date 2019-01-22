using Microsoft.EntityFrameworkCore.Migrations;

namespace digitek.brannProsjektering.Migrations
{
    public partial class updateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ResponseText",
                table: "UseRecords",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "ExecutionNr",
                table: "UseRecords",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionNr",
                table: "UseRecords");

            migrationBuilder.AlterColumn<int>(
                name: "ResponseText",
                table: "UseRecords",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
