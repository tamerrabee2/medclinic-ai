using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedClinic.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalHoldAndRetentionFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "VoiceNotes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "VoiceNotes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "VoiceNotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Visits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Visits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Visits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ToothRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ToothRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ToothRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "RefreshTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "RadiologyStudies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "RadiologyStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "RadiologyStudies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Prescriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Prescriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Prescriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "PrescriptionItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "PrescriptionItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "PrescriptionItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Patients",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "PatientPortalMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "PatientPortalMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "PatientPortalMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "PatientPortalAccesses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "PatientPortalAccesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "PatientPortalAccesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "PatientBriefs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "PatientBriefs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "PatientBriefs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "NotificationLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "NotificationLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "NotificationLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "MedicalImages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "MedicalImages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "MedicalImages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "MedicalAnnotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "MedicalAnnotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "MedicalAnnotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "LabResults",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "LabResults",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "LabResults",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "LabResultItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "LabResultItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "LabResultItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "LabOrders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "LabOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "LabOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "InvoiceItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "InvoiceItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "InvoiceItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "InsuranceProviders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "InsuranceProviders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "InsuranceProviders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "InsurancePolicies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "InsurancePolicies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "InsurancePolicies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "InsuranceClaims",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "InsuranceClaims",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "InsuranceClaims",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "FollowUpIntelligences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "FollowUpIntelligences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "FollowUpIntelligences",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "FhirSyncRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "FhirSyncRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "FhirSyncRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ExternalLabSyncs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ExternalLabSyncs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ExternalLabSyncs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ExternalLabProviders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ExternalLabProviders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ExternalLabProviders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Doctors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Doctors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "DicomStudies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DicomStudies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "DicomStudies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "DicomSeries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DicomSeries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "DicomSeries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "DicomInstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DicomInstances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "DicomInstances",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "DentalRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DentalRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "DentalRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "DentalCharts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "DentalCharts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "DentalCharts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ConsentRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ConsentRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ConsentRecords",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ConsentAuditEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ConsentAuditEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ConsentAuditEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Clinics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Clinics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Clinics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ClinicReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ClinicReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ClinicReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ClinicMembers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ClinicMembers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ClinicMembers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "ClinicalTimelineEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "ClinicalTimelineEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "ClinicalTimelineEvents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "BodyMapAnnotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "BodyMapAnnotations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "BodyMapAnnotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "AuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "AiDecisionAudits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "AiDecisionAudits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "AiDecisionAudits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "AIConversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "AIConversations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "AIConversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "AIConversationMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "AIConversationMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "AIConversationMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLegalHold",
                table: "AIAnalyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalHoldReason",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetentionExpiresAt",
                table: "AIAnalyses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "VoiceNotes");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "VoiceNotes");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "VoiceNotes");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ToothRecords");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ToothRecords");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ToothRecords");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "RadiologyStudies");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "RadiologyStudies");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "RadiologyStudies");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "PatientPortalMessages");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "PatientPortalMessages");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "PatientPortalMessages");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "PatientPortalAccesses");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "PatientPortalAccesses");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "PatientPortalAccesses");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "PatientBriefs");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "PatientBriefs");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "PatientBriefs");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "NotificationLogs");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "MedicalImages");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "MedicalImages");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "MedicalImages");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "MedicalAnnotations");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "MedicalAnnotations");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "MedicalAnnotations");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "LabResults");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "LabResults");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "LabResults");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "LabResultItems");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "LabResultItems");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "LabResultItems");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "InsuranceProviders");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "InsuranceProviders");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "InsuranceProviders");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "InsuranceClaims");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "FollowUpIntelligences");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "FollowUpIntelligences");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "FollowUpIntelligences");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "FhirSyncRecords");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "FhirSyncRecords");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "FhirSyncRecords");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ExternalLabSyncs");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ExternalLabSyncs");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ExternalLabSyncs");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ExternalLabProviders");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ExternalLabProviders");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ExternalLabProviders");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "DicomStudies");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DicomStudies");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "DicomStudies");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "DicomSeries");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DicomSeries");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "DicomSeries");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "DicomInstances");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DicomInstances");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "DicomInstances");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "DentalRecords");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DentalRecords");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "DentalRecords");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "DentalCharts");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "DentalCharts");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "DentalCharts");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ConsentRecords");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ConsentAuditEvents");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ConsentAuditEvents");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ConsentAuditEvents");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ClinicReports");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ClinicReports");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ClinicReports");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ClinicMembers");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ClinicMembers");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ClinicMembers");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "ClinicalTimelineEvents");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "ClinicalTimelineEvents");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "ClinicalTimelineEvents");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "BodyMapAnnotations");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "BodyMapAnnotations");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "BodyMapAnnotations");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "AiDecisionAudits");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "AiDecisionAudits");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "AiDecisionAudits");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "AIConversationMessages");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "AIConversationMessages");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "AIConversationMessages");

            migrationBuilder.DropColumn(
                name: "IsLegalHold",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "LegalHoldReason",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "RetentionExpiresAt",
                table: "AIAnalyses");
        }
    }
}
