'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { useLanguage } from '@/lib/i18n/LanguageContext';
import {
  LayoutDashboard,
  Users,
  CalendarDays,
  Sparkles,
  Building2,
  LogOut,
  ChevronRight,
  ShieldCheck,
  Stethoscope,
  Activity,
  Smile,
  TestTubes,
  ScanLine,
  Pill,
  ReceiptText,
  Bell,
  TrendingUp,
  ShieldAlert,
  FlaskConical,
  UserCheck
} from 'lucide-react';

export const Sidebar: React.FC = () => {
  const pathname = usePathname();
  const { user, logout, clinicId } = useAuth();
  const { t } = useLanguage();

  const NAV_GROUPS = [
    {
      title: t.clinicalPractice,
      items: [
        { href: '/dashboard', label: t.dashboard, icon: LayoutDashboard },
        { href: '/dashboard/patients', label: t.patients, icon: Users },
        { href: '/dashboard/visits', label: t.visits, icon: Stethoscope },
        { href: '/dashboard/appointments', label: t.appointments, icon: CalendarDays },
        { href: '/dashboard/canvas', label: t.canvas, icon: Activity },
        { href: '/dashboard/dental', label: t.dentalChart, icon: Smile },
      ]
    },
    {
      title: t.aiAndDiagnostics,
      items: [
        { href: '/dashboard/ai-assistant', label: t.aiAssistant, icon: Sparkles, badge: 'AI' },
        { href: '/dashboard/lab-analyzer', label: t.aiLabAnalyzer, icon: TestTubes, badge: 'Pipeline' },
        { href: '/dashboard/laboratory', label: t.labOrdersDesk, icon: FlaskConical },
        { href: '/dashboard/radiology', label: t.radiologyPACS, icon: ScanLine, badge: 'Vision' },
        { href: '/dashboard/prescriptions', label: t.prescriptions, icon: Pill },
      ]
    },
    {
      title: t.operationsAndManagement,
      items: [
        { href: '/dashboard/billing', label: t.billing, icon: ReceiptText },
        { href: '/dashboard/analytics', label: t.analytics, icon: TrendingUp },
        { href: '/dashboard/users', label: t.userAccounts, icon: UserCheck },
        { href: '/dashboard/staff', label: t.clinicStaff, icon: Users },
        { href: '/dashboard/audit-logs', label: t.auditLogs, icon: ShieldAlert },
        { href: '/dashboard/notifications', label: t.notifications, icon: Bell, badge: '3' },
      ]
    }
  ];

  return (
    <aside className="w-64 border-r border-slate-800/80 bg-slate-950/80 backdrop-blur-xl flex flex-col justify-between h-screen sticky top-0 shrink-0 select-none">
      <div className="flex flex-col gap-4 p-4 overflow-y-auto flex-1 pr-2">
        {/* Logo Header */}
        <Link href="/dashboard" className="flex items-center gap-3 px-2 pt-1">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-sky-500 to-indigo-600 flex items-center justify-center text-white shadow-lg shadow-sky-500/20 shrink-0">
            <Stethoscope className="w-6 h-6" />
          </div>
          <div>
            <div className="font-bold text-base tracking-wide text-white flex items-center gap-1.5">
              <span>MedClinic</span>
              <span className="text-xs px-1.5 py-0.5 rounded bg-sky-500/20 text-sky-400 font-mono">AI</span>
            </div>
            <div className="text-[11px] text-slate-400">Clinical Decision ERP</div>
          </div>
        </Link>

        {/* Tenant Clinic Badge */}
        <div className="glass-panel p-2.5 rounded-xl flex items-center justify-between border-slate-800 shrink-0">
          <div className="flex items-center gap-2 overflow-hidden">
            <Building2 className="w-4 h-4 text-sky-400 shrink-0" />
            <div className="truncate">
              <div className="text-xs font-semibold text-slate-200 truncate">
                {user?.clinicName || 'Al-Amal Medical Center'}
              </div>
              <div className="text-[10px] text-slate-400 font-mono">
                {clinicId ? `ID: ${clinicId.slice(0, 8)}...` : 'Default Clinic'}
              </div>
            </div>
          </div>
          <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse shrink-0" title="Tenant Connected" />
        </div>

        {/* Grouped Navigation Links */}
        <nav className="flex flex-col gap-5 pt-1">
          {NAV_GROUPS.map((group, gIdx) => (
            <div key={gIdx} className="space-y-1">
              <div className="px-3 text-[10px] font-bold tracking-wider text-slate-500 uppercase font-mono">
                {group.title}
              </div>

              <div className="space-y-0.5">
                {group.items.map((item) => {
                  const Icon = item.icon;
                  const isActive = pathname === item.href;

                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      className={`flex items-center justify-between px-3 py-2 rounded-xl text-xs font-medium transition-all ${
                        isActive
                          ? 'bg-sky-500/15 text-sky-400 border border-sky-500/30 shadow-sm'
                          : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
                      }`}
                    >
                      <div className="flex items-center gap-2.5 min-w-0">
                        <Icon className={`w-4 h-4 shrink-0 ${isActive ? 'text-sky-400' : 'text-slate-400'}`} />
                        <span className="truncate">{item.label}</span>
                      </div>

                      {item.badge && (
                        <span className="px-1.5 py-0.2 text-[9px] font-bold rounded-full bg-gradient-to-r from-sky-500 to-indigo-500 text-white shrink-0">
                          {item.badge}
                        </span>
                      )}
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>
      </div>

      {/* User Footer & Logout */}
      <div className="p-3.5 border-t border-slate-800/80 shrink-0">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2.5 overflow-hidden">
            <div className="w-8 h-8 rounded-lg bg-indigo-500/20 border border-indigo-500/30 flex items-center justify-center text-xs font-bold text-indigo-300 shrink-0">
              {user?.firstName ? user.firstName[0] : 'D'}
            </div>
            <div className="truncate">
              <div className="text-xs font-semibold text-slate-200 truncate">
                {user ? `${user.firstName} ${user.lastName}` : 'Dr. Sarah Al-Mansoor'}
              </div>
              <div className="text-[10px] text-slate-400">
                {user?.roles?.[0] || 'Doctor'}
              </div>
            </div>
          </div>

          <button
            onClick={logout}
            className="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition"
            title="Sign Out"
          >
            <LogOut className="w-4 h-4" />
          </button>
        </div>
      </div>
    </aside>
  );
};
