import type { ApiResponse } from '../types/api.types';

let accessToken: string | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function getAccessToken() {
  return accessToken;
}

async function refreshToken(): Promise<string | null> {
  const res = await fetch('/api/auth/refresh', {
    method: 'POST',
    credentials: 'include'
  });
  if (!res.ok) return null;
  const json = (await res.json()) as ApiResponse<{ accessToken: string }>;
  if (!json.success || !json.data) return null;
  accessToken = json.data.accessToken;
  return accessToken;
}

export async function apiClient<T>(
  url: string,
  options: RequestInit = {},
  retry = true
): Promise<T> {
  const headers = new Headers(options.headers);
  if (!headers.has('Content-Type') && options.body && !(options.body instanceof FormData))
    headers.set('Content-Type', 'application/json');
  if (accessToken)
    headers.set('Authorization', `Bearer ${accessToken}`);

  const res = await fetch(url, { ...options, headers, credentials: 'include' });

  if (res.status === 401 && retry && !url.includes('/auth/login') && !url.includes('/auth/register')) {
    const newToken = await refreshToken();
    if (newToken) return apiClient<T>(url, options, false);
    throw new Error('UNAUTHORIZED');
  }

  const json = (await res.json()) as ApiResponse<T>;
  if (!json.success)
    throw new Error(json.error?.message ?? 'Bir hata oluştu.');

  return json.data as T;
}
