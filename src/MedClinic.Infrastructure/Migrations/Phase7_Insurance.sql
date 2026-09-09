-- Phase 7: Insurance
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

CREATE INDEX "IX_InsurancePolicies_ClinicId_PatientId" ON "InsurancePolicies"("ClinicId", "PatientId");
CREATE INDEX "IX_InsuranceClaims_ClinicId_PatientId" ON "InsuranceClaims"("ClinicId", "PatientId");
