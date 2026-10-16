import { useRef, useState } from 'react';
import { resolveMediaUrl } from '../../utils/mediaUrl';

type Props = {
  label: string;
  hint?: string;
  value?: string;
  onChange: (url: string) => void;
  onUpload: (file: File) => Promise<{ url: string }>;
  variant: 'logo' | 'cover';
};

export function ImageUploadField({ label, hint, value, onChange, onUpload, variant }: Props) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  const pick = async (file: File | null) => {
    if (!file) return;
    setError('');
    setBusy(true);
    try {
      const res = await onUpload(file);
      onChange(res.url);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Yükleme başarısız.');
    } finally {
      setBusy(false);
      if (inputRef.current) inputRef.current.value = '';
    }
  };

  const previewUrl = resolveMediaUrl(value);
  const previewClass = variant === 'logo' ? 'upload-preview upload-preview--logo' : 'upload-preview upload-preview--cover';

  return (
    <div className="form-group">
      <label>{label}</label>
      {previewUrl && (
        <div className={previewClass}>
          <img src={previewUrl} alt="" />
        </div>
      )}
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp,image/gif"
        disabled={busy}
        onChange={e => pick(e.target.files?.[0] ?? null)}
      />
      {hint && <p style={{ fontSize: '0.8rem', color: 'var(--color-muted)', marginTop: '0.35rem' }}>{hint}</p>}
      <details style={{ marginTop: '0.5rem' }}>
        <summary style={{ fontSize: '0.8rem', cursor: 'pointer', color: 'var(--color-muted)' }}>Veya URL yapıştır</summary>
        <input
          className="input"
          style={{ marginTop: '0.35rem' }}
          value={value ?? ''}
          onChange={e => onChange(e.target.value)}
          placeholder="https://..."
        />
      </details>
      {error && <p className="error-text">{error}</p>}
      {busy && <p style={{ fontSize: '0.8rem', color: 'var(--color-muted)' }}>Yükleniyor…</p>}
      <style>{`
        .upload-preview { margin: 0.5rem 0; overflow: hidden; border-radius: 12px; border: 1px solid var(--color-border); background: #f5f5f5; }
        .upload-preview--logo { width: 96px; height: 96px; border-radius: 50%; }
        .upload-preview--cover { width: 100%; max-width: 320px; height: 120px; }
        .upload-preview img { width: 100%; height: 100%; object-fit: cover; display: block; }
      `}</style>
    </div>
  );
}
