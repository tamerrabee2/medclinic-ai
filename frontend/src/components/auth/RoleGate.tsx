'use client';

import React from 'react';
import { usePermissions } from '@/lib/auth';

interface RoleGateProps {
  role?: string;
  roles?: string[];
  fallback?: React.ReactNode;
  children: React.ReactNode;
}

export const RoleGate: React.FC<RoleGateProps> = ({
  role,
  roles,
  fallback = null,
  children,
}) => {
  const { hasRole, hasAnyRole } = usePermissions();

  let allowed = true;

  if (role && !hasRole(role)) {
    allowed = false;
  }

  if (allowed && roles && roles.length > 0 && !hasAnyRole(roles)) {
    allowed = false;
  }

  if (!allowed) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
