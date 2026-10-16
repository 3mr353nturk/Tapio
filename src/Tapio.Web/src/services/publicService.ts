import type { PublicBusiness } from '../types/api.types';
import { apiClient } from './apiClient';

export const publicService = {
  getBusiness(slug: string) {
    return apiClient<PublicBusiness>(`/api/public/businesses/${slug}`);
  },

  recordEvent(payload: {
    businessSlug: string;
    standCode?: string;
    eventType: string;
    sessionId: string;
  }) {
    const body = JSON.stringify(payload);
    if (navigator.sendBeacon) {
      navigator.sendBeacon('/api/public/events', new Blob([body], { type: 'application/json' }));
      return Promise.resolve();
    }
    return fetch('/api/public/events', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body,
      keepalive: true
    });
  }
};
