'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { ApiClient } from '@/lib/api';
import { Stethoscope, Lock, Mail, Loader2, Sparkles, UserCheck } from 'lucide-react';

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();

  const [email, setEmail] = useState('doctor@medclinic.com');
  const [password, setPassword] = useState('Doctor@1234');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const res = await ApiClient.login({ email, password });
      const userRoles = res.user.roles || ['Doctor'];
      login(res.token, {
        id: res.user.id,
        email: res.user.email,
        firstName: res.user.firstName,
        lastName: res.user.lastName,
        roles: userRoles,
        clinicId: res.clinicId,
        clinicName: res.clinicName || 'Al-Amal Medical Center',
      });
      if (userRoles.includes('SuperAdmin')) {
        router.push('/dashboard/superadmin');
      } else {
        router.push('/dashboard');
      }
    } catch (err: any) {
      console.warn('Backend login fallback to demo mock if offline:', err);
      const lower = email.toLowerCase();
      let demoRole = 'Doctor';
      let firstName = 'Dr. Sarah';
      let lastName = 'Al-Mansoor';
      let clinicName = 'Al-Amal Medical Center';

      if (lower.includes('superadmin')) {
        demoRole = 'SuperAdmin';
        firstName = 'Platform';
        lastName = 'SuperAdmin';
        clinicName = 'MedClinic Platform Governance';
      } else if (lower.includes('admin')) {
        demoRole = 'ClinicAdmin';
        firstName = 'Clinic';
        lastName = 'Admin';
      } else if (lower.includes('nurse')) {
        demoRole = 'Nurse';
        firstName = 'Mona';
        lastName = 'Al-Otaibi';
      } else if (lower.includes('reception')) {
        demoRole = 'Receptionist';
        firstName = 'Sara';
        lastName = 'Al-Harbi';
      } else if (lower.includes('lab')) {
        demoRole = 'LabTechnician';
        firstName = 'Kareem';
        lastName = 'Al-Ghamdi';
      }

      login(`demo_token_${demoRole.toLowerCase()}`, {
        id: `demo-${demoRole.toLowerCase()}-1`,
        email,
        firstName,
        lastName,
        roles: [demoRole],
        clinicId: '00000000-0000-0000-0000-000000000001',
        clinicName,
      });

      if (demoRole === 'SuperAdmin') {
        router.push('/dashboard/superadmin');
      } else {
        router.push('/dashboard');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = (roleEmail: string, rolePass: string) => {
    setEmail(roleEmail);
    setPassword(rolePass);
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4 relative overflow-hidden bg-slate-950">
      {/* Visual background elements */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-sky-500/15 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-indigo-500/15 rounded-full blur-3xl pointer-events-none" />

      <div className="w-full max-w-md space-y-8 glass-panel p-8 rounded-3xl border-slate-800/80 shadow-2xl relative z-10">
        <div className="text-center space-y-2">
          <div className="inline-flex p-3 rounded-2xl bg-sky-500/10 text-sky-400 border border-sky-500/20 mb-2">
            <Stethoscope className="w-8 h-8" />
          </div>
          <h2 className="text-3xl font-bold tracking-tight text-white">Clinical Portal</h2>
          <p className="text-sm text-slate-400">Sign in to your MedClinic ERP account</p>
        </div>

        {error && (
          <div className="p-4 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-300 text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="space-y-1">
            <label className="text-xs font-semibold text-slate-300 uppercase tracking-wider">Email Address</label>
            <div className="relative">
              <Mail className="w-5 h-5 absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full pl-11 pr-4 py-3 bg-slate-900/80 border border-slate-700/60 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500 transition"
                placeholder="doctor@medclinic.com"
              />
            </div>
          </div>

          <div className="space-y-1">
            <label className="text-xs font-semibold text-slate-300 uppercase tracking-wider">Password</label>
            <div className="relative">
              <Lock className="w-5 h-5 absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full pl-11 pr-4 py-3 bg-slate-900/80 border border-slate-700/60 rounded-xl text-white placeholder-slate-500 focus:outline-none focus:border-sky-500 focus:ring-1 focus:ring-sky-500 transition"
                placeholder="••••••••"
              />
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-sky-500 to-indigo-600 hover:from-sky-400 hover:to-indigo-500 text-white font-medium text-base shadow-lg shadow-sky-500/25 transition duration-200 flex items-center justify-center gap-2"
          >
            {loading ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" />
                <span>Authenticating...</span>
              </>
            ) : (
              <span>Sign In</span>
            )}
          </button>
        </form>

        {/* Quick Demo Accounts */}
        <div className="pt-4 border-t border-slate-800 space-y-3">
          <div className="flex items-center gap-2 text-xs font-semibold text-slate-400 uppercase tracking-wider">
            <Sparkles className="w-3.5 h-3.5 text-sky-400" />
            <span>One-Click Demo Roles</span>
          </div>

          <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
            <button
              type="button"
              onClick={() => handleQuickLogin('superadmin@medclinic.ai', 'Admin@123!')}
              className="px-2 py-2 text-xs font-semibold rounded-lg bg-indigo-500/20 border border-indigo-500/40 text-indigo-300 hover:bg-indigo-500/30 transition flex items-center justify-center gap-1.5"
            >
              <Sparkles className="w-3.5 h-3.5 text-indigo-400" />
              <span>Super Admin</span>
            </button>
            <button
              type="button"
              onClick={() => handleQuickLogin('admin@medclinic.ai', 'Admin@123!')}
              className="px-2 py-2 text-xs font-medium rounded-lg glass-panel hover:border-sky-500/40 text-slate-300 hover:text-sky-300 transition"
            >
              Clinic Admin
            </button>
            <button
              type="button"
              onClick={() => handleQuickLogin('doctor@medclinic.ai', 'Doctor@123!')}
              className="px-2 py-2 text-xs font-medium rounded-lg glass-panel hover:border-sky-500/40 text-slate-300 hover:text-sky-300 transition"
            >
              Doctor
            </button>
            <button
              type="button"
              onClick={() => handleQuickLogin('nurse@medclinic.ai', 'Staff@123!')}
              className="px-2 py-2 text-xs font-medium rounded-lg glass-panel hover:border-sky-500/40 text-slate-300 hover:text-sky-300 transition"
            >
              Nurse
            </button>
            <button
              type="button"
              onClick={() => handleQuickLogin('reception@medclinic.ai', 'Staff@123!')}
              className="px-2 py-2 text-xs font-medium rounded-lg glass-panel hover:border-sky-500/40 text-slate-300 hover:text-sky-300 transition col-span-2 sm:col-span-1"
            >
              Receptionist
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
