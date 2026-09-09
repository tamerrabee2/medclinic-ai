'use client';

import React from 'react';
import Link from 'next/link';
import {
  ShieldCheck,
  Lock,
  Server,
  FileCheck2,
  KeyRound,
  Stethoscope,
  ArrowRight,
  EyeOff,
  Database,
  CheckCircle2,
  AlertTriangle
} from 'lucide-react';

const SECURITY_PILLARS = [
  {
    icon: Lock,
    title: 'HIPAA & GDPR Compliance by Design',
    desc: 'All Protected Health Information (PHI) is encrypted at rest using AES-256 and in transit using TLS 1.3 with automated certificate renewal.'
  },
  {
    icon: Database,
    title: 'Strict Multi-Tenant Isolation',
    desc: 'Each clinic functions as an independent tenant. The TenantContext and X-Clinic-Id headers enforce database boundary isolation at the EF Core query layer.'
  },
  {
    icon: FileCheck2,
    title: 'Immutable Security Audit Trail',
    desc: 'Every access, view, modification, AI inference, and prescription event is recorded with cryptographic timestamps, actor ID, and IP address for compliance audits.'
  },
  {
    icon: KeyRound,
    title: 'Role-Based Access Control (RBAC)',
    desc: '8 distinct clinical tiers (SuperAdmin, ClinicAdmin, Doctor, Nurse, Radiologist, LabTechnician, Receptionist, Accountant) prevent unauthorized privilege escalation.'
  },
  {
    icon: EyeOff,
    title: 'Zero-Leakage AI Guardrails',
    desc: 'Patient identification metadata (National ID, Phone, Address) is strictly de-identified before dispatch to external LLM providers, with zero data retention policies.'
  },
  {
    icon: Server,
    title: 'On-Premises & Local Model Support',
    desc: 'Hospitals with strict data sovereignty regulations can deploy MedClinic AI completely air-gapped on local servers utilizing self-hosted Ollama AI models.'
  }
];

export default function SecurityPage() {
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
          <Link href="/features" className="hover:text-white transition">Features</Link>
          <Link href="/pricing" className="hover:text-white transition">Pricing</Link>
          <Link href="/security" className="text-sky-400">Security & Trust</Link>
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

      {/* Main Hero */}
      <main className="flex-1 max-w-6xl mx-auto px-6 py-16 w-full space-y-16">
        <div className="text-center space-y-4 max-w-3xl mx-auto">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-semibold">
            <ShieldCheck className="w-3.5 h-3.5" />
            <span>Enterprise Security & Trust Center</span>
          </div>
          <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight text-white">
            Uncompromising Security for Sensitive Medical Data.
          </h1>
          <p className="text-slate-400 text-base">
            Healthcare compliance is built into our core Clean Architecture. Learn how we safeguard electronic health records, diagnostic imaging, and clinical AI interactions.
          </p>
        </div>

        {/* Security Pillars */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {SECURITY_PILLARS.map((p, idx) => {
            const Icon = p.icon;
            return (
              <div
                key={idx}
                className="glass-panel p-6 rounded-2xl bg-slate-900/60 border border-slate-800/80 space-y-3"
              >
                <div className="w-10 h-10 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
                  <Icon className="w-5 h-5" />
                </div>
                <h2 className="text-base font-bold text-white">{p.title}</h2>
                <p className="text-xs text-slate-400 leading-relaxed">{p.desc}</p>
              </div>
            );
          })}
        </div>

        {/* Medical Ethics & Safety Guarantee */}
        <div className="p-8 rounded-2xl bg-slate-900/40 border border-slate-800 space-y-4">
          <div className="flex items-center gap-2 text-amber-400 font-bold text-sm">
            <AlertTriangle className="w-4 h-4" />
            <span>Class II Medical Decision Support & AI Ethics Statement</span>
          </div>
          <p className="text-xs text-slate-300 leading-relaxed">
            MedClinic AI operates under the ethical guidance of non-autonomous clinical decision support. The platform is designed to assist licensed healthcare practitioners and does not replace professional clinical judgment. Diagnostic recommendations, contraindication warnings, and radiology heatmaps serve as advisory references and require direct human validation before incorporation into permanent health records.
          </p>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-900 py-8 text-center text-xs text-slate-500">
        <p>© 2026 MedClinic AI ERP Inc. SOC-2 & HIPAA Compliant Architecture.</p>
      </footer>
    </div>
  );
}
