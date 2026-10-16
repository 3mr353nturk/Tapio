import QRCode from 'qrcode';
import { useEffect, useMemo, useState } from 'react';
import { businessService } from '../../services/businessService';
import type { Stand } from '../../types/api.types';
import { getPublicBaseUrl, publicBusinessPath, publicBusinessUrl } from '../../utils/publicUrl';

type Props = {
  slug: string;
};

export function PublicSharePanel({ slug }: Props) {
  const [stands, setStands] = useState<Stand[]>([]);
  const [standCode, setStandCode] = useState('');
  const [qrDataUrl, setQrDataUrl] = useState('');
  const [copyMsg, setCopyMsg] = useState('');
  const [error, setError] = useState('');

  const fullUrl = useMemo(() => publicBusinessUrl(slug, standCode || undefined), [slug, standCode]);
  const shortPath = useMemo(() => publicBusinessPath(slug, standCode || undefined), [slug, standCode]);
  const base = getPublicBaseUrl();

  useEffect(() => {
    businessService.getStands()
      .then(setStands)
      .catch(err => setError(err instanceof Error ? err.message : 'Standlar yüklenemedi.'));
  }, []);

  useEffect(() => {
    let cancelled = false;
    QRCode.toDataURL(fullUrl, { width: 280, margin: 2, errorCorrectionLevel: 'M' })
      .then(url => { if (!cancelled) setQrDataUrl(url); })
      .catch(() => { if (!cancelled) setQrDataUrl(''); });
    return () => { cancelled = true; };
  }, [fullUrl]);

  const copy = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopyMsg('Kopyalandı.');
      setTimeout(() => setCopyMsg(''), 2000);
    } catch {
      setCopyMsg('Kopyalanamadı.');
    }
  };

  const downloadPng = () => {
    if (!qrDataUrl) return;
    const a = document.createElement('a');
    a.href = qrDataUrl;
    a.download = `tapio-qr-${slug}${standCode ? `-${standCode}` : ''}.png`;
    a.click();
  };

  const downloadSvg = async () => {
    const svg = await QRCode.toString(fullUrl, { type: 'svg', margin: 2, errorCorrectionLevel: 'M' });
    const blob = new Blob([svg], { type: 'image/svg+xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `tapio-qr-${slug}${standCode ? `-${standCode}` : ''}.svg`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="card form-panel">
      <h3 className="card-section-title">Public link, QR ve NFC</h3>
      <p style={{ fontSize: '0.85rem', color: 'var(--color-muted)', marginTop: 0 }}>
        QR kod ve NFC etiketi <strong>aynı URL</strong>yi açmalıdır. Kısa yol: <code>{shortPath}</code>
      </p>

      {base.includes('localhost') && (
        <p className="error-text" style={{ fontSize: '0.8rem' }}>
          Dışarıdan demo için ngrok kullanın ve <code>.env.local</code> içinde{' '}
          <code>VITE_PUBLIC_BASE_URL=https://….ngrok-free.app</code> ayarlayın; ardından sayfayı yenileyin.
        </p>
      )}

      <div className="form-group" style={{ maxWidth: 320 }}>
        <label>Stand (isteğe bağlı — analitik için ?s= kodu)</label>
        <select className="select" value={standCode} onChange={e => setStandCode(e.target.value)}>
          <option value="">Genel link (stand yok)</option>
          {stands.filter(s => s.isActive).map(s => (
            <option key={s.id} value={s.standCode}>{s.standCode}</option>
          ))}
        </select>
        {error && <p className="error-text">{error}</p>}
      </div>

      <div className="form-group">
        <label>Tam link (QR / NFC)</label>
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <input className="input" readOnly value={fullUrl} style={{ flex: '1 1 200px' }} />
          <button type="button" className="btn btn-secondary btn-sm" onClick={() => copy(fullUrl)}>Kopyala</button>
          <a className="btn btn-secondary btn-sm" href={fullUrl} target="_blank" rel="noreferrer">Aç</a>
        </div>
        {copyMsg && <p className="success-text" style={{ fontSize: '0.8rem' }}>{copyMsg}</p>}
      </div>

      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1.5rem', alignItems: 'flex-start', marginTop: '1rem' }}>
        {qrDataUrl && (
          <img src={qrDataUrl} alt="QR kod" width={200} height={200} style={{ borderRadius: 8, border: '1px solid var(--color-border)' }} />
        )}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          <button type="button" className="btn btn-primary" onClick={downloadPng}>QR indir (PNG)</button>
          <button type="button" className="btn btn-secondary" onClick={() => void downloadSvg()}>QR indir (SVG)</button>
          <p style={{ fontSize: '0.75rem', color: 'var(--color-muted)', maxWidth: 260, margin: 0 }}>
            NFC: telefonunuzun NFC araçlarıyla veya etiket yazıcısıyla yukarıdaki <strong>tam linki</strong> kaydedin (QR ile birebir aynı).
          </p>
        </div>
      </div>
    </div>
  );
}
