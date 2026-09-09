'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Stethoscope,
  User,
  Activity,
  Heart,
  FileText,
  Pill,
  TestTubes,
  ScanLine,
  CheckCircle2,
  Clock,
  AlertTriangle,
  Plus,
  Save,
  Search,
  Sparkles,
  Calendar,
  ChevronRight,
  ShieldCheck
} from 'lucide-react';

interface ClinicalVisit {
  id: string;
  patientName: string;
  mrn: string;
  age: number;
  gender: string;
  date: string;
  time: string;
  chiefComplaint: string;
  status: 'in_progress' | 'completed' | 'scheduled';
  vitals: { bp: string; hr: number; temp: number; spo2: number };
  primaryDiagnosis: string;
  icdCode: string;
  prescriptionsCount: number;
  labsCount: number;
}

const RECENT_VISITS: ClinicalVisit[] = [
  {
    id: 'VIS-2026-0901',
    patientName: 'Tariq Al-Mansoor',
    mrn: 'P-10024',
    age: 46,
    gender: 'Male',
    date: 'Today',
    time: '10:00 AM',
    chiefComplaint: 'Productive cough for 3 days, pleuritic chest pain and mild fever.',
    status: 'in_progress',
    vitals: { bp: '138/88', hr: 74, temp: 37.4, spo2: 97 },
    primaryDiagnosis: 'Community-Acquired Pneumonia (Right Lower Lobe)',
    icdCode: 'J18.9',
    prescriptionsCount: 2,
    labsCount: 2
  },
  {
    id: 'VIS-2026-0902',
    patientName: 'Nour Mostafa',
    mrn: 'P-10022',
    age: 32,
    gender: 'Female',
    date: 'Today',
    time: '09:00 AM',
    chiefComplaint: 'Routine prenatal follow-up, 24 weeks gestation, mild lumbar ache.',
    status: 'completed',
    vitals: { bp: '118/74', hr: 78, temp: 36.6, spo2: 99 },
    primaryDiagnosis: 'Normal Supervision of Pregnancy (2nd Trimester)',
    icdCode: 'Z34.8',
    prescriptionsCount: 1,
    labsCount: 1
  },
  {
    id: 'VIS-2026-0903',
    patientName: 'Omar Al-Husseini',
    mrn: 'P-10021',
    age: 58,
    gender: 'Male',
    date: 'Yesterday',
    time: '03:30 PM',
    chiefComplaint: 'Hypertension quarterly assessment, medication tolerability check.',
    status: 'completed',
    vitals: { bp: '142/90', hr: 68, temp: 36.7, spo2: 98 },
    primaryDiagnosis: 'Essential Primary Hypertension',
    icdCode: 'I10',
    prescriptionsCount: 2,
    labsCount: 1
  }
];

export default function VisitsPage() {
  const [visits, setVisits] = useState<ClinicalVisit[]>(RECENT_VISITS);
  const [selectedVisit, setSelectedVisit] = useState<ClinicalVisit>(RECENT_VISITS[0]);
  const [isSaved, setIsSaved] = useState(false);

  // Active form editable states
  const [chiefComplaint, setChiefComplaint] = useState(selectedVisit.chiefComplaint);
  const [diagnosis, setDiagnosis] = useState(selectedVisit.primaryDiagnosis);
  const [icdCode, setIcdCode] = useState(selectedVisit.icdCode);
  const [doctorNotes, setDoctorNotes] = useState(
    'Chest auscultation reveals localized coarse crackles over right lower base. CXR ordered to confirm alveolar infiltrate. Advised hydration and complete rest.'
  );

  const handleSaveVisit = () => {
    setIsSaved(true);
    setVisits(
      visits.map((v) =>
        v.id === selectedVisit.id
          ? { ...v, chiefComplaint, primaryDiagnosis: diagnosis, icdCode, status: 'completed' }
          : v
      )
    );
    setTimeout(() => setIsSaved(false), 3000);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-sky-500/20 text-sky-300 border border-sky-500/30">
              Module 13 (EMR)
            </span>
            <span className="text-xs text-slate-400">Clinical Encounters</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <Stethoscope className="w-8 h-8 text-sky-400" />
            Patient Clinical Encounters & Visits Studio
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Structured physician workflow: Chief complaint, physical examination, ICD-10 diagnosis, and treatment orders.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={handleSaveVisit}
            className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-sm font-semibold text-white shadow-lg shadow-sky-500/20 flex items-center gap-2 transition"
          >
            <Save className="w-4 h-4" />
            <span>{isSaved ? 'Encounter Finalized ✓' : 'Finalize & Save Encounter'}</span>
          </button>
        </div>
      </div>

      {/* Main Studio Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left 4 Cols: Visits Queue & Patient List */}
        <div className="lg:col-span-4 space-y-4">
          <div className="glass-panel p-4 rounded-2xl border-slate-800 space-y-3">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-bold text-white flex items-center gap-2">
                <Clock className="w-4 h-4 text-sky-400" />
                Today's Encounter Queue
              </h2>
              <span className="text-[11px] font-mono text-slate-400">{visits.length} Total</span>
            </div>

            <div className="space-y-2 max-h-[600px] overflow-y-auto pr-1">
              {visits.map((v) => {
                const isSelected = v.id === selectedVisit.id;
                return (
                  <div
                    key={v.id}
                    onClick={() => {
                      setSelectedVisit(v);
                      setChiefComplaint(v.chiefComplaint);
                      setDiagnosis(v.primaryDiagnosis);
                      setIcdCode(v.icdCode);
                    }}
                    className={`p-3.5 rounded-xl border cursor-pointer transition text-xs space-y-1.5 ${
                      isSelected
                        ? 'bg-sky-500/15 border-sky-500/50 shadow-md shadow-sky-500/5'
                        : 'bg-slate-900/50 border-slate-800 text-slate-300 hover:bg-slate-900'
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <span className="font-bold text-white text-sm">{v.patientName}</span>
                      <span
                        className={`px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                          v.status === 'in_progress'
                            ? 'bg-amber-500/20 text-amber-300 border border-amber-500/30 animate-pulse'
                            : 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/30'
                        }`}
                      >
                        {v.status === 'in_progress' ? 'In Examination' : 'Completed'}
                      </span>
                    </div>

                    <div className="text-[11px] text-slate-400 flex items-center gap-2">
                      <span>MRN: <b className="text-slate-300 font-mono">{v.mrn}</b></span>
                      <span>•</span>
                      <span>{v.age}Y / {v.gender}</span>
                      <span>•</span>
                      <span className="font-mono text-sky-400">{v.time}</span>
                    </div>

                    <p className="text-[11px] text-slate-400 truncate">{v.chiefComplaint}</p>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Right 8 Cols: Comprehensive Clinical Record Form */}
        <div className="lg:col-span-8 space-y-4">
          {/* Patient Overview Strip */}
          <div className="glass-panel p-4 rounded-2xl border-slate-800 flex flex-wrap items-center justify-between gap-4">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-sky-500/20 border border-sky-500/40 flex items-center justify-center text-sky-300 font-bold">
                <User className="w-5 h-5" />
              </div>
              <div>
                <div className="text-sm font-bold text-white flex items-center gap-2">
                  <span>{selectedVisit.patientName}</span>
                  <span className="text-xs text-slate-400 font-mono font-normal">({selectedVisit.mrn})</span>
                </div>
                <div className="text-xs text-slate-400">
                  Encounter Ref: <span className="font-mono text-sky-400">{selectedVisit.id}</span>
                </div>
              </div>
            </div>

            {/* Vitals Summary */}
            <div className="flex items-center gap-4 text-xs font-mono">
              <div className="bg-slate-900 px-3 py-1.5 rounded-xl border border-slate-800 text-center">
                <span className="text-[10px] text-slate-500 block">BP (mmHg)</span>
                <span className="font-bold text-amber-400">{selectedVisit.vitals.bp}</span>
              </div>
              <div className="bg-slate-900 px-3 py-1.5 rounded-xl border border-slate-800 text-center">
                <span className="text-[10px] text-slate-500 block">Pulse (bpm)</span>
                <span className="font-bold text-emerald-400">{selectedVisit.vitals.hr}</span>
              </div>
              <div className="bg-slate-900 px-3 py-1.5 rounded-xl border border-slate-800 text-center">
                <span className="text-[10px] text-slate-500 block">Temp (°C)</span>
                <span className="font-bold text-slate-200">{selectedVisit.vitals.temp}</span>
              </div>
              <div className="bg-slate-900 px-3 py-1.5 rounded-xl border border-slate-800 text-center">
                <span className="text-[10px] text-slate-500 block">SpO2 (%)</span>
                <span className="font-bold text-sky-400">{selectedVisit.vitals.spo2}</span>
              </div>
            </div>
          </div>

          {/* Clinical Documentation Form */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4 text-xs">
            {/* 1. Chief Complaint & History */}
            <div>
              <label className="text-slate-300 font-bold block mb-1">
                1. Chief Complaint & History of Present Illness (HPI)
              </label>
              <textarea
                rows={2}
                value={chiefComplaint}
                onChange={(e) => setChiefComplaint(e.target.value)}
                className="w-full p-3 rounded-xl bg-slate-900 border border-slate-800 text-slate-200 focus:outline-none focus:border-sky-500 transition leading-relaxed"
              />
            </div>

            {/* 2. Physical Examination */}
            <div>
              <label className="text-slate-300 font-bold block mb-1">
                2. Physical Examination Findings
              </label>
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 text-[11px]">
                <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                  <span className="text-slate-400 block font-semibold">HEENT</span>
                  <span className="text-slate-200">Normocephalic, mucosa moist</span>
                </div>
                <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                  <span className="text-slate-400 block font-semibold">Cardiovascular</span>
                  <span className="text-slate-200">S1, S2 audible, no murmurs</span>
                </div>
                <div className="p-2.5 rounded-xl bg-amber-500/10 border border-amber-500/30">
                  <span className="text-amber-400 block font-semibold">Respiratory</span>
                  <span className="text-amber-200">Crackles right base</span>
                </div>
                <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                  <span className="text-slate-400 block font-semibold">Abdomen</span>
                  <span className="text-slate-200">Soft, non-tender</span>
                </div>
              </div>
            </div>

            {/* 3. Diagnosis & ICD-10 */}
            <div className="grid grid-cols-3 gap-3">
              <div className="col-span-2">
                <label className="text-slate-300 font-bold block mb-1">
                  3. Primary Clinical Assessment / Diagnosis
                </label>
                <input
                  type="text"
                  value={diagnosis}
                  onChange={(e) => setDiagnosis(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-white focus:outline-none focus:border-sky-500"
                />
              </div>
              <div>
                <label className="text-slate-300 font-bold block mb-1">ICD-10 Code</label>
                <input
                  type="text"
                  value={icdCode}
                  onChange={(e) => setIcdCode(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-sky-400 font-mono font-bold focus:outline-none focus:border-sky-500"
                />
              </div>
            </div>

            {/* 4. Physician Treatment Notes */}
            <div>
              <label className="text-slate-300 font-bold block mb-1">
                4. Physician Clinical Progress Notes & Treatment Strategy
              </label>
              <textarea
                rows={3}
                value={doctorNotes}
                onChange={(e) => setDoctorNotes(e.target.value)}
                className="w-full p-3 rounded-xl bg-slate-900 border border-slate-800 text-slate-200 focus:outline-none focus:border-sky-500 transition leading-relaxed"
              />
            </div>

            {/* 5. Integrated Action Shortcuts */}
            <div className="pt-3 border-t border-slate-800">
              <label className="text-slate-400 font-semibold block mb-2">
                Immediate Diagnostic & Therapeutic Orders for this Encounter:
              </label>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                <Link
                  href="/dashboard/prescriptions"
                  className="p-3 rounded-xl bg-teal-500/10 border border-teal-500/30 hover:bg-teal-500/20 text-teal-300 transition flex items-center justify-between"
                >
                  <div className="flex items-center gap-2">
                    <Pill className="w-4 h-4 text-teal-400" />
                    <span>Prescribe Medication</span>
                  </div>
                  <ChevronRight className="w-4 h-4" />
                </Link>

                <Link
                  href="/dashboard/lab-analyzer"
                  className="p-3 rounded-xl bg-amber-500/10 border border-amber-500/30 hover:bg-amber-500/20 text-amber-300 transition flex items-center justify-between"
                >
                  <div className="flex items-center gap-2">
                    <TestTubes className="w-4 h-4 text-amber-400" />
                    <span>Order Lab Assay</span>
                  </div>
                  <ChevronRight className="w-4 h-4" />
                </Link>

                <Link
                  href="/dashboard/radiology"
                  className="p-3 rounded-xl bg-indigo-500/10 border border-indigo-500/30 hover:bg-indigo-500/20 text-indigo-300 transition flex items-center justify-between"
                >
                  <div className="flex items-center gap-2">
                    <ScanLine className="w-4 h-4 text-indigo-400" />
                    <span>Order Radiology Scan</span>
                  </div>
                  <ChevronRight className="w-4 h-4" />
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
