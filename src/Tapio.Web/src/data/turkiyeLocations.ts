import raw from './turkiyeIlIlce.json';
import { TURKEY_PROVINCE_NAMES } from './turkiyeProvinces';

type RawEntry = { province: string; districts: { id: number; name: string }[] };
type RawMap = Record<string, RawEntry>;

const collator = new Intl.Collator('tr');

const entries = Object.values(raw as RawMap);

const districtsByProvince = new Map<string, string[]>(
  entries.map(e => [
    e.province,
    e.districts.map(d => d.name).sort(collator.compare)
  ])
);

export const TURKEY_PROVINCES = [...TURKEY_PROVINCE_NAMES];

export function getDistrictsForProvince(province: string): string[] {
  if (!province) return [];
  const exact = districtsByProvince.get(province);
  if (exact) return exact;
  const normalized = normalizeProvince(province);
  return districtsByProvince.get(normalized) ?? [];
}

export function normalizeProvince(value?: string | null): string {
  if (!value) return '';
  const trimmed = value.trim();
  const match = TURKEY_PROVINCES.find(p => p.localeCompare(trimmed, 'tr', { sensitivity: 'accent' }) === 0);
  return match ?? trimmed;
}

export function isKnownProvince(value: string): boolean {
  return normalizeProvince(value) !== '' && TURKEY_PROVINCES.some(p => p === normalizeProvince(value));
}
