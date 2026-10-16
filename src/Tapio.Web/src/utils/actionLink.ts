import { getPublicBaseUrl, rewriteToPublicBase } from './publicUrl';

/** Aksiyon değerini güvenli dış link / tel / mailto URL'sine çevirir. */
export function resolveActionHref(raw: string): string {
  const value = raw.trim();
  if (!value) return '#';

  if (/^(tel:|mailto:|sms:|geo:)/i.test(value)) return value;

  const digits = value.replace(/\s/g, '');
  if (/^\+?\d{10,15}$/.test(digits)) return `tel:${digits}`;

  if (value.startsWith('/')) {
    return rewriteToPublicBase(`${getPublicBaseUrl()}${value}`);
  }

  if (/^https?:\/\//i.test(value)) {
    return rewriteToPublicBase(value);
  }

  if (value.startsWith('//')) return rewriteToPublicBase(`https:${value}`);
  if (value.startsWith('www.')) return `https://${value}`;

  return value.includes('.') ? `https://${value}` : value;
}

export function isNavigableActionHref(href: string): boolean {
  if (!href || href === '#') return false;
  if (/^https?:\/\//i.test(href)) return true;
  if (/^(tel:|mailto:|sms:|geo:)/i.test(href)) return true;
  return false;
}
