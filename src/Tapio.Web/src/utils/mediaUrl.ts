/** Profil / public görselleri — /uploads API proxy veya tam URL. */
export function resolveMediaUrl(raw?: string | null): string {
  const value = raw?.trim();
  if (!value) return '';
  if (/^https?:\/\//i.test(value) || value.startsWith('data:')) return value;
  if (value.startsWith('/')) return value;
  return `/${value.replace(/^\/+/, '')}`;
}
