'use client';

import React, { useState } from 'react';
import { PermissionGate } from '@/components/auth/PermissionGate';
import {
  ScanLine,
  ZoomIn,
  ZoomOut,
  RotateCw,
  SunMedium,
  Contrast,
  SlidersHorizontal,
  Eye,
  EyeOff,
  Sparkles,
  AlertTriangle,
  FileCheck2,
  Maximize2,
  RefreshCcw,
  CheckCircle2,
  Layers,
  Activity,
  User,
  Download,
  Upload
} from 'lucide-react';

interface ModalityScan {
  id: string;
  name: string;
  type: string;
  bodyPart: string;
  date: string;
  aiScore: number;
  findingTitle: string;
  radsCategory: string;
  summary: string;
  findings: { text: string; confidence: number; isCritical?: boolean }[];
  svgView: 'cxr' | 'brain' | 'spine' | 'dental';
}

const SCANS: ModalityScan[] = [
  {
    id: 'SCAN-01',
    name: 'Chest X-Ray (PA View)',
    type: 'Digital Radiography (DX)',
    bodyPart: 'Thorax / Lungs',
    date: 'Today, 09:15 AM',
    aiScore: 94.2,
    findingTitle: 'Right Lower Lobe Consolidation',
    radsCategory: 'ACR: Clinically Significant',
    summary: 'Prominent alveolar infiltrate in right lower zone suspicious for bacterial pneumonia. No pleural effusion or tension pneumothorax detected. Cardiothoracic ratio is within normal limits.',
    findings: [
      { text: 'Right lower lobe focal consolidation', confidence: 94.2, isCritical: true },
      { text: 'Costophrenic angles sharp and clear', confidence: 98.6 },
      { text: 'Cardiothoracic ratio normal (< 0.50)', confidence: 96.1 },
      { text: 'Trachea midline; no hilar lymphadenopathy', confidence: 92.4 }
    ],
    svgView: 'cxr'
  },
  {
    id: 'SCAN-02',
    name: 'Brain MRI (T2 Axial FLAIR)',
    type: 'Magnetic Resonance (MR)',
    bodyPart: 'Cerebrum & Ventricles',
    date: 'Yesterday, 04:30 PM',
    aiScore: 89.5,
    findingTitle: 'Periventricular White Matter Hyperintensities',
    radsCategory: 'Fazekas Scale Grade 1',
    summary: 'Mild punctate non-confluent white matter microangiopathy compatible with small vessel chronic ischemic changes. No intracranial acute hemorrhage, mass effect, or midline shift.',
    findings: [
      { text: 'Punctate T2/FLAIR hyperintensities', confidence: 89.5 },
      { text: 'No acute diffusion restriction (DWI/ADC)', confidence: 97.8 },
      { text: 'Ventricular system normal size for age', confidence: 95.3 }
    ],
    svgView: 'brain'
  },
  {
    id: 'SCAN-03',
    name: 'Dental Panoramic OPG',
    type: 'Orthopantomogram',
    bodyPart: 'Maxillofacial & Teeth',
    date: 'Sep 02, 2026',
    aiScore: 92.8,
    findingTitle: 'Impacted Lower Third Molar (Tooth #48)',
    radsCategory: 'Pell & Gregory Class II',
    summary: 'Mesioangular impaction of right mandibular third molar encroaching upon the inferior alveolar nerve canal. Periapical radiolucency observed at apex of tooth #36.',
    findings: [
      { text: 'Mesioangular impaction Tooth #48', confidence: 93.4, isCritical: true },
      { text: 'Periapical radiolucency Tooth #36', confidence: 91.2 },
      { text: 'Temporomandibular joints symmetrical', confidence: 96.0 }
    ],
    svgView: 'dental'
  }
];

export default function RadiologyViewerPage() {
  const [selectedScan, setSelectedScan] = useState<ModalityScan>(SCANS[0]);
  const [zoom, setZoom] = useState<number>(100);
  const [rotation, setRotation] = useState<number>(0);
  const [brightness, setBrightness] = useState<number>(100);
  const [contrast, setContrast] = useState<number>(100);
  const [invert, setInvert] = useState<boolean>(false);
  const [showOverlay, setShowOverlay] = useState<boolean>(true);
  const [isReviewed, setIsReviewed] = useState<boolean>(false);

  const handleReset = () => {
    setZoom(100);
    setRotation(0);
    setBrightness(100);
    setContrast(100);
    setInvert(false);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
              Module 18, 19 & 27
            </span>
            <span className="text-xs text-slate-400">Radiology PACS & Vision AI</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <ScanLine className="w-8 h-8 text-sky-400" />
            Medical Imaging & AI Computer Vision Viewer
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Interactive multi-modality diagnostic viewer with AI lesion detection and heatmap overlays.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <PermissionGate permission="Radiology.Create">
            <button
              onClick={() => alert('DICOM Upload Gateway: Ready for DICOM Web STOW-RS')}
              className="px-3.5 py-2 rounded-xl bg-sky-600 hover:bg-sky-500 text-white text-xs font-semibold flex items-center gap-1.5 transition shadow-lg shadow-sky-600/20"
            >
              <Upload className="w-3.5 h-3.5" />
              <span>Upload DICOM</span>
            </button>
          </PermissionGate>

          {/* Scan Selector Tabs */}
          <div className="flex items-center gap-2 bg-slate-900/80 p-1 rounded-xl border border-slate-800">
            {SCANS.map((s) => (
              <button
                key={s.id}
                onClick={() => {
                  setSelectedScan(s);
                  setIsReviewed(false);
                }}
                className={`px-3 py-2 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 ${
                  selectedScan.id === s.id
                    ? 'bg-sky-500 text-white shadow-md'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'
                }`}
              >
                <Layers className="w-3.5 h-3.5" />
                <span>{s.name.split(' ')[0]}</span>
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Mandatory Safety Notice */}
      <div className="flex items-center justify-between p-3.5 rounded-xl bg-amber-500/10 border border-amber-500/25 text-amber-200 text-xs">
        <div className="flex items-center gap-2.5">
          <AlertTriangle className="w-4 h-4 text-amber-400 shrink-0" />
          <span>
            <b>Clinical Safety Guardrail (Section 27):</b> Computer vision inferences are assistive only and must not replace board-certified radiologist confirmation.
          </span>
        </div>
        <span className="text-[10px] font-mono uppercase tracking-wider text-amber-400/80">
          CE / FDA SAMD Class II
        </span>
      </div>

      {/* Viewer Main Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left 8 Cols: Imaging Canvas & Controls */}
        <div className="lg:col-span-8 flex flex-col gap-3">
          {/* Interactive Toolbar */}
          <div className="glass-panel p-3 rounded-xl border-slate-800 flex flex-wrap items-center justify-between gap-3 text-xs">
            {/* Zoom & Rotation */}
            <div className="flex items-center gap-1.5 border-r border-slate-800 pr-3">
              <button
                onClick={() => setZoom((z) => Math.max(50, z - 15))}
                className="p-1.5 rounded-lg bg-slate-900 text-slate-300 hover:text-white hover:bg-slate-800"
                title="Zoom Out"
              >
                <ZoomOut className="w-4 h-4" />
              </button>
              <span className="font-mono text-slate-300 w-12 text-center">{zoom}%</span>
              <button
                onClick={() => setZoom((z) => Math.min(250, z + 15))}
                className="p-1.5 rounded-lg bg-slate-900 text-slate-300 hover:text-white hover:bg-slate-800"
                title="Zoom In"
              >
                <ZoomIn className="w-4 h-4" />
              </button>
              <button
                onClick={() => setRotation((r) => (r + 90) % 360)}
                className="p-1.5 rounded-lg bg-slate-900 text-slate-300 hover:text-white hover:bg-slate-800 ml-1"
                title="Rotate 90°"
              >
                <RotateCw className="w-4 h-4" />
              </button>
            </div>

            {/* Brightness & Contrast */}
            <div className="flex items-center gap-3 border-r border-slate-800 pr-3">
              <div className="flex items-center gap-1.5 text-slate-400">
                <SunMedium className="w-3.5 h-3.5" />
                <input
                  type="range"
                  min="50"
                  max="150"
                  value={brightness}
                  onChange={(e) => setBrightness(Number(e.target.value))}
                  className="w-16 accent-sky-400 h-1 bg-slate-800 rounded-lg cursor-pointer"
                />
              </div>
              <div className="flex items-center gap-1.5 text-slate-400">
                <Contrast className="w-3.5 h-3.5" />
                <input
                  type="range"
                  min="50"
                  max="150"
                  value={contrast}
                  onChange={(e) => setContrast(Number(e.target.value))}
                  className="w-16 accent-sky-400 h-1 bg-slate-800 rounded-lg cursor-pointer"
                />
              </div>
            </div>

            {/* Invert & Overlays */}
            <div className="flex items-center gap-2">
              <button
                onClick={() => setInvert(!invert)}
                className={`px-2.5 py-1.5 rounded-lg font-medium transition ${
                  invert
                    ? 'bg-sky-500 text-white'
                    : 'bg-slate-900 text-slate-300 hover:bg-slate-800'
                }`}
              >
                Invert Colors
              </button>

              <button
                onClick={() => setShowOverlay(!showOverlay)}
                className={`px-2.5 py-1.5 rounded-lg font-medium flex items-center gap-1.5 transition ${
                  showOverlay
                    ? 'bg-indigo-600 text-white shadow-md shadow-indigo-600/30'
                    : 'bg-slate-900 text-slate-400 hover:bg-slate-800'
                }`}
              >
                {showOverlay ? <Eye className="w-3.5 h-3.5" /> : <EyeOff className="w-3.5 h-3.5" />}
                <span>AI Heatmap ROI</span>
              </button>

              <button
                onClick={handleReset}
                className="p-1.5 rounded-lg text-slate-400 hover:text-slate-200 hover:bg-slate-900"
                title="Reset Settings"
              >
                <RefreshCcw className="w-4 h-4" />
              </button>
            </div>
          </div>

          {/* Medical Scan Display Canvas */}
          <div className="relative aspect-[4/3] rounded-2xl bg-black border border-slate-800 overflow-hidden flex items-center justify-center select-none shadow-2xl">
            {/* DICOM / Medical Metadata Overlay (Four Corners) */}
            <div className="absolute top-4 left-4 z-20 font-mono text-[11px] text-emerald-400/90 leading-tight">
              <div>PATIENT: AL-MANSOOR, TARIQ</div>
              <div>ID: P-10024 | DOB: 1980-04-12</div>
              <div>MOD: {selectedScan.type}</div>
            </div>

            <div className="absolute top-4 right-4 z-20 font-mono text-[11px] text-emerald-400/90 text-right leading-tight">
              <div>MEDCLINIC PACS v4.2</div>
              <div>SCAN: {selectedScan.id}</div>
              <div>DATE: {selectedScan.date}</div>
            </div>

            <div className="absolute bottom-4 left-4 z-20 font-mono text-[11px] text-slate-400 leading-tight">
              <div>ZOOM: {zoom}% | ROT: {rotation}°</div>
              <div>BRIGHT: {brightness}% | CONTRAST: {contrast}%</div>
            </div>

            <div className="absolute bottom-4 right-4 z-20 font-mono text-[11px] text-amber-400/90 text-right leading-tight">
              <div>WINDOW: LUNG / MEDIASTINUM</div>
              <div>SLICE: 1 / 1 (COMPOSITE)</div>
            </div>

            {/* The Medical Scan Graphics (Styled SVG Radiograph) */}
            <div
              className="w-full h-full flex items-center justify-center transition-transform duration-150"
              style={{
                transform: `scale(${zoom / 100}) rotate(${rotation}deg)`,
                filter: `brightness(${brightness}%) contrast(${contrast}%) ${invert ? 'invert(1)' : ''}`
              }}
            >
              {selectedScan.svgView === 'cxr' && (
                <svg className="w-4/5 h-4/5 text-slate-200" viewBox="0 0 400 400" fill="none" xmlns="http://www.w3.org/2000/svg">
                  {/* Ribs / Thoracic Cage Outline */}
                  <path d="M200 60 V340" stroke="#475569" strokeWidth="6" strokeDasharray="4 4" />
                  <path d="M140 100 C110 130 90 190 90 270 C140 310 180 300 200 290" stroke="#334155" strokeWidth="4" fill="#0f172a" fillOpacity="0.8" />
                  <path d="M260 100 C290 130 310 190 310 270 C260 310 220 300 200 290" stroke="#334155" strokeWidth="4" fill="#0f172a" fillOpacity="0.8" />
                  
                  {/* Left & Right Rib Contours */}
                  <path d="M130 130 Q170 145 200 140" stroke="#64748b" strokeWidth="3" />
                  <path d="M120 170 Q170 185 200 180" stroke="#64748b" strokeWidth="3" />
                  <path d="M115 210 Q170 225 200 220" stroke="#64748b" strokeWidth="3" />
                  <path d="M110 250 Q170 265 200 260" stroke="#64748b" strokeWidth="3" />

                  <path d="M270 130 Q230 145 200 140" stroke="#64748b" strokeWidth="3" />
                  <path d="M280 170 Q230 185 200 180" stroke="#64748b" strokeWidth="3" />
                  <path d="M285 210 Q230 225 200 220" stroke="#64748b" strokeWidth="3" />
                  <path d="M290 250 Q230 265 200 260" stroke="#64748b" strokeWidth="3" />

                  {/* Cardiac Silhouette */}
                  <ellipse cx="215" cy="240" rx="45" ry="35" fill="#1e293b" stroke="#475569" strokeWidth="2" opacity="0.9" />

                  {/* Right Lower Lobe Infiltrate Graphic */}
                  <circle cx="145" cy="240" r="30" fill="#e2e8f0" fillOpacity="0.3" filter="blur(10px)" />
                  <circle cx="140" cy="245" r="18" fill="#ffffff" fillOpacity="0.5" filter="blur(5px)" />

                  {/* AI Heatmap / ROI Overlay */}
                  {showOverlay && (
                    <g className="animate-pulse">
                      {/* Bounding Box */}
                      <rect x="110" y="205" width="70" height="70" rx="8" stroke="#f43f5e" strokeWidth="2" strokeDasharray="5 3" fill="#f43f5e" fillOpacity="0.2" />
                      {/* ROI Label Tag */}
                      <rect x="110" y="185" width="85" height="18" rx="4" fill="#f43f5e" />
                      <text x="115" y="198" fill="#ffffff" fontSize="9" fontWeight="bold" fontFamily="monospace">
                        INFILTRATE 94%
                      </text>
                    </g>
                  )}
                </svg>
              )}

              {selectedScan.svgView === 'brain' && (
                <svg className="w-4/5 h-4/5 text-slate-200" viewBox="0 0 400 400" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <ellipse cx="200" cy="200" rx="120" ry="140" stroke="#475569" strokeWidth="4" fill="#0f172a" />
                  <path d="M200 70 V330" stroke="#334155" strokeWidth="2" strokeDasharray="3 3" />
                  <path d="M170 180 Q190 150 200 180 Q210 150 230 180 Q210 230 200 210 Q190 230 170 180" stroke="#64748b" strokeWidth="3" fill="#1e293b" />
                  
                  {/* FLAIR Hyperintensities */}
                  <circle cx="160" cy="170" r="6" fill="#f8fafc" fillOpacity="0.8" filter="blur(2px)" />
                  <circle cx="235" cy="185" r="8" fill="#f8fafc" fillOpacity="0.8" filter="blur(2px)" />

                  {showOverlay && (
                    <g className="animate-pulse">
                      <circle cx="160" cy="170" r="14" stroke="#eab308" strokeWidth="2" fill="#eab308" fillOpacity="0.25" />
                      <text x="120" y="150" fill="#eab308" fontSize="9" fontWeight="bold" fontFamily="monospace">
                        FLAIR LESION 89.5%
                      </text>
                    </g>
                  )}
                </svg>
              )}

              {selectedScan.svgView === 'dental' && (
                <svg className="w-4/5 h-4/5 text-slate-200" viewBox="0 0 400 400" fill="none" xmlns="http://www.w3.org/2000/svg">
                  {/* Mandible Curve */}
                  <path d="M60 220 Q200 340 340 220" stroke="#475569" strokeWidth="6" fill="none" />
                  {/* Teeth Arcs */}
                  {Array.from({ length: 14 }).map((_, i) => (
                    <rect
                      key={i}
                      x={80 + i * 18}
                      y={200 + Math.sin((i / 13) * Math.PI) * 45}
                      width="12"
                      height="20"
                      rx="3"
                      fill="#cbd5e1"
                      stroke="#475569"
                    />
                  ))}
                  {/* Impacted molar #48 */}
                  <rect x="315" y="240" width="18" height="12" rx="3" fill="#f8fafc" stroke="#f43f5e" transform="rotate(35 315 240)" />

                  {showOverlay && (
                    <g className="animate-pulse">
                      <rect x="295" y="220" width="45" height="45" rx="6" stroke="#f43f5e" strokeWidth="2" strokeDasharray="3 3" fill="#f43f5e" fillOpacity="0.2" />
                      <text x="270" y="215" fill="#f43f5e" fontSize="9" fontWeight="bold" fontFamily="monospace">
                        IMPACTED #48 (93%)
                      </text>
                    </g>
                  )}
                </svg>
              )}
            </div>
          </div>
        </div>

        {/* Right 4 Cols: AI Structured Radiology Report & Validation */}
        <div className="lg:col-span-4 space-y-4">
          {/* AI Finding Card */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-slate-800">
              <div className="flex items-center gap-2 text-sky-400 font-bold text-sm">
                <Sparkles className="w-4 h-4" />
                <span>AI Inferred Observations</span>
              </div>
              <span className="px-2 py-0.5 rounded text-[11px] font-mono font-bold bg-sky-500/20 text-sky-300">
                {selectedScan.aiScore}% Match
              </span>
            </div>

            {/* Primary Finding */}
            <div>
              <div className="text-xs text-slate-400 font-medium">Primary Indication:</div>
              <div className="text-base font-bold text-white mt-0.5">{selectedScan.findingTitle}</div>
              <div className="inline-block mt-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
                {selectedScan.radsCategory}
              </div>
            </div>

            {/* In-depth Narrative */}
            <p className="text-xs text-slate-300 leading-relaxed bg-slate-900/60 p-3.5 rounded-xl border border-slate-800/80">
              {selectedScan.summary}
            </p>

            {/* Detected Sub-findings */}
            <div className="space-y-2">
              <div className="text-xs font-semibold text-slate-300">Deconstructed Signatures:</div>
              <div className="space-y-2">
                {selectedScan.findings.map((item, idx) => (
                  <div
                    key={idx}
                    className="flex items-center justify-between p-2.5 rounded-xl bg-slate-900/40 border border-slate-800 text-xs"
                  >
                    <span className={item.isCritical ? 'font-semibold text-rose-300' : 'text-slate-300'}>
                      {item.text}
                    </span>
                    <span className="font-mono text-[11px] font-bold text-sky-400 shrink-0 ml-2">
                      {item.confidence}%
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Radiologist / Attending Sign-Off */}
          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
            <div className="flex items-center gap-2 text-slate-200 font-bold text-sm">
              <FileCheck2 className="w-4 h-4 text-emerald-400" />
              <span>Radiology Confirmation Gate</span>
            </div>

            <p className="text-xs text-slate-400 leading-relaxed">
              Sign off on imaging report to append finalized impression to PACS registry and clinical notes.
            </p>

            {isReviewed ? (
              <div className="p-3.5 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-300 text-xs space-y-1">
                <div className="font-bold flex items-center gap-1.5">
                  <CheckCircle2 className="w-4 h-4 text-emerald-400" />
                  <span>Report Signed & Archived</span>
                </div>
                <div className="text-[11px] text-emerald-400/80 font-mono">
                  Validated by Attending Radiologist • Status: FINAL
                </div>
              </div>
            ) : (
              <PermissionGate permission="Radiology.Report">
                <button
                  onClick={() => setIsReviewed(true)}
                  className="w-full py-3 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 text-white font-semibold text-sm shadow-lg shadow-emerald-600/20 transition flex items-center justify-center gap-2"
                >
                  <CheckCircle2 className="w-4 h-4" />
                  <span>Validate & Finalize PACS Report</span>
                </button>
              </PermissionGate>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
