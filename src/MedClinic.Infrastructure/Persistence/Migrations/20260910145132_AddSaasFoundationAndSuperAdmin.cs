using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaasFoundationAndSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingStatus",
                table: "Clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComplianceStatus",
                table: "Clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "Clinics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "Clinics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuspendedByUserId",
                table: "Clinics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Clinics",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndsAt",
                table: "Clinics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    MaxDoctors = table.Column<int>(type: "integer", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    MaxPatients = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    MonthlyAiRequestsLimit = table.Column<int>(type: "integer", nullable: false),
                    MaxDicomStudiesMonthly = table.Column<int>(type: "integer", nullable: false),
                    FeaturesJson = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLegalHold = table.Column<bool>(type: "boolean", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "text", nullable: true),
                    RetentionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantLifecycleAuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    OldValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedByUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLegalHold = table.Column<bool>(type: "boolean", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "text", nullable: true),
                    RetentionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantLifecycleAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantLifecycleAuditEvents_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TenantLifecycleAuditEvents_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<int>(type: "integer", nullable: false),
                    PeriodStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: false),
                    QuotaLimit = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLegalHold = table.Column<bool>(type: "boolean", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "text", nullable: true),
                    RetentionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageMetrics_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BillingStatus = table.Column<int>(type: "integer", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextBillingDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GracePeriodEndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    PlanCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TierSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AnnualPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrencySnapshot = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    FeaturesSnapshot = table.Column<string>(type: "text", nullable: false),
                    MaxDoctorsSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MaxUsersSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MaxPatientsSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MaxStorageBytesSnapshot = table.Column<long>(type: "bigint", nullable: false),
                    MonthlyAiRequestsLimitSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MaxDicomStudiesMonthlySnapshot = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLegalHold = table.Column<bool>(type: "boolean", nullable: false),
                    LegalHoldReason = table.Column<string>(type: "text", nullable: true),
                    RetentionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicSubscriptions_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicSubscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "AnnualPrice", "Code", "CreatedAt", "CreatedBy", "Currency", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "FeaturesJson", "IsActive", "IsDeleted", "IsLegalHold", "LegalHoldReason", "MaxDicomStudiesMonthly", "MaxDoctors", "MaxPatients", "MaxStorageBytes", "MaxUsers", "MonthlyAiRequestsLimit", "MonthlyPrice", "Name", "RetentionExpiresAt", "Tier", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 490.00m, "basic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "USD", null, null, "Essential clinical features for solo practitioners and small clinics.", 1, "[\"patient_portal\"]", true, false, false, null, 0, 3, 500, 5368709120L, 5, 100, 49.00m, "Basic Clinic", null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 1490.00m, "pro", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "USD", null, null, "Advanced features including AI Copilot, DICOM PACS, and lab integrations.", 2, "[\"ai_copilot\",\"dicom_pacs\",\"external_labs\",\"patient_portal\",\"advanced_reports\"]", true, false, false, null, 100, 15, 5000, 53687091200L, 25, 1500, 149.00m, "Professional Clinic", null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 4990.00m, "enterprise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "USD", null, null, "Unlimited multi-specialty clinical operations with full API access and custom branding.", 3, "[\"ai_copilot\",\"dicom_pacs\",\"dental\",\"external_labs\",\"insurance\",\"patient_portal\",\"advanced_reports\",\"custom_branding\",\"api_access\"]", true, false, false, null, 0, 0, 0, 0L, 0, 0, 499.00m, "Enterprise Hospital", null, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_BillingStatus",
                table: "Clinics",
                column: "BillingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ComplianceStatus",
                table: "Clinics",
                column: "ComplianceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_LifecycleStatus",
                table: "Clinics",
                column: "LifecycleStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSubscriptions_BillingStatus",
                table: "ClinicSubscriptions",
                column: "BillingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSubscriptions_ClinicId_IsActive",
                table: "ClinicSubscriptions",
                columns: new[] { "ClinicId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSubscriptions_Status",
                table: "ClinicSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicSubscriptions_SubscriptionPlanId",
                table: "ClinicSubscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Code",
                table: "SubscriptionPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantLifecycleAuditEvents_ClinicId_EventType",
                table: "TenantLifecycleAuditEvents",
                columns: new[] { "ClinicId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantLifecycleAuditEvents_ClinicId_Timestamp",
                table: "TenantLifecycleAuditEvents",
                columns: new[] { "ClinicId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantLifecycleAuditEvents_EventType",
                table: "TenantLifecycleAuditEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_TenantLifecycleAuditEvents_PerformedByUserId",
                table: "TenantLifecycleAuditEvents",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageMetrics_ClinicId_MetricType_PeriodStartUtc",
                table: "UsageMetrics",
                columns: new[] { "ClinicId", "MetricType", "PeriodStartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageMetrics_ClinicId_PeriodEndUtc",
                table: "UsageMetrics",
                columns: new[] { "ClinicId", "PeriodEndUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicSubscriptions");

            migrationBuilder.DropTable(
                name: "TenantLifecycleAuditEvents");

            migrationBuilder.DropTable(
                name: "UsageMetrics");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_BillingStatus",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_ComplianceStatus",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_LifecycleStatus",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "BillingStatus",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ComplianceStatus",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "SuspendedByUserId",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "TrialEndsAt",
                table: "Clinics");
        }
    }
}
