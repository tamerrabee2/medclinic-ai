using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsageEventScopedIdempotencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsageEvents_ClinicId_IdempotencyKey",
                table: "UsageEvents");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEvents_ClinicId_MetricType_IdempotencyKey",
                table: "UsageEvents",
                columns: new[] { "ClinicId", "MetricType", "IdempotencyKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UsageEvents_ClinicId_MetricType_IdempotencyKey",
                table: "UsageEvents");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEvents_ClinicId_IdempotencyKey",
                table: "UsageEvents",
                columns: new[] { "ClinicId", "IdempotencyKey" },
                unique: true);
        }
    }
}
