"use client";

import { useMutation } from "@tanstack/react-query";
import { Calendar, Sparkles, Loader2, CheckCircle, XCircle } from "lucide-react";

type Priority = 0 | 1 | 2 | 3;

const PRIORITY_LABELS: Record<Priority, { label: string; color: string }> = {
  0: { label: "Urgent", color: "text-red-600 bg-red-50 dark:bg-red-900/20 border-red-200" },
  1: { label: "High", color: "text-orange-600 bg-orange-50 dark:bg-orange-900/20 border-orange-200" },
  2: { label: "Routine", color: "text-blue-600 bg-blue-50 dark:bg-blue-900/20 border-blue-200" },
  3: { label: "Low", color: "text-gray-600 bg-gray-50 dark:bg-gray-800 border-gray-200" },
};

interface FollowUpSuggestionsCardProps {
  patientId: string;
  visitId?: string;
}

export function FollowUpSuggestionsCard({ patientId, visitId }: FollowUpSuggestionsCardProps) {
  const generate = useMutation({
    mutationFn: () =>
      fetch(`/api/v1/patients/${patientId}/follow-up/suggestions${visitId ? `?visitId=${visitId}` : ""}`, {
        method: "POST",
      }).then((r) => r.json()),
  });

  const suggestions = generate.data ?? [];

  return (
    <div className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
      <div className="flex items-center justify-between p-4 border-b border-gray-100 dark:border-gray-700">
        <div className="flex items-center gap-2">
          <Calendar className="w-4 h-4 text-teal-600" />
          <span className="text-sm font-semibold text-gray-900 dark:text-white">AI Follow-up Suggestions</span>
        </div>
        <button
          onClick={() => generate.mutate()}
          disabled={generate.isPending}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-teal-600 hover:bg-teal-700 text-white text-xs font-medium"
        >
          {generate.isPending ? <Loader2 className="w-3 h-3 animate-spin" /> : <Sparkles className="w-3 h-3" />}
          Generate
        </button>
      </div>

      {suggestions.length > 0 && (
        <div className="p-4 space-y-3">
          {suggestions.map((s: any) => {
            const p = PRIORITY_LABELS[s.priority as Priority] ?? PRIORITY_LABELS[2];
            return (
              <div key={s.id} className={`rounded-xl border p-4 space-y-2 ${p.color}`}>
                <div className="flex items-start justify-between">
                  <p className="text-sm font-medium">{s.reason}</p>
                  <span className={`text-xs font-semibold px-2 py-0.5 rounded-full border ${p.color}`}>
                    {p.label}
                  </span>
                </div>
                <p className="text-xs opacity-80">{s.recommendedAction}</p>
                {s.suggestedDate && (
                  <p className="text-xs opacity-60">
                    Suggested: {new Date(s.suggestedDate).toLocaleDateString()}
                  </p>
                )}
                <div className="flex gap-2 pt-1">
                  <button className="flex items-center gap-1 text-xs px-3 py-1.5 rounded-lg bg-green-600 hover:bg-green-700 text-white">
                    <CheckCircle className="w-3 h-3" /> Approve
                  </button>
                  <button className="flex items-center gap-1 text-xs px-3 py-1.5 rounded-lg bg-white dark:bg-gray-700 border border-gray-200 dark:border-gray-600 text-gray-600 dark:text-gray-300">
                    <XCircle className="w-3 h-3" /> Dismiss
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
