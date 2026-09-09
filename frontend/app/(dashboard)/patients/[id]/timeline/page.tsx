"use client";

import { useParams } from "next/navigation";
import { useQuery, useMutation } from "@tanstack/react-query";
import { Stethoscope, FlaskConical, Image, Pill, Calendar, Mic, Brain, Loader2, Sparkles } from "lucide-react";

const EVENT_ICONS: Record<number, React.ReactNode> = {
  0: <Stethoscope className="w-4 h-4" />,  // Visit
  1: <Brain className="w-4 h-4" />,          // Diagnosis
  2: <Pill className="w-4 h-4" />,           // Prescription
  3: <FlaskConical className="w-4 h-4" />,   // LabOrder
  4: <FlaskConical className="w-4 h-4" />,   // LabResult
  5: <Image className="w-4 h-4" />,          // RadiologyStudy
  6: <Image className="w-4 h-4" />,          // ImagingAnalysis
  7: <Mic className="w-4 h-4" />,            // VoiceNote
  8: <Brain className="w-4 h-4" />,          // PatientBrief
  9: <Calendar className="w-4 h-4" />,       // Appointment
  12: <Calendar className="w-4 h-4" />,      // FollowUp
};

const EVENT_COLORS: Record<number, string> = {
  0: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300",
  1: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300",
  2: "bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-300",
  3: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300",
  4: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300",
  5: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300",
  6: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300",
  7: "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-300",
  8: "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-300",
  9: "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300",
  12: "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300",
};

export default function PatientTimelinePage() {
  const { id: patientId } = useParams<{ id: string }>();

  const { data, isLoading } = useQuery({
    queryKey: ["timeline", patientId],
    queryFn: () =>
      fetch(`/api/v1/patients/${patientId}/timeline?pageSize=100`).then((r) => r.json()),
  });

  const summarize = useMutation({
    mutationFn: () =>
      fetch(`/api/v1/patients/${patientId}/timeline/summarize`, { method: "POST" }).then((r) => r.json()),
  });

  const events = data?.items ?? [];

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white">Clinical Timeline</h1>
        <button
          onClick={() => summarize.mutate()}
          disabled={summarize.isPending}
          className="flex items-center gap-2 px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-700 text-white text-sm font-medium"
        >
          {summarize.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Sparkles className="w-4 h-4" />}
          AI Summary
        </button>
      </div>

      {/* AI Summary Result */}
      {summarize.data && (
        <div className="rounded-2xl border border-purple-200 dark:border-purple-800 bg-purple-50 dark:bg-purple-900/20 p-5 space-y-3">
          <div className="flex items-center gap-2">
            <Sparkles className="w-4 h-4 text-purple-600" />
            <span className="text-sm font-semibold text-purple-800 dark:text-purple-200">AI Timeline Summary</span>
            <span className="text-xs bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300 px-2 py-0.5 rounded-full ml-auto">
              Requires Doctor Review
            </span>
          </div>
          <p className="text-sm text-gray-700 dark:text-gray-300">{summarize.data.narrativeSummary}</p>
          {summarize.data.keyChanges?.length > 0 && (
            <ul className="space-y-1">
              {summarize.data.keyChanges.map((c: string, i: number) => (
                <li key={i} className="text-sm text-gray-600 dark:text-gray-400 flex gap-2">
                  <span className="text-purple-500">•</span>{c}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {/* Timeline */}
      {isLoading ? (
        <div className="flex justify-center py-12">
          <Loader2 className="w-8 h-8 text-purple-600 animate-spin" />
        </div>
      ) : (
        <div className="relative">
          <div className="absolute left-6 top-0 bottom-0 w-px bg-gray-200 dark:bg-gray-700" />
          <div className="space-y-4">
            {events.map((event: any) => (
              <div key={event.id} className={`relative pl-14 ${ event.isAIHighlighted ? "" : "" }`}>
                <div className={`absolute left-3 w-6 h-6 rounded-full flex items-center justify-center z-10 ${
                  EVENT_COLORS[event.eventType] ?? "bg-gray-100 text-gray-500"
                }`}>
                  {EVENT_ICONS[event.eventType] ?? <span className="w-2 h-2 rounded-full bg-current" />}
                </div>
                <div className={`rounded-xl border p-4 ${
                  event.isAIHighlighted
                    ? "border-purple-300 bg-purple-50 dark:border-purple-700 dark:bg-purple-900/10"
                    : "border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800"
                }`}>
                  <div className="flex items-start justify-between gap-2">
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{event.title}</p>
                    <time className="text-xs text-gray-400 whitespace-nowrap">
                      {new Date(event.eventDate).toLocaleDateString()}
                    </time>
                  </div>
                  {event.description && (
                    <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{event.description}</p>
                  )}
                  {event.aiSignificance && (
                    <p className="text-xs text-purple-600 dark:text-purple-400 mt-2 flex gap-1 items-center">
                      <Sparkles className="w-3 h-3" />{event.aiSignificance}
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
