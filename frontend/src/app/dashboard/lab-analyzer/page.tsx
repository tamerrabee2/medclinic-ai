'use client';

import React, { useState } from 'react';
import { PermissionGate } from '@/components/auth/PermissionGate';
import {
  TestTubes,
  UploadCloud,
  FileText,
  CheckCircle2,
  AlertTriangle,
  Clock,
  Sparkles,
  ArrowRight,
  TrendingUp,
  TrendingDown,
  ShieldAlert,
  Printer,
  FileCheck,
  User,
  RefreshCw,
  Info
} from 'lucide-react';

interface LabParameter {
  id: string;
  name: string;
  category: 'Hematology' | 'Metabolic' | 'Lipid Panel' | 'Renal';
  value: number;
  unit: string;
  minRange: number;
  maxRange: number;
  status: 'normal' | 'high' | 'low' | 'critical';
  interpretation: string;
}

const INITIAL_LABS: LabParameter[] = [
  {
    id: '1',
    name: 'HbA1c (Glycated Hemoglobin)',
    category: 'Metabolic',
    value: 8.4,
    unit: '%',
    minRange: 4.0,
    maxRange: 5.6,
    status: 'high',
    interpretation: 'Poor glycemic control indicating persistent hyperglycemia over last 90 days.'
  },
  {
    id: '2',
    name: 'Fasting Blood Glucose',
    category: 'Metabolic',
    value: 168,
    unit: 'mg/dL',
    minRange: 70,
    maxRange: 99,
    status: 'high',
    interpretation: 'Elevated fasting state consistent with unmanaged type 2 diabetes.'
  },
  {
    id: '3',
    name: 'Serum Creatinine',
    category: 'Renal',
    value: 1.45,
    unit: 'mg/dL',
    minRange: 0.7,
    maxRange: 1.3,
    status: 'high',
    interpretation: 'Mild elevation, possible early diabetic nephropathy. Monitor eGFR.'
  },
  {
    id: '4',
    name: 'eGFR (Estimated Glomerular Filtration)',
    category: 'Renal',
    value: 58,
    unit: 'mL/min/1.73m²',
    minRange: 60,
    maxRange: 120,
    status: 'low',
    interpretation: 'Stage 3a mild-to-moderate reduction in kidney filtration.'
  },
  {
    id: '5',
    name: 'LDL Cholesterol',
    category: 'Lipid Panel',
    value: 162,
    unit: 'mg/dL',
    minRange: 0,
    maxRange: 100,
    status: 'critical',
    interpretation: 'Atherogenic risk high; statin therapy dose titration indicated.'
  },
  {
    id: '6',
    name: 'HDL Cholesterol',
    category: 'Lipid Panel',
    value: 38,
    unit: 'mg/dL',
    minRange: 40,
    maxRange: 60,
    status: 'low',
    interpretation: 'Sub-therapeutic cardio-protective lipid levels.'
  },
  {
    id: '7',
    name: 'Hemoglobin (Hb)',
    category: 'Hematology',
    value: 14.2,
    unit: 'g/dL',
    minRange: 13.5,
    maxRange: 17.5,
    status: 'normal',
    interpretation: 'Within normal male adult reference physiological parameters.'
  },
  {
    id: '8',
    name: 'Platelet Count',
    category: 'Hematology',
    value: 245,
    unit: '10³/µL',
    minRange: 150,
    maxRange: 450,
    status: 'normal',
    interpretation: 'Normal clotting baseline; no thrombocytopenia.'
  }
];

const PIPELINE_STEPS = [
  { id: 1, name: 'File Upload', desc: 'PDF / Image ingested' },
  { id: 2, name: 'Validation', desc: 'Format & Hash verified' },
  { id: 3, name: 'OCR / Vision', desc: 'Values digitized' },
  { id: 4, name: 'Reference Mapping', desc: 'Ranges normalized' },
  { id: 5, name: 'AI Correlation', desc: 'Clinical insights generated' },
  { id: 6, name: 'Doctor Sign-off', desc: 'Review & validation' }
];

export default function LabAnalyzerPage() {
  const [pipelineStep, setPipelineStep] = useState<number>(5);
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [isSignedOff, setIsSignedOff] = useState<boolean>(false);
  const [activeCategory, setActiveCategory] = useState<string>('all');
  const [labs, setLabs] = useState<LabParameter[]>(INITIAL_LABS);

  const filteredLabs = activeCategory === 'all'
    ? labs
    : labs.filter(l => l.category === activeCategory);

  const simulateReprocess = () => {
    setIsProcessing(true);
    setPipelineStep(1);
    setIsSignedOff(false);

    let step = 1;
    const interval = setInterval(() => {
      step++;
      setPipelineStep(step);
      if (step >= 5) {
        clearInterval(interval);
        setIsProcessing(false);
      }
    }, 600);
  };

  const handleSignOff = () => {
    setIsSignedOff(true);
    setPipelineStep(6);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-500/20 text-amber-300 border border-amber-500/30">
              Module 21 & 46
            </span>
            <span className="text-xs text-slate-400">Clinical Intelligence</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <TestTubes className="w-8 h-8 text-sky-400" />
            AI Lab Analyzer & Diagnostic Pipeline
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Automated laboratory report OCR, multi-variable extraction, and longitudinal medical correlation.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={simulateReprocess}
            disabled={isProcessing}
            className="px-4 py-2.5 rounded-xl border border-slate-700 bg-slate-900/60 hover:bg-slate-800 text-sm font-medium text-slate-300 flex items-center gap-2 transition"
          >
            <RefreshCw className={`w-4 h-4 ${isProcessing ? 'animate-spin text-sky-400' : ''}`} />
            <span>Reprocess Document</span>
          </button>

          <button
            onClick={() => window.print()}
            className="px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-sm font-semibold text-white shadow-lg shadow-sky-500/20 flex items-center gap-2 transition"
          >
            <Printer className="w-4 h-4" />
            <span>Print Clinical Summary</span>
          </button>
        </div>
      </div>

      {/* Patient Context & Processing Timeline Banner */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-5">
        {/* Patient Summary Bar */}
        <div className="flex flex-wrap items-center justify-between gap-4 pb-4 border-b border-slate-800/80">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-sky-500/20 border border-sky-500/40 flex items-center justify-center text-sky-300 font-bold">
              <User className="w-5 h-5" />
            </div>
            <div>
              <div className="text-sm font-bold text-white">Tariq Al-Mansoor</div>
              <div className="text-xs text-slate-400 flex items-center gap-2">
                <span>MRN: <b className="text-slate-300">#P-10024</b></span>
                <span>•</span>
                <span>Age: <b className="text-slate-300">46 Y / Male</b></span>
                <span>•</span>
                <span>Known History: <b className="text-amber-400">T2DM, Essential Hypertension</b></span>
              </div>
            </div>
          </div>

          <div className="flex items-center gap-2 text-xs">
            <span className="text-slate-400">Specimen Collected:</span>
            <span className="font-mono text-slate-200">Today, 08:30 AM</span>
            <span className="text-slate-400 ml-2">Lab Ref:</span>
            <span className="font-mono text-sky-400 font-semibold">LAB-2026-0904</span>
          </div>
        </div>

        {/* 6-Step Visual Pipeline */}
        <div>
          <div className="flex items-center justify-between text-xs text-slate-400 mb-2 font-medium">
            <span>Automated Analysis Pipeline (Section 46 Architecture)</span>
            <span className="text-sky-400 font-mono">
              Step {pipelineStep} of 6 {isSignedOff ? '(Fully Approved)' : isProcessing ? '(Processing...)' : ''}
            </span>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-6 gap-2">
            {PIPELINE_STEPS.map((step) => {
              const isDone = pipelineStep > step.id || (step.id === 6 && isSignedOff);
              const isCurrent = pipelineStep === step.id && !isSignedOff;

              return (
                <div
                  key={step.id}
                  className={`p-3 rounded-xl border text-left transition-all ${
                    isDone
                      ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-300'
                      : isCurrent
                      ? 'bg-sky-500/15 border-sky-500/50 text-sky-300 shadow-md shadow-sky-500/10'
                      : 'bg-slate-900/40 border-slate-800/80 text-slate-500'
                  }`}
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-[10px] font-bold uppercase tracking-wider font-mono">
                      Phase {step.id}
                    </span>
                    {isDone ? (
                      <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400" />
                    ) : isCurrent ? (
                      <span className="w-2 h-2 rounded-full bg-sky-400 animate-ping" />
                    ) : (
                      <Clock className="w-3.5 h-3.5" />
                    )}
                  </div>
                  <div className="text-xs font-semibold text-slate-200 truncate">{step.name}</div>
                  <div className="text-[10px] text-slate-400 truncate mt-0.5">{step.desc}</div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Main Grid: Left Table & Right AI Correlation Panel */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Lab Results Table (2 Cols) */}
        <div className="lg:col-span-2 space-y-4">
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            {/* Table Header & Category Tabs */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
              <h2 className="text-base font-bold text-white flex items-center gap-2">
                <FileText className="w-4 h-4 text-sky-400" />
                Extracted Biomarkers & Quantitative Values
              </h2>

              <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
                {['all', 'Metabolic', 'Lipid Panel', 'Renal', 'Hematology'].map((cat) => (
                  <button
                    key={cat}
                    onClick={() => setActiveCategory(cat)}
                    className={`px-3 py-1 rounded-lg font-medium transition ${
                      activeCategory === cat
                        ? 'bg-sky-500 text-white shadow-sm'
                        : 'text-slate-400 hover:text-slate-200'
                    }`}
                  >
                    {cat === 'all' ? 'All Values' : cat}
                  </button>
                ))}
              </div>
            </div>

            {/* Results Table */}
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-slate-800 text-xs font-semibold text-slate-400 uppercase tracking-wider">
                    <th className="py-3 px-3">Test Parameter</th>
                    <th className="py-3 px-3">Result</th>
                    <th className="py-3 px-3">Reference Range</th>
                    <th className="py-3 px-3">Status</th>
                    <th className="py-3 px-3">Clinical Interpretation</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/60">
                  {filteredLabs.map((lab) => {
                    const isNormal = lab.status === 'normal';
                    const isHigh = lab.status === 'high';
                    const isLow = lab.status === 'low';
                    const isCritical = lab.status === 'critical';

                    return (
                      <tr key={lab.id} className="hover:bg-slate-900/40 transition">
                        <td className="py-3 px-3">
                          <div className="font-semibold text-slate-200">{lab.name}</div>
                          <div className="text-[11px] text-slate-500">{lab.category}</div>
                        </td>

                        <td className="py-3 px-3 font-mono font-bold text-slate-100 whitespace-nowrap">
                          {lab.value} <span className="text-xs font-normal text-slate-400">{lab.unit}</span>
                        </td>

                        <td className="py-3 px-3 text-xs text-slate-400 font-mono">
                          {lab.minRange} - {lab.maxRange} {lab.unit}
                        </td>

                        <td className="py-3 px-3 whitespace-nowrap">
                          {isNormal && (
                            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                              <CheckCircle2 className="w-3 h-3" /> Normal
                            </span>
                          )}
                          {isHigh && (
                            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-500/10 text-amber-400 border border-amber-500/20">
                              <TrendingUp className="w-3 h-3" /> High
                            </span>
                          )}
                          {isLow && (
                            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-cyan-500/10 text-cyan-400 border border-cyan-500/20">
                              <TrendingDown className="w-3 h-3" /> Low
                            </span>
                          )}
                          {isCritical && (
                            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-rose-500/20 text-rose-400 border border-rose-500/40 animate-pulse">
                              <ShieldAlert className="w-3 h-3" /> Critical
                            </span>
                          )}
                        </td>

                        <td className="py-3 px-3 text-xs text-slate-300 max-w-xs">
                          {lab.interpretation}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        {/* Right AI Clinical Summary & Sign-off Panel */}
        <div className="space-y-4">
          {/* AI Clinical Correlation Card */}
          <div className="glass-panel p-5 rounded-2xl border-sky-500/30 bg-gradient-to-b from-sky-950/30 to-indigo-950/20 space-y-4 shadow-xl shadow-sky-500/5">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-sky-400 font-bold text-sm">
                <Sparkles className="w-4 h-4" />
                <span>AI Clinical Differential & Insights</span>
              </div>
              <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-sky-500/20 text-sky-300">
                Confidence 96.4%
              </span>
            </div>

            <div className="text-xs text-slate-300 leading-relaxed space-y-2.5">
              <p>
                <b>Primary Pattern:</b> Triad of high HbA1c (8.4%), elevated fasting glucose (168 mg/dL), and critical LDL (162 mg/dL) points to <b>uncontrolled Type 2 Diabetic Dyslipidemia</b> with microvascular risk.
              </p>
              <div className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/20 text-amber-200">
                <div className="font-semibold flex items-center gap-1.5 mb-1">
                  <AlertTriangle className="w-3.5 h-3.5 text-amber-400" />
                  Renal Watch Alert:
                </div>
                <span>eGFR at 58 mL/min with Creatinine 1.45 indicates Stage 3a Chronic Kidney Disease onset. Recommend spot urine albumin-to-creatinine ratio (ACR).</span>
              </div>
            </div>

            <div className="space-y-2 pt-2 border-t border-slate-800">
              <div className="text-xs font-semibold text-slate-200">Suggested Clinical Actions:</div>
              <ul className="text-xs text-slate-400 space-y-1.5 list-disc list-inside">
                <li>Titrate Metformin or introduce SGLT2 inhibitor (Empagliflozin).</li>
                <li>Initiate Atorvastatin 40mg daily for lipid target LDL &lt; 70 mg/dL.</li>
                <li>Follow up renal profile & HbA1c in 12 weeks.</li>
              </ul>
            </div>
          </div>

          {/* Doctor Approval & Sign-Off Card */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <div className="flex items-center gap-2 text-slate-200 font-bold text-sm">
              <FileCheck className="w-4 h-4 text-emerald-400" />
              <span>Attending Physician Sign-Off</span>
            </div>

            <p className="text-xs text-slate-400 leading-relaxed">
              Medical safety regulations require attending physician confirmation before committing values into permanent patient EMR.
            </p>

            {isSignedOff ? (
              <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-300 text-xs space-y-1.5">
                <div className="font-bold flex items-center gap-1.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                  <span>Report Approved & Validated</span>
                </div>
                <div className="text-[11px] text-emerald-400/80 font-mono">
                  Dr. Sarah Al-Mansoor • License #MED-88421 • Timestamp: {new Date().toLocaleTimeString()}
                </div>
              </div>
            ) : (
              <PermissionGate permission="Lab.Update">
                <button
                  onClick={handleSignOff}
                  className="w-full py-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-semibold text-sm shadow-lg shadow-emerald-600/20 transition flex items-center justify-center gap-2"
                >
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Verify & Sign-Off Report</span>
                </button>
              </PermissionGate>
            )}

            <div className="flex items-center gap-1.5 text-[10px] text-slate-500">
              <Info className="w-3.5 h-3.5 shrink-0" />
              <span>Audited under HIPAA & Saudi MoH digital health privacy standard.</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
