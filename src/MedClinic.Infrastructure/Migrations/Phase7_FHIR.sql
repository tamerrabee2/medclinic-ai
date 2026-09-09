-- Phase 7: FHIR Integration
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

CREATE INDEX "IX_FhirSyncRecords_ClinicId_PatientId" ON "FhirSyncRecords"("ClinicId", "PatientId");
CREATE INDEX "IX_FhirSyncRecords_ResourceType_Status" ON "FhirSyncRecords"("ResourceType", "Status");
