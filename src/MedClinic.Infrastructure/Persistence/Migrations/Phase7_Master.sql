-- =============================================================
-- Phase 7 Master Migration (run all in order)
-- =============================================================
-- 1. Dental
-- \i Phase7_Dental.sql
-- 2. DICOM
-- \i Phase7_DICOM.sql
-- 3. FHIR
-- \i Phase7_FHIR.sql
-- 4. External Labs
-- \i Phase7_ExternalLabs.sql
-- 5. Insurance
-- \i Phase7_Insurance.sql
-- 6. Patient Portal
-- \i Phase7_PatientPortal.sql
-- =============================================================

-- DICOM
CREATE TABLE IF NOT EXISTS "DicomStudies" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "DoctorId" uuid,
    "VisitId" uuid,
    "StudyInstanceUid" varchar(128) NOT NULL,
    "AccessionNumber" varchar(64),
    "StudyDescription" varchar(256),
    "StudyDate" timestamptz NOT NULL,
    "Modality" varchar(16) NOT NULL,
    "PacsStudyUrl" text,
    "WadoRsBaseUrl" text,
    "Status" integer NOT NULL DEFAULT 0,
    "SeriesCount" integer NOT NULL DEFAULT 0,
    "InstanceCount" integer NOT NULL DEFAULT 0,
    "HasAIAnalysis" boolean NOT NULL DEFAULT false,
    "AIFindings" text,
    "AIReviewedByDoctor" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_DicomStudies" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_DicomStudies_StudyInstanceUid" UNIQUE ("StudyInstanceUid")
);

CREATE TABLE IF NOT EXISTS "DicomSeries" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "DicomStudyId" uuid NOT NULL,
    "SeriesInstanceUid" varchar(128) NOT NULL,
    "SeriesNumber" integer NOT NULL DEFAULT 0,
    "SeriesDescription" varchar(256),
    "Modality" varchar(16) NOT NULL,
    "InstanceCount" integer NOT NULL DEFAULT 0,
    "BodyPartExamined" varchar(64),
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_DicomSeries" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DicomSeries_DicomStudies" FOREIGN KEY ("DicomStudyId") REFERENCES "DicomStudies"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "DicomInstances" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "DicomSeriesId" uuid NOT NULL,
    "SopInstanceUid" varchar(128) NOT NULL,
    "SopClassUid" varchar(128) NOT NULL,
    "InstanceNumber" integer NOT NULL DEFAULT 0,
    "WadoUri" text,
    "ThumbnailUrl" text,
    "Rows" integer,
    "Columns" integer,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_DicomInstances" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DicomInstances_DicomSeries" FOREIGN KEY ("DicomSeriesId") REFERENCES "DicomSeries"("Id") ON DELETE CASCADE
);

-- FHIR
CREATE TABLE IF NOT EXISTS "FhirSyncRecords" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "ResourceType" varchar(64) NOT NULL,
    "ResourceId" varchar(128) NOT NULL,
    "ExternalSystem" varchar(64),
    "PayloadJson" text NOT NULL DEFAULT '{}',
    "Direction" integer NOT NULL DEFAULT 1,
    "Status" integer NOT NULL DEFAULT 0,
    "ErrorMessage" text,
    "SyncedAt" timestamptz,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_FhirSyncRecords" PRIMARY KEY ("Id")
);

-- External Labs
CREATE TABLE IF NOT EXISTS "ExternalLabProviders" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "Name" varchar(128) NOT NULL,
    "ProviderCode" varchar(64) NOT NULL,
    "BaseUrl" text,
    "AuthType" varchar(64),
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_ExternalLabProviders" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ExternalLabSyncs" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "LabOrderId" uuid,
    "ExternalLabProviderId" uuid NOT NULL,
    "ExternalOrderId" varchar(128) NOT NULL,
    "Status" varchar(64) NOT NULL DEFAULT 'Pending',
    "PayloadJson" text NOT NULL DEFAULT '{}',
    "ResultJson" text,
    "SyncedAt" timestamptz,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_ExternalLabSyncs" PRIMARY KEY ("Id")
);

-- Insurance
CREATE TABLE IF NOT EXISTS "InsuranceProviders" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "Name" varchar(128) NOT NULL,
    "PayerCode" varchar(64) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_InsuranceProviders" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "InsurancePolicies" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "InsuranceProviderId" uuid NOT NULL,
    "PolicyNumber" varchar(128) NOT NULL,
    "MemberId" varchar(128) NOT NULL,
    "ValidFrom" timestamptz NOT NULL,
    "ValidTo" timestamptz NOT NULL,
    "CoveragePercentage" numeric(5,2) NOT NULL DEFAULT 0,
    "IsPrimary" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_InsurancePolicies" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "InsuranceClaims" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "InvoiceId" uuid NOT NULL,
    "InsurancePolicyId" uuid NOT NULL,
    "ClaimNumber" varchar(128) NOT NULL,
    "ClaimedAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "ApprovedAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "Status" integer NOT NULL DEFAULT 0,
    "PayloadJson" text NOT NULL DEFAULT '{}',
    "ResponseJson" text,
    "SubmittedAt" timestamptz,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_InsuranceClaims" PRIMARY KEY ("Id")
);

-- Patient Portal
CREATE TABLE IF NOT EXISTS "PatientPortalAccesses" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "Email" varchar(256) NOT NULL,
    "PasswordHash" text NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "LastLoginAt" timestamptz,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_PatientPortalAccesses" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PatientPortalMessages" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "UserId" uuid,
    "SenderType" varchar(32) NOT NULL,
    "Subject" varchar(256) NOT NULL,
    "Body" text NOT NULL,
    "IsRead" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_PatientPortalMessages" PRIMARY KEY ("Id")
);

-- ── Indexes ──────────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS "IX_DicomStudies_PatientId_StudyDate"   ON "DicomStudies"("PatientId", "StudyDate" DESC);
CREATE INDEX IF NOT EXISTS "IX_DicomStudies_ClinicId_Modality"     ON "DicomStudies"("ClinicId", "Modality");
CREATE INDEX IF NOT EXISTS "IX_DicomSeries_DicomStudyId"           ON "DicomSeries"("DicomStudyId");
CREATE INDEX IF NOT EXISTS "IX_DicomInstances_DicomSeriesId"       ON "DicomInstances"("DicomSeriesId");
CREATE INDEX IF NOT EXISTS "IX_FhirSyncRecords_ClinicId_PatientId" ON "FhirSyncRecords"("ClinicId", "PatientId");
CREATE INDEX IF NOT EXISTS "IX_ExternalLabSyncs_ClinicId_PatientId" ON "ExternalLabSyncs"("ClinicId", "PatientId");
CREATE INDEX IF NOT EXISTS "IX_InsurancePolicies_ClinicId_PatientId" ON "InsurancePolicies"("ClinicId", "PatientId");
CREATE INDEX IF NOT EXISTS "IX_InsuranceClaims_ClinicId_PatientId" ON "InsuranceClaims"("ClinicId", "PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientPortalAccesses_ClinicId_PatientId" ON "PatientPortalAccesses"("ClinicId", "PatientId");
CREATE INDEX IF NOT EXISTS "IX_PatientPortalAccesses_Email"        ON "PatientPortalAccesses"("Email");
CREATE INDEX IF NOT EXISTS "IX_PatientPortalMessages_ClinicId_PatientId" ON "PatientPortalMessages"("ClinicId", "PatientId");
