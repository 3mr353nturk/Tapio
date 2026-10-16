export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: { code: string; message: string };
}

export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'BusinessUser';
  businessId?: string;
  businessSlug?: string;
  emailVerified: boolean;
}

export interface LoginResponse {
  accessToken: string;
  user: UserInfo;
}

export interface PublicBusiness {
  name: string;
  slug: string;
  logoUrl?: string;
  coverImageUrl?: string;
  description?: string;
  actions: PublicAction[];
  campaigns: PublicCampaign[];
}

export interface PublicAction {
  type: string;
  title: string;
  value: string;
  displayOrder: number;
}

export interface PublicCampaign {
  title: string;
  description?: string;
  imageUrl?: string;
}

export interface BusinessProfile {
  id: string;
  name: string;
  slug: string;
  category?: string;
  logoUrl?: string;
  coverImageUrl?: string;
  description?: string;
  phone?: string;
  email?: string;
  whatsAppNumber?: string;
  instagramUrl?: string;
  googleMapsUrl?: string;
  googleReviewUrl?: string;
  menuUrl?: string;
  address?: string;
  city?: string;
  district?: string;
}

export interface BusinessAction {
  id: string;
  type: string;
  title: string;
  value: string;
  displayOrder: number;
  isEnabled: boolean;
}

export interface Campaign {
  id: string;
  title: string;
  description?: string;
  imageUrl?: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface Stand {
  id: string;
  standCode: string;
  isActive: boolean;
  createdDate: string;
}

export interface AnalyticsSummary {
  pageViews: number;
  actionClicks: number;
  topAction?: string;
  activeCampaigns: number;
  activeStands: number;
}

export interface OnboardingStep {
  key: string;
  label: string;
  completed: boolean;
  optional: boolean;
}

export interface OnboardingResponse {
  steps: OnboardingStep[];
  completedCount: number;
  totalCount: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AdminBusiness {
  id: string;
  name: string;
  slug: string;
  category?: string;
  isActive: boolean;
  registrationSource: string;
  createdDate: string;
}

export interface AdminStand {
  id: string;
  standCode: string;
  businessId?: string;
  businessName?: string;
  isActive: boolean;
  createdDate: string;
}

export interface AdminStats {
  totalBusinesses: number;
  activeBusinesses: number;
  totalStands: number;
  unassignedStands: number;
  totalEventsToday: number;
}

export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'BusinessUser';
  isActive: boolean;
  emailVerified: boolean;
}

export type ListQueryParams = {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDesc?: boolean;
  unassignedOnly?: boolean;
};
