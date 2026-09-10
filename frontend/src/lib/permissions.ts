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
  | 'Prescriptions.Sign'
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
    'Prescriptions.Sign',
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
    'Prescriptions.Sign',
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
    'Prescriptions.Sign',
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
    'Lab.Read', 'Lab.Create',
    'Radiology.Read',
    'Billing.Read',
    'PatientConsents.View', 'PatientConsents.Manage'
  ],
  Receptionist: [
    'Patients.Read', 'Patients.Create',
    'Appointments.Read', 'Appointments.Create', 'Appointments.Update', 'Appointments.Cancel',
    'Billing.Read', 'Billing.Create',
    'PatientConsents.View'
  ],
  LabTechnician: [
    'Patients.Read',
    'Lab.Read', 'Lab.Create', 'Lab.Update', 'Lab.EnterResults'
  ],
  Radiologist: [
    'Patients.Read',
    'Radiology.Read', 'Radiology.Create', 'Radiology.Update', 'Radiology.Report', 'Radiology.AI',
    'AI.Assist'
  ],
  Pharmacist: [
    'Patients.Read',
    'MedicalRecords.Read',
    'Billing.Read', 'Billing.Create', 'Billing.Update'
  ],
  Accountant: [
    'Billing.Read', 'Billing.Create', 'Billing.Update', 'Billing.Delete',
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
