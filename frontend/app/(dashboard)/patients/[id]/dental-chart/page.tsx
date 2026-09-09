"use client";

import { useEffect, useMemo, useState } from "react";
import { useParams, useSearchParams } from "next/navigation";
import { Sparkles, Save, Loader2, AlertCircle } from "lucide-react";

const TEETH = [
  18,17,16,15,14,13,12,11,
  21,22,23,24,25,26,27,28,
  48,47,46,45,44,43,42,41,
  31,32,33,34,35,36,37,38,
];

const STATUS_OPTIONS = [
  { value: 0, label: "Normal", color: "bg-emerald-100 text-emerald-700" },
  { value: 1, label: "Caries", color: "bg-red-100 text-red-700" },
  { value: 2, label: "Filling", color: "bg-blue-100 text-blue-700" },
  { value: 3, label: "Crown", color: "bg-amber-100 text-amber-700" },
  { value: 4, label: "Missing", color: "bg-gray-100 text-gray-700" },
  { value: 5, label: "Implant", color: "bg-purple-100 text-purple-700" },
  { value: 6, label: "Root Canal", color: "bg-indigo-100 text-indigo-700" },
  { value: 7, label: "Extraction", color: "bg-rose-100 text-rose-700" },
];

export default function DentalChartPage() {
  const { id: patientId } = useParams<{ id: string }>();
  const searchParams = useSearchParams();
  const visitId = searchParams.get("visitId");

  const [notes, setNotes] = useState("");
  const [teeth, setTeeth] = useState<Record<number, { status: number; notes: string }>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [analysis, setAnalysis] = useState<any>(null);
  const [analyzing, setAnalyzing] = useState(false);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      const res = await fetch(`/api/v1/patients/${patientId}/dental-chart${visitId ? `?visitId=${visitId}` : ""}`);
      const data = await res.json();
      setNotes(data.notes ?? "");

      const map: Record<number, { status: number; notes: string }> = {};
      (data.teeth ?? []).forEach((t: any) => {
        map[t.toothNumber] = { status: t.status, notes: t.notes ?? "" };
      });
      setTeeth(map);
      setLoading(false);
    };
    load();
  }, [patientId, visitId]);

  const summary = useMemo(() => {
    const counts = new Map<number, number>();
    Object.values(teeth).forEach((t) => counts.set(t.status, (counts.get(t.status) ?? 0) + 1));
    return counts;
  }, [teeth]);

  const updateTooth = (toothNumber: number, field: "status" | "notes", value: string | number) => {
    setTeeth((prev) => ({
      ...prev,
      [toothNumber]: {
        status: prev[toothNumber]?.status ?? 0,
        notes: prev[toothNumber]?.notes ?? "",
        [field]: value,
      },
    }));
  };

  const save = async () => {
    setSaving(true);
    const payload = {
      visitId,
      notes,
      teeth: TEETH.map((toothNumber) => ({
        toothNumber,
        status: teeth[toothNumber]?.status ?? 0,
        notes: teeth[toothNumber]?.notes ?? "",
        surfaceDataJson: "{}",
      })),
    };

    await fetch(`/api/v1/patients/${patientId}/dental-chart`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    setSaving(false);
  };

  const runAnalysis = async () => {
    setAnalyzing(true);
    const res = await fetch(`/api/v1/patients/${patientId}/dental-chart/analysis${visitId ? `?visitId=${visitId}` : ""}`, {
      method: "POST",
    });
    setAnalysis(await res.json());
    setAnalyzing(false);
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Dental Chart</h1>
          <p className="text-sm text-gray-500">Interactive odontogram for charting and AI-assisted review.</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={runAnalysis}
            disabled={analyzing || loading}
            className="inline-flex items-center gap-2 rounded-xl bg-purple-600 px-4 py-2 text-white hover:bg-purple-700 disabled:opacity-60"
          >
            {analyzing ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
            AI Review
          </button>
          <button
            onClick={save}
            disabled={saving || loading}
            className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
            Save Chart
          </button>
        </div>
      </div>

      <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800 dark:border-amber-900/40 dark:bg-amber-950/30 dark:text-amber-200 flex gap-2">
        <AlertCircle className="h-4 w-4 mt-0.5" />
        AI dental findings are advisory only and require dentist review before treatment decisions.
      </div>

      {loading ? (
        <div className="flex justify-center py-16"><Loader2 className="h-8 w-8 animate-spin text-blue-600" /></div>
      ) : (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 xl:grid-cols-8 gap-3">
            {TEETH.map((toothNumber) => {
              const current = teeth[toothNumber] ?? { status: 0, notes: "" };
              const option = STATUS_OPTIONS.find((s) => s.value === current.status) ?? STATUS_OPTIONS[0];
              return (
                <div key={toothNumber} className="rounded-2xl border border-gray-200 bg-white p-3 shadow-sm dark:border-gray-700 dark:bg-gray-900">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-semibold text-gray-900 dark:text-white">{toothNumber}</span>
                    <span className={`rounded-full px-2 py-0.5 text-[10px] font-medium ${option.color}`}>{option.label}</span>
                  </div>
                  <select
                    value={current.status}
                    onChange={(e) => updateTooth(toothNumber, "status", Number(e.target.value))}
                    className="w-full rounded-lg border border-gray-200 px-2 py-2 text-sm dark:border-gray-700 dark:bg-gray-800"
                  >
                    {STATUS_OPTIONS.map((status) => (
                      <option key={status.value} value={status.value}>{status.label}</option>
                    ))}
                  </select>
                  <textarea
                    value={current.notes}
                    onChange={(e) => updateTooth(toothNumber, "notes", e.target.value)}
                    rows={2}
                    placeholder="Notes"
                    className="mt-2 w-full rounded-lg border border-gray-200 px-2 py-2 text-xs dark:border-gray-700 dark:bg-gray-800"
                  />
                </div>
              );
            })}
          </div>

          <div className="grid gap-6 lg:grid-cols-3">
            <div className="lg:col-span-2 rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-700 dark:bg-gray-900">
              <h2 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Clinical Notes</h2>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={6}
                className="w-full rounded-xl border border-gray-200 px-3 py-3 text-sm dark:border-gray-700 dark:bg-gray-800"
                placeholder="Overall dental chart notes"
              />
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-700 dark:bg-gray-900">
              <h2 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Status Summary</h2>
              <div className="space-y-2">
                {STATUS_OPTIONS.map((status) => (
                  <div key={status.value} className="flex items-center justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-300">{status.label}</span>
                    <span className="font-medium text-gray-900 dark:text-white">{summary.get(status.value) ?? 0}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {analysis && (
            <div className="rounded-2xl border border-purple-200 bg-purple-50 p-5 dark:border-purple-900/40 dark:bg-purple-950/20">
              <div className="flex items-center gap-2 mb-3">
                <Sparkles className="h-4 w-4 text-purple-600" />
                <h2 className="text-sm font-semibold text-purple-900 dark:text-purple-200">AI Dental Review</h2>
                <span className="ml-auto rounded-full bg-amber-100 px-2 py-1 text-[10px] font-medium text-amber-700 dark:bg-amber-900/30 dark:text-amber-300">
                  Requires Dentist Review
                </span>
              </div>
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-purple-700 dark:text-purple-300 mb-2">Findings</h3>
                  <ul className="space-y-2 text-sm text-gray-700 dark:text-gray-300">
                    {(analysis.findings ?? []).map((item: string, idx: number) => <li key={idx}>• {item}</li>)}
                  </ul>
                </div>
                <div>
                  <h3 className="text-xs font-semibold uppercase tracking-wide text-purple-700 dark:text-purple-300 mb-2">Recommended Actions</h3>
                  <ul className="space-y-2 text-sm text-gray-700 dark:text-gray-300">
                    {(analysis.recommendedActions ?? []).map((item: string, idx: number) => <li key={idx}>• {item}</li>)}
                  </ul>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
