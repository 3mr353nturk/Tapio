/** QR / NFC / paylaşım linkleri için public taban URL (ngrok: VITE_PUBLIC_BASE_URL). */
export function getPublicBaseUrl(): string {
  const fromEnv = import.meta.env.VITE_PUBLIC_BASE_URL?.trim();
  if (fromEnv) return fromEnv.replace(/\/$/, '');
  if (typeof window !== 'undefined') return window.location.origin;
  return '';
}

export function publicBusinessPath(slug: string, standCode?: string): string {
  const qs = standCode ? `?s=${encodeURIComponent(standCode)}` : '';
  return `/t/${slug}${qs}`;
}

/** QR ve NFC için tam URL (kısa yol + domain). */
export function publicBusinessUrl(slug: string, standCode?: string): string {
  return `${getPublicBaseUrl()}${publicBusinessPath(slug, standCode)}`;
}

export function publicAssetUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `${getPublicBaseUrl()}${normalized}`;
}

/** localhost / eski domain → VITE_PUBLIC_BASE_URL (menü, public sayfa, upload). */
export function rewriteToPublicBase(href: string): string {
  const base = getPublicBaseUrl().trim();
  if (!base || !href) return href;

  try {
    if (href.startsWith('/')) {
      return `${base.replace(/\/$/, '')}${href}`;
    }

    const parsed = new URL(href);
    const isLocal =
      parsed.hostname === 'localhost' ||
      parsed.hostname === '127.0.0.1' ||
      parsed.hostname.endsWith('.local');

    const isAppPath =
      parsed.pathname.startsWith('/menus/') ||
      parsed.pathname.startsWith('/t/') ||
      parsed.pathname.startsWith('/uploads/') ||
      parsed.pathname.startsWith('/app');

    if (isLocal && isAppPath) {
      return `${base.replace(/\/$/, '')}${parsed.pathname}${parsed.search}${parsed.hash}`;
    }
  } catch {
    /* ignore */
  }

  return href;
}
