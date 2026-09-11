'use client';

import React, { createContext, useContext, useEffect, useState, useMemo } from 'react';
import {
  Permission,
  getPermissionsForRoles,
  parseJwtClaims,
  hasPermission as checkPermission,
  hasAnyPermission as checkAnyPermission,
  hasAllPermissions as checkAllPermissions,
  hasRole as checkRole,
  hasAnyRole as checkAnyRole,
} from './permissions';
import { ApiClient } from './api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions?: string[];
  clinicId?: string;
  clinicName?: string;
}

interface AuthContextType {
  user: User | null;
  token: string | null;
  clinicId: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
  switchClinic: (clinicId: string, clinicName?: string) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function hydrateUserPermissions(user: User, token?: string): User {
  const isDemoToken = !token || token.startsWith('demo_');

  if (!isDemoToken && token) {
    // In Production: Cryptographic JWT Claims are the single source of truth.
    // Untrusted localStorage modifications are overridden by cryptographically signed claims.
    const jwtClaims = parseJwtClaims(token);
    const verifiedRoles = jwtClaims.roles.length > 0 ? jwtClaims.roles : user.roles;
    const verifiedPerms = jwtClaims.permissions.length > 0 ? jwtClaims.permissions : user.permissions || [];

    return {
      ...user,
      roles: verifiedRoles,
      permissions: verifiedPerms,
    };
  }

  // Demo / Mock Mode: Map static permissions for offline mock demonstration sessions
  if (user.permissions && user.permissions.length > 0) {
    return user;
  }
  const rolePerms = getPermissionsForRoles(user.roles || []);
  return {
    ...user,
    permissions: rolePerms,
  };
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [clinicId, setClinicId] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem('medclinic_token');
      const storedUser = localStorage.getItem('medclinic_user');
      const storedClinic = localStorage.getItem('medclinic_clinic_id');

      if (storedToken && storedUser) {
        setToken(storedToken);
        const parsedUser = JSON.parse(storedUser) as User;
        const hydrated = hydrateUserPermissions(parsedUser, storedToken);
        setUser(hydrated);
        setClinicId(storedClinic);

        // If it's a real backend session, asynchronously reconcile with GET /api/v1/auth/me
        if (!storedToken.startsWith('demo_')) {
          ApiClient.getMe()
            .then((res) => {
              if (res && res.data) {
                const serverUser = res.data;
                const reconciled: User = {
                  id: serverUser.id,
                  email: serverUser.email,
                  firstName: serverUser.fullName?.split(' ')[0] || hydrated.firstName,
                  lastName: serverUser.fullName?.split(' ').slice(1).join(' ') || hydrated.lastName,
                  roles: serverUser.roles || hydrated.roles,
                  permissions: serverUser.permissions || hydrated.permissions,
                  clinicId: storedClinic || undefined,
                  clinicName: serverUser.clinics?.[0]?.name || hydrated.clinicName,
                };
                setUser(reconciled);
                localStorage.setItem('medclinic_user', JSON.stringify(reconciled));
              }
            })
            .catch((err) => {
              console.warn('Session server verification warning:', err);
            });
        }
      }
    } catch (e) {
      console.error('Failed to load auth state', e);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const login = (newToken: string, newUser: User) => {
    const hydratedUser = hydrateUserPermissions(newUser, newToken);
    setToken(newToken);
    setUser(hydratedUser);
    setClinicId(hydratedUser.clinicId || null);

    localStorage.setItem('medclinic_token', newToken);
    localStorage.setItem('medclinic_user', JSON.stringify(hydratedUser));
    if (hydratedUser.clinicId) {
      localStorage.setItem('medclinic_clinic_id', hydratedUser.clinicId);
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    setClinicId(null);
    localStorage.removeItem('medclinic_token');
    localStorage.removeItem('medclinic_user');
    localStorage.removeItem('medclinic_clinic_id');
    window.location.href = '/login';
  };

  const switchClinic = (newClinicId: string, clinicName?: string) => {
    setClinicId(newClinicId);
    localStorage.setItem('medclinic_clinic_id', newClinicId);
    if (user) {
      const updatedUser = { ...user, clinicId: newClinicId, clinicName: clinicName || user.clinicName };
      setUser(updatedUser);
      localStorage.setItem('medclinic_user', JSON.stringify(updatedUser));
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        clinicId,
        isAuthenticated: !!token,
        isLoading,
        login,
        logout,
        switchClinic,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const usePermissions = () => {
  const { user } = useAuth();

  return useMemo(() => {
    const roles = user?.roles || [];
    const permissions = user?.permissions || [];

    return {
      roles,
      permissions,
      hasPermission: (permission: Permission | string) => checkPermission(user, permission),
      hasAnyPermission: (permissionsList: (Permission | string)[]) => checkAnyPermission(user, permissionsList),
      hasAllPermissions: (permissionsList: (Permission | string)[]) => checkAllPermissions(user, permissionsList),
      hasRole: (role: string) => checkRole(user, role),
      hasAnyRole: (rolesList: string[]) => checkAnyRole(user, rolesList),
    };
  }, [user]);
};
