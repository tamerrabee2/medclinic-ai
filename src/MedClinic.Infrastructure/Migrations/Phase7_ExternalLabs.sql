-- Phase 7: External Labs
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

CREATE INDEX "IX_ExternalLabSyncs_ClinicId_PatientId" ON "ExternalLabSyncs"("ClinicId", "PatientId");
