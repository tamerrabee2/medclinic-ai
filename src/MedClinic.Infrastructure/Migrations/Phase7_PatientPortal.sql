-- Phase 7: Patient Portal
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

CREATE INDEX "IX_PatientPortalAccesses_ClinicId_PatientId" ON "PatientPortalAccesses"("ClinicId", "PatientId");
CREATE INDEX "IX_PatientPortalMessages_ClinicId_PatientId" ON "PatientPortalMessages"("ClinicId", "PatientId");
