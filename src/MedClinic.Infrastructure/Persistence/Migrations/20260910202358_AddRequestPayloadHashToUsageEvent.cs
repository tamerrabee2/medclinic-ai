using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestPayloadHashToUsageEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestPayloadHash",
                table: "UsageEvents",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestPayloadHash",
                table: "UsageEvents");
        }
    }
}
