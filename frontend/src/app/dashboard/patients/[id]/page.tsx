'use client';

import React, { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  User,
  Activity,
  Heart,
  Calendar,
  Phone,
  ShieldAlert,
  Clock,
  Pill,
  FileText,
  TestTubes,
  ScanLine,
  Smile,
  ReceiptText,
  ChevronLeft,
  ArrowUpRight,
  CheckCircle2,
  AlertTriangle,
  Stethoscope,
  Share2,
  Printer
} from 'lucide-react';

interface PatientRecord {
  id: string;
  mrn: string;
  name: string;
  arabicName: string;
  gender: string;
  age: number;
  dob: string;
  nationalId: string;
  phone: string;
  bloodType: string;
  insurance: string;
  allergies: string[];
  chronicDiseases: string[];
  vitals: {
    bp: string;
    bpStatus: 'normal' | 'warning' | 'critical';
    hr: number;
    hrStatus: 'normal' | 'warning';
    spo2: number;
    temp: number;
    respRate: number;
    bmi: number;
  };
  timeline: {
    id: string;
    date: string;
    type: 'encounter' | 'lab' | 'imaging' | 'prescription' | 'dental';
    title: string;
    doctor: string;
    summary: string;
    badge?: string;
  }[];
}

const PATIENT_DATA: PatientRecord = {
  id: 'P-10024',
  mrn: 'P-10024',
  name: 'Tariq Al-Mansoor',
  arabicName: 'طارق عبد العزيز المنصور',
  gender: 'Male',
  age: 46,
  dob: '1980-04-12',
  nationalId: '1084920194',
  phone: '+966 50 123 4567',
  bloodType: 'O+',
  insurance: 'Bupa Arabia (Gold VIP #99104)',
  allergies: ['Penicillin (Severe Urticaria / Anaphylaxis Risk)'],
  chronicDiseases: ['Type 2 Diabetes Mellitus', 'Essential Hypertension', 'Mild CKD Stage 3a'],
  vitals: {
    bp: '138/88 mmHg',
    bpStatus: 'warning',
    hr: 74,
    hrStatus: 'normal',
    spo2: 98,
    temp: 36.8,
    respRate: 16,
    bmi: 27.8
  },
  timeline: [
    {
      id: 't-1',
      date: 'Today, 09:30 AM',
      type: 'lab',
      title: 'Comprehensive Metabolic Panel & Glycemic Assay',
      doctor: 'Dr. Sarah Al-Mansoor',
      summary: 'HbA1c marked at 8.4%, fasting glucose 168 mg/dL. Statin adjustment recommended.',
      badge: 'Automated AI Extraction'
    },
    {
      id: 't-2',
      date: 'Today, 09:15 AM',
      type: 'imaging',
      title: 'Digital Chest X-Ray (PA View)',
      doctor: 'Radiology Department',
      summary: 'Right lower lobe focal consolidation detected. Alveolar pattern consistent with pneumonia.',
      badge: 'AI Heatmap Verified'
    },
    {
      id: 't-3',
      date: 'Today, 08:45 AM',
      type: 'encounter',
      title: 'Follow-up Clinical Consultation',
      doctor: 'Dr. Sarah Al-Mansoor',
      summary: 'Patient presented with 3-day history of productive cough, mild fever, and glycemic fluctuations.',
      badge: 'Internal Medicine'
    },
    {
      id: 't-4',
      date: 'Sep 02, 2026',
      type: 'dental',
      title: 'Panoramic OPG & Tooth #48 Assessment',
      doctor: 'Dr. Khalid Al-Najjar',
      summary: 'Mesioangular impaction of right mandibular third molar. Surgical extraction planned.',
      badge: 'FDI Dental Chart'
    },
    {
      id: 't-5',
      date: 'Aug 14, 2026',
      type: 'prescription',
      title: 'e-Prescription Dispensed (Refill)',
      doctor: 'Dr. Sarah Al-Mansoor',
      summary: 'Metformin HCl 850mg BID + Atorvastatin 20mg QHS. Verified no penicillin cross-reaction.'
    }
  ]
};

export default function PatientDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<'timeline' | 'soap' | 'prescriptions' | 'billing'>('timeline');

  return (
    <div className="space-y-6">
      {/* Top Navigation & Breadcrumbs */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <Link
            href="/dashboard/patients"
            className="p-2 rounded-xl bg-slate-900 border border-slate-800 text-slate-400 hover:text-white transition flex items-center gap-1 text-xs"
          >
            <ChevronLeft className="w-4 h-4" />
            <span>Patients Registry</span>
          </Link>
          <span className="text-slate-600">/</span>
          <span className="text-xs font-mono text-sky-400 font-bold">MRN: {PATIENT_DATA.mrn}</span>
        </div>

        {/* Action Shortcuts */}
        <div className="flex items-center gap-2">
          <Link
            href="/dashboard/canvas"
            className="px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs font-medium text-slate-300 hover:text-sky-400 hover:border-sky-500/40 flex items-center gap-1.5 transition"
          >
            <Activity className="w-3.5 h-3.5" />
            <span>Body Canvas</span>
          </Link>

          <Link
            href="/dashboard/dental"
            className="px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs font-medium text-slate-300 hover:text-amber-400 hover:border-amber-500/40 flex items-center gap-1.5 transition"
          >
            <Smile className="w-3.5 h-3.5" />
            <span>Dental Chart</span>
          </Link>

          <Link
            href="/dashboard/prescriptions"
            className="px-3 py-2 rounded-xl bg-teal-600 hover:bg-teal-500 text-xs font-semibold text-white flex items-center gap-1.5 transition shadow-lg shadow-teal-600/20"
          >
            <Pill className="w-3.5 h-3.5" />
            <span>Prescribe Rx</span>
          </Link>
        </div>
      </div>

      {/* Patient Profile Demographics Banner */}
      <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-5">
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-sky-600 to-indigo-600 border border-sky-400/30 flex items-center justify-center text-white font-bold text-2xl shadow-xl shadow-sky-500/20">
              {PATIENT_DATA.name[0]}
            </div>
            <div>
              <div className="flex items-center gap-3">
                <h1 className="text-xl md:text-2xl font-bold text-white">{PATIENT_DATA.name}</h1>
                <span className="text-sm text-slate-400 font-arabic">{PATIENT_DATA.arabicName}</span>
              </div>
              <div className="flex flex-wrap items-center gap-3 text-xs text-slate-400 mt-1">
                <span>MRN: <b className="text-slate-200 font-mono">{PATIENT_DATA.mrn}</b></span>
                <span>•</span>
                <span>National ID: <b className="text-slate-200 font-mono">{PATIENT_DATA.nationalId}</b></span>
                <span>•</span>
                <span>Age: <b className="text-slate-200">{PATIENT_DATA.age} Y ({PATIENT_DATA.gender})</b></span>
                <span>•</span>
                <span>Blood: <b className="text-rose-400 font-bold">{PATIENT_DATA.bloodType}</b></span>
              </div>
            </div>
          </div>

          <div className="flex flex-col items-start lg:items-end gap-1.5 text-xs">
            <div className="px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 font-semibold flex items-center gap-1.5">
              <CheckCircle2 className="w-3.5 h-3.5" />
              <span>Active EHR File</span>
            </div>
            <div className="text-slate-400">{PATIENT_DATA.insurance}</div>
          </div>
        </div>

        {/* Chronic Conditions & Allergy Warning Banner */}
        <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-slate-800/80">
          <div className="flex items-center gap-2 mr-2 text-xs font-semibold text-slate-400">
            Conditions:
          </div>
          {PATIENT_DATA.chronicDiseases.map((c, idx) => (
            <span
              key={idx}
              className="px-2.5 py-1 rounded-lg text-xs font-medium bg-slate-900 border border-slate-800 text-slate-300"
            >
              {c}
            </span>
          ))}

          {/* Allergy Pill */}
          {PATIENT_DATA.allergies.map((a, idx) => (
            <span
              key={idx}
              className="px-2.5 py-1 rounded-lg text-xs font-bold bg-rose-500/15 border border-rose-500/30 text-rose-300 flex items-center gap-1.5"
            >
              <ShieldAlert className="w-3.5 h-3.5 text-rose-400" />
              <span>Allergy: {a}</span>
            </span>
          ))}
        </div>
      </div>

      {/* Vitals Strip */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3">
        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">Blood Pressure</div>
          <div className="text-base font-bold text-amber-400 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.bp}
          </div>
          <div className="text-[10px] text-amber-400/80 font-medium">Stage 1 HTN</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">Heart Rate</div>
          <div className="text-base font-bold text-emerald-400 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.hr} <span className="text-[10px] text-slate-500 font-normal">bpm</span>
          </div>
          <div className="text-[10px] text-emerald-400/80 font-medium">Normal Sinus</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">SpO2 Oxygen</div>
          <div className="text-base font-bold text-sky-400 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.spo2} <span className="text-[10px] text-slate-500 font-normal">%</span>
          </div>
          <div className="text-[10px] text-sky-400/80 font-medium">Optimal</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">Temperature</div>
          <div className="text-base font-bold text-slate-200 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.temp} <span className="text-[10px] text-slate-500 font-normal">°C</span>
          </div>
          <div className="text-[10px] text-slate-400 font-medium">Afebrile</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">Respiratory Rate</div>
          <div className="text-base font-bold text-slate-200 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.respRate} <span className="text-[10px] text-slate-500 font-normal">/min</span>
          </div>
          <div className="text-[10px] text-slate-400 font-medium">Eupneic</div>
        </div>

        <div className="glass-panel p-4 rounded-xl border-slate-800">
          <div className="text-[11px] text-slate-400">BMI</div>
          <div className="text-base font-bold text-amber-400 mt-0.5 font-mono">
            {PATIENT_DATA.vitals.bmi}
          </div>
          <div className="text-[10px] text-amber-400/80 font-medium">Overweight (Class I)</div>
        </div>
      </div>

      {/* Tabs Navigation */}
      <div className="border-b border-slate-800 flex items-center gap-6 text-sm font-semibold">
        <button
          onClick={() => setActiveTab('timeline')}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'timeline' ? 'text-sky-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Clock className="w-4 h-4" />
          <span>Longitudinal Timeline (Section 65)</span>
          {activeTab === 'timeline' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-sky-400 rounded-full" />
          )}
        </button>

        <button
          onClick={() => setActiveTab('soap')}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'soap' ? 'text-sky-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <FileText className="w-4 h-4" />
          <span>SOAP Clinical Notes</span>
          {activeTab === 'soap' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-sky-400 rounded-full" />
          )}
        </button>

        <button
          onClick={() => setActiveTab('prescriptions')}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'prescriptions' ? 'text-sky-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <Pill className="w-4 h-4" />
          <span>Therapeutics History</span>
          {activeTab === 'prescriptions' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-sky-400 rounded-full" />
          )}
        </button>

        <button
          onClick={() => setActiveTab('billing')}
          className={`pb-3 transition relative flex items-center gap-2 ${
            activeTab === 'billing' ? 'text-sky-400' : 'text-slate-400 hover:text-slate-200'
          }`}
        >
          <ReceiptText className="w-4 h-4" />
          <span>Financial Ledger</span>
          {activeTab === 'billing' && (
            <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-sky-400 rounded-full" />
          )}
        </button>
      </div>

      {/* Tab Content 1: Longitudinal Timeline */}
      {activeTab === 'timeline' && (
        <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-6">
          <h2 className="text-base font-bold text-white flex items-center gap-2">
            <Clock className="w-4 h-4 text-sky-400" />
            Chronological Care Sequence
          </h2>

          <div className="relative pl-6 space-y-6 before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-800">
            {PATIENT_DATA.timeline.map((event) => {
              let Icon = Stethoscope;
              let iconColor = 'text-sky-400 bg-sky-500/10 border-sky-500/30';
              if (event.type === 'lab') {
                Icon = TestTubes;
                iconColor = 'text-amber-400 bg-amber-500/10 border-amber-500/30';
              } else if (event.type === 'imaging') {
                Icon = ScanLine;
                iconColor = 'text-indigo-400 bg-indigo-500/10 border-indigo-500/30';
              } else if (event.type === 'prescription') {
                Icon = Pill;
                iconColor = 'text-teal-400 bg-teal-500/10 border-teal-500/30';
              } else if (event.type === 'dental') {
                Icon = Smile;
                iconColor = 'text-emerald-400 bg-emerald-500/10 border-emerald-500/30';
              }

              return (
                <div key={event.id} className="relative group">
                  {/* Timeline bullet */}
                  <div
                    className={`absolute -left-[31px] top-1 w-7 h-7 rounded-full border flex items-center justify-center ${iconColor}`}
                  >
                    <Icon className="w-3.5 h-3.5" />
                  </div>

                  {/* Timeline card */}
                  <div className="p-4 rounded-xl bg-slate-900/50 border border-slate-800 hover:border-slate-700 transition space-y-2">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <div className="flex items-center gap-2">
                        <span className="font-bold text-sm text-white">{event.title}</span>
                        {event.badge && (
                          <span className="text-[10px] px-2 py-0.5 rounded-full bg-slate-800 text-slate-300 font-medium">
                            {event.badge}
                          </span>
                        )}
                      </div>
                      <span className="text-xs text-slate-400 font-mono">{event.date}</span>
                    </div>

                    <p className="text-xs text-slate-300 leading-relaxed">{event.summary}</p>

                    <div className="text-[11px] text-slate-500 font-medium">
                      Physician / Service: <span className="text-slate-400">{event.doctor}</span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Tab Content 2: SOAP Notes */}
      {activeTab === 'soap' && (
        <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-5">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <FileText className="w-4 h-4 text-sky-400" />
              Standardized Clinical Encounter Notes (SOAP)
            </h2>
            <span className="text-xs font-mono text-slate-400">Dr. Sarah Al-Mansoor • Today</span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-xs">
            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
              <div className="font-bold text-sky-400 uppercase tracking-wider flex items-center gap-2">
                <span>[S] Subjective</span>
              </div>
              <p className="text-slate-300 leading-relaxed">
                46-year-old male with history of T2D and HTN complains of 3 days of progressive productive cough with yellow sputum, right pleuritic chest discomfort, and mild fever. Denies orthopnea or lower extremity edema.
              </p>
            </div>

            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
              <div className="font-bold text-indigo-400 uppercase tracking-wider flex items-center gap-2">
                <span>[O] Objective</span>
              </div>
              <p className="text-slate-300 leading-relaxed">
                Vitals: BP 138/88, HR 74, RR 16, SpO2 98% on room air, Temp 36.8°C.
                Chest: Decreased breath sounds and localized bronchial breathing over right lower lung field.
                CXR confirms right lower lobe alveolar consolidation.
              </p>
            </div>

            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
              <div className="font-bold text-amber-400 uppercase tracking-wider flex items-center gap-2">
                <span>[A] Assessment</span>
              </div>
              <p className="text-slate-300 leading-relaxed">
                1. Community-Acquired Pneumonia (CAP) - Right Lower Lobe.
                2. Type 2 Diabetes Mellitus - suboptimal glycemic control (HbA1c 8.4%).
                3. Essential Hypertension - Stage 1, moderately controlled.
              </p>
            </div>

            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 space-y-2">
              <div className="font-bold text-emerald-400 uppercase tracking-wider flex items-center gap-2">
                <span>[P] Plan</span>
              </div>
              <p className="text-slate-300 leading-relaxed">
                1. Antibiotic therapy: Initiate non-penicillin agent (Clarithromycin / Azithromycin or Respiratory Fluoroquinolone) due to severe penicillin allergy.
                2. Antipyretic / Analgesic: Paracetamol PRN.
                3. Endocrinologist consult for diabetes regimen optimization.
                4. Repeat CXR and clinical follow-up in 10 days.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Tab Content 3: Prescriptions */}
      {activeTab === 'prescriptions' && (
        <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <Pill className="w-4 h-4 text-teal-400" />
              Active Medication Regimen
            </h2>
            <Link
              href="/dashboard/prescriptions"
              className="text-xs text-sky-400 hover:text-sky-300 font-semibold flex items-center gap-1"
            >
              <span>Open Prescriptions Studio</span>
              <ArrowUpRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          <div className="space-y-3">
            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 flex items-center justify-between">
              <div>
                <div className="font-bold text-white text-sm">Glucophage (Metformin HCl) 850 mg</div>
                <div className="text-xs text-slate-400 mt-0.5">Oral Tablet • Twice Daily with meals • Refill: 90 Days</div>
              </div>
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                Active
              </span>
            </div>

            <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 flex items-center justify-between">
              <div>
                <div className="font-bold text-white text-sm">Lipitor (Atorvastatin Calcium) 20 mg</div>
                <div className="text-xs text-slate-400 mt-0.5">Oral Tablet • Once Daily at Bedtime • Refill: 90 Days</div>
              </div>
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                Active
              </span>
            </div>
          </div>
        </div>
      )}

      {/* Tab Content 4: Financial Ledger */}
      {activeTab === 'billing' && (
        <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <ReceiptText className="w-4 h-4 text-amber-400" />
              Patient Encounter Invoices & Payment Ledger
            </h2>
            <Link
              href="/dashboard/billing"
              className="text-xs text-amber-400 hover:text-amber-300 font-semibold flex items-center gap-1"
            >
              <span>All Clinic Billing</span>
              <ArrowUpRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-slate-800 flex items-center justify-between">
            <div>
              <div className="font-mono text-xs font-bold text-sky-400">INV-2026-0041</div>
              <div className="text-xs text-slate-300 font-semibold mt-0.5">
                Consultation + CMP + HbA1c Assay
              </div>
              <div className="text-[11px] text-slate-400">Total: 950 SAR (Insurance: 760 SAR | Patient: 190 SAR)</div>
            </div>
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              Settled (190 SAR)
            </span>
          </div>
        </div>
      )}
    </div>
  );
}
