using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiDecisionAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiDecisionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Capability = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InputHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OutputHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ConfidenceScore = table.Column<double>(type: "double precision", nullable: true),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_AiDecisionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiDecisionAudits_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDecisionAudits_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDecisionAudits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AiDecisionAudits_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalTimelineEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEntityType = table.Column<string>(type: "text", nullable: true),
                    AISignificance = table.Column<string>(type: "text", nullable: true),
                    IsAIHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalTimelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalTimelineEvents_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalTimelineEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    PeriodFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    DataJson = table.Column<string>(type: "text", nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicReports_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DentalCharts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentalCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentalCharts_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DentalCharts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DicomStudies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudyInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AccessionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    StudyDescription = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    StudyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PacsStudyUrl = table.Column<string>(type: "text", nullable: true),
                    WadoRsBaseUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SeriesCount = table.Column<int>(type: "integer", nullable: false),
                    InstanceCount = table.Column<int>(type: "integer", nullable: false),
                    HasAIAnalysis = table.Column<bool>(type: "boolean", nullable: false),
                    AIFindings = table.Column<string>(type: "text", nullable: true),
                    AIReviewedByDoctor = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomStudies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DicomStudies_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DicomStudies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLabProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: true),
                    AuthType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLabProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLabProviders_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FhirSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalSystem = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FhirSyncRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FhirSyncRecords_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FhirSyncRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FollowUpIntelligences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    RecommendedAction = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    SuggestedDaysFromNow = table.Column<int>(type: "integer", nullable: false),
                    SuggestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggersJson = table.Column<string>(type: "text", nullable: false),
                    DoctorApproved = table.Column<bool>(type: "boolean", nullable: false),
                    DoctorDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    DoctorNote = table.Column<string>(type: "text", nullable: true),
                    ActionedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpIntelligences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowUpIntelligences_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowUpIntelligences_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsurancePolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    ResponseJson = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PayerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsuranceProviders_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Recipient = table.Column<string>(type: "text", nullable: true),
                    DeliveryStatus = table.Column<int>(type: "integer", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientBriefs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    RecentChangesJson = table.Column<string>(type: "text", nullable: false),
                    PendingItemsJson = table.Column<string>(type: "text", nullable: false),
                    AlertsJson = table.Column<string>(type: "text", nullable: false),
                    AIModel = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    RequiresDoctorReview = table.Column<bool>(type: "boolean", nullable: false),
                    DoctorReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientBriefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientBriefs_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientBriefs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPortalAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPortalAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalAccesses_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientPortalAccesses_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPortalMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SenderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPortalMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalMessages_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientPortalMessages_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    AudioFileUrl = table.Column<string>(type: "text", nullable: false),
                    AudioFileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    RawTranscript = table.Column<string>(type: "text", nullable: true),
                    TranscriptLanguage = table.Column<string>(type: "text", nullable: true),
                    TranscriptConfidence = table.Column<decimal>(type: "numeric", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "text", nullable: true),
                    HistoryOfPresentIllness = table.Column<string>(type: "text", nullable: true),
                    PhysicalExamination = table.Column<string>(type: "text", nullable: true),
                    Assessment = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true),
                    DoctorApproved = table.Column<bool>(type: "boolean", nullable: false),
                    DoctorApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceNotes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoiceNotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToothRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DentalChartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToothNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SurfaceDataJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToothRecords_DentalCharts_DentalChartId",
                        column: x => x.DentalChartId,
                        principalTable: "DentalCharts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DicomSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DicomStudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SeriesNumber = table.Column<int>(type: "integer", nullable: false),
                    SeriesDescription = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Modality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    InstanceCount = table.Column<int>(type: "integer", nullable: false),
                    BodyPartExamined = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DicomSeries_DicomStudies_DicomStudyId",
                        column: x => x.DicomStudyId,
                        principalTable: "DicomStudies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLabSyncs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalLabProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalOrderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    ResultJson = table.Column<string>(type: "text", nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLabSyncs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLabSyncs_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalLabSyncs_ExternalLabProviders_ExternalLabProviderId",
                        column: x => x.ExternalLabProviderId,
                        principalTable: "ExternalLabProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalLabSyncs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MemberId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoveragePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsurancePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsurancePolicies_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsurancePolicies_InsuranceProviders_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalTable: "InsuranceProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsurancePolicies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DicomInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DicomSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SopInstanceUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SopClassUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InstanceNumber = table.Column<int>(type: "integer", nullable: false),
                    WadoUri = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    Rows = table.Column<int>(type: "integer", nullable: true),
                    Columns = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DicomInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DicomInstances_DicomSeries_DicomSeriesId",
                        column: x => x.DicomSeriesId,
                        principalTable: "DicomSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_ClinicId_Capability",
                table: "AiDecisionAudits",
                columns: new[] { "ClinicId", "Capability" });

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_CorrelationId",
                table: "AiDecisionAudits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_DoctorId",
                table: "AiDecisionAudits",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_PatientId",
                table: "AiDecisionAudits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_ReviewedByUserId",
                table: "AiDecisionAudits",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_ReviewStatus",
                table: "AiDecisionAudits",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AiDecisionAudits_VisitId",
                table: "AiDecisionAudits",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTimelineEvents_ClinicId",
                table: "ClinicalTimelineEvents",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalTimelineEvents_PatientId",
                table: "ClinicalTimelineEvents",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReports_ClinicId",
                table: "ClinicReports",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalCharts_ClinicId",
                table: "DentalCharts",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalCharts_PatientId",
                table: "DentalCharts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DicomInstances_DicomSeriesId",
                table: "DicomInstances",
                column: "DicomSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_DicomSeries_DicomStudyId",
                table: "DicomSeries",
                column: "DicomStudyId");

            migrationBuilder.CreateIndex(
                name: "IX_DicomStudies_ClinicId_Modality",
                table: "DicomStudies",
                columns: new[] { "ClinicId", "Modality" });

            migrationBuilder.CreateIndex(
                name: "IX_DicomStudies_PatientId_StudyDate",
                table: "DicomStudies",
                columns: new[] { "PatientId", "StudyDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DicomStudies_StudyInstanceUid",
                table: "DicomStudies",
                column: "StudyInstanceUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabProviders_ClinicId",
                table: "ExternalLabProviders",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabSyncs_ClinicId_PatientId",
                table: "ExternalLabSyncs",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabSyncs_ExternalLabProviderId",
                table: "ExternalLabSyncs",
                column: "ExternalLabProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLabSyncs_PatientId",
                table: "ExternalLabSyncs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_FhirSyncRecords_ClinicId_PatientId",
                table: "FhirSyncRecords",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_FhirSyncRecords_PatientId",
                table: "FhirSyncRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpIntelligences_ClinicId",
                table: "FollowUpIntelligences",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpIntelligences_PatientId",
                table: "FollowUpIntelligences",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ClinicId_PatientId",
                table: "InsuranceClaims",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_PatientId",
                table: "InsuranceClaims",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_ClinicId_PatientId",
                table: "InsurancePolicies",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_InsuranceProviderId",
                table: "InsurancePolicies",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_PatientId",
                table: "InsurancePolicies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProviders_ClinicId",
                table: "InsuranceProviders",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_ClinicId",
                table: "NotificationLogs",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientBriefs_ClinicId",
                table: "PatientBriefs",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientBriefs_PatientId",
                table: "PatientBriefs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccesses_ClinicId_PatientId",
                table: "PatientPortalAccesses",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccesses_Email",
                table: "PatientPortalAccesses",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccesses_PatientId",
                table: "PatientPortalAccesses",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalMessages_ClinicId_PatientId",
                table: "PatientPortalMessages",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalMessages_PatientId",
                table: "PatientPortalMessages",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_DentalChartId",
                table: "ToothRecords",
                column: "DentalChartId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceNotes_ClinicId",
                table: "VoiceNotes",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceNotes_PatientId",
                table: "VoiceNotes",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiDecisionAudits");

            migrationBuilder.DropTable(
                name: "ClinicalTimelineEvents");

            migrationBuilder.DropTable(
                name: "ClinicReports");

            migrationBuilder.DropTable(
                name: "DicomInstances");

            migrationBuilder.DropTable(
                name: "ExternalLabSyncs");

            migrationBuilder.DropTable(
                name: "FhirSyncRecords");

            migrationBuilder.DropTable(
                name: "FollowUpIntelligences");

            migrationBuilder.DropTable(
                name: "InsuranceClaims");

            migrationBuilder.DropTable(
                name: "InsurancePolicies");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "PatientBriefs");

            migrationBuilder.DropTable(
                name: "PatientPortalAccesses");

            migrationBuilder.DropTable(
                name: "PatientPortalMessages");

            migrationBuilder.DropTable(
                name: "ToothRecords");

            migrationBuilder.DropTable(
                name: "VoiceNotes");

            migrationBuilder.DropTable(
                name: "DicomSeries");

            migrationBuilder.DropTable(
                name: "ExternalLabProviders");

            migrationBuilder.DropTable(
                name: "InsuranceProviders");

            migrationBuilder.DropTable(
                name: "DentalCharts");

            migrationBuilder.DropTable(
                name: "DicomStudies");
        }
    }
}
