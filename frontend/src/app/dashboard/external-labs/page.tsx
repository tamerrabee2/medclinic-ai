"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { Send, FlaskConical, Download, Loader2 } from "lucide-react";
import { PermissionGate } from "@/components/auth/PermissionGate";

export default function ExternalLabsPage() {
  const [payloadJson, setPayloadJson] = useState('{\n  "tests": ["CBC", "HbA1c"]\n}');
  const [lastSync, setLastSync] = useState<any>(null);

  const submitMutation = useMutation({
    mutationFn: async () => {
      const res = await fetch("/api/v1/external-labs/submit", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          patientId: "CURRENT_PATIENT_ID",
          labOrderId: crypto.randomUUID(),
          providerId: crypto.randomUUID(),
          payloadJson,
        }),
      });
      return res.json();
    },
    onSuccess: setLastSync
  });

  const fetchMutation = useMutation({
    mutationFn: async () => {
      const res = await fetch(`/api/v1/external-labs/${lastSync.id}/fetch`, { method: "POST" });
      return res.json();
    },
    onSuccess: setLastSync
  });

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-gray-900 dark:text-white">External Labs Integration</h1>
        <p className="text-sm text-gray-500">Submit lab orders to external providers and fetch returned results.</p>
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-4">
        <div className="flex items-center gap-2 text-gray-900 dark:text-white font-medium">
          <FlaskConical className="h-4 w-4 text-blue-600" /> Order Payload
        </div>
        <textarea
          value={payloadJson}
          onChange={(e) => setPayloadJson(e.target.value)}
          rows={10}
          className="w-full rounded-xl border border-gray-200 px-3 py-3 text-sm dark:border-gray-700 dark:bg-gray-800"
        />
        <div className="flex gap-2">
          <PermissionGate permission="Lab.Create">
            <button onClick={() => submitMutation.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700">
              {submitMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />} Submit Order
            </button>
          </PermissionGate>
          {lastSync && (
            <PermissionGate permission="Lab.EnterResults">
              <button onClick={() => fetchMutation.mutate()} className="inline-flex items-center gap-2 rounded-xl bg-teal-600 px-4 py-2 text-white hover:bg-teal-700">
                {fetchMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />} Fetch Result
              </button>
            </PermissionGate>
          )}
        </div>
      </div>

      {lastSync && (
        <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 p-5 space-y-3">
          <p className="text-sm font-medium text-gray-900 dark:text-white">Sync Status: {lastSync.status}</p>
          <p className="text-xs text-gray-400">External Order: {lastSync.externalOrderId}</p>
          {lastSync.resultJson && <pre className="overflow-auto rounded-xl bg-gray-50 dark:bg-gray-800 p-4 text-xs">{lastSync.resultJson}</pre>}
        </div>
      )}
    </div>
  );
}
