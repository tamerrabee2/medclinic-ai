"use client";

import { useParams } from "next/navigation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { RefreshCw, Database, CheckCircle2, Clock3, AlertCircle } from "lucide-react";

export default function PatientFhirPage() {
  const { id: patientId } = useParams<{ id: string }>();
  const qc = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["patient-fhir-history", patientId],
    queryFn: () => fetch(`/api/v1/patients/${patientId}/fhir/history`).then(r => r.json())
  });

  const exportMutation = useMutation({
    mutationFn: () => fetch(`/api/v1/patients/${patientId}/fhir/export`, { method: "POST" }).then(r => r.json()),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["patient-fhir-history", patientId] })
  });

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-white">FHIR Integration</h1>
          <p className="text-sm text-gray-500">Export patient records to FHIR resources and review sync history.</p>
        </div>
        <button
          onClick={() => exportMutation.mutate()}
          className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
        >
          <RefreshCw className={`h-4 w-4 ${exportMutation.isPending ? "animate-spin" : ""}`} />
          Export Patient
        </button>
      </div>

      <div className="rounded-xl border border-blue-200 bg-blue-50 p-4 text-sm text-blue-800 dark:border-blue-900/40 dark:bg-blue-950/30 dark:text-blue-200 flex gap-2">
        <Database className="h-4 w-4 mt-0.5" />
        FHIR export keeps interoperability boundaries clean and auditable per patient.
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white dark:bg-gray-900 dark:border-gray-700 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 dark:border-gray-800 font-medium text-gray-900 dark:text-white">Sync History</div>
        {isLoading ? (
          <div className="p-6 text-sm text-gray-500">Loading...</div>
        ) : (data?.length ?? 0) === 0 ? (
          <div className="p-6 text-sm text-gray-500">No FHIR sync records yet.</div>
        ) : (
          <div className="divide-y divide-gray-100 dark:divide-gray-800">
            {data.map((item: any) => (
              <div key={item.id} className="p-5 flex items-center gap-4">
                <div>
                  {item.status === "Synced" ? <CheckCircle2 className="h-5 w-5 text-green-600" /> : item.status === "Pending" ? <Clock3 className="h-5 w-5 text-amber-600" /> : <AlertCircle className="h-5 w-5 text-red-600" />}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 dark:text-white">{item.resourceType} / {item.resourceId}</p>
                  <p className="text-xs text-gray-400">{item.direction} • {item.status}</p>
                </div>
                <div className="text-xs text-gray-400">{item.syncedAt ? new Date(item.syncedAt).toLocaleString() : "—"}</div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
