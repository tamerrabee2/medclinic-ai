'use client';

import React, { useState } from 'react';
import {
  ShieldAlert,
  Search,
  Filter,
  CheckCircle2,
  AlertTriangle,
  Clock,
  User,
  Download,
  Lock,
  FileCheck,
  Eye,
  Activity,
  Terminal
} from 'lucide-react';

interface AuditEntry {
  id: string;
  timestamp: string;
  user: string;
  role: string;
  action: string;
  resource: string;
  ipAddress: string;
  status: 'success' | 'warning' | 'denied';
  details: string;
}

const INITIAL_LOGS: AuditEntry[] = [
  {
    id: 'LOG-9921',
    timestamp: 'Today, 10:14 AM',
    user: 'Dr. Sarah Al-Mansoor',
    role: 'Attending Physician',
    action: 'AI_REPORT_APPROVED',
    resource: 'LAB-2026-0904 (Tariq Al-Mansoor)',
    ipAddress: '192.168.1.42',
    status: 'success',
    details: 'Validated lab biomarker differential and authorized commit to EMR.'
  },
  {
    id: 'LOG-9920',
    timestamp: 'Today, 09:48 AM',
    user: 'Dr. Sarah Al-Mansoor',
    role: 'Attending Physician',
    action: 'PRESCRIPTION_TRANSMITTED',
    resource: 'RX-2026-88192 (P-10024)',
    ipAddress: '192.168.1.42',
    status: 'success',
    details: 'Transmitted Metformin + Atorvastatin regimen; allergy safety check passed.'
  },
  {
    id: 'LOG-9919',
    timestamp: 'Today, 09:16 AM',
    user: 'Radiology PACS Worker',
    role: 'Automated AI Gateway',
    action: 'AI_IMAGE_ANALYZED',
    resource: 'SCAN-01 (Thoracic CXR)',
    ipAddress: '10.0.0.15',
    status: 'success',
    details: 'Inferred right lower lobe alveolar consolidation with 94.2% confidence.'
  },
  {
    id: 'LOG-9918',
    timestamp: 'Today, 08:30 AM',
    user: 'Mona Al-Ghamdi',
    role: 'Receptionist',
    action: 'PATIENT_RECORD_VIEWED',
    resource: 'P-10024 (Tariq Al-Mansoor)',
    ipAddress: '192.168.1.18',
    status: 'success',
    details: 'Accessed demographics for check-in and appointment verification.'
  },
  {
    id: 'LOG-9917',
    timestamp: 'Yesterday, 06:12 PM',
    user: 'Unknown / External',
    role: 'Guest Session',
    action: 'AUTH_FAILED',
    resource: '/api/v1/patients',
    ipAddress: '82.102.14.99',
    status: 'denied',
    details: 'Unauthorized token attempt blocked by ASP.NET Core JWT Middleware.'
  }
];

export default function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditEntry[]>(INITIAL_LOGS);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  const filtered = logs.filter((l) => {
    const matchesSearch =
      l.user.toLowerCase().includes(searchQuery.toLowerCase()) ||
      l.action.toLowerCase().includes(searchQuery.toLowerCase()) ||
      l.resource.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || l.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <span className="px-2 py-0.5 rounded text-[11px] font-bold bg-rose-500/20 text-rose-300 border border-rose-500/30">
              Module 35 (Security)
            </span>
            <span className="text-xs text-slate-400">HIPAA & Digital Health Audit Trail</span>
          </div>
          <h1 className="text-2xl md:text-3xl font-bold text-white mt-1 flex items-center gap-3">
            <ShieldAlert className="w-8 h-8 text-rose-400" />
            Security Audit Trail & Compliance Logging
          </h1>
          <p className="text-sm text-slate-400 mt-0.5">
            Cryptographic ledger tracking all clinical data accesses, AI inference requests, and patient modifications.
          </p>
        </div>

        <button
          onClick={() => window.print()}
          className="px-4 py-2.5 rounded-xl border border-slate-700 bg-slate-900 hover:bg-slate-800 text-xs font-semibold text-slate-200 flex items-center gap-2 transition"
        >
          <Download className="w-4 h-4 text-rose-400" />
          <span>Export Compliance Audit</span>
        </button>
      </div>

      {/* Security Telemetry Metric Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Audit Events Logged (24h)</div>
            <div className="text-2xl font-black text-white mt-1 font-mono">1,894</div>
            <div className="text-[11px] text-emerald-400 mt-1">100% Tamper-Resistant Storage</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-sky-500/10 border border-sky-500/30 flex items-center justify-center text-sky-400">
            <Activity className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Security Gate Rejections</div>
            <div className="text-2xl font-black text-rose-400 mt-1 font-mono">2</div>
            <div className="text-[11px] text-rose-300 mt-1">Zero unauthorized data breaches</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-rose-500/10 border border-rose-500/30 flex items-center justify-center text-rose-400">
            <Lock className="w-6 h-6" />
          </div>
        </div>

        <div className="glass-panel p-5 rounded-2xl border-slate-800 flex items-center justify-between">
          <div>
            <div className="text-xs font-semibold text-slate-400">Regulatory Compliance</div>
            <div className="text-2xl font-black text-emerald-400 mt-1 font-mono">Certified</div>
            <div className="text-[11px] text-emerald-300 mt-1">Saudi MoH & HIPAA Standards</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
            <FileCheck className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* Logs Table Card */}
      <div className="glass-panel p-5 rounded-2xl border-slate-800 space-y-4">
        {/* Controls */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="relative max-w-sm w-full">
            <Search className="w-4 h-4 absolute left-3 top-3 text-slate-500" />
            <input
              type="text"
              placeholder="Search by user, action, or resource..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-4 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-rose-500"
            />
          </div>

          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 text-xs">
            {[
              { id: 'all', label: 'All Events' },
              { id: 'success', label: 'Authorized' },
              { id: 'denied', label: 'Blocked / Denied' }
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setStatusFilter(tab.id)}
                className={`px-3 py-1 rounded-lg font-semibold transition ${
                  statusFilter === tab.id
                    ? 'bg-rose-500 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </div>

        {/* Logs Table */}
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead>
              <tr className="border-b border-slate-800 text-xs font-semibold text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Timestamp</th>
                <th className="py-3 px-3">Operator</th>
                <th className="py-3 px-3">Action Event</th>
                <th className="py-3 px-3">Resource Target</th>
                <th className="py-3 px-3">IP Address</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3">Audit Details</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60 text-xs">
              {filtered.map((log) => (
                <tr key={log.id} className="hover:bg-slate-900/40 transition">
                  <td className="py-3 px-3 font-mono text-slate-400 whitespace-nowrap">
                    {log.timestamp}
                  </td>

                  <td className="py-3 px-3">
                    <div className="font-semibold text-slate-200">{log.user}</div>
                    <div className="text-[10px] text-slate-500">{log.role}</div>
                  </td>

                  <td className="py-3 px-3 font-mono font-bold text-sky-400">
                    {log.action}
                  </td>

                  <td className="py-3 px-3 text-slate-300 font-mono text-[11px]">
                    {log.resource}
                  </td>

                  <td className="py-3 px-3 text-slate-500 font-mono">
                    {log.ipAddress}
                  </td>

                  <td className="py-3 px-3 whitespace-nowrap">
                    {log.status === 'success' && (
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
                        <CheckCircle2 className="w-3 h-3" /> ALLOWED
                      </span>
                    )}
                    {log.status === 'denied' && (
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-500/20 text-rose-400 border border-rose-500/40">
                        <Lock className="w-3 h-3" /> BLOCKED
                      </span>
                    )}
                  </td>

                  <td className="py-3 px-3 text-slate-400 max-w-xs truncate">
                    {log.details}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
