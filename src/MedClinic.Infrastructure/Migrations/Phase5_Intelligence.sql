-- Phase 5: Intelligence — Voice Scribe, Patient Brief, Follow-up, Timeline
-- EF Core migration reference (actual migrations generated via dotnet-ef)

-- VoiceNotes
CREATE TABLE IF NOT EXISTS "VoiceNotes" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "DoctorId" uuid NOT NULL,
    "VisitId" uuid,
    "AudioFileUrl" text NOT NULL,
    "AudioFileSizeBytes" bigint NOT NULL DEFAULT 0,
    "DurationSeconds" integer NOT NULL DEFAULT 0,
    "MimeType" varchar(50) NOT NULL DEFAULT 'audio/webm',
    "RawTranscript" text,
    "TranscriptLanguage" varchar(10),
    "TranscriptConfidence" decimal(5,4),
    "Status" integer NOT NULL DEFAULT 0,
    "ChiefComplaint" text,
    "HistoryOfPresentIllness" text,
    "PhysicalExamination" text,
    "Assessment" text,
    "Plan" text,
    "AdditionalNotes" text,
    "DoctorApproved" boolean NOT NULL DEFAULT false,
    "DoctorApprovedAt" timestamptz,
    "ApprovedByDoctorId" uuid,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_VoiceNotes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_VoiceNotes_Clinics" FOREIGN KEY ("ClinicId") REFERENCES "Clinics"("Id"),
    CONSTRAINT "FK_VoiceNotes_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_VoiceNotes_PatientId" ON "VoiceNotes"("PatientId");
CREATE INDEX "IX_VoiceNotes_ClinicId_Status" ON "VoiceNotes"("ClinicId", "Status");

-- PatientBriefs
CREATE TABLE IF NOT EXISTS "PatientBriefs" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "VisitId" uuid,
    "Summary" text NOT NULL,
    "RecentChangesJson" text NOT NULL DEFAULT '[]',
    "PendingItemsJson" text NOT NULL DEFAULT '[]',
    "AlertsJson" text NOT NULL DEFAULT '[]',
    "AIModel" varchar(100) NOT NULL DEFAULT '',
    "ConfidenceScore" decimal(5,4) NOT NULL DEFAULT 0,
    "RequiresDoctorReview" boolean NOT NULL DEFAULT true,
    "DoctorReviewed" boolean NOT NULL DEFAULT false,
    "ReviewedAt" timestamptz,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_PatientBriefs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PatientBriefs_Clinics" FOREIGN KEY ("ClinicId") REFERENCES "Clinics"("Id"),
    CONSTRAINT "FK_PatientBriefs_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_PatientBriefs_PatientId" ON "PatientBriefs"("PatientId");

-- FollowUpIntelligences
CREATE TABLE IF NOT EXISTS "FollowUpIntelligences" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "VisitId" uuid,
    "Reason" text NOT NULL,
    "RecommendedAction" text NOT NULL,
    "Priority" integer NOT NULL DEFAULT 2,
    "SuggestedDaysFromNow" integer NOT NULL DEFAULT 0,
    "SuggestedDate" timestamptz,
    "TriggersJson" text NOT NULL DEFAULT '[]',
    "DoctorApproved" boolean NOT NULL DEFAULT false,
    "DoctorDismissed" boolean NOT NULL DEFAULT false,
    "DoctorNote" text,
    "ActionedAt" timestamptz,
    "ActionedByDoctorId" uuid,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_FollowUpIntelligences" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FollowUpIntelligences_Clinics" FOREIGN KEY ("ClinicId") REFERENCES "Clinics"("Id"),
    CONSTRAINT "FK_FollowUpIntelligences_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_FollowUpIntelligences_PatientId_Priority" ON "FollowUpIntelligences"("PatientId", "Priority");

-- ClinicalTimelineEvents
CREATE TABLE IF NOT EXISTS "ClinicalTimelineEvents" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "ClinicId" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "EventType" integer NOT NULL DEFAULT 0,
    "Title" varchar(500) NOT NULL,
    "Description" text,
    "EventDate" timestamptz NOT NULL,
    "SourceEntityId" uuid,
    "SourceEntityType" varchar(100),
    "AISignificance" text,
    "IsAIHighlighted" boolean NOT NULL DEFAULT false,
    "MetadataJson" text NOT NULL DEFAULT '{}',
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "CreatedBy" uuid,
    "UpdatedAt" timestamptz,
    "UpdatedBy" uuid,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_ClinicalTimelineEvents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ClinicalTimelineEvents_Clinics" FOREIGN KEY ("ClinicId") REFERENCES "Clinics"("Id"),
    CONSTRAINT "FK_ClinicalTimelineEvents_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients"("Id")
);
CREATE INDEX "IX_ClinicalTimelineEvents_PatientId_EventDate" ON "ClinicalTimelineEvents"("PatientId", "EventDate" DESC);
CREATE INDEX "IX_ClinicalTimelineEvents_AIHighlighted" ON "ClinicalTimelineEvents"("IsAIHighlighted") WHERE "IsAIHighlighted" = true;
