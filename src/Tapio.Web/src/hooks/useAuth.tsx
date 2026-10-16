import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import type { UserInfo } from '../types/api.types';
import { authService } from '../services/authService';
import { getAccessToken, setAccessToken } from '../services/apiClient';

interface AuthContextValue {
  user: UserInfo | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<UserInfo>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = async () => {
    if (!getAccessToken()) {
      setUser(null);
      return;
    }
    const me = await authService.me();
    setUser(me);
  };

  useEffect(() => {
    (async () => {
      try {
        if (!getAccessToken()) {
          const res = await fetch('/api/auth/refresh', { method: 'POST', credentials: 'include' });
          if (res.ok) {
            const json = await res.json();
            if (json.success && json.data?.accessToken) {
              setAccessToken(json.data.accessToken);
            }
          }
        }
        if (getAccessToken()) await refreshUser();
      } catch {
        setUser(null);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const login = async (email: string, password: string) => {
    const result = await authService.login(email, password);
    setUser(result.user);
    return result.user;
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
