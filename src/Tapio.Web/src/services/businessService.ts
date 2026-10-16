import type {
  AnalyticsSummary,
  BusinessAction,
  BusinessProfile,
  Campaign,
  OnboardingResponse,
  Stand
} from '../types/api.types';
import { apiClient } from './apiClient';

export const businessService = {
  getProfile: () => apiClient<BusinessProfile>('/api/business/profile'),
  updateProfile: (data: Partial<BusinessProfile>) =>
    apiClient<BusinessProfile>('/api/business/profile', { method: 'PUT', body: JSON.stringify(data) }),

  uploadLogo: (file: File) => {
    const body = new FormData();
    body.append('file', file);
    return apiClient<{ url: string }>('/api/business/media/logo', { method: 'POST', body });
  },
  uploadCover: (file: File) => {
    const body = new FormData();
    body.append('file', file);
    return apiClient<{ url: string }>('/api/business/media/cover', { method: 'POST', body });
  },

  getActions: () => apiClient<BusinessAction[]>('/api/business/actions'),
  createAction: (data: Omit<BusinessAction, 'id'>) =>
    apiClient<BusinessAction>('/api/business/actions', { method: 'POST', body: JSON.stringify(data) }),
  updateAction: (id: string, data: Partial<BusinessAction>) =>
    apiClient<BusinessAction>(`/api/business/actions/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteAction: (id: string) =>
    apiClient(`/api/business/actions/${id}`, { method: 'DELETE' }),

  getCampaigns: () => apiClient<Campaign[]>('/api/business/campaigns'),
  createCampaign: (data: Omit<Campaign, 'id'>) =>
    apiClient<Campaign>('/api/business/campaigns', { method: 'POST', body: JSON.stringify(data) }),
  updateCampaign: (id: string, data: Partial<Campaign>) =>
    apiClient<Campaign>(`/api/business/campaigns/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deleteCampaign: (id: string) =>
    apiClient(`/api/business/campaigns/${id}`, { method: 'DELETE' }),

  getStands: () => apiClient<Stand[]>('/api/business/stands'),
  getOnboarding: () => apiClient<OnboardingResponse>('/api/business/onboarding'),

  getAnalyticsSummary: (from?: string, to?: string) => {
    const params = new URLSearchParams();
    if (from) params.set('from', from);
    if (to) params.set('to', to);
    return apiClient<AnalyticsSummary>(`/api/business/analytics/summary?${params}`);
  }
};
