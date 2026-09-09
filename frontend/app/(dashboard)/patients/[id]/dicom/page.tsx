"use client";

import { useRef, useState } from "react";
import { useParams } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  Upload, Eye, Sparkles, CheckCircle, AlertCircle, Loader2, FileImage, Activity,
} from "lucide-react";

const MODALITY_COLORS: Record<string, string> = {
  CT:   "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300",
  MR:   "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300",
  CR:   "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300",
  DX:   "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300",
  US:   "bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-300",
  PET:  "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300",
  NM:   "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300",
};

function ModalityBadge({ modality }: { modality: string }) {
  const cls = MODALITY_COLORS[modality.toUpperCase()] ?? "bg-gray-100 text-gray-700";
  return <span className={`rounded-full px-2.5 py-0.5 text-xs font-bold ${cls}`}>{modality}</span>;
}

export default function DicomStudiesPage() {
  const { id: patientId } = useParams<{ id: string }>();
  const qc = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);

  const [uploading, setUploading] = useState(false);
  const [selectedStudy, setSelectedStudy] = useState<any>(null);
  const [analysis, setAnalysis] = useState<Record<string, any>>({});

  const { data, isLoading } = useQuery({
    queryKey: ["dicom-studies", patientId],
    queryFn: () =>
      fetch(`/api/v1/patients/${patientId}/dicom`).then((r) => r.json()),
  });

  const studies = data?.items ?? [];

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    const form = new FormData();
    form.append("dicomFile", file);
    form.append("modality", "CT");
    form.append("studyDescription", file.name);
    await fetch(`/api/v1/patients/${patientId}/dicom/upload`, { method: "POST", body: form });
    await qc.invalidateQueries({ queryKey: ["dicom-studies", patientId] });
    setUploading(false);
  };

  const runAnalysis = async (studyId: string) => {
    const res = await fetch(`/api/v1/patients/${patientId}/dicom/${studyId}/analyze`, { method: "POST" });
    const data = await res.json();
    setAnalysis((prev) => ({ ...prev, [studyId]: data }));
  };

  const approveAI = async (studyId: string) => {
    await fetch(`/api/v1/patients/${patientId}/dicom/${studyId}/approve-ai`, { method: "POST" });
    await qc.invalidateQueries({ queryKey: ["dicom-studies", patientId] });
  };

  return (
    <div className="p-6 space-y-6 max-w-5xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-gray-900 dark:text-white">DICOM Studies / PACS</h1>
          <p className="text-sm text-gray-500">Upload DICOM files and run AI imaging analysis.</p>
        </div>
        <div>
          <input ref={fileRef} type="file" accept=".dcm,.dicom" className="hidden" onChange={handleUpload} />
          <button
            onClick={() => fileRef.current?.click()}
            disabled={uploading}
            className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2 text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {uploading ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
            Upload DICOM
          </button>
        </div>
      </div>

      {/* AI Safety */}
      <div className="flex gap-2 rounded-xl border border-amber-200 bg-amber-50 dark:border-amber-900/40 dark:bg-amber-950/30 p-4 text-sm text-amber-800 dark:text-amber-200">
        <AlertCircle className="h-4 w-4 mt-0.5 shrink-0" />
        AI imaging findings are assistive only. <strong className="ml-1">Radiologist / physician review and approval required</strong> before clinical use.
      </div>

      {/* Studies */}
      {isLoading ? (
        <div className="flex justify-center py-16"><Loader2 className="h-8 w-8 animate-spin text-blue-600" /></div>
      ) : studies.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-gray-200 dark:border-gray-700 py-20 text-center gap-3">
          <FileImage className="h-10 w-10 text-gray-300" />
          <p className="text-gray-500">No DICOM studies yet. Upload a .dcm file to get started.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {studies.map((study: any) => {
            const aiResult = analysis[study.id];
            return (
              <div key={study.id} className="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden">
                {/* Study header */}
                <div className="flex items-start gap-4 p-5">
                  <div className="w-10 h-10 rounded-xl bg-blue-50 dark:bg-blue-900/20 flex items-center justify-center shrink-0">
                    <Activity className="h-5 w-5 text-blue-600" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <p className="text-sm font-semibold text-gray-900 dark:text-white">
                        {study.studyDescription ?? "Unnamed Study"}
                      </p>
                      <ModalityBadge modality={study.modality} />
                      {study.hasAIAnalysis && (
                        <span className={`rounded-full px-2 py-0.5 text-[10px] font-medium ${
                          study.aiReviewedByDoctor
                            ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300"
                            : "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300"
                        }`}>
                          {study.aiReviewedByDoctor ? "AI Approved" : "AI Pending Review"}
                        </span>
                      )}
                    </div>
                    <p className="text-xs text-gray-400 mt-1">
                      {new Date(study.studyDate).toLocaleDateString()} &middot;
                      {study.seriesCount} series &middot;
                      {study.instanceCount} instances
                    </p>
                    {study.accessionNumber && (
                      <p className="text-xs text-gray-400">ACC: {study.accessionNumber}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    {study.viewerUrl && (
                      <a
                        href={study.viewerUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center gap-1.5 rounded-lg border border-gray-200 dark:border-gray-700 px-3 py-1.5 text-xs font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800"
                      >
                        <Eye className="h-3.5 w-3.5" /> Open Viewer
                      </a>
                    )}
                    {!aiResult && (
                      <button
                        onClick={() => runAnalysis(study.id)}
                        className="inline-flex items-center gap-1.5 rounded-lg bg-purple-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-purple-700"
                      >
                        <Sparkles className="h-3.5 w-3.5" /> AI Analysis
                      </button>
                    )}
                  </div>
                </div>

                {/* AI Analysis Result */}
                {aiResult && (
                  <div className="border-t border-gray-100 dark:border-gray-700 bg-purple-50 dark:bg-purple-950/20 p-5 space-y-4">
                    <div className="flex items-center gap-2">
                      <Sparkles className="h-4 w-4 text-purple-600" />
                      <span className="text-sm font-semibold text-purple-900 dark:text-purple-200">AI Imaging Analysis</span>
                      <span className="ml-auto text-xs text-gray-400">
                        Confidence: {Math.round((aiResult.confidenceScore ?? 0) * 100)}%
                      </span>
                    </div>

                    <p className="text-sm text-gray-700 dark:text-gray-300">{aiResult.findings}</p>

                    {aiResult.impressions?.length > 0 && (
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-wide text-purple-700 dark:text-purple-300 mb-1">Impressions</p>
                        <ul className="space-y-1">
                          {aiResult.impressions.map((imp: string, i: number) => (
                            <li key={i} className="text-sm text-gray-600 dark:text-gray-400 flex gap-2">
                              <span className="text-purple-400">•</span>{imp}
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {aiResult.recommendations?.length > 0 && (
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-wide text-teal-700 dark:text-teal-300 mb-1">Recommendations</p>
                        <ul className="space-y-1">
                          {aiResult.recommendations.map((rec: string, i: number) => (
                            <li key={i} className="text-sm text-gray-600 dark:text-gray-400 flex gap-2">
                              <span className="text-teal-400">•</span>{rec}
                            </li>
                          ))}
                        </ul>
                      </div>
                    )}

                    {!study.aiReviewedByDoctor && (
                      <button
                        onClick={() => approveAI(study.id)}
                        className="inline-flex items-center gap-2 rounded-xl bg-green-600 px-4 py-2 text-white text-sm font-medium hover:bg-green-700"
                      >
                        <CheckCircle className="h-4 w-4" /> Approve AI Findings
                      </button>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
