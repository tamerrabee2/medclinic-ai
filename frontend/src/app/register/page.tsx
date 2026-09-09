'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import {
  Stethoscope,
  Building2,
  Mail,
  Lock,
  User,
  Phone,
  ArrowRight,
  CheckCircle2,
  Sparkles,
  ShieldCheck
} from 'lucide-react';

export default function RegisterPage() {
  const router = useRouter();
  const [step, setStep] = useState<1 | 2>(1);
  const [formData, setFormData] = useState({
    clinicName: '',
    specialty: 'Multi-Specialty Polyclinic',
    country: 'Egypt',
    phone: '',
    adminName: '',
    adminEmail: '',
    adminPassword: '',
    selectedPlan: 'polyclinic-pro'
  });
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (step === 1) {
      setStep(2);
      return;
    }

    setLoading(true);
    // Simulate clinic provisioning & tenant creation
    setTimeout(() => {
      setLoading(false);
      setSuccess(true);
      setTimeout(() => {
        router.push('/dashboard');
      }, 1500);
    }, 1200);
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col justify-between selection:bg-sky-500 selection:text-white">
      {/* Top Navbar */}
      <header className="h-16 border-b border-slate-800/80 bg-slate-950/80 backdrop-blur-md px-6 sm:px-12 flex items-center justify-between">
        <Link href="/" className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-sky-500 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-sky-500/20">
            <Stethoscope className="w-5 h-5" />
          </div>
          <span className="font-bold text-lg text-white">
            MedClinic <span className="text-sky-400 font-mono text-xs px-1.5 py-0.5 bg-sky-500/10 rounded">AI</span>
          </span>
        </Link>

        <div className="text-xs text-slate-400">
          Already have an account?{' '}
          <Link href="/login" className="text-sky-400 hover:text-sky-300 font-semibold transition">
            Sign In
          </Link>
        </div>
      </header>

      {/* Main Form Container */}
      <main className="flex-1 flex items-center justify-center p-6">
        <div className="max-w-lg w-full glass-panel bg-slate-900/70 border border-slate-800 rounded-3xl p-8 sm:p-10 shadow-2xl relative space-y-8">
          {success ? (
            <div className="text-center py-8 space-y-4">
              <div className="w-16 h-16 rounded-full bg-emerald-500/20 border border-emerald-500/40 text-emerald-400 mx-auto flex items-center justify-center animate-bounce">
                <CheckCircle2 className="w-8 h-8" />
              </div>
              <h2 className="text-2xl font-bold text-white">Clinic Workspace Provisioned!</h2>
              <p className="text-xs text-slate-400">
                Setting up tenant database boundaries and preparing your AI Clinical Copilot...
              </p>
              <div className="w-8 h-8 border-2 border-sky-500 border-t-transparent rounded-full animate-spin mx-auto pt-4" />
            </div>
          ) : (
            <>
              {/* Stepper Header */}
              <div className="space-y-3 text-center">
                <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-sky-500/10 border border-sky-500/20 text-sky-400 text-xs font-semibold">
                  <Sparkles className="w-3.5 h-3.5" />
                  <span>Step {step} of 2: {step === 1 ? 'Clinic Details' : 'Admin Credentials'}</span>
                </div>
                <h1 className="text-2xl sm:text-3xl font-extrabold text-white">Register Your Medical Clinic</h1>
                <p className="text-xs text-slate-400">
                  Instant multi-tenant isolation, automated HIPAA audit stream, and AI clinical assistant.
                </p>
              </div>

              {/* Form */}
              <form onSubmit={handleSubmit} className="space-y-4">
                {step === 1 ? (
                  <>
                    <div className="space-y-1.5">
                      <label className="text-xs font-medium text-slate-300">Clinic / Hospital Name</label>
                      <div className="relative">
                        <Building2 className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                        <input
                          type="text"
                          required
                          value={formData.clinicName}
                          onChange={(e) => setFormData({ ...formData, clinicName: e.target.value })}
                          placeholder="e.g. Al-Amal Specialized Medical Center"
                          className="w-full pl-9 pr-4 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 placeholder-slate-600 focus:outline-none focus:border-sky-500 transition"
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-3">
                      <div className="space-y-1.5">
                        <label className="text-xs font-medium text-slate-300">Specialty</label>
                        <select
                          value={formData.specialty}
                          onChange={(e) => setFormData({ ...formData, specialty: e.target.value })}
                          className="w-full px-3 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 focus:outline-none focus:border-sky-500 transition"
                        >
                          <option value="Multi-Specialty Polyclinic">Multi-Specialty Polyclinic</option>
                          <option value="Cardiology">Cardiology</option>
                          <option value="Internal Medicine">Internal Medicine</option>
                          <option value="Dental Surgery">Dental Surgery</option>
                          <option value="Radiology & Imaging">Radiology & Imaging</option>
                          <option value="Pediatrics">Pediatrics</option>
                        </select>
                      </div>

                      <div className="space-y-1.5">
                        <label className="text-xs font-medium text-slate-300">Country</label>
                        <select
                          value={formData.country}
                          onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                          className="w-full px-3 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 focus:outline-none focus:border-sky-500 transition"
                        >
                          <option value="Egypt">Egypt 🇪🇬</option>
                          <option value="Saudi Arabia">Saudi Arabia 🇸🇦</option>
                          <option value="UAE">United Arab Emirates 🇦🇪</option>
                          <option value="Jordan">Jordan 🇯🇴</option>
                          <option value="Kuwait">Kuwait 🇰🇼</option>
                        </select>
                      </div>
                    </div>

                    <div className="space-y-1.5">
                      <label className="text-xs font-medium text-slate-300">Official Phone Number</label>
                      <div className="relative">
                        <Phone className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                        <input
                          type="tel"
                          required
                          value={formData.phone}
                          onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                          placeholder="+20 100 123 4567"
                          className="w-full pl-9 pr-4 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 placeholder-slate-600 focus:outline-none focus:border-sky-500 transition"
                        />
                      </div>
                    </div>
                  </>
                ) : (
                  <>
                    <div className="space-y-1.5">
                      <label className="text-xs font-medium text-slate-300">Medical Director / Admin Full Name</label>
                      <div className="relative">
                        <User className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                        <input
                          type="text"
                          required
                          value={formData.adminName}
                          onChange={(e) => setFormData({ ...formData, adminName: e.target.value })}
                          placeholder="Dr. Ahmed Hassan"
                          className="w-full pl-9 pr-4 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 placeholder-slate-600 focus:outline-none focus:border-sky-500 transition"
                        />
                      </div>
                    </div>

                    <div className="space-y-1.5">
                      <label className="text-xs font-medium text-slate-300">Administrator Work Email</label>
                      <div className="relative">
                        <Mail className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                        <input
                          type="email"
                          required
                          value={formData.adminEmail}
                          onChange={(e) => setFormData({ ...formData, adminEmail: e.target.value })}
                          placeholder="ahmed.hassan@alamal-clinic.com"
                          className="w-full pl-9 pr-4 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 placeholder-slate-600 focus:outline-none focus:border-sky-500 transition"
                        />
                      </div>
                    </div>

                    <div className="space-y-1.5">
                      <label className="text-xs font-medium text-slate-300">Account Password</label>
                      <div className="relative">
                        <Lock className="w-4 h-4 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
                        <input
                          type="password"
                          required
                          minLength={8}
                          value={formData.adminPassword}
                          onChange={(e) => setFormData({ ...formData, adminPassword: e.target.value })}
                          placeholder="••••••••••••"
                          className="w-full pl-9 pr-4 py-2 bg-slate-950/80 border border-slate-800 rounded-xl text-xs text-slate-200 placeholder-slate-600 focus:outline-none focus:border-sky-500 transition"
                        />
                      </div>
                    </div>
                  </>
                )}

                <div className="pt-4 flex items-center gap-3">
                  {step === 2 && (
                    <button
                      type="button"
                      onClick={() => setStep(1)}
                      className="w-1/3 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-bold text-slate-300 transition"
                    >
                      Back
                    </button>
                  )}
                  <button
                    type="submit"
                    disabled={loading}
                    className="flex-1 py-2.5 rounded-xl bg-sky-500 hover:bg-sky-400 text-slate-950 font-bold text-xs transition shadow-lg shadow-sky-500/20 flex items-center justify-center gap-2 cursor-pointer"
                  >
                    <span>{step === 1 ? 'Continue to Admin Credentials' : loading ? 'Provisioning...' : 'Complete Registration'}</span>
                    <ArrowRight className="w-4 h-4" />
                  </button>
                </div>
              </form>
            </>
          )}
        </div>
      </main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-slate-500 border-t border-slate-900">
        <p>© 2026 MedClinic AI. All medical records protected by multi-tenant encryption.</p>
      </footer>
    </div>
  );
}
