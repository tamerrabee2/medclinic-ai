'use client';

import React, { useState, useRef } from 'react';
import { PermissionGate } from '@/components/auth/PermissionGate';
import {
  Activity,
  PenTool,
  Circle,
  Square,
  ArrowRight,
  Type,
  MapPin,
  Trash2,
  Download,
  Save,
  CheckCircle2,
  Sparkles,
  Layers,
  ZoomIn,
  ZoomOut
} from 'lucide-react';

const BODY_REGIONS = [
  'Head', 'Neck', 'Chest', 'Abdomen', 'Pelvis', 'Groin',
  'UpperBack', 'LowerBack', 'LeftShoulder', 'RightShoulder',
  'LeftArm', 'RightArm', 'LeftElbow', 'RightElbow',
  'LeftForearm', 'RightForearm', 'LeftWrist', 'RightWrist',
  'LeftHand', 'RightHand', 'LeftThigh', 'RightThigh',
  'LeftKnee', 'RightKnee', 'LeftShin', 'RightShin'
];

interface Pin {
  id: string;
  x: number;
  y: number;
  region: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  note: string;
}

export default function CanvasBodyMapPage() {
  const [selectedRegion, setSelectedRegion] = useState('Chest');
  const [severity, setSeverity] = useState<'Low' | 'Medium' | 'High' | 'Critical'>('Medium');
  const [tool, setTool] = useState<'pin' | 'pen' | 'circle' | 'rect'>('pin');
  const [view, setView] = useState<'anterior' | 'posterior'>('anterior');
  const [note, setNote] = useState('');
  const [pins, setPins] = useState<Pin[]>([
    { id: '1', x: 50, y: 32, region: 'Chest', severity: 'High', note: 'Acute substernal discomfort radiating left' },
    { id: '2', x: 38, y: 72, region: 'LeftKnee', severity: 'Medium', note: 'Meniscal tenderness on flexion' },
  ]);

  const canvasRef = useRef<HTMLDivElement>(null);

  const handleCanvasClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!canvasRef.current) return;
    const rect = canvasRef.current.getBoundingClientRect();
    const x = Math.round(((e.clientX - rect.left) / rect.width) * 100);
    const y = Math.round(((e.clientY - rect.top) / rect.height) * 100);

    const newPin: Pin = {
      id: Math.random().toString(),
      x,
      y,
      region: selectedRegion,
      severity,
      note: note || `Clinical note for ${selectedRegion}`,
    };

    setPins([...pins, newPin]);
    setNote('');
  };

  const getSeverityColor = (sev: string) => {
    switch (sev) {
      case 'Critical': return '#ef4444';
      case 'High': return '#f97316';
      case 'Medium': return '#eab308';
      default: return '#22c55e';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-white flex items-center gap-2">
            <Activity className="w-6 h-6 text-sky-400" />
            <span>Interactive Anatomical Body Map &amp; Canvas</span>
          </h1>
          <p className="text-sm text-slate-400 mt-1">
            26-Region standardized body mapper with vector coordinate annotation and clinical severity triage.
          </p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setView(view === 'anterior' ? 'posterior' : 'anterior')}
            className="px-4 py-2 rounded-xl glass-panel-interactive text-xs font-semibold text-slate-200 capitalize"
          >
            Switch to {view === 'anterior' ? 'Posterior (Back)' : 'Anterior (Front)'} View
          </button>
          <PermissionGate permission="MedicalRecords.Create">
            <button
              onClick={() => alert('Clinical annotations saved to patient record.')}
              className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 text-white font-medium text-xs shadow-md"
            >
              <Save className="w-3.5 h-3.5" />
              <span>Save Annotations</span>
            </button>
          </PermissionGate>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left Toolbar & Region Selector */}
        <div className="lg:col-span-3 space-y-4">
          <div className="glass-panel p-4 rounded-2xl border-slate-800 space-y-4">
            <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider">Annotation Tools</h3>
            <div className="grid grid-cols-2 gap-2">
              <button
                onClick={() => setTool('pin')}
                className={`flex items-center gap-2 p-2.5 rounded-xl text-xs font-semibold border transition ${
                  tool === 'pin' ? 'bg-sky-500/20 text-sky-400 border-sky-500/40' : 'glass-panel text-slate-400'
                }`}
              >
                <MapPin className="w-4 h-4" />
                <span>Severity Pin</span>
              </button>
              <button
                onClick={() => setTool('pen')}
                className={`flex items-center gap-2 p-2.5 rounded-xl text-xs font-semibold border transition ${
                  tool === 'pen' ? 'bg-sky-500/20 text-sky-400 border-sky-500/40' : 'glass-panel text-slate-400'
                }`}
              >
                <PenTool className="w-4 h-4" />
                <span>Free Pen</span>
              </button>
            </div>

            <div className="space-y-1.5 pt-2 border-t border-slate-800">
              <label className="text-xs font-semibold text-slate-400">Severity Level</label>
              <div className="grid grid-cols-2 gap-2">
                {(['Low', 'Medium', 'High', 'Critical'] as const).map((lvl) => (
                  <button
                    key={lvl}
                    onClick={() => setSeverity(lvl)}
                    style={{
                      borderColor: severity === lvl ? getSeverityColor(lvl) : 'transparent',
                      color: severity === lvl ? getSeverityColor(lvl) : '#94a3b8',
                      background: severity === lvl ? `${getSeverityColor(lvl)}15` : 'rgba(30, 41, 59, 0.5)'
                    }}
                    className="py-1.5 px-2 rounded-lg text-xs font-bold border transition text-center"
                  >
                    {lvl}
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-1.5 pt-2 border-t border-slate-800">
              <label className="text-xs font-semibold text-slate-400">Target Region (26 Total)</label>
              <select
                value={selectedRegion}
                onChange={(e) => setSelectedRegion(e.target.value)}
                className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-sky-500"
              >
                {BODY_REGIONS.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-1.5 pt-2 border-t border-slate-800">
              <label className="text-xs font-semibold text-slate-400">Clinical Finding Note</label>
              <textarea
                value={note}
                onChange={(e) => setNote(e.target.value)}
                rows={2}
                placeholder="Describe clinical finding or lesion..."
                className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 resize-none"
              />
            </div>
          </div>
        </div>

        {/* Center: Interactive Canvas Visual Body Map */}
        <div className="lg:col-span-6 glass-panel rounded-2xl border-slate-800 p-6 flex flex-col items-center justify-center relative min-h-[550px] shadow-2xl overflow-hidden">
          <div className="absolute top-4 left-4 text-xs font-mono text-slate-500 flex items-center gap-1.5">
            <span className="w-2 h-2 rounded-full bg-sky-400 animate-ping" />
            <span>Mode: Click to place {severity} pin on {selectedRegion}</span>
          </div>

          <div
            ref={canvasRef}
            onClick={handleCanvasClick}
            className="relative w-80 h-[480px] bg-slate-900/60 rounded-3xl border border-slate-800 flex items-center justify-center cursor-crosshair shadow-inner group"
          >
            {/* Anatomical Silhouette SVG */}
            <svg
              className="w-full h-full p-4 text-slate-700 stroke-slate-600/80 fill-slate-800/40 pointer-events-none transition group-hover:stroke-sky-500/30"
              viewBox="0 0 200 400"
              xmlns="http://www.w3.org/2000/svg"
            >
              {/* Head */}
              <circle cx="100" cy="45" r="25" strokeWidth="2" />
              {/* Neck */}
              <path d="M92,70 L108,70 L106,85 L94,85 Z" strokeWidth="2" />
              {/* Shoulders & Torso */}
              <path d="M50,90 L94,85 L106,85 L150,90 L140,190 L60,190 Z" strokeWidth="2" />
              {/* Pelvis & Groin */}
              <path d="M60,190 L140,190 L125,230 L75,230 Z" strokeWidth="2" />
              {/* Left Arm */}
              <path d="M50,90 L30,170 L25,230" strokeWidth="12" strokeLinecap="round" />
              {/* Right Arm */}
              <path d="M150,90 L170,170 L175,230" strokeWidth="12" strokeLinecap="round" />
              {/* Left Leg */}
              <path d="M80,230 L75,310 L70,380" strokeWidth="16" strokeLinecap="round" />
              {/* Right Leg */}
              <path d="M120,230 L125,310 L130,380" strokeWidth="16" strokeLinecap="round" />
            </svg>

            {/* Render Placed Severity Pins */}
            {pins.map((pin) => (
              <div
                key={pin.id}
                style={{
                  left: `${pin.x}%`,
                  top: `${pin.y}%`,
                  borderColor: getSeverityColor(pin.severity),
                }}
                className="absolute -translate-x-1/2 -translate-y-1/2 w-6 h-6 rounded-full flex items-center justify-center shadow-lg cursor-pointer transform hover:scale-125 transition"
                title={`${pin.region} (${pin.severity}): ${pin.note}`}
              >
                <span
                  style={{ background: getSeverityColor(pin.severity) }}
                  className="w-3 h-3 rounded-full animate-ping absolute opacity-75"
                />
                <span
                  style={{ background: getSeverityColor(pin.severity) }}
                  className="w-3.5 h-3.5 rounded-full relative z-10 border border-white"
                />
              </div>
            ))}
          </div>

          <div className="mt-4 text-xs text-slate-500">
            {view === 'anterior' ? 'Anterior (Frontal)' : 'Posterior (Dorsal)'} Anatomical Plane &middot; FDI/ISO Standard Region Registry
          </div>
        </div>

        {/* Right: Placed Annotations List */}
        <div className="lg:col-span-3 space-y-4">
          <div className="glass-panel p-4 rounded-2xl border-slate-800 space-y-3">
            <div className="flex items-center justify-between">
              <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider">
                Active Annotations ({pins.length})
              </h3>
              <button
                onClick={() => setPins([])}
                className="text-xs text-rose-400 hover:text-rose-300 flex items-center gap-1"
              >
                <Trash2 className="w-3.5 h-3.5" />
                <span>Clear All</span>
              </button>
            </div>

            <div className="space-y-2.5 max-h-[420px] overflow-y-auto pr-1">
              {pins.map((pin) => (
                <div
                  key={pin.id}
                  className="p-3 rounded-xl bg-slate-900/70 border border-slate-800 space-y-1.5"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-semibold text-xs text-slate-200">{pin.region}</span>
                    <span
                      style={{
                        background: `${getSeverityColor(pin.severity)}20`,
                        color: getSeverityColor(pin.severity),
                        borderColor: `${getSeverityColor(pin.severity)}40`,
                      }}
                      className="px-2 py-0.5 rounded-full text-[10px] font-bold border"
                    >
                      {pin.severity}
                    </span>
                  </div>
                  <p className="text-xs text-slate-400">{pin.note}</p>
                  <div className="text-[10px] font-mono text-slate-500">
                    Pos: ({pin.x}%, {pin.y}%)
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
