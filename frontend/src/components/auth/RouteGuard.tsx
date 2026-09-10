'use client';

import React, { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth, usePermissions } from '@/lib/auth';
import { Permission } from '@/lib/permissions';
import { Loader2 } from 'lucide-react';

interface RouteGuardProps {
  roles?: string[];
  permissions?: (Permission | string)[];
  anyPermissions?: (Permission | string)[];
  children: React.ReactNode;
}

export const RouteGuard: React.FC<RouteGuardProps> = ({
  roles,
  permissions,
  anyPermissions,
  children,
}) => {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();
  const { hasAnyRole, hasAllPermissions, hasAnyPermission } = usePermissions();

  const isRoleAllowed = !roles || roles.length === 0 || hasAnyRole(roles);
  const isPermissionsAllowed =
    (!permissions || permissions.length === 0 || hasAllPermissions(permissions)) &&
    (!anyPermissions || anyPermissions.length === 0 || hasAnyPermission(anyPermissions));

  const isAuthorized = isRoleAllowed && isPermissionsAllowed;

  useEffect(() => {
    if (!isLoading) {
      if (!isAuthenticated) {
        router.replace('/login');
      } else if (!isAuthorized) {
        router.replace('/unauthorized');
      }
    }
  }, [isLoading, isAuthenticated, isAuthorized, router]);

  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-slate-950">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="h-8 w-8 animate-spin text-sky-400" />
          <span className="text-xs font-medium tracking-wide text-slate-400">
            Verifying security credentials...
          </span>
        </div>
      </div>
    );
  }

  if (!isAuthenticated || !isAuthorized) {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-slate-950">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="h-8 w-8 animate-spin text-rose-400" />
          <span className="text-xs font-medium tracking-wide text-slate-400">
            Redirecting...
          </span>
        </div>
      </div>
    );
  }

  return <>{children}</>;
};
