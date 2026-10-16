export const BUSINESS_CATEGORIES = [
  'Restoran',
  'Kafe',
  'Bar / Pub',
  'Fast Food',
  'Pastane / Fırın',
  'Market',
  'Eczane',
  'Berber / Kuaför',
  'Güzellik Merkezi',
  'Otel / Konaklama',
  'Spor Salonu',
  'Sağlık Kliniği',
  'Eğitim / Kurs',
  'Otomotiv',
  'Perakende',
  'Hizmet',
  'Diğer'
] as const;

export type BusinessCategory = (typeof BUSINESS_CATEGORIES)[number];
