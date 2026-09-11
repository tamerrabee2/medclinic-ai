'use client';

import React, { useState } from 'react';
import { PermissionGate } from '@/components/auth/PermissionGate';
import {
  Pill,
  Plus,
  Trash2,
  AlertTriangle,
  CheckCircle2,
  Printer,
  Sparkles,
  Search,
  Clock,
  QrCode,
  FileCheck,
  ShieldAlert,
  User,
  X,
  Stethoscope
} from 'lucide-react';

interface MedicationItem {
  id: string;
  name: string;
  generic: string;
  strength: string;
  form: string;
  route: string;
  frequency: string;
  duration: string;
  instructions: string;
}

const COMMON_DRUGS = [
  { name: 'Augmentin', generic: 'Amoxicillin / Clavulanate', strength: '1000 mg', form: 'Oral Tablet', category: 'Antibiotic' },
  { name: 'Glucophage', generic: 'Metformin Hydrochloride', strength: '850 mg', form: 'Oral Tablet', category: 'Antidiabetic' },
  { name: 'Lipitor', generic: 'Atorvastatin Calcium', strength: '20 mg', form: 'Oral Tablet', category: 'Lipid Lowering' },
  { name: 'Zestril', generic: 'Lisinopril', strength: '10 mg', form: 'Oral Tablet', category: 'Antihypertensive' },
  { name: 'Nexium', generic: 'Esomeprazole Magnesium', strength: '40 mg', form: 'Delayed-Release Capsule', category: 'Proton Pump Inhibitor' },
  { name: 'Panadol Extra', generic: 'Paracetamol / Caffeine', strength: '500/65 mg', form: 'Oral Tablet', category: 'Analgesic' },
  { name: 'Ventolin', generic: 'Salbutamol Sulfate', strength: '100 mcg', form: 'Inhaler', category: 'Bronchodilator' }
];

const INITIAL_ITEMS: MedicationItem[] = [
  {
    id: 'rx-1',
    name: 'Glucophage',
    generic: 'Metformin Hydrochloride',
    strength: '850 mg',
    form: 'Oral Tablet',
    route: 'Oral',
    frequency: 'Twice daily (BID)',
    duration: '90 Days',
    instructions: 'Take with or immediately after meals to reduce gastrointestinal upset.'
  },
  {
    id: 'rx-2',
    name: 'Lipitor',
    generic: 'Atorvastatin Calcium',
    strength: '20 mg',
    form: 'Oral Tablet',
    route: 'Oral',
    frequency: 'Once daily at bedtime (QHS)',
    duration: '90 Days',
    instructions: 'Take at night. Avoid grapefruit juice.'
  }
];

export default function PrescriptionsPage() {
  const [items, setItems] = useState<MedicationItem[]>(INITIAL_ITEMS);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedDrug, setSelectedDrug] = useState(COMMON_DRUGS[0]);
  const [frequency, setFrequency] = useState('Once daily (OD)');
  const [route, setRoute] = useState('Oral');
  const [duration, setDuration] = useState('14 Days');
  const [instructions, setInstructions] = useState('Take after food with full glass of water.');
  const [showPrintModal, setShowPrintModal] = useState(false);
  const [isPrescribed, setIsPrescribed] = useState(false);

  // Filter drug catalog
  const filteredCatalog = COMMON_DRUGS.filter(
    (d) =>
      d.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      d.generic.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleAddMedication = () => {
    const newItem: MedicationItem = {
      id: `rx-${Date.now()}`,
      name: selectedDrug.name,
      generic: selectedDrug.generic,
      strength: selectedDrug.strength,
      form: selectedDrug.form,
      route,
      frequency,
      duration,
      instructions
    };
    setItems([...items, newItem]);
  };

  const handleRemove = (id: string) => {
    setItems(items.filter((i) => i.id !== id));
  };

  // Real-time Allergy detection: Check if Augmentin/Amoxicillin is in list while patient has penicillin allergy
  const hasPenicillinConflict = items.some((i) =>
    i.generic.toLowerCase().includes('amoxicillin')
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-teal-500/20 text-teal-300 border border-teal-500/30">
              Module 32
            </span>
            <span className="text-xs text-slate-400">Clinical Therapeutics</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <Pill className="w-8 h-8 text-sky-400" />
            Electronic Prescriptions & Drug Safety Studio
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Dosing regimen builder with real-time allergy cross-checking and digital Rx dispatch.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setShowPrintModal(true)}
            disabled={items.length === 0}
            className="px-4 py-2.5 rounded-xl border border-slate-700 bg-slate-900/80 hover:bg-slate-800 text-sm font-semibold text-slate-200 flex items-center gap-2 transition disabled:opacity-50"
          >
            <Printer className="w-4 h-4 text-sky-400" />
            <span>Preview Official Rx</span>
          </button>

          <PermissionGate permission="Prescriptions.Sign">
            <button
              onClick={() => setIsPrescribed(true)}
              disabled={items.length === 0 || hasPenicillinConflict}
              className="px-5 py-2.5 rounded-xl bg-gradient-to-r from-teal-500 to-sky-600 hover:from-teal-400 hover:to-sky-500 text-sm font-semibold text-white shadow-lg shadow-teal-500/20 flex items-center gap-2 transition disabled:opacity-50"
            >
              <FileCheck className="w-4 h-4" />
              <span>{isPrescribed ? 'Transmitted to Pharmacy' : 'Sign & Transmit Rx'}</span>
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* Patient Header & Clinical Warnings */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-teal-500/20 border border-teal-500/40 flex items-center justify-center text-teal-300 font-bold">
              <User className="w-5 h-5" />
            </div>
            <div>
              <div className="text-sm font-bold text-white">Tariq Al-Mansoor</div>
              <div className="text-xs text-slate-400 flex items-center gap-2">
                <span>MRN: <b className="text-slate-300">#P-10024</b></span>
                <span>•</span>
                <span>Age: <b className="text-slate-300">46 Y</b></span>
                <span>•</span>
                <span>Weight: <b className="text-slate-300">84 kg</b></span>
                <span>•</span>
                <span>eGFR: <b className="text-amber-400">58 mL/min (Mild Impairment)</b></span>
              </div>
            </div>
          </div>

          {/* Known Allergies Alert */}
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-rose-500/15 border border-rose-500/30 text-rose-300 text-xs font-semibold">
            <ShieldAlert className="w-4 h-4 text-rose-400" />
            <span>Documented Allergy: <b>Penicillin Class (Severe Urticaria)</b></span>
          </div>
        </div>

        {/* Dynamic Conflict Alert if penicillin drug added */}
        {hasPenicillinConflict && (
          <div className="p-4 rounded-xl bg-rose-950/60 border-2 border-rose-500/70 text-rose-200 text-xs flex items-start gap-3 animate-pulse">
            <AlertTriangle className="w-5 h-5 text-rose-400 shrink-0 mt-0.5" />
            <div className="space-y-1">
              <div className="font-bold text-sm text-white">
                CRITICAL CONTRAINDICATION DETECTED (Section 32 AI Safety Gate)
              </div>
              <div>
                You have added an <b>Amoxicillin-containing compound</b>. Patient has a confirmed <b>severe Penicillin allergy</b>. Transmission is blocked until this agent is removed or explicitly counter-overridden.
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Main Studio Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left 5 Cols: Medication Selector & Regimen Builder */}
        <div className="lg:col-span-5 space-y-4">
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <Sparkles className="w-4 h-4 text-teal-400" />
              Medication Formulator
            </h2>

            {/* Catalog Search */}
            <div className="relative">
              <Search className="w-4 h-4 absolute left-3 top-3 text-slate-400" />
              <input
                type="text"
                placeholder="Search medication or generic..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-9 pr-4 py-2.5 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-teal-500 transition"
              />
            </div>

            {/* Quick Drug Buttons */}
            <div className="max-h-48 overflow-y-auto space-y-1.5 pr-1">
              {filteredCatalog.map((drug) => (
                <div
                  key={drug.name}
                  onClick={() => setSelectedDrug(drug)}
                  className={`p-2.5 rounded-xl border cursor-pointer transition flex items-center justify-between text-xs ${
                    selectedDrug.name === drug.name
                      ? 'bg-teal-500/15 border-teal-500/50 text-teal-200'
                      : 'bg-slate-900/40 border-slate-800 text-slate-300 hover:bg-slate-900'
                  }`}
                >
                  <div>
                    <div className="font-bold text-white">{drug.name} ({drug.strength})</div>
                    <div className="text-[11px] text-slate-400">{drug.generic}</div>
                  </div>
                  <span className="text-[10px] px-2 py-0.5 rounded bg-slate-800 text-slate-400 font-mono">
                    {drug.category}
                  </span>
                </div>
              ))}
            </div>

            {/* Regimen Fields */}
            <div className="grid grid-cols-2 gap-3 pt-2 border-t border-slate-800">
              <div>
                <label className="text-[11px] text-slate-400 block mb-1 font-medium">Frequency</label>
                <select
                  value={frequency}
                  onChange={(e) => setFrequency(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-200 focus:outline-none focus:border-teal-500"
                >
                  <option>Once daily (OD)</option>
                  <option>Twice daily (BID)</option>
                  <option>Three times daily (TID)</option>
                  <option>Four times daily (QID)</option>
                  <option>As needed (PRN)</option>
                </select>
              </div>

              <div>
                <label className="text-[11px] text-slate-400 block mb-1 font-medium">Route</label>
                <select
                  value={route}
                  onChange={(e) => setRoute(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-200 focus:outline-none focus:border-teal-500"
                >
                  <option>Oral</option>
                  <option>Intravenous (IV)</option>
                  <option>Intramuscular (IM)</option>
                  <option>Sublingual</option>
                  <option>Inhalation</option>
                  <option>Topical</option>
                </select>
              </div>

              <div className="col-span-2">
                <label className="text-[11px] text-slate-400 block mb-1 font-medium">Duration</label>
                <div className="flex items-center gap-2">
                  {['5 Days', '7 Days', '14 Days', '30 Days', '90 Days'].map((d) => (
                    <button
                      key={d}
                      type="button"
                      onClick={() => setDuration(d)}
                      className={`flex-1 py-1.5 rounded-lg text-xs font-semibold transition ${
                        duration === d
                          ? 'bg-teal-500 text-white shadow-sm'
                          : 'bg-slate-900 text-slate-400 border border-slate-800 hover:text-slate-200'
                      }`}
                    >
                      {d}
                    </button>
                  ))}
                </div>
              </div>

              <div className="col-span-2">
                <label className="text-[11px] text-slate-400 block mb-1 font-medium">Patient Instructions</label>
                <input
                  type="text"
                  value={instructions}
                  onChange={(e) => setInstructions(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-200 focus:outline-none focus:border-teal-500"
                />
              </div>
            </div>

            <PermissionGate permission="Prescriptions.Create">
              <button
                onClick={handleAddMedication}
                className="w-full py-2.5 rounded-xl bg-teal-600 hover:bg-teal-500 text-white font-semibold text-xs transition flex items-center justify-center gap-2 shadow-lg shadow-teal-600/20"
              >
                <Plus className="w-4 h-4" />
                <span>Add to Active Prescription</span>
              </button>
            </PermissionGate>
          </div>
        </div>

        {/* Right 7 Cols: Active Prescription Queue & Interactions */}
        <div className="lg:col-span-7 space-y-4">
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-base font-bold text-white flex items-center gap-2">
                <FileCheck className="w-4 h-4 text-sky-400" />
                Active Prescription Items ({items.length})
              </h2>

              <span className="text-xs text-slate-400 font-mono">
                Rx Serial: #RX-2026-88192
              </span>
            </div>

            {items.length === 0 ? (
              <div className="p-12 text-center text-slate-500 text-xs border border-dashed border-slate-800 rounded-xl">
                No medications added to this prescription yet. Select a drug from the left panel.
              </div>
            ) : (
              <div className="space-y-3">
                {items.map((item, idx) => {
                  const isAllergicDrug = item.generic.toLowerCase().includes('amoxicillin');

                  return (
                    <div
                      key={item.id}
                      className={`p-4 rounded-xl border transition flex items-start justify-between gap-4 ${
                        isAllergicDrug
                          ? 'bg-rose-950/40 border-rose-500/60'
                          : 'bg-slate-900/50 border-slate-800/80 hover:border-slate-700'
                      }`}
                    >
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <span className="w-5 h-5 rounded-full bg-slate-800 text-slate-300 font-mono text-[11px] font-bold flex items-center justify-center">
                            {idx + 1}
                          </span>
                          <span className="font-bold text-white text-sm">{item.name}</span>
                          <span className="text-xs text-teal-400 font-mono font-medium">{item.strength}</span>
                          <span className="text-[11px] text-slate-400 font-mono">({item.form})</span>
                        </div>

                        <div className="text-xs text-slate-400 pl-7">
                          Generic: <span className="text-slate-300 font-medium">{item.generic}</span>
                        </div>

                        <div className="text-xs text-sky-400 pl-7 flex items-center gap-3">
                          <span>Route: <b>{item.route}</b></span>
                          <span>•</span>
                          <span>Sig: <b>{item.frequency}</b></span>
                          <span>•</span>
                          <span>Duration: <b>{item.duration}</b></span>
                        </div>

                        <div className="text-xs text-slate-400 italic pl-7 mt-1">
                          "{item.instructions}"
                        </div>
                      </div>

                      <button
                        onClick={() => handleRemove(item.id)}
                        className="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition"
                        title="Remove Drug"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Printable Prescription Modal */}
      {showPrintModal && (
        <div className="fixed inset-0 z-50 bg-black/80 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white text-slate-900 max-w-2xl w-full rounded-2xl p-8 shadow-2xl space-y-6 relative max-h-[90vh] overflow-y-auto">
            {/* Modal Close Button */}
            <button
              onClick={() => setShowPrintModal(false)}
              className="absolute top-4 right-4 p-2 rounded-lg text-slate-500 hover:text-slate-900 hover:bg-slate-100 transition"
            >
              <X className="w-5 h-5" />
            </button>

            {/* Official Rx Header */}
            <div className="flex items-center justify-between border-b-2 border-slate-900 pb-4">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-xl bg-sky-600 text-white flex items-center justify-center font-bold">
                  <Stethoscope className="w-7 h-7" />
                </div>
                <div>
                  <div className="text-xl font-black text-slate-900 uppercase tracking-tight">
                    MedClinic AI Health Center
                  </div>
                  <div className="text-xs text-slate-600">
                    Clinical Department of Internal Medicine • Lic: #MOH-2026-904
                  </div>
                </div>
              </div>
              <div className="text-right">
                <div className="text-xs text-slate-500">Official e-Prescription</div>
                <div className="text-sm font-mono font-bold text-slate-900">RX-2026-88192</div>
              </div>
            </div>

            {/* Patient Demographic Row */}
            <div className="grid grid-cols-3 gap-4 text-xs bg-slate-50 p-4 rounded-xl border border-slate-200">
              <div>
                <span className="text-slate-500 block">Patient Name:</span>
                <span className="font-bold text-slate-900">Tariq Al-Mansoor</span>
              </div>
              <div>
                <span className="text-slate-500 block">MRN / ID:</span>
                <span className="font-mono font-bold text-slate-900">P-10024</span>
              </div>
              <div>
                <span className="text-slate-500 block">Date & Age:</span>
                <span className="font-mono font-bold text-slate-900">Sep 04, 2026 (46Y)</span>
              </div>
            </div>

            {/* Classical Rx Symbol & Drug Table */}
            <div className="space-y-4">
              <div className="text-3xl font-serif font-black text-sky-700 italic select-none">
                ℞
              </div>

              <div className="space-y-3 divide-y divide-slate-100">
                {items.map((item, idx) => (
                  <div key={item.id} className="pt-2 text-xs">
                    <div className="font-bold text-sm text-slate-900">
                      {idx + 1}. {item.name} ({item.strength}) - {item.generic}
                    </div>
                    <div className="text-slate-700 mt-0.5">
                      <b>Sig:</b> {item.frequency} via {item.route} route for {item.duration}
                    </div>
                    <div className="text-slate-500 italic">
                      Note: {item.instructions}
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Signature & Barcode Footer */}
            <div className="pt-6 border-t-2 border-slate-200 flex items-end justify-between">
              <div className="flex items-center gap-3">
                <QrCode className="w-16 h-16 text-slate-800" />
                <div className="text-[10px] text-slate-500 leading-tight">
                  <div>Scan at participating pharmacy</div>
                  <div>Cryptographic hash verified</div>
                  <div className="font-mono text-slate-700">HASH: 9b1e...4f8c</div>
                </div>
              </div>

              <div className="text-right">
                <div className="font-serif italic text-lg text-slate-800 font-bold">
                  Dr. Sarah Al-Mansoor, MD
                </div>
                <div className="text-xs text-slate-500">Consultant Physician • Lic: #MED-88421</div>
                <div className="text-[10px] text-emerald-600 font-mono font-semibold mt-1">
                  ✓ Digitally Signed & Timestamped
                </div>
              </div>
            </div>

            {/* Modal Actions */}
            <div className="flex items-center justify-end gap-3 pt-2">
              <button
                onClick={() => window.print()}
                className="px-5 py-2 rounded-xl bg-slate-900 hover:bg-slate-800 text-white font-semibold text-xs flex items-center gap-2"
              >
                <Printer className="w-4 h-4" />
                <span>Print Document</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
