'use client';

import React from 'react';
import Link from 'next/link';
import { useAuth } from '@/lib/auth';
import { ShieldAlert, ArrowLeft, LogOut, Lock } from 'lucide-react';

export default function UnauthorizedPage() {
  const { user, logout } = useAuth();
  const userRoles = user?.roles?.join(', ') || 'Standard User';

  return (
    <div className="min-h-screen flex items-center justify-center p-4 relative overflow-hidden bg-slate-950 text-slate-100">
      {/* Visual background ambient glow */}
      <div className="absolute top-1/3 left-1/4 w-96 h-96 bg-rose-500/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/3 right-1/4 w-96 h-96 bg-amber-500/10 rounded-full blur-3xl pointer-events-none" />

      <div className="w-full max-w-lg space-y-6 glass-panel p-8 rounded-3xl border-slate-800/80 shadow-2xl relative z-10 text-center">
        <div className="inline-flex p-4 rounded-2xl bg-rose-500/10 text-rose-400 border border-rose-500/20 mb-2 animate-pulse">
          <ShieldAlert className="w-10 h-10" />
        </div>

        <div className="space-y-2">
          <span className="px-2.5 py-1 text-xs font-mono font-semibold rounded-full bg-rose-500/20 text-rose-300 border border-rose-500/30">
            HTTP 403 · FORBIDDEN
          </span>
          <h1 className="text-3xl font-bold tracking-tight text-white mt-2">
            Access Restricted
          </h1>
          <p className="text-sm text-slate-400 max-w-md mx-auto">
            You do not have the required permissions or role clearance to access this system partition.
          </p>
        </div>

        {/* User Identity Details */}
        <div className="p-4 rounded-2xl bg-slate-900/60 border border-slate-800 text-left space-y-2">
          <div className="flex items-center justify-between text-xs">
            <span className="text-slate-400 flex items-center gap-1.5">
              <Lock className="w-3.5 h-3.5 text-slate-500" />
              Active Identity:
            </span>
            <span className="font-medium text-slate-200 truncate max-w-[200px]">
              {user?.email || 'Authenticated Session'}
            </span>
          </div>

          <div className="flex items-center justify-between text-xs">
            <span className="text-slate-400">Assigned Role(s):</span>
            <span className="font-mono text-xs font-semibold px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
              {userRoles}
            </span>
          </div>

          {user?.clinicName && (
            <div className="flex items-center justify-between text-xs pt-1 border-t border-slate-800/60">
              <span className="text-slate-400">Clinic Domain:</span>
              <span className="text-slate-300 truncate">{user.clinicName}</span>
            </div>
          )}
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-3 pt-2">
          <Link
            href="/dashboard"
            className="w-full sm:w-auto px-5 py-2.5 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-sm shadow-lg shadow-sky-500/25 transition flex items-center justify-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Return to Dashboard</span>
          </Link>

          <button
            type="button"
            onClick={logout}
            className="w-full sm:w-auto px-5 py-2.5 rounded-xl border border-slate-700/80 bg-slate-900/80 hover:bg-slate-800 text-slate-300 hover:text-white font-medium text-sm transition flex items-center justify-center gap-2"
          >
            <LogOut className="w-4 h-4 text-slate-400" />
            <span>Sign Out</span>
          </button>
        </div>
      </div>
    </div>
  );
}
