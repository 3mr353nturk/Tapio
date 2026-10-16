import type { BusinessProfile } from '../types/api.types';

export const profileFieldLabels: Record<keyof Pick<
  BusinessProfile,
  | 'name'
  | 'category'
  | 'description'
  | 'phone'
  | 'email'
  | 'whatsAppNumber'
  | 'instagramUrl'
  | 'googleMapsUrl'
  | 'googleReviewUrl'
  | 'menuUrl'
  | 'logoUrl'
  | 'coverImageUrl'
  | 'address'
  | 'city'
  | 'district'
>, string> = {
  name: 'İşletme adı',
  category: 'Kategori',
  description: 'Açıklama',
  phone: 'Telefon',
  email: 'E-posta',
  whatsAppNumber: 'WhatsApp numarası',
  instagramUrl: 'Instagram URL',
  googleMapsUrl: 'Google Haritalar URL',
  googleReviewUrl: 'Google yorum URL',
  menuUrl: 'Menü URL',
  logoUrl: 'Logo URL',
  coverImageUrl: 'Kapak görseli URL',
  address: 'Adres',
  city: 'İl',
  district: 'İlçe'
};

export const adminBusinessFormLabels: Record<string, string> = {
  name: 'İşletme adı',
  category: 'Kategori',
  phone: 'Telefon',
  email: 'İşletme e-postası',
  address: 'Adres',
  city: 'İl',
  district: 'İlçe',
};

export const actionTypeOptions = [
  { value: 'Menu', label: 'Menü' },
  { value: 'WhatsApp', label: 'WhatsApp' },
  { value: 'Instagram', label: 'Instagram' },
  { value: 'GoogleReview', label: 'Google yorum' },
  { value: 'GoogleMaps', label: 'Google Haritalar' },
  { value: 'Phone', label: 'Telefon' },
  { value: 'Campaign', label: 'Kampanya' }
] as const;

export function actionTypeLabel(type: string): string {
  return actionTypeOptions.find(o => o.value === type)?.label ?? type;
}

export function registrationSourceLabel(source: string): string {
  switch (source) {
    case 'Admin':
      return 'Admin';
    case 'SelfService':
    case 'SelfRegister':
      return 'Self kayıt';
    default:
      return source;
  }
}

export function userRoleLabel(role: string): string {
  switch (role) {
    case 'Admin':
      return 'Yönetici';
    case 'BusinessUser':
      return 'İşletme kullanıcısı';
    default:
      return role;
  }
}

export function formatDateTime(value: string): string {
  return new Date(value).toLocaleString('tr-TR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}
