export type Permission =
  | 'Patients.Read'
  | 'Patients.Create'
  | 'Patients.Update'
  | 'Patients.Delete'
  | 'Appointments.Read'
  | 'Appointments.Create'
  | 'Appointments.Update'
  | 'Appointments.Cancel'
  | 'MedicalRecords.Read'
  | 'MedicalRecords.Create'
  | 'MedicalRecords.Update'
  | 'MedicalRecords.Delete'
  | 'Prescriptions.Read'
  | 'Prescriptions.Create'
  | 'Prescriptions.Update'
  | 'Prescriptions.Sign'
  | 'Prescriptions.Verify'
  | 'Notifications.Read'
  | 'Notifications.Manage'
  | 'Lab.Read'
  | 'Lab.Create'
  | 'Lab.Update'
  | 'Lab.EnterResults'
  | 'Radiology.Read'
  | 'Radiology.Create'
  | 'Radiology.Update'
  | 'Radiology.Report'
  | 'Radiology.AI'
  | 'Billing.Read'
  | 'Billing.Create'
  | 'Billing.Update'
  | 'Billing.Delete'
  | 'Users.Read'
  | 'Users.Manage'
  | 'Clinics.Read'
  | 'Clinics.Manage'
  | 'AI.Assist'
  | 'AI.Admin'
  | 'AI.Analysis'
  | 'AIDecisions.View'
  | 'AIDecisions.Export'
  | 'AIDecisions.Review'
  | 'Reports.Read'
  | 'Reports.Export'
  | 'PatientConsents.View'
  | 'PatientConsents.Manage'
  | 'PatientConsents.Revoke'
  | 'PatientConsents.Audit'
  | 'PatientConsents.Export';

export const ROLE_PERMISSIONS: Record<string, Permission[]> = {
  SuperAdmin: [
    'Patients.Read', 'Patients.Create', 'Patients.Update', 'Patients.Delete',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update', 'Appointments.Cancel',
    'MedicalRecords.Read', 'MedicalRecords.Create', 'MedicalRecords.Update', 'MedicalRecords.Delete',
    'Prescriptions.Read', 'Prescriptions.Create', 'Prescriptions.Update', 'Prescriptions.Sign', 'Prescriptions.Verify',
    'Notifications.Read', 'Notifications.Manage',
    'Lab.Read', 'Lab.Create', 'Lab.Update', 'Lab.EnterResults',
    'Radiology.Read', 'Radiology.Create', 'Radiology.Update', 'Radiology.Report', 'Radiology.AI',
    'Billing.Read', 'Billing.Create', 'Billing.Update', 'Billing.Delete',
    'Users.Read', 'Users.Manage',
    'Clinics.Read', 'Clinics.Manage',
    'AI.Assist', 'AI.Admin', 'AI.Analysis',
    'AIDecisions.View', 'AIDecisions.Export', 'AIDecisions.Review',
    'Reports.Read', 'Reports.Export',
    'PatientConsents.View', 'PatientConsents.Manage', 'PatientConsents.Revoke', 'PatientConsents.Audit', 'PatientConsents.Export'
  ],
  ClinicAdmin: [
    'Patients.Read', 'Patients.Create', 'Patients.Update', 'Patients.Delete',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update', 'Appointments.Cancel',
    'MedicalRecords.Read', 'MedicalRecords.Create', 'MedicalRecords.Update',
    'Prescriptions.Read', 'Prescriptions.Create', 'Prescriptions.Update', 'Prescriptions.Sign', 'Prescriptions.Verify',
    'Notifications.Read', 'Notifications.Manage',
    'Lab.Read', 'Lab.Create', 'Lab.Update', 'Lab.EnterResults',
    'Radiology.Read', 'Radiology.Create', 'Radiology.Update', 'Radiology.Report',
    'Billing.Read', 'Billing.Create', 'Billing.Update',
    'Users.Read', 'Users.Manage',
    'Clinics.Read', 'Clinics.Manage',
    'AI.Assist', 'AI.Analysis',
    'AIDecisions.View', 'AIDecisions.Export', 'AIDecisions.Review',
    'Reports.Read', 'Reports.Export',
    'PatientConsents.View', 'PatientConsents.Manage', 'PatientConsents.Revoke', 'PatientConsents.Audit', 'PatientConsents.Export'
  ],
  Doctor: [
    'Patients.Read', 'Patients.Create', 'Patients.Update',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update',
    'MedicalRecords.Read', 'MedicalRecords.Create', 'MedicalRecords.Update',
    'Prescriptions.Read', 'Prescriptions.Create', 'Prescriptions.Update', 'Prescriptions.Sign',
    'Notifications.Read',
    'Lab.Read', 'Lab.Create', 'Lab.Update',
    'Radiology.Read', 'Radiology.Create', 'Radiology.Update',
    'Billing.Read',
    'AI.Assist', 'AI.Analysis',
    'AIDecisions.View', 'AIDecisions.Export', 'AIDecisions.Review',
    'Reports.Read',
    'PatientConsents.View', 'PatientConsents.Manage', 'PatientConsents.Revoke', 'PatientConsents.Audit', 'PatientConsents.Export'
  ],
  Nurse: [
    'Patients.Read', 'Patients.Create', 'Patients.Update',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update',
    'MedicalRecords.Read', 'MedicalRecords.Create', 'MedicalRecords.Update',
    'Prescriptions.Read',
    'Notifications.Read',
    'Lab.Read', 'Lab.Create',
    'Radiology.Read',
    'Billing.Read',
    'PatientConsents.View', 'PatientConsents.Manage'
  ],
  Receptionist: [
    'Patients.Read', 'Patients.Create',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update', 'Appointments.Cancel',
    'Billing.Read', 'Billing.Create',
    'Notifications.Read',
    'PatientConsents.View'
  ],
  LabTechnician: [
    'Patients.Read',
    'Notifications.Read',
    'Lab.Read', 'Lab.Create', 'Lab.Update', 'Lab.EnterResults'
  ],
  Radiologist: [
    'Patients.Read',
    'Notifications.Read',
    'Radiology.Read', 'Radiology.Create', 'Radiology.Update', 'Radiology.Report', 'Radiology.AI',
    'AI.Assist'
  ],
  Pharmacist: [
    'Patients.Read',
    'MedicalRecords.Read',
    'Prescriptions.Read', 'Prescriptions.Verify',
    'Notifications.Read',
    'Billing.Read', 'Billing.Create', 'Billing.Update'
  ],
  Accountant: [
    'Billing.Read', 'Billing.Create', 'Billing.Update', 'Billing.Delete',
    'Notifications.Read',
    'Reports.Read', 'Reports.Export'
  ]
};

export interface UserAuthProfile {
  roles?: string[];
  permissions?: string[];
}

export function getPermissionsForRoles(roles: string[] = []): Permission[] {
  const perms = new Set<Permission>();
  for (const role of roles) {
    const list = ROLE_PERMISSIONS[role] || [];
    for (const p of list) {
      perms.add(p);
    }
  }
  return Array.from(perms);
}

export function parseJwtClaims(token: string): { roles: string[]; permissions: string[] } {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) {
      return { roles: [], permissions: [] };
    }
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const payload = JSON.parse(jsonPayload);

    // Extract roles (can be string, array, or standard MS schema)
    const rawRoles =
      payload.role ||
      payload.roles ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      [];
    const roles: string[] = Array.isArray(rawRoles) ? rawRoles : rawRoles ? [rawRoles] : [];

    // Extract permissions
    const rawPerms = payload.permission || payload.permissions || [];
    const permissions: string[] = Array.isArray(rawPerms) ? rawPerms : rawPerms ? [rawPerms] : [];

    return { roles, permissions };
  } catch (err) {
    return { roles: [], permissions: [] };
  }
}

export function hasPermission(
  user: UserAuthProfile | null | undefined,
  permission: Permission | string
): boolean {
  if (!user) return false;
  if (user.roles?.includes('SuperAdmin')) return true;
  return user.permissions?.includes(permission as Permission) ?? false;
}

export function hasAnyPermission(
  user: UserAuthProfile | null | undefined,
  permissions: (Permission | string)[]
): boolean {
  if (!user) return false;
  if (user.roles?.includes('SuperAdmin')) return true;
  if (!permissions.length) return true;
  return permissions.some((p) => user.permissions?.includes(p as Permission));
}

export function hasAllPermissions(
  user: UserAuthProfile | null | undefined,
  permissions: (Permission | string)[]
): boolean {
  if (!user) return false;
  if (user.roles?.includes('SuperAdmin')) return true;
  if (!permissions.length) return true;
  return permissions.every((p) => user.permissions?.includes(p as Permission));
}

export function hasRole(
  user: UserAuthProfile | null | undefined,
  role: string
): boolean {
  if (!user || !user.roles) return false;
  return user.roles.includes(role);
}

export function hasAnyRole(
  user: UserAuthProfile | null | undefined,
  roles: string[]
): boolean {
  if (!user || !user.roles) return false;
  if (!roles.length) return true;
  return roles.some((r) => user.roles?.includes(r));
}

// ── Centralized Route Policy Engine ─────────────────────────────────────

export interface RoutePolicy {
  roles?: string[];
  permissions?: (Permission | string)[];
  anyPermissions?: (Permission | string)[];
}

export const ROUTE_POLICIES: Record<string, RoutePolicy> = {
  '/dashboard/superadmin': { roles: ['SuperAdmin'] },
  '/dashboard/patients': { anyPermissions: ['Patients.Read'] },
  '/dashboard/visits': { anyPermissions: ['MedicalRecords.Read'] },
  '/dashboard/appointments': { anyPermissions: ['Appointments.Read'] },
  '/dashboard/prescriptions': { anyPermissions: ['Prescriptions.Read', 'Prescriptions.Sign'] },
  '/dashboard/laboratory': { anyPermissions: ['Lab.Read'] },
  '/dashboard/lab-analyzer': { anyPermissions: ['Lab.Read', 'AI.Assist'] },
  '/dashboard/external-labs': { anyPermissions: ['Lab.Read', 'Lab.Create'] },
  '/dashboard/radiology': { anyPermissions: ['Radiology.Read'] },
  '/dashboard/canvas': { anyPermissions: ['MedicalRecords.Read'] },
  '/dashboard/dental': { anyPermissions: ['MedicalRecords.Read'] },
  '/dashboard/insurance': { anyPermissions: ['Billing.Read', 'Billing.Create'] },
  '/dashboard/billing': { anyPermissions: ['Billing.Read', 'Billing.Create'] },
  '/dashboard/reports': { anyPermissions: ['Reports.Read'] },
  '/dashboard/analytics': { anyPermissions: ['Reports.Read'] },
  '/dashboard/ai-assistant': { anyPermissions: ['AI.Assist'] },
  '/dashboard/ai-assistant/voice-scribe': { anyPermissions: ['AI.Assist', 'MedicalRecords.Create'] },
  '/dashboard/users': { anyPermissions: ['Users.Read', 'Users.Manage'] },
  '/dashboard/staff': { anyPermissions: ['Users.Read', 'Users.Manage'] },
  '/dashboard/audit-logs': { anyPermissions: ['AIDecisions.View', 'AuditLogs.Read', 'PatientConsents.Audit'] },
  '/dashboard/notifications': { anyPermissions: ['Notifications.Read', 'Patients.Read'] },
};

export function matchRoutePolicy(pathname: string): RoutePolicy | null {
  // Normalize pathname: remove trailing slash except if root
  const cleanPath = pathname.length > 1 && pathname.endsWith('/') ? pathname.slice(0, -1) : pathname;

  // 1. Check exact match
  if (ROUTE_POLICIES[cleanPath]) {
    return ROUTE_POLICIES[cleanPath];
  }

  // 2. Check longest prefix match for subroutes (e.g. /dashboard/visits/123 -> /dashboard/visits)
  const matchingKey = Object.keys(ROUTE_POLICIES)
    .filter((route) => cleanPath.startsWith(route + '/'))
    .sort((a, b) => b.length - a.length)[0];

  if (matchingKey) {
    return ROUTE_POLICIES[matchingKey];
  }

  return null;
}

export function isRouteAuthorized(
  pathname: string,
  user: UserAuthProfile | null | undefined
): boolean {
  if (!user) return false;
  if (user.roles?.includes('SuperAdmin')) return true;

  const policy = matchRoutePolicy(pathname);
  if (!policy) {
    // If no policy specified (e.g. root /dashboard), all authenticated users are authorized
    return true;
  }

  if (policy.roles && policy.roles.length > 0) {
    if (!hasAnyRole(user, policy.roles)) return false;
  }

  if (policy.permissions && policy.permissions.length > 0) {
    if (!hasAllPermissions(user, policy.permissions)) return false;
  }

  if (policy.anyPermissions && policy.anyPermissions.length > 0) {
    if (!hasAnyPermission(user, policy.anyPermissions)) return false;
  }

  return true;
}
