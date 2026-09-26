import { createContext, useContext, useState, type ReactNode } from 'react';
import { apiClient } from '../api/client';
import type { AuthResponse } from '../types';

interface AuthUser {
  userId: string;
  fullName: string;
  role: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  login: (email: string, password: string) => Promise<void>;
  register: (fullName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function persistAuth(data: AuthResponse) {
  localStorage.setItem('token', data.token);
  const user: AuthUser = { userId: data.userId, fullName: data.fullName, role: data.role };
  localStorage.setItem('user', JSON.stringify(user));
  return user;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const raw = localStorage.getItem('user');
    return raw ? (JSON.parse(raw) as AuthUser) : null;
  });

  async function login(email: string, password: string) {
    const response = await apiClient.post<AuthResponse>('/auth/login', { email, password });
    setUser(persistAuth(response.data));
  }

  async function register(fullName: string, email: string, password: string) {
    const response = await apiClient.post<AuthResponse>('/auth/register', { fullName, email, password });
    setUser(persistAuth(response.data));
  }

  function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  }

  return <AuthContext.Provider value={{ user, login, register, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return ctx;
}
