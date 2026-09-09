'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import {
  Check,
  Sparkles,
  ShieldCheck,
  Zap,
  Building2,
  Stethoscope,
  ArrowRight,
  HelpCircle,
  Clock,
  Layers
} from 'lucide-react';

const TIERS = [
  {
    name: 'Solo Practice',
    badge: 'Independent Physician',
    priceMonthly: 89,
    priceAnnually: 71,
    desc: 'Perfect for private specialty clinics seeking an AI Clinical Copilot without large overhead.',
    features: [
      'Single Doctor License',
      'Unlimited Patient Records (EMR)',
      'Clinical AI Doctor Copilot (GPT-4o / Gemini)',
      'Standard Lab Analyzer & Biomarkers',
      'Electronic Prescriptions & Contraindications',
      'Medical Canvas & Body Map annotations',
      'Export PDF reports with clinic letterhead',
      'Standard Email & Chat Support'
    ],
    highlight: false,
    cta: 'Start 14-Day Free Trial',
    popular: false
  },
  {
    name: 'Polyclinic Pro',
    badge: 'Most Popular',
    priceMonthly: 249,
    priceAnnually: 199,
    desc: 'Designed for multi-specialty clinics requiring collaborative workflows, PACS imaging, and billing.',
    features: [
      'Up to 10 Doctor & Staff Accounts',
      'Everything in Solo Practice',
      'DICOM PACS Medical Imaging & AI Heatmap',
      '6-Phase AI Lab Diagnostic Pipeline',
      'FDI World Dental Chart Module',
      'Voice AI Clinical Scribe (Speech-to-SOAP)',
      'Insurance Claims & 15% VAT Invoicing',
      'Multi-tenancy Role-Based Access (RBAC)',
      'Priority 24/7 Clinical Support'
    ],
    highlight: true,
    cta: 'Get Started with Pro',
    popular: true
  },
  {
    name: 'Hospital & Enterprise',
    badge: 'Medical Centers & Networks',
    priceMonthly: 699,
    priceAnnually: 559,
    desc: 'Full-scale clinical ERP for medical hospitals and polyclinic chains needing custom LLM deployment.',
    features: [
      'Unlimited Doctors, Nurses & Technicians',
      'Everything in Polyclinic Pro',
      'Self-hosted Local AI (Ollama / vLLM On-Premises)',
      'External Reference Lab REST API Sync',
      'Custom HL7 / FHIR Clinical Gateway',
      'Full HIPAA/GDPR Immutable Security Audit Stream',
      'Dedicated Customer Success Manager',
      '99.99% SLA Uptime Guarantee'
    ],
    highlight: false,
    cta: 'Contact Medical Sales',
    popular: false
  }
];

export default function PricingPage() {
  const [annual, setAnnual] = useState(true);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col selection:bg-sky-500 selection:text-white">
      {/* Public Top Navbar */}
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
          <Link href="/pricing" className="text-sky-400">Pricing</Link>
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

      {/* Main Pricing Hero */}
      <main className="flex-1 max-w-7xl mx-auto px-6 py-16 w-full space-y-16">
        <div className="text-center space-y-4 max-w-3xl mx-auto">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-sky-500/10 border border-sky-500/20 text-sky-400 text-xs font-semibold">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Predictable Medical SaaS Pricing</span>
          </div>
          <h1 className="text-4xl sm:text-5xl font-extrabold tracking-tight text-white">
            Invest in Clinical Precision, Not Overhead.
          </h1>
          <p className="text-slate-400 text-base">
            Choose the plan tailored for your practice. Zero surprise charges, HIPAA-compliant encryption, and swappable clinical AI models.
          </p>

          {/* Billing Switcher Toggle */}
          <div className="pt-4 flex items-center justify-center gap-3">
            <span className={`text-xs font-semibold ${!annual ? 'text-white' : 'text-slate-400'}`}>Monthly Billing</span>
            <button
              onClick={() => setAnnual(!annual)}
              className="w-12 h-6 rounded-full bg-slate-800 p-1 border border-slate-700 transition relative"
            >
              <div
                className={`w-4 h-4 rounded-full bg-sky-400 transition transform ${
                  annual ? 'translate-x-6' : 'translate-x-0'
                }`}
              />
            </button>
            <div className="flex items-center gap-1.5">
              <span className={`text-xs font-semibold ${annual ? 'text-white' : 'text-slate-400'}`}>Annual Billing</span>
              <span className="px-2 py-0.5 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-[10px] font-bold">
                Save 20%
              </span>
            </div>
          </div>
        </div>

        {/* Pricing Cards Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 items-stretch">
          {TIERS.map((tier, idx) => (
            <div
              key={idx}
              className={`rounded-2xl p-8 flex flex-col justify-between transition-all relative ${
                tier.highlight
                  ? 'glass-panel bg-slate-900/90 border-2 border-sky-500 shadow-2xl shadow-sky-500/10 -translate-y-2'
                  : 'glass-panel bg-slate-950/60 border border-slate-800/80 hover:border-slate-700'
              }`}
            >
              {tier.popular && (
                <div className="absolute -top-3.5 left-1/2 -translate-x-1/2 px-3 py-1 rounded-full bg-gradient-to-r from-sky-500 to-indigo-500 text-slate-950 font-bold text-[10px] uppercase tracking-wider shadow-md">
                  Recommended for Clinics
                </div>
              )}

              <div className="space-y-6">
                <div>
                  <div className="text-xs font-mono text-sky-400 font-bold uppercase tracking-wider">{tier.badge}</div>
                  <h2 className="text-2xl font-bold text-white mt-1">{tier.name}</h2>
                  <p className="text-xs text-slate-400 mt-2 leading-relaxed">{tier.desc}</p>
                </div>

                <div className="flex items-baseline gap-1">
                  <span className="text-4xl font-extrabold text-white">
                    ${annual ? tier.priceAnnually : tier.priceMonthly}
                  </span>
                  <span className="text-slate-400 text-xs">/ doctor / month</span>
                </div>

                <div className="pt-4 border-t border-slate-800/60 space-y-3">
                  <div className="text-xs font-semibold text-slate-300">Included Capabilities:</div>
                  <ul className="space-y-2.5">
                    {tier.features.map((feat, fIdx) => (
                      <li key={fIdx} className="flex items-start gap-2.5 text-xs text-slate-300">
                        <Check className="w-4 h-4 text-emerald-400 shrink-0 mt-0.5" />
                        <span>{feat}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </div>

              <div className="pt-8">
                <Link
                  href="/register"
                  className={`w-full py-3 rounded-xl font-bold text-xs flex items-center justify-center gap-2 transition shadow-md ${
                    tier.highlight
                      ? 'bg-sky-500 hover:bg-sky-400 text-slate-950 shadow-sky-500/20'
                      : 'bg-slate-800 hover:bg-slate-700 text-white'
                  }`}
                >
                  <span>{tier.cta}</span>
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </div>
            </div>
          ))}
        </div>

        {/* Clinical Enterprise FAQ */}
        <div className="pt-12 border-t border-slate-800/80 max-w-4xl mx-auto space-y-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-white">Frequently Asked Clinical Questions</h2>
            <p className="text-xs text-slate-400 mt-1">Everything you need to know about compliance, billing, and AI safety.</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="p-5 rounded-xl bg-slate-900/50 border border-slate-800 space-y-2">
              <h3 className="text-sm font-semibold text-white flex items-center gap-2">
                <HelpCircle className="w-4 h-4 text-sky-400" />
                Are AI diagnoses approved automatically?
              </h3>
              <p className="text-xs text-slate-400 leading-relaxed">
                Never. MedClinic AI enforces a strict physician review gate. All AI differentials, biomarker summaries, and radiology heatmaps must be officially signed off by a licensed doctor.
              </p>
            </div>

            <div className="p-5 rounded-xl bg-slate-900/50 border border-slate-800 space-y-2">
              <h3 className="text-sm font-semibold text-white flex items-center gap-2">
                <ShieldCheck className="w-4 h-4 text-emerald-400" />
                Where is patient data stored?
              </h3>
              <p className="text-xs text-slate-400 leading-relaxed">
                Data is isolated within dedicated PostgreSQL schemas with AES-256 encryption. For Enterprise clients, we offer on-premises local deployment via Docker and local Ollama models.
              </p>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-slate-900 py-8 text-center text-xs text-slate-500">
        <p>© 2026 MedClinic AI ERP Inc. Class II Medical Decision Support System. All rights reserved.</p>
      </footer>
    </div>
  );
}
