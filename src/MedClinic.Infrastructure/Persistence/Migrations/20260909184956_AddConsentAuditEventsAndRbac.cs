using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentAuditEventsAndRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrantedByUserId",
                table: "ConsentRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevocationReason",
                table: "ConsentRecords",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "ConsentRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevokedByUserId",
                table: "ConsentRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsentAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    ConsentType = table.Column<int>(type: "integer", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentAuditEvents_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConsentAuditEvents_ConsentRecords_ConsentRecordId",
                        column: x => x.ConsentRecordId,
                        principalTable: "ConsentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentAuditEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_GrantedByUserId",
                table: "ConsentRecords",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_RevokedByUserId",
                table: "ConsentRecords",
                column: "RevokedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAuditEvents_ClinicId_ConsentRecordId",
                table: "ConsentAuditEvents",
                columns: new[] { "ClinicId", "ConsentRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAuditEvents_ClinicId_PatientId_Timestamp",
                table: "ConsentAuditEvents",
                columns: new[] { "ClinicId", "PatientId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAuditEvents_ConsentRecordId",
                table: "ConsentAuditEvents",
                column: "ConsentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAuditEvents_PatientId",
                table: "ConsentAuditEvents",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentAuditEvents_PerformedByUserId",
                table: "ConsentAuditEvents",
                column: "PerformedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentRecords_AspNetUsers_GrantedByUserId",
                table: "ConsentRecords",
                column: "GrantedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentRecords_AspNetUsers_RevokedByUserId",
                table: "ConsentRecords",
                column: "RevokedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentRecords_AspNetUsers_GrantedByUserId",
                table: "ConsentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentRecords_AspNetUsers_RevokedByUserId",
                table: "ConsentRecords");

            migrationBuilder.DropTable(
                name: "ConsentAuditEvents");

            migrationBuilder.DropIndex(
                name: "IX_ConsentRecords_GrantedByUserId",
                table: "ConsentRecords");

            migrationBuilder.DropIndex(
                name: "IX_ConsentRecords_RevokedByUserId",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "GrantedByUserId",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "RevokedByUserId",
                table: "ConsentRecords");
        }
    }
}
