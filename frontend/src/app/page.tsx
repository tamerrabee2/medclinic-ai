'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { Stethoscope, Sparkles, ArrowRight, ShieldCheck } from 'lucide-react';
import Link from 'next/link';

export default function HomePage() {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      router.push('/dashboard');
    }
  }, [isAuthenticated, isLoading, router]);

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-6 relative overflow-hidden">
      {/* Background Decorative Glows */}
      <div className="absolute -top-40 -left-40 w-96 h-96 bg-sky-500/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -right-40 w-96 h-96 bg-indigo-500/20 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-4xl w-full text-center space-y-8 relative z-10">
        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full glass-panel border-sky-500/30 text-sky-400 text-sm font-medium animate-pulse">
          <Sparkles className="w-4 h-4 text-sky-400" />
          <span>Next-Gen Clinical AI Platform &middot; .NET 10 &amp; React 19</span>
        </div>

        <h1 className="text-5xl md:text-7xl font-extrabold tracking-tight bg-gradient-to-r from-sky-400 via-teal-300 to-indigo-400 bg-clip-text text-transparent">
          MedClinic AI
        </h1>

        <p className="text-lg md:text-xl text-slate-400 max-w-2xl mx-auto leading-relaxed">
          Integrated Intelligent Healthcare Management System featuring Clinical Decision Support, 
          Interactive Anatomical Canvas, 32-Teeth FDI Dental Chart, and Multi-Tenant Isolation.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 justify-center items-center pt-4">
          <Link
            href="/login"
            className="flex items-center gap-3 px-8 py-4 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-semibold text-lg shadow-lg shadow-sky-500/25 transition duration-200"
          >
            <span>Launch Clinical Portal</span>
            <ArrowRight className="w-5 h-5" />
          </Link>
          <Link
            href="/dashboard"
            className="flex items-center gap-2 px-8 py-4 rounded-xl glass-panel-interactive text-slate-200 font-semibold text-lg"
          >
            <Stethoscope className="w-5 h-5 text-sky-400" />
            <span>Open Dashboard</span>
          </Link>
        </div>

        {/* Highlight Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 pt-12 text-left">
          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-3">
            <div className="w-10 h-10 rounded-lg bg-sky-500/10 flex items-center justify-center text-sky-400 font-bold">
              AI
            </div>
            <h3 className="text-lg font-semibold text-slate-200">Doctor AI Copilot</h3>
            <p className="text-sm text-slate-400">
              Lab results diagnosis, radiology evaluation, and clinical discharge summaries.
            </p>
          </div>

          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-3">
            <div className="w-10 h-10 rounded-lg bg-indigo-500/10 flex items-center justify-center text-indigo-400 font-bold">
              26
            </div>
            <h3 className="text-lg font-semibold text-slate-200">Body Map &amp; Canvas</h3>
            <p className="text-sm text-slate-400">
              Interactive 26-region body mapper and image annotations with vector precision.
            </p>
          </div>

          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-3">
            <div className="w-10 h-10 rounded-lg bg-teal-500/10 flex items-center justify-center text-teal-400 font-bold">
              32
            </div>
            <h3 className="text-lg font-semibold text-slate-200">FDI Dental Matrix</h3>
            <p className="text-sm text-slate-400">
              Full adult arch tooth charting with 11 conditions, color coding, and treatment tracking.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
