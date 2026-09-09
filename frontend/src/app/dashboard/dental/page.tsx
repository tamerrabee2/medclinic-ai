'use client';

import React, { useState } from 'react';
import {
  Smile,
  ShieldCheck,
  Save,
  CheckCircle2,
  AlertTriangle,
  RotateCcw,
  Sparkles
} from 'lucide-react';

const CONDITIONS = [
  { name: 'Healthy', color: '#22C55E', desc: 'Sound tooth structure' },
  { name: 'Cavity', color: '#EF4444', desc: 'Active caries lesion' },
  { name: 'Filling', color: '#3B82F6', desc: 'Restorative composite/amalgam' },
  { name: 'Crown', color: '#F59E0B', desc: 'Full prosthetic coverage' },
  { name: 'RootCanal', color: '#EC4899', desc: 'Endodontic therapy completed' },
  { name: 'Missing', color: '#6B7280', desc: 'Congenitally absent or removed' },
  { name: 'Implant', color: '#8B5CF6', desc: 'Osseointegrated fixture' },
  { name: 'Bridge', color: '#14B8A6', desc: 'Fixed partial denture unit' },
  { name: 'Veneer', color: '#06B6D4', desc: 'Esthetic labial veneer' },
  { name: 'Extracted', color: '#9CA3AF', desc: 'Surgically extracted' },
  { name: 'Impacted', color: '#F97316', desc: 'Unerupted or partially erupted' },
];

// FDI 32 Adult Quadrants
const QUADRANT_1 = [18, 17, 16, 15, 14, 13, 12, 11];
const QUADRANT_2 = [21, 22, 23, 24, 25, 26, 27, 28];
const QUADRANT_4 = [48, 47, 46, 45, 44, 43, 42, 41];
const QUADRANT_3 = [31, 32, 33, 34, 35, 36, 37, 38];

export default function DentalChartPage() {
  const [selectedCondition, setSelectedCondition] = useState('Cavity');
  const [selectedTooth, setSelectedTooth] = useState<number | null>(16);
  const [toothChart, setToothChart] = useState<Record<number, { condition: string; notes?: string }>>({
    16: { condition: 'Cavity', notes: 'Occlusal deep dentin caries' },
    11: { condition: 'Crown', notes: 'Zirconia crown placed 2024' },
    26: { condition: 'Filling', notes: 'MOD composite restoration' },
    36: { condition: 'RootCanal', notes: 'Treated with gutta-percha' },
    48: { condition: 'Impacted', notes: 'Horizontal impaction, asymptomatic' },
  });

  const handleToothClick = (toothNumber: number) => {
    setSelectedTooth(toothNumber);
    setToothChart((prev) => ({
      ...prev,
      [toothNumber]: {
        condition: selectedCondition,
        notes: prev[toothNumber]?.notes || `Status: ${selectedCondition}`,
      },
    }));
  };

  const getToothColor = (toothNumber: number) => {
    const entry = toothChart[toothNumber];
    if (!entry) return '#22C55E'; // default Healthy
    const cond = CONDITIONS.find((c) => c.name === entry.condition);
    return cond ? cond.color : '#22C55E';
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-white flex items-center gap-2">
            <Smile className="w-6 h-6 text-indigo-400" />
            <span>FDI World Dental Chart (32 Adult Teeth)</span>
          </h1>
          <p className="text-sm text-slate-400 mt-1">
            Standardized two-digit FDI odontogram with real-time condition tracking and treatment planning.
          </p>
        </div>

        <button
          onClick={() => alert('Dental chart saved successfully.')}
          className="flex items-center gap-1.5 px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-sm shadow-md"
        >
          <Save className="w-4 h-4" />
          <span>Save Odontogram</span>
        </button>
      </div>

      {/* Main Layout: Chart on Left, Palette & Info on Right */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left: Interactive FDI Odontogram */}
        <div className="lg:col-span-8 glass-panel p-6 rounded-2xl border-slate-800 space-y-6 shadow-xl">
          <div className="flex items-center justify-between border-b border-slate-800 pb-3">
            <div className="text-sm font-semibold text-slate-300">
              Active Tool: <span className="font-bold text-sky-400">{selectedCondition}</span>
            </div>
            <div className="text-xs text-slate-400">Click any tooth to apply condition</div>
          </div>

          {/* Upper Arch (Maxilla) */}
          <div className="space-y-2">
            <div className="text-xs font-bold text-slate-400 uppercase tracking-wider text-center">
              Maxillary Arch (Upper Teeth)
            </div>
            <div className="flex justify-center gap-1 sm:gap-2 flex-wrap">
              {/* Quadrant 1 */}
              <div className="flex gap-1">
                {QUADRANT_1.map((t) => (
                  <ToothButton
                    key={t}
                    toothNumber={t}
                    color={getToothColor(t)}
                    isSelected={selectedTooth === t}
                    onClick={() => handleToothClick(t)}
                  />
                ))}
              </div>
              <div className="w-px bg-slate-700 h-16 self-center mx-1" />
              {/* Quadrant 2 */}
              <div className="flex gap-1">
                {QUADRANT_2.map((t) => (
                  <ToothButton
                    key={t}
                    toothNumber={t}
                    color={getToothColor(t)}
                    isSelected={selectedTooth === t}
                    onClick={() => handleToothClick(t)}
                  />
                ))}
              </div>
            </div>
          </div>

          {/* Mid-Arch Divider */}
          <div className="border-t border-dashed border-slate-800 my-4 flex items-center justify-center">
            <span className="px-3 py-0.5 rounded-full bg-slate-900 border border-slate-800 text-[10px] text-slate-400 uppercase tracking-widest">
              Occlusal Plane
            </span>
          </div>

          {/* Lower Arch (Mandible) */}
          <div className="space-y-2">
            <div className="flex justify-center gap-1 sm:gap-2 flex-wrap">
              {/* Quadrant 4 */}
              <div className="flex gap-1">
                {QUADRANT_4.map((t) => (
                  <ToothButton
                    key={t}
                    toothNumber={t}
                    color={getToothColor(t)}
                    isSelected={selectedTooth === t}
                    onClick={() => handleToothClick(t)}
                  />
                ))}
              </div>
              <div className="w-px bg-slate-700 h-16 self-center mx-1" />
              {/* Quadrant 3 */}
              <div className="flex gap-1">
                {QUADRANT_3.map((t) => (
                  <ToothButton
                    key={t}
                    toothNumber={t}
                    color={getToothColor(t)}
                    isSelected={selectedTooth === t}
                    onClick={() => handleToothClick(t)}
                  />
                ))}
              </div>
            </div>
            <div className="text-xs font-bold text-slate-400 uppercase tracking-wider text-center pt-2">
              Mandibular Arch (Lower Teeth)
            </div>
          </div>
        </div>

        {/* Right: 11 Conditions Palette & Tooth Inspector */}
        <div className="lg:col-span-4 space-y-4">
          {/* Condition Selector */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-3">
            <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider">
              Diagnostic Conditions (11 Total)
            </h3>
            <div className="grid grid-cols-2 gap-2">
              {CONDITIONS.map((c) => (
                <button
                  key={c.name}
                  onClick={() => setSelectedCondition(c.name)}
                  style={{
                    borderColor: selectedCondition === c.name ? c.color : 'transparent',
                    background: selectedCondition === c.name ? `${c.color}20` : 'rgba(30, 41, 59, 0.4)',
                  }}
                  className="flex items-center gap-2 p-2 rounded-xl border text-xs font-semibold text-slate-200 hover:bg-slate-900 transition"
                >
                  <span
                    style={{ background: c.color }}
                    className="w-3 h-3 rounded-full shrink-0 shadow-sm"
                  />
                  <span className="truncate">{c.name}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Selected Tooth Inspector */}
          {selectedTooth && (
            <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-3">
              <div className="flex items-center justify-between border-b border-slate-800 pb-2">
                <span className="font-bold text-sm text-white">
                  Tooth #{selectedTooth} Details
                </span>
                <span
                  style={{
                    color: getToothColor(selectedTooth),
                    background: `${getToothColor(selectedTooth)}15`,
                    borderColor: `${getToothColor(selectedTooth)}30`,
                  }}
                  className="text-xs px-2.5 py-0.5 rounded-full font-bold border"
                >
                  {toothChart[selectedTooth]?.condition || 'Healthy'}
                </span>
              </div>

              <div className="text-xs text-slate-400 space-y-2">
                <div>
                  <span className="font-semibold text-slate-300">Quadrant: </span>
                  {selectedTooth < 20 ? 'Upper Right (1)' : selectedTooth < 30 ? 'Upper Left (2)' : selectedTooth < 40 ? 'Lower Left (3)' : 'Lower Right (4)'}
                </div>
                <div>
                  <label className="block font-semibold text-slate-300 mb-1">Clinical Note:</label>
                  <textarea
                    rows={2}
                    value={toothChart[selectedTooth]?.notes || ''}
                    onChange={(e) => {
                      const text = e.target.value;
                      setToothChart((prev) => ({
                        ...prev,
                        [selectedTooth]: {
                          ...prev[selectedTooth],
                          condition: prev[selectedTooth]?.condition || 'Healthy',
                          notes: text,
                        },
                      }));
                    }}
                    className="w-full p-2 bg-slate-900 border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-sky-500 resize-none"
                    placeholder="Enter observation or procedure notes..."
                  />
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function ToothButton({
  toothNumber,
  color,
  isSelected,
  onClick,
}: {
  toothNumber: number;
  color: string;
  isSelected: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      style={{
        borderColor: isSelected ? '#38bdf8' : 'rgba(255, 255, 255, 0.1)',
        backgroundColor: `${color}15`,
      }}
      className={`w-10 h-16 rounded-xl border flex flex-col items-center justify-between p-1.5 transition-all hover:scale-105 ${
        isSelected ? 'ring-2 ring-sky-400 ring-offset-2 ring-offset-slate-950 scale-105' : ''
      }`}
    >
      <span className="text-[10px] font-mono font-bold text-slate-400">{toothNumber}</span>
      <div
        style={{ backgroundColor: color }}
        className="w-5 h-6 rounded-md shadow-md border border-white/20"
      />
    </button>
  );
}
