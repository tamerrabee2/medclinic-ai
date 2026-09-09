'use client';

import React from 'react';
import Link from 'next/link';
import {
  Sparkles,
  Stethoscope,
  TestTubes,
  ScanLine,
  Activity,
  Smile,
  Mic,
  Pill,
  ReceiptText,
  ShieldCheck,
  Zap,
  ArrowRight,
  BrainCircuit,
  Eye,
  CheckCircle2
} from 'lucide-react';

const FEATURES_DEEP_DIVE = [
  {
    icon: Sparkles,
    color: 'text-sky-400 bg-sky-500/10 border-sky-500/30',
    title: 'Clinical AI Doctor Copilot',
    subtitle: 'Zero-Cost Mock & Swappable LLMs',
    description: 'Empowers clinicians with real-time longitudinal patient briefs, ICD-10 suggestions, and conversational medical query resolution without sacrificing physician agency.',
    bullets: [
      'Multi-variable patient history synthesis',
      'Context-aware differential diagnosis suggestions',
      'Configurable API Gateway (Gemini 1.5, GPT-4o, and Mock Mode)'
    ]
  },
  {
    icon: TestTubes,
    color: 'text-amber-400 bg-amber-500/10 border-amber-500/30',
    title: 'AI Laboratory Diagnostic Pipeline',
    subtitle: '6-Phase Automated Processing',
    description: 'Converts raw PDF/Image lab reports into structured numerical biomarkers with reference range deviation badges and automated clinical differentials.',
    bullets: [
      'OCR document validation and numerical biomarker extraction',
      'Dynamic Critical / High / Low severity flagging',
      'Mandatory physician license sign-off gate'
    ]
  },
  {
    icon: ScanLine,
    color: 'text-indigo-400 bg-indigo-500/10 border-indigo-500/30',
    title: 'Radiology PACS & Medical Imaging',
    subtitle: 'Interactive DICOM Viewer with AI Heatmaps',
    description: 'Inspect Chest X-Rays, Brain MRIs, and Dental OPGs with zoom, rotation, brightness/contrast control, and AI bounding box lesion overlays.',
    bullets: [
      'Non-destructive visual canvas and measurement tools',
      'Structured ACR / BI-RADS clinical finding panels',
      'Class II medical device compliance disclaimers'
    ]
  },
  {
    icon: Activity,
    color: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/30',
    title: 'Interactive Body Map & Medical Canvas',
    subtitle: '2D SVG Anatomical Mapping',
    description: 'Physicians can map patient symptoms directly onto front and back interactive SVG anatomical regions with pain intensity indicators.',
    bullets: [
      'Vector anatomical zones with real-time symptom tagging',
      'Medical drawing tools (Pen, Brush, Arrow, Callouts)',
      'Layered annotation storage preserving original clinical imagery'
    ]
  },
  {
    icon: Smile,
    color: 'text-pink-400 bg-pink-500/10 border-pink-500/30',
    title: 'FDI World Dental Chart Module',
    subtitle: 'Standardized Odontogram',
    description: 'Full adult (11-48) and pediatric dental notation enabling dentists to record caries, restorations, endodontic treatments, and crowns with one click.',
    bullets: [
      'FDI Two-Digit numbering system',
      'Color-coded tooth condition history',
      'Treatment plan cost projection linked to billing'
    ]
  },
  {
    icon: Mic,
    color: 'text-purple-400 bg-purple-500/10 border-purple-500/30',
    title: 'Voice AI Clinical Scribe',
    subtitle: 'Speech-to-SOAP Automation',
    description: 'Listen to patient consultations and automatically format spoken dialogue into structured Subjective, Objective, Assessment, and Plan (SOAP) clinical records.',
    bullets: [
      'Bilingual Arabic (Egyptian/Gulf) and English voice recognition',
      'Automatic ICD-10 and prescription extraction',
      'Eliminates up to 2 hours of clinical documentation per day'
    ]
  }
];

export default function FeaturesPage() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col selection:bg-sky-500 selection:text-white">
      {/* Navbar */}
      <header className="h-16 border-b border-slate-800/80 bg-slate-950/80 backdrop-blur-md px-6 sm:px-12 flex items-center justify-between sticky top-0 z-40">
        <Link href="/" className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-sky-500 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-sky-500/20">
            <Stethoscope className="w-5 h-5" />
          </div>
          <span className="font-bold text-lg text-white">
            MedClinic <span className="text-sky-400 font-mono text-xs px-1.5 py-0.5 bg-sky-500/10 rounded">AI</span>
          </span>
        </Link>

        <nav className="hidden md:flex items-center gap-6 text-sm text-slate-300 font-medium">
          <Link href="/features" className="text-sky-400">Features</Link>
          <Link href="/pricing" className="hover:text-white transition">Pricing</Link>
          <Link href="/security" className="hover:text-white transition">Security & Trust</Link>
        </nav>

        <div className="flex items-center gap-3">
          <Link
            href="/login"
            className="px-4 py-2 rounded-xl text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-900 transition"
          >
            Sign In
          </Link>
          <Link
            href="/register"
            className="px-4 py-2 rounded-xl bg-sky-500 hover:bg-sky-400 text-slate-950 font-bold text-xs transition shadow-lg shadow-sky-500/20"
          >
            Get Started
          </Link>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 max-w-7xl mx-auto px-6 py-16 w-full space-y-16">
        <div className="text-center space-y-4 max-w-3xl mx-auto">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-sky-500/10 border border-sky-500/20 text-sky-400 text-xs font-semibold">
            <BrainCircuit className="w-3.5 h-3.5" />
            <span>Architecture & Feature Matrix</span>
          </div>
          <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight text-white">
            Crafted for Clinicians. Powered by Intelligence.
          </h1>
          <p className="text-slate-400 text-base">
            MedClinic AI is not another generic hospital ERP. It is an intelligent clinical workspace engineered to elevate diagnostic speed and eliminate administrative fatigue.
          </p>
        </div>

        {/* Feature Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {FEATURES_DEEP_DIVE.map((f, idx) => {
            const Icon = f.icon;
            return (
              <div
                key={idx}
                className="glass-panel p-6 rounded-2xl bg-slate-900/60 border border-slate-800/80 hover:border-slate-700 transition flex flex-col justify-between space-y-6"
              >
                <div className="space-y-4">
                  <div className={`w-12 h-12 rounded-xl border flex items-center justify-center ${f.color}`}>
                    <Icon className="w-6 h-6" />
                  </div>
                  <div>
                    <div className="text-[10px] font-mono text-sky-400 uppercase font-bold tracking-wider">{f.subtitle}</div>
                    <h2 className="text-xl font-bold text-white mt-1">{f.title}</h2>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">{f.description}</p>
                  <ul className="space-y-2 pt-2 border-t border-slate-800/60">
                    {f.bullets.map((b, bIdx) => (
                      <li key={bIdx} className="flex items-start gap-2 text-xs text-slate-300">
                        <CheckCircle2 className="w-3.5 h-3.5 text-emerald-400 shrink-0 mt-0.5" />
                        <span>{b}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="pt-2">
                  <Link
                    href="/register"
                    className="text-xs font-semibold text-sky-400 hover:text-sky-300 flex items-center gap-1.5 transition"
                  >
                    <span>Try in Sandbox</span>
                    <ArrowRight className="w-3.5 h-3.5" />
                  </Link>
                </div>
              </div>
            );
          })}
        </div>

        {/* Bottom CTA Banner */}
        <div className="p-8 sm:p-12 rounded-3xl bg-gradient-to-r from-sky-950/60 to-indigo-950/60 border border-sky-500/30 text-center space-y-4 shadow-2xl">
          <h2 className="text-2xl sm:text-3xl font-extrabold text-white">Experience the AI Clinical Workspace Today</h2>
          <p className="text-slate-400 text-xs sm:text-sm max-w-xl mx-auto">
            Get instant access to live demo data, realistic AI copilot simulations, and DICOM imaging tools with zero API cost.
          </p>
          <div className="pt-2">
            <Link
              href="/register"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-sky-500 hover:bg-sky-400 text-slate-950 font-bold text-xs transition shadow-lg shadow-sky-500/20"
            >
              <span>Create Free Clinic Workspace</span>
              <ArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-900 py-8 text-center text-xs text-slate-500">
        <p>© 2026 MedClinic AI ERP Inc. Designed for Modern Healthcare Practices.</p>
      </footer>
    </div>
  );
}
