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
  authorizationHydrated: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
  switchClinic: (clinicId: string, clinicName?: string) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function hydrateUserPermissions(user: User, token?: string): User {
  const isDemoToken = !token || token.startsWith('demo_');

  if (!isDemoToken && token) {
    // In Production: Cryptographic JWT Claims and /api/v1/auth/me are the single source of truth.
    // Untrusted localStorage roles or permissions are never trusted or fallen back to.
    const jwtClaims = parseJwtClaims(token);

    return {
      ...user,
      roles: jwtClaims.roles,
      permissions: jwtClaims.permissions,
    };
  }

  // Demo / Mock Mode: Map static permissions strictly for offline mock demonstration sessions
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
  const [authorizationHydrated, setAuthorizationHydrated] = useState(false);

  useEffect(() => {
    try {
      const storedToken = localStorage.getItem('medclinic_token');
      const storedUser = localStorage.getItem('medclinic_user');
      const storedClinic = localStorage.getItem('medclinic_clinic_id');

      if (storedToken && storedUser) {
        setToken(storedToken);
        setClinicId(storedClinic);
        const parsedUser = JSON.parse(storedUser) as User;

        if (storedToken.startsWith('demo_')) {
          // Demo Mode: Immediate static hydration
          const hydrated = hydrateUserPermissions(parsedUser, storedToken);
          setUser(hydrated);
          setIsLoading(false);
          setAuthorizationHydrated(true);
        } else {
          // Production Mode: Zero trust in localStorage.
          // Cryptographically decode initial JWT claims for non-privileged scaffolding,
          // but hold authorizationHydrated=false until /api/v1/auth/me confirms identity.
          const jwtClaims = parseJwtClaims(storedToken);
          const initialUser: User = {
            id: parsedUser.id,
            email: parsedUser.email,
            firstName: parsedUser.firstName,
            lastName: parsedUser.lastName,
            roles: jwtClaims.roles,
            permissions: jwtClaims.permissions,
            clinicId: storedClinic || undefined,
            clinicName: parsedUser.clinicName,
          };
          setUser(initialUser);
          setIsLoading(false);

          // Asynchronously verify with authoritative server endpoint GET /api/v1/auth/me
          ApiClient.getMe()
            .then((res) => {
              if (res && res.data) {
                const serverUser = res.data;
                const reconciled: User = {
                  id: serverUser.id,
                  email: serverUser.email,
                  firstName: serverUser.fullName?.split(' ')[0] || initialUser.firstName,
                  lastName: serverUser.fullName?.split(' ').slice(1).join(' ') || initialUser.lastName,
                  roles: serverUser.roles || jwtClaims.roles || [],
                  permissions: serverUser.permissions || jwtClaims.permissions || [],
                  clinicId: storedClinic || undefined,
                  clinicName: serverUser.clinics?.[0]?.name || initialUser.clinicName,
                };
                setUser(reconciled);
                localStorage.setItem('medclinic_user', JSON.stringify(reconciled));
                setAuthorizationHydrated(true);
              } else {
                throw new Error('Invalid /api/v1/auth/me payload');
              }
            })
            .catch((err) => {
              console.error('Authoritative auth/me check failed. Invalidating session:', err);
              // Clear session and redirect to /login
              setToken(null);
              setUser(null);
              setClinicId(null);
              setAuthorizationHydrated(false);
              localStorage.removeItem('medclinic_token');
              localStorage.removeItem('medclinic_user');
              localStorage.removeItem('medclinic_clinic_id');
              window.location.href = '/login';
            });
        }
      } else {
        setIsLoading(false);
        setAuthorizationHydrated(true);
      }
    } catch (e) {
      console.error('Failed to load auth state', e);
      setIsLoading(false);
      setAuthorizationHydrated(true);
    }
  }, []);

  const login = (newToken: string, newUser: User) => {
    const hydratedUser = hydrateUserPermissions(newUser, newToken);
    setToken(newToken);
    setUser(hydratedUser);
    setClinicId(hydratedUser.clinicId || null);
    setAuthorizationHydrated(true);

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
    setAuthorizationHydrated(false);
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
        authorizationHydrated,
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
