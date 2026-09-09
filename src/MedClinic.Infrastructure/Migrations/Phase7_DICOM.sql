-- Phase 7: DICOM / PACS Integration
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
    CONSTRAINT "FK_DicomStudies_Clinics" FOREIGN KEY ("ClinicId") REFERENCES "Clinics"("Id"),
    CONSTRAINT "FK_DicomStudies_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id"),
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

CREATE INDEX "IX_DicomStudies_PatientId_StudyDate" ON "DicomStudies"("PatientId", "StudyDate" DESC);
CREATE INDEX "IX_DicomStudies_ClinicId_Modality" ON "DicomStudies"("ClinicId", "Modality");
CREATE INDEX "IX_DicomSeries_DicomStudyId" ON "DicomSeries"("DicomStudyId");
CREATE INDEX "IX_DicomInstances_DicomSeriesId" ON "DicomInstances"("DicomSeriesId");
