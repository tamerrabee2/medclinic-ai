'use client';

import React, { createContext, useContext, useEffect, useState } from 'react';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
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
        setUser(JSON.parse(storedUser));
        setClinicId(storedClinic);
      }
    } catch (e) {
      console.error('Failed to load auth state', e);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const login = (newToken: string, newUser: User) => {
    setToken(newToken);
    setUser(newUser);
    setClinicId(newUser.clinicId || null);

    localStorage.setItem('medclinic_token', newToken);
    localStorage.setItem('medclinic_user', JSON.stringify(newUser));
    if (newUser.clinicId) {
      localStorage.setItem('medclinic_clinic_id', newUser.clinicId);
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
