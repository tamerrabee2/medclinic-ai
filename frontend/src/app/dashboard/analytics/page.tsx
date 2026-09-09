'use client';

import React, { useState } from 'react';
import {
  TrendingUp,
  Users,
  Calendar,
  DollarSign,
  Sparkles,
  TestTubes,
  Activity,
  ArrowUpRight,
  ArrowDownRight,
  Filter,
  Download,
  ShieldCheck,
  Building,
  CheckCircle2
} from 'lucide-react';

export default function AnalyticsPage() {
  const [timeRange, setTimeRange] = useState<'7d' | '30d' | '90d' | '1y'>('30d');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
              Module 66 (BI)
            </span>
            <span className="text-xs text-slate-400">Executive Intelligence</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <TrendingUp className="w-8 h-8 text-sky-400" />
            Clinical Analytics & Healthcare Intelligence
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Operational metrics, patient throughput, revenue cycle, and AI diagnostic acceptance rates.
          </p>
        </div>

        {/* Time Range Filter & Export */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
            {[
              { id: '7d', label: '7 Days' },
              { id: '30d', label: '30 Days' },
              { id: '90d', label: '90 Days' },
              { id: '1y', label: '1 Year' }
            ].map((t) => (
              <button
                key={t.id}
                onClick={() => setTimeRange(t.id as any)}
                className={`px-3 py-1.5 rounded-lg font-semibold transition ${
                  timeRange === t.id
                    ? 'bg-sky-500 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>

          <button
            onClick={() => window.print()}
            className="px-4 py-2.5 rounded-xl border border-slate-700 bg-slate-900 hover:bg-slate-800 text-xs font-semibold text-slate-200 flex items-center gap-2 transition"
          >
            <Download className="w-4 h-4 text-sky-400" />
            <span>Export Report</span>
          </button>
        </div>
      </div>

      {/* Top 4 Primary KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Total Patient Encounters</div>
            <div className="text-2xl font-black text-white mt-1 font-mono">1,482</div>
            <div className="text-[11px] text-emerald-400 flex items-center gap-1 mt-1 font-medium">
              <ArrowUpRight className="w-3.5 h-3.5" /> +18.4% vs last period
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
            <Users className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Gross Clinic Revenue</div>
            <div className="text-2xl font-black text-emerald-400 mt-1 font-mono">
              342,850 <span className="text-xs font-normal text-slate-400">SAR</span>
            </div>
            <div className="text-[11px] text-emerald-400 flex items-center gap-1 mt-1 font-medium">
              <ArrowUpRight className="w-3.5 h-3.5" /> +12.6% MTD
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
            <DollarSign className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Appointment Show Rate</div>
            <div className="text-2xl font-black text-indigo-400 mt-1 font-mono">92.4%</div>
            <div className="text-[11px] text-indigo-300 flex items-center gap-1 mt-1">
              7.6% Cancellation / No-Show
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-indigo-500/10 border border-indigo-500/30 flex items-center justify-center text-indigo-400">
            <Calendar className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">AI Diagnostic Doctor Sign-off</div>
            <div className="text-2xl font-black text-teal-400 mt-1 font-mono">96.8%</div>
            <div className="text-[11px] text-teal-300 flex items-center gap-1 mt-1">
              High physician concordance
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-teal-500/10 border border-teal-500/30 flex items-center justify-center text-teal-400">
            <Sparkles className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Visual Analytics Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left 8 Cols: Volume Trends & Projections */}
        <div className="lg:col-span-8 space-y-4">
          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-base font-bold text-white flex items-center gap-2">
                  <Activity className="w-4 h-4 text-sky-400" />
                  Weekly Patient Attendance & Capacity Distribution
                </h2>
                <p className="text-xs text-slate-400 mt-0.5">
                  Comparison between completed encounters, scheduled visits, and no-shows.
                </p>
              </div>
              <span className="text-xs font-mono px-2.5 py-1 rounded bg-slate-900 text-slate-400 border border-slate-800">
                Avg: 48 Visits / Day
              </span>
            </div>

            {/* Custom Bar Visualization */}
            <div className="h-64 flex items-end justify-between gap-4 pt-8 pb-4 border-b border-slate-800/80 select-none">
              {[
                { day: 'Sun', completed: 52, noshow: 4, height: '75%' },
                { day: 'Mon', completed: 64, noshow: 3, height: '92%' },
                { day: 'Tue', completed: 58, noshow: 5, height: '82%' },
                { day: 'Wed', completed: 61, noshow: 2, height: '88%' },
                { day: 'Thu', completed: 70, noshow: 4, height: '100%' },
                { day: 'Fri', completed: 22, noshow: 1, height: '32%' },
                { day: 'Sat', completed: 45, noshow: 3, height: '65%' }
              ].map((item) => (
                <div key={item.day} className="flex-1 flex flex-col items-center gap-2 h-full justify-end group">
                  <div className="w-full max-w-[48px] flex flex-col items-center justify-end h-full">
                    {/* Tooltip on hover */}
                    <div className="opacity-0 group-hover:opacity-100 transition text-[10px] font-mono text-sky-300 mb-1">
                      {item.completed} pts
                    </div>
                    {/* Bar graphic */}
                    <div
                      className="w-full rounded-xl bg-gradient-to-t from-sky-600 to-indigo-500 group-hover:from-sky-500 group-hover:to-indigo-400 transition-all shadow-md shadow-sky-500/10"
                      style={{ height: item.height }}
                    />
                  </div>
                  <span className="text-xs font-semibold text-slate-400 group-hover:text-white transition">
                    {item.day}
                  </span>
                </div>
              ))}
            </div>

            <div className="flex items-center justify-center gap-6 text-xs text-slate-400 pt-2">
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 rounded bg-indigo-500" />
                <span>Completed Consultations</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-3 h-3 rounded bg-slate-700" />
                <span>Operational Capacity Buffer</span>
              </div>
            </div>
          </div>
        </div>

        {/* Right 4 Cols: Top Clinical Diagnoses & Specialties */}
        <div className="lg:col-span-4 space-y-4">
          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
            <h2 className="text-base font-bold text-white flex items-center gap-2">
              <ShieldCheck className="w-4 h-4 text-emerald-400" />
              Prevalent Pathologies (ICD-10)
            </h2>

            <div className="space-y-3">
              {[
                { name: 'Type 2 Diabetes Mellitus', icd: 'E11.9', percentage: 28, count: 415, color: 'bg-amber-500' },
                { name: 'Essential Hypertension', icd: 'I10', percentage: 24, count: 356, color: 'bg-rose-500' },
                { name: 'Community-Acquired Pneumonia', icd: 'J18.9', percentage: 16, count: 238, color: 'bg-sky-500' },
                { name: 'Dental Caries & Pulpitis', icd: 'K02.9', percentage: 14, count: 207, color: 'bg-teal-500' },
                { name: 'Hyperlipidemia', icd: 'E78.5', percentage: 11, count: 163, color: 'bg-indigo-500' }
              ].map((item) => (
                <div key={item.icd} className="space-y-1 text-xs">
                  <div className="flex items-center justify-between">
                    <span className="font-semibold text-slate-200">{item.name}</span>
                    <span className="font-mono text-slate-400">{item.count} pts ({item.percentage}%)</span>
                  </div>
                  <div className="w-full h-2 rounded-full bg-slate-800 overflow-hidden">
                    <div
                      className={`h-full rounded-full ${item.color}`}
                      style={{ width: `${item.percentage * 2.5}%` }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-3">
            <div className="text-xs font-bold text-slate-300">Departmental Revenue Contribution</div>
            <div className="grid grid-cols-2 gap-2 text-xs">
              <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                <span className="text-[10px] text-slate-400 block">Internal Medicine</span>
                <span className="font-bold text-white font-mono">148,200 SAR</span>
              </div>
              <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                <span className="text-[10px] text-slate-400 block">Dental Center</span>
                <span className="font-bold text-white font-mono">96,400 SAR</span>
              </div>
              <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                <span className="text-[10px] text-slate-400 block">Laboratory Orders</span>
                <span className="font-bold text-white font-mono">54,100 SAR</span>
              </div>
              <div className="p-2.5 rounded-xl bg-slate-900 border border-slate-800">
                <span className="text-[10px] text-slate-400 block">Radiology PACS</span>
                <span className="font-bold text-white font-mono">44,150 SAR</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
