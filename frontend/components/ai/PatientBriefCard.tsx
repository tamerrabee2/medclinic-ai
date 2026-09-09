"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { Brain, Sparkles, AlertCircle, ChevronDown, ChevronUp, Loader2 } from "lucide-react";

interface PatientBriefCardProps {
  patientId: string;
  visitId?: string;
}

export function PatientBriefCard({ patientId, visitId }: PatientBriefCardProps) {
  const [expanded, setExpanded] = useState(false);

  const generate = useMutation({
    mutationFn: () =>
      fetch(`/api/v1/patients/${patientId}/brief${visitId ? `?visitId=${visitId}` : ""}`, {
        method: "POST",
      }).then((r) => r.json()),
  });

  const brief = generate.data;

  return (
    <div className="rounded-2xl border border-indigo-200 dark:border-indigo-800 bg-white dark:bg-gray-800 overflow-hidden">
      {/* Header */}
      <div className="flex items-center gap-3 p-4 bg-gradient-to-r from-indigo-50 to-purple-50 dark:from-indigo-900/20 dark:to-purple-900/20">
        <div className="w-8 h-8 rounded-lg bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center">
          <Brain className="w-4 h-4 text-indigo-600" />
        </div>
        <div className="flex-1">
          <p className="text-sm font-semibold text-gray-900 dark:text-white">AI Patient Brief</p>
          <p className="text-xs text-gray-500">Requires doctor review before use</p>
        </div>
        {!brief && (
          <button
            onClick={() => generate.mutate()}
            disabled={generate.isPending}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-medium"
          >
            {generate.isPending ? <Loader2 className="w-3 h-3 animate-spin" /> : <Sparkles className="w-3 h-3" />}
            Generate
          </button>
        )}
      </div>

      {/* Content */}
      {brief && (
        <div className="p-4 space-y-3">
          <div className="flex items-start gap-2 p-3 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800">
            <AlertCircle className="w-4 h-4 text-amber-600 shrink-0 mt-0.5" />
            <p className="text-xs text-amber-700 dark:text-amber-300">AI-generated. Doctor approval required before clinical use.</p>
          </div>

          <p className="text-sm text-gray-700 dark:text-gray-300">{brief.summary}</p>

          {brief.alerts?.length > 0 && (
            <div className="space-y-1">
              {brief.alerts.map((a: string, i: number) => (
                <div key={i} className="flex items-center gap-2 text-xs text-red-600 dark:text-red-400">
                  <span className="w-1.5 h-1.5 rounded-full bg-red-500" />{a}
                </div>
              ))}
            </div>
          )}

          <button
            onClick={() => setExpanded(!expanded)}
            className="flex items-center gap-1 text-xs text-indigo-600 hover:text-indigo-700"
          >
            {expanded ? <ChevronUp className="w-3 h-3" /> : <ChevronDown className="w-3 h-3" />}
            {expanded ? "Show less" : "Show more"}
          </button>

          {expanded && (
            <div className="space-y-3 pt-2 border-t border-gray-100 dark:border-gray-700">
              {brief.recentChanges?.length > 0 && (
                <div>
                  <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Recent Changes</p>
                  <ul className="space-y-0.5">
                    {brief.recentChanges.map((c: string, i: number) => (
                      <li key={i} className="text-sm text-gray-600 dark:text-gray-400 flex gap-2">
                        <span className="text-green-500">↓</span>{c}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
              {brief.pendingItems?.length > 0 && (
                <div>
                  <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-1">Pending</p>
                  <ul className="space-y-0.5">
                    {brief.pendingItems.map((p: string, i: number) => (
                      <li key={i} className="text-sm text-gray-600 dark:text-gray-400 flex gap-2">
                        <span className="text-amber-500">•</span>{p}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
