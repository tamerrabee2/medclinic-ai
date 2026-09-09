using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestrictConsentAuditCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAuditEvents_ConsentRecords_ConsentRecordId",
                table: "ConsentAuditEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAuditEvents_ConsentRecords_ConsentRecordId",
                table: "ConsentAuditEvents",
                column: "ConsentRecordId",
                principalTable: "ConsentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAuditEvents_ConsentRecords_ConsentRecordId",
                table: "ConsentAuditEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAuditEvents_ConsentRecords_ConsentRecordId",
                table: "ConsentAuditEvents",
                column: "ConsentRecordId",
                principalTable: "ConsentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
