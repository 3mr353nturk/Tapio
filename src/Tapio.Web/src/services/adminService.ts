import type { AdminBusiness, AdminStand, AdminStats, AdminUser, ListQueryParams, PagedResult } from '../types/api.types';
import { apiClient } from './apiClient';

function toQuery(params: ListQueryParams): string {
  const q = new URLSearchParams();
  if (params.page) q.set('page', String(params.page));
  if (params.pageSize) q.set('pageSize', String(params.pageSize));
  if (params.search?.trim()) q.set('search', params.search.trim());
  if (params.sortBy) q.set('sortBy', params.sortBy);
  if (params.sortDesc !== undefined) q.set('sortDesc', String(params.sortDesc));
  if (params.unassignedOnly) q.set('unassignedOnly', 'true');
  return q.toString();
}

export const adminService = {
  getStats: () => apiClient<AdminStats>('/api/admin/stats'),
  getBusinesses: (params: ListQueryParams = {}) =>
    apiClient<PagedResult<AdminBusiness>>(`/api/admin/businesses?${toQuery(params)}`),
  createBusiness: (data: Record<string, string | undefined>) =>
    apiClient('/api/admin/businesses', { method: 'POST', body: JSON.stringify(data) }),
  assignUserToBusiness: (userId: string, businessId: string, role: 'Owner' | 'Manager' | 'Staff' = 'Owner') =>
    apiClient(`/api/admin/users/${userId}/businesses`, {
      method: 'POST',
      body: JSON.stringify({ businessId, role })
    }),
  setBusinessStatus: (id: string, isActive: boolean) =>
    apiClient(`/api/admin/businesses/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify(isActive),
      headers: { 'Content-Type': 'application/json' }
    }),

  getUsers: (params: ListQueryParams = {}) =>
    apiClient<PagedResult<AdminUser>>(`/api/admin/users?${toQuery(params)}`),

  getStands: (params: ListQueryParams = {}) =>
    apiClient<PagedResult<AdminStand>>(`/api/admin/stands?${toQuery(params)}`),
  createStand: () => apiClient<AdminStand>('/api/admin/stands', { method: 'POST' }),
  assignStand: (standId: string, businessId: string) =>
    apiClient<AdminStand>(`/api/admin/stands/${standId}/assign`, {
      method: 'PUT',
      body: JSON.stringify({ businessId })
    }),
  unassignStand: (standId: string) =>
    apiClient<AdminStand>(`/api/admin/stands/${standId}/unassign`, { method: 'PUT' })
};
