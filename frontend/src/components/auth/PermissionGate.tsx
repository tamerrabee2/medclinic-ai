'use client';

import React from 'react';
import { usePermissions } from '@/lib/auth';
import { Permission } from '@/lib/permissions';

interface PermissionGateProps {
  permission?: Permission | string;
  anyPermissions?: (Permission | string)[];
  allPermissions?: (Permission | string)[];
  fallback?: React.ReactNode;
  children: React.ReactNode;
}

export const PermissionGate: React.FC<PermissionGateProps> = ({
  permission,
  anyPermissions,
  allPermissions,
  fallback = null,
  children,
}) => {
  const { hasPermission, hasAnyPermission, hasAllPermissions } = usePermissions();

  let allowed = true;

  if (permission && !hasPermission(permission)) {
    allowed = false;
  }

  if (allowed && anyPermissions && anyPermissions.length > 0 && !hasAnyPermission(anyPermissions)) {
    allowed = false;
  }

  if (allowed && allPermissions && allPermissions.length > 0 && !hasAllPermissions(allPermissions)) {
    allowed = false;
  }

  if (!allowed) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
