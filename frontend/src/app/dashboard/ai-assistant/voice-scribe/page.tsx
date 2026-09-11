"use client";

import { useState, useRef } from "react";
import { PermissionGate } from "@/components/auth/PermissionGate";
import { Mic, MicOff, Upload, CheckCircle, AlertCircle, Loader2 } from "lucide-react";

type RecordingState = "idle" | "recording" | "stopped" | "uploading" | "transcribing" | "done" | "error";

interface StructuredNote {
  chiefComplaint?: string;
  historyOfPresentIllness?: string;
  physicalExamination?: string;
  assessment?: string;
  plan?: string;
  rawTranscript?: string;
  confidence?: number;
}

export default function VoiceScribePage() {
  const [state, setState] = useState<RecordingState>("idle");
  const [noteId, setNoteId] = useState<string | null>(null);
  const [structured, setStructured] = useState<StructuredNote | null>(null);
  const [approved, setApproved] = useState(false);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<BlobPart[]>([]);

  const startRecording = async () => {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    const recorder = new MediaRecorder(stream, { mimeType: "audio/webm" });
    chunksRef.current = [];
    recorder.ondataavailable = (e) => chunksRef.current.push(e.data);
    recorder.onstop = handleRecordingStop;
    recorder.start();
    mediaRecorderRef.current = recorder;
    setState("recording");
  };

  const stopRecording = () => {
    mediaRecorderRef.current?.stop();
    setState("stopped");
  };

  const handleRecordingStop = async () => {
    setState("uploading");
    const blob = new Blob(chunksRef.current, { type: "audio/webm" });
    const form = new FormData();
    form.append("audioFile", blob, "recording.webm");
    form.append("patientId", "CURRENT_PATIENT_ID"); // replaced by context

    const uploadRes = await fetch("/api/v1/voice-scribe", { method: "POST", body: form });
    const { id } = await uploadRes.json();
    setNoteId(id);

    setState("transcribing");
    const transcribeRes = await fetch(`/api/v1/voice-scribe/${id}/transcribe`, { method: "POST" });
    const data = await transcribeRes.json();
    setStructured(data);
    setState("done");
  };

  const handleApprove = async () => {
    await fetch(`/api/v1/voice-scribe/${noteId}/approve`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(structured),
    });
    setApproved(true);
  };

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-xl bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
          <Mic className="w-5 h-5 text-purple-600" />
        </div>
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-white">Voice Scribe</h1>
          <p className="text-sm text-gray-500">Record → AI Transcribe → Doctor Review → Save</p>
        </div>
      </div>

      {/* AI Safety Banner */}
      <div className="rounded-lg border border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-900/20 p-4 flex gap-3">
        <AlertCircle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
        <p className="text-sm text-amber-800 dark:text-amber-200">
          AI-generated notes are for assistance only. <strong>Doctor review and approval required</strong> before saving to medical record.
        </p>
      </div>

      {/* Recording Controls */}
      {(state === "idle" || state === "recording") && (
        <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-8 flex flex-col items-center gap-6">
          <button
            onClick={state === "idle" ? startRecording : stopRecording}
            className={`w-20 h-20 rounded-full flex items-center justify-center transition-all shadow-lg ${
              state === "recording"
                ? "bg-red-500 hover:bg-red-600 animate-pulse"
                : "bg-purple-600 hover:bg-purple-700"
            }`}
          >
            {state === "recording" ? (
              <MicOff className="w-8 h-8 text-white" />
            ) : (
              <Mic className="w-8 h-8 text-white" />
            )}
          </button>
          <p className="text-sm text-gray-500">
            {state === "idle" ? "Click to start recording" : "Recording... Click to stop"}
          </p>
        </div>
      )}

      {/* Processing State */}
      {(state === "uploading" || state === "transcribing") && (
        <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-8 flex flex-col items-center gap-4">
          <Loader2 className="w-10 h-10 text-purple-600 animate-spin" />
          <p className="text-sm text-gray-600 dark:text-gray-400">
            {state === "uploading" ? "Uploading audio..." : "AI is transcribing and structuring note..."}
          </p>
        </div>
      )}

      {/* Structured Note Review */}
      {state === "done" && structured && !approved && (
        <div className="space-y-4">
          <div className="rounded-2xl border border-purple-200 dark:border-purple-800 bg-white dark:bg-gray-800 p-6 space-y-4">
            <div className="flex items-center gap-2 mb-2">
              <span className="text-xs font-medium bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300 px-2 py-1 rounded-full">
                AI GENERATED — Requires Doctor Review
              </span>
            </div>
            {[
              { label: "Chief Complaint", key: "chiefComplaint" as keyof StructuredNote },
              { label: "History of Present Illness", key: "historyOfPresentIllness" as keyof StructuredNote },
              { label: "Physical Examination", key: "physicalExamination" as keyof StructuredNote },
              { label: "Assessment", key: "assessment" as keyof StructuredNote },
              { label: "Plan", key: "plan" as keyof StructuredNote },
            ].map(({ label, key }) => (
              <div key={key}>
                <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">{label}</label>
                <textarea
                  className="w-full rounded-lg border border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 text-sm p-3 resize-none focus:ring-2 focus:ring-purple-500 focus:outline-none"
                  rows={3}
                  value={(structured[key] as string) ?? ""}
                  onChange={(e) => setStructured((s) => s ? { ...s, [key]: e.target.value } : s)}
                />
              </div>
            ))}
          </div>
          <PermissionGate permission="MedicalRecords.Create">
            <button
              onClick={handleApprove}
              className="w-full py-3 rounded-xl bg-green-600 hover:bg-green-700 text-white font-medium flex items-center justify-center gap-2"
            >
              <CheckCircle className="w-5 h-5" />
              Approve & Save to Medical Record
            </button>
          </PermissionGate>
        </div>
      )}

      {/* Approved */}
      {approved && (
        <div className="rounded-2xl border border-green-200 bg-green-50 dark:border-green-800 dark:bg-green-900/20 p-6 flex items-center gap-3">
          <CheckCircle className="w-6 h-6 text-green-600" />
          <div>
            <p className="font-medium text-green-800 dark:text-green-200">Clinical note approved and saved</p>
            <p className="text-sm text-green-700 dark:text-green-300">Added to patient medical record.</p>
          </div>
        </div>
      )}
    </div>
  );
}
