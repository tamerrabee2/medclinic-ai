-- Phase 7: Dental Module
CREATE TABLE IF NOT EXISTS "DentalCharts" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "VisitId" uuid,
    "Notes" text NOT NULL DEFAULT '',
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_DentalCharts" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ToothRecords" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "DentalChartId" uuid NOT NULL,
    "ToothNumber" integer NOT NULL,
    "Status" integer NOT NULL DEFAULT 0,
    "Notes" text,
    "SurfaceDataJson" text NOT NULL DEFAULT '{}',
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_ToothRecords" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToothRecords_DentalCharts" FOREIGN KEY ("DentalChartId") REFERENCES "DentalCharts"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_DentalCharts_ClinicId_PatientId" ON "DentalCharts"("ClinicId", "PatientId");
CREATE INDEX "IX_ToothRecords_DentalChartId_ToothNumber" ON "ToothRecords"("DentalChartId", "ToothNumber");
