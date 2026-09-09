'use client';

import React from 'react';
import Link from 'next/link';
import {
  Users,
  CalendarCheck,
  Stethoscope,
  Sparkles,
  TrendingUp,
  Clock,
  ArrowUpRight,
  Activity,
  Plus,
  Smile
} from 'lucide-react';

export default function DashboardOverviewPage() {
  const stats = [
    { label: "Today's Appointments", value: '24', change: '+12% from yesterday', icon: CalendarCheck, color: 'text-sky-400', bg: 'bg-sky-500/10' },
    { label: 'Active Patients', value: '1,420', change: '+38 this month', icon: Users, color: 'text-indigo-400', bg: 'bg-indigo-500/10' },
    { label: 'Completed Consultations', value: '18', change: '75% completion rate', icon: Stethoscope, color: 'text-emerald-400', bg: 'bg-emerald-500/10' },
    { label: 'AI Diagnostic Queries', value: '156', change: '99.4% accuracy rating', icon: Sparkles, color: 'text-cyan-400', bg: 'bg-cyan-500/10' },
  ];

  const queue = [
    { id: '1', patient: 'Omar Al-Husseini', time: '10:00 AM', doctor: 'Dr. Sarah Al-Mansoor', type: 'Follow-up', status: 'In Consultation', color: 'bg-amber-500/15 text-amber-300 border-amber-500/30' },
    { id: '2', patient: 'Nour El-Din Mostafa', time: '10:30 AM', doctor: 'Dr. Sarah Al-Mansoor', type: 'Hypertension Review', status: 'Waiting', color: 'bg-sky-500/15 text-sky-300 border-sky-500/30' },
    { id: '3', patient: 'Laila Mahmoud', time: '11:00 AM', doctor: 'Dr. Tariq Ziyad', type: 'Dental Examination', status: 'Confirmed', color: 'bg-slate-700/50 text-slate-300 border-slate-600' },
    { id: '4', patient: 'Khaled bin Walid', time: '09:15 AM', doctor: 'Dr. Sarah Al-Mansoor', type: 'Lab Evaluation', status: 'Completed', color: 'bg-emerald-500/15 text-emerald-300 border-emerald-500/30' },
  ];

  return (
    <div className="space-y-8">
      {/* Top Welcome Bar */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl md:text-3xl font-bold tracking-tight text-white flex items-center gap-2">
            <span>Clinical Workspace</span>
            <span className="text-xs px-2 py-0.5 rounded-full bg-emerald-500/20 text-emerald-400 font-semibold border border-emerald-500/30">
              Live .NET 10 ERP
            </span>
          </h1>
          <p className="text-sm text-slate-400 mt-1">
            Welcome back, Dr. Sarah. You have 6 scheduled consultations remaining today.
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Link
            href="/dashboard/ai-assistant"
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-sm shadow-md shadow-sky-500/20 transition"
          >
            <Sparkles className="w-4 h-4" />
            <span>AI Clinical Assistant</span>
          </Link>
          <Link
            href="/dashboard/appointments"
            className="flex items-center gap-2 px-4 py-2.5 rounded-xl glass-panel-interactive text-slate-200 font-medium text-sm"
          >
            <Plus className="w-4 h-4 text-sky-400" />
            <span>Book Appointment</span>
          </Link>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, idx) => {
          const Icon = stat.icon;
          return (
            <div key={idx} className="glass-panel p-5 rounded-2xl border-slate-800/80 space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">{stat.label}</span>
                <div className={`p-2 rounded-xl ${stat.bg} ${stat.color}`}>
                  <Icon className="w-5 h-5" />
                </div>
              </div>
              <div>
                <div className="text-3xl font-extrabold text-white tracking-tight">{stat.value}</div>
                <div className="text-xs text-slate-400 flex items-center gap-1 mt-1">
                  <TrendingUp className="w-3.5 h-3.5 text-emerald-400" />
                  <span>{stat.change}</span>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Main Grid: Queue & Fast Launch Panels */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left 2 Cols: Queue */}
        <div className="lg:col-span-2 glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Clock className="w-5 h-5 text-sky-400" />
              <h2 className="font-bold text-lg text-white">Today's Patient Queue</h2>
            </div>
            <Link href="/dashboard/appointments" className="text-xs font-semibold text-sky-400 hover:text-sky-300 flex items-center gap-1">
              <span>View All</span>
              <ArrowUpRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          <div className="divide-y divide-slate-800/60 overflow-hidden">
            {queue.map((item) => (
              <div key={item.id} className="py-3.5 flex items-center justify-between gap-4 hover:bg-slate-900/30 px-2 rounded-lg transition">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-slate-800/80 flex items-center justify-center font-bold text-slate-300 text-sm">
                    {item.patient[0]}
                  </div>
                  <div>
                    <div className="font-semibold text-slate-100 text-sm">{item.patient}</div>
                    <div className="text-xs text-slate-400 flex items-center gap-2">
                      <span>{item.type}</span>
                      <span>&bull;</span>
                      <span>{item.doctor}</span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <span className="text-xs font-mono text-slate-400">{item.time}</span>
                  <span className={`text-xs px-2.5 py-1 rounded-full font-medium border ${item.color}`}>
                    {item.status}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Right Col: Quick Clinical Launch Tools */}
        <div className="space-y-4">
          <div className="glass-panel p-6 rounded-2xl border-slate-800 space-y-4">
            <h2 className="font-bold text-lg text-white flex items-center gap-2">
              <Activity className="w-5 h-5 text-indigo-400" />
              <span>Specialized Workstations</span>
            </h2>

            <div className="space-y-3">
              <Link
                href="/dashboard/canvas"
                className="block p-4 rounded-xl bg-slate-900/60 hover:bg-slate-900 border border-slate-800 hover:border-sky-500/40 transition group"
              >
                <div className="flex items-center justify-between mb-1">
                  <div className="font-semibold text-sm text-slate-200 group-hover:text-sky-300 transition">
                    Interactive Anatomical Canvas
                  </div>
                  <span className="text-[11px] px-2 py-0.5 rounded bg-sky-500/20 text-sky-400">26 Regions</span>
                </div>
                <p className="text-xs text-slate-400">
                  Visual body mapping, pain location markers, and radiology annotations.
                </p>
              </Link>

              <Link
                href="/dashboard/dental"
                className="block p-4 rounded-xl bg-slate-900/60 hover:bg-slate-900 border border-slate-800 hover:border-indigo-500/40 transition group"
              >
                <div className="flex items-center justify-between mb-1">
                  <div className="font-semibold text-sm text-slate-200 group-hover:text-indigo-300 transition flex items-center gap-1.5">
                    <Smile className="w-4 h-4 text-indigo-400" />
                    <span>FDI World Dental Chart</span>
                  </div>
                  <span className="text-[11px] px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-400">32 Teeth</span>
                </div>
                <p className="text-xs text-slate-400">
                  Adult anatomical arches with 11 conditions, color coding, and caries tracking.
                </p>
              </Link>

              <Link
                href="/dashboard/ai-assistant"
                className="block p-4 rounded-xl bg-slate-900/60 hover:bg-slate-900 border border-slate-800 hover:border-cyan-500/40 transition group"
              >
                <div className="flex items-center justify-between mb-1">
                  <div className="font-semibold text-sm text-slate-200 group-hover:text-cyan-300 transition flex items-center gap-1.5">
                    <Sparkles className="w-4 h-4 text-cyan-400" />
                    <span>Doctor AI Copilot</span>
                  </div>
                  <span className="text-[11px] px-2 py-0.5 rounded bg-cyan-500/20 text-cyan-400">Multi-Model</span>
                </div>
                <p className="text-xs text-slate-400">
                  Ask AI about lab findings, generate clinical letters, and review symptoms.
                </p>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
