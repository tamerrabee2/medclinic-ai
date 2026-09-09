'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Search,
  User,
  Calendar,
  FileText,
  Activity,
  Smile,
  Sparkles,
  X,
  CreditCard,
  Microscope,
  ImageIcon,
  Stethoscope,
  TrendingUp,
  ShieldAlert,
  Users,
  FlaskConical,
  Bell,
  UserCheck
} from 'lucide-react';

interface SearchResult {
  title: string;
  category: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
}

interface CommandPaletteProps {
  isOpen: boolean;
  onClose: () => void;
}

export const CommandPalette: React.FC<CommandPaletteProps> = ({
  isOpen,
  onClose,
}) => {
  const [query, setQuery] = useState('');
  const router = useRouter();

  const allItems: SearchResult[] = [
    { title: 'Clinical Dashboard', category: 'General', href: '/dashboard', icon: Activity },
    { title: 'Patients Registry', category: 'EMR', href: '/dashboard/patients', icon: User },
    { title: 'Visits & Encounters (SOAP)', category: 'EMR', href: '/dashboard/visits', icon: Stethoscope },
    { title: 'Schedule & Calendar', category: 'Appointments', href: '/dashboard/appointments', icon: Calendar },
    { title: 'Interactive Body Map Canvas', category: 'Medical Canvas', href: '/dashboard/canvas', icon: Activity },
    { title: 'FDI World Dental Chart', category: 'Dentistry', href: '/dashboard/dental', icon: Smile },
    { title: 'AI Clinical Copilot', category: 'AI Tools', href: '/dashboard/ai-assistant', icon: Sparkles },
    { title: 'AI Laboratory Analyzer', category: 'Lab & Diagnostics', href: '/dashboard/lab-analyzer', icon: Microscope },
    { title: 'Laboratory Orders Desk', category: 'Lab & Diagnostics', href: '/dashboard/laboratory', icon: FlaskConical },
    { title: 'Radiology & Medical Imaging Viewer', category: 'Radiology', href: '/dashboard/radiology', icon: ImageIcon },
    { title: 'Prescriptions Builder', category: 'Pharmacy', href: '/dashboard/prescriptions', icon: FileText },
    { title: 'Billing & Invoices', category: 'Finance', href: '/dashboard/billing', icon: CreditCard },
    { title: 'Executive BI & Clinical Analytics', category: 'Analytics', href: '/dashboard/analytics', icon: TrendingUp },
    { title: 'User Accounts & IAM (إدارة المستخدمين)', category: 'Administration', href: '/dashboard/users', icon: UserCheck },
    { title: 'Clinic Staff & RBAC Management', category: 'Administration', href: '/dashboard/staff', icon: Users },
    { title: 'Security Audit Trail & Compliance', category: 'Security', href: '/dashboard/audit-logs', icon: ShieldAlert },
    { title: 'Alerts & Notifications Center', category: 'Alerts', href: '/dashboard/notifications', icon: Bell },
  ];

  const filtered = query.trim()
    ? allItems.filter((i) =>
        i.title.toLowerCase().includes(query.toLowerCase()) ||
        i.category.toLowerCase().includes(query.toLowerCase())
      )
    : allItems;

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        isOpen ? onClose() : null;
      }
      if (e.key === 'Escape' && isOpen) {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-20 p-4 bg-slate-950/80 backdrop-blur-md animate-in fade-in duration-150">
      <div className="glass-panel w-full max-w-2xl rounded-2xl border-slate-700 shadow-2xl overflow-hidden">
        {/* Search Input Bar */}
        <div className="flex items-center px-4 border-b border-slate-800">
          <Search className="w-5 h-5 text-sky-400 shrink-0" />
          <input
            type="text"
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search patients, doctors, appointments, lab results, radiology, prescriptions (Ctrl + K)..."
            className="w-full px-4 py-3.5 bg-transparent text-sm text-white placeholder-slate-500 focus:outline-none"
          />
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-slate-400 hover:text-white"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Results List */}
        <div className="max-h-80 overflow-y-auto p-2 divide-y divide-slate-800/40">
          {filtered.length === 0 ? (
            <div className="p-8 text-center text-sm text-slate-500">
              No matching clinical records or tools found for "{query}".
            </div>
          ) : (
            filtered.map((item, idx) => {
              const Icon = item.icon;
              return (
                <div
                  key={idx}
                  onClick={() => {
                    router.push(item.href);
                    onClose();
                  }}
                  className="flex items-center justify-between p-3 rounded-xl hover:bg-slate-900/60 cursor-pointer transition group"
                >
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-lg bg-slate-800/80 group-hover:bg-sky-500/20 group-hover:text-sky-400 text-slate-400 transition">
                      <Icon className="w-4 h-4" />
                    </div>
                    <div>
                      <div className="text-sm font-semibold text-slate-200 group-hover:text-white">
                        {item.title}
                      </div>
                      <div className="text-xs text-slate-500">{item.category}</div>
                    </div>
                  </div>
                  <span className="text-[10px] font-mono text-slate-600 group-hover:text-slate-400">
                    Jump to &rarr;
                  </span>
                </div>
              );
            })
          )}
        </div>

        {/* Keyboard Footer */}
        <div className="p-3 bg-slate-900/60 border-t border-slate-800 text-[11px] text-slate-500 flex items-center justify-between">
          <div>Use &uarr; &darr; to navigate &middot; Enter to select &middot; Esc to close</div>
          <kbd className="px-1.5 py-0.5 rounded bg-slate-800 border border-slate-700 font-mono text-[10px] text-slate-400">
            Ctrl + K
          </kbd>
        </div>
      </div>
    </div>
  );
};
