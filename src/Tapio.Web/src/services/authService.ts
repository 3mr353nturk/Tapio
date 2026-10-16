import type { LoginResponse, UserInfo } from '../types/api.types';
import { apiClient, setAccessToken } from './apiClient';

export const authService = {
  async login(email: string, password: string) {
    const data = await apiClient<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });
    setAccessToken(data.accessToken);
    return data;
  },

  async register(payload: Record<string, string>) {
    const data = await apiClient<LoginResponse & { business: { name: string; slug: string } }>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload)
    });
    setAccessToken(data.accessToken);
    return data;
  },

  async me() {
    return apiClient<UserInfo>('/api/auth/me');
  },

  async logout() {
    try {
      await apiClient('/api/auth/logout', { method: 'POST' });
    } finally {
      setAccessToken(null);
    }
  },

  async verifyEmail(token: string) {
    return apiClient('/api/auth/verify-email', {
      method: 'POST',
      body: JSON.stringify({ token })
    });
  },

  async forgotPassword(email: string) {
    return apiClient('/api/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email })
    });
  },

  async resetPassword(token: string, newPassword: string) {
    return apiClient('/api/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify({ token, newPassword })
    });
  }
};
