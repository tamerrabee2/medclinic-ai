'use client';

import React, { useEffect, useMemo } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAuth } from '@/lib/auth';
import { isRouteAuthorized } from '@/lib/permissions';
import { Sidebar } from '@/components/layout/Sidebar';
import { Navbar } from '@/components/layout/Navbar';
import { Loader2 } from 'lucide-react';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { user, isAuthenticated, isLoading, authorizationHydrated } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  const isAuthorized = useMemo(() => {
    if (!isAuthenticated || !user || !authorizationHydrated) return false;
    return isRouteAuthorized(pathname, user);
  }, [isAuthenticated, user, pathname, authorizationHydrated]);

  useEffect(() => {
    if (!isLoading && authorizationHydrated) {
      if (!isAuthenticated) {
        router.replace('/login');
      } else if (!isAuthorized) {
        router.replace('/unauthorized');
      }
    }
  }, [isAuthenticated, isLoading, authorizationHydrated, isAuthorized, router]);

  if (isLoading || !authorizationHydrated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-950 text-sky-400">
        <Loader2 className="w-8 h-8 animate-spin" />
      </div>
    );
  }

  if (!isAuthenticated || !isAuthorized) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-950 text-slate-400">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="w-8 h-8 animate-spin text-rose-400" />
          <span className="text-xs font-mono">Verifying authorization...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex bg-slate-950 text-slate-100 selection:bg-sky-500 selection:text-white">
      <Sidebar />
      <div className="flex-1 flex flex-col min-w-0">
        <Navbar />
        <main className="flex-1 p-6 md:p-8 overflow-y-auto max-w-7xl w-full mx-auto">
          {children}
        </main>
      </div>
    </div>
  );
}
