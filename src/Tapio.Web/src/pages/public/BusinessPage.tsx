import { useEffect, useState } from 'react';
import { Helmet } from 'react-helmet-async';
import { useParams, useSearchParams } from 'react-router-dom';
import { publicService } from '../../services/publicService';
import type { PublicBusiness } from '../../types/api.types';
import { isNavigableActionHref, resolveActionHref } from '../../utils/actionLink';
import { resolveMediaUrl } from '../../utils/mediaUrl';
import { getSessionId } from '../../utils/sessionId';
import { ErrorState, LoadingState } from '../../components/shared/StateViews';

export function BusinessPage() {
  const { slug = '' } = useParams();
  const [search] = useSearchParams();
  const standCode = search.get('s') ?? undefined;
  const [data, setData] = useState<PublicBusiness | null>(null);
  const [error, setError] = useState('');
  const [actionHint, setActionHint] = useState('');
  const sessionId = getSessionId();

  useEffect(() => {
    publicService.getBusiness(slug)
      .then(setData)
      .catch(err => setError(err instanceof Error ? err.message : 'Sayfa yüklenemedi.'));
  }, [slug]);

  useEffect(() => {
    if (!slug) return;
    publicService.recordEvent({ businessSlug: slug, standCode, eventType: 'PageView', sessionId });
  }, [slug, standCode, sessionId]);

  const onAction = (eventType: string, rawValue: string) => {
    setActionHint('');
    publicService.recordEvent({ businessSlug: slug, standCode, eventType, sessionId });
    const href = resolveActionHref(rawValue);
    if (!isNavigableActionHref(href)) {
      setActionHint('Bu aksiyon için geçerli bir link tanımlı değil. İşletme panelinden Değer alanına https://... veya telefon numarası girin.');
      return;
    }
    if (/^https?:\/\//i.test(href)) {
      window.open(href, '_blank', 'noopener,noreferrer');
      return;
    }
    window.location.href = href;
  };

  if (error) return <ErrorState message={error} />;
  if (!data) return <LoadingState />;

  const canonical = `https://tapio.app/t/${data.slug}`;

  return (
    <>
      <Helmet>
        <title>{data.name} | Tapio</title>
        <meta name="description" content={data.description ?? `${data.name} dijital sayfası`} />
        <meta property="og:title" content={data.name} />
        <meta property="og:description" content={data.description ?? `${data.name} dijital sayfası`} />
        {data.coverImageUrl && <meta property="og:image" content={resolveMediaUrl(data.coverImageUrl)} />}
        <link rel="canonical" href={canonical} />
      </Helmet>
      <div className="public-page">
        {data.coverImageUrl && <div className="cover" style={{ backgroundImage: `url(${resolveMediaUrl(data.coverImageUrl)})` }} />}
        <div className="content">
          {data.logoUrl && <img src={resolveMediaUrl(data.logoUrl)} alt={data.name} className="logo" />}
          <h1>{data.name}</h1>
          {data.description && <p className="desc">{data.description}</p>}
          {actionHint && <p className="error-text" style={{ marginTop: '1rem' }}>{actionHint}</p>}
          <div className="actions">
            {data.actions.map(action => (
              <button key={`${action.type}-${action.displayOrder}`} className="action-btn" onClick={() => onAction(`${action.type}Click`, action.value)}>
                {action.title}
              </button>
            ))}
          </div>
          {data.campaigns.length > 0 && (
            <section>
              <h3>Kampanyalar</h3>
              {data.campaigns.map(c => (
                <div key={c.title} className="campaign card">
                  {c.imageUrl && <img src={c.imageUrl} alt={c.title} />}
                  <strong>{c.title}</strong>
                  {c.description && <p>{c.description}</p>}
                </div>
              ))}
            </section>
          )}
        </div>
      </div>
      <style>{`
        .public-page { max-width: 480px; margin: 0 auto; min-height: 100vh; background: #fff; }
        .cover { height: 180px; background-size: cover; background-position: center; }
        .content { padding: 1rem 1rem 2rem; text-align: center; }
        .logo { width: 88px; height: 88px; border-radius: 50%; object-fit: cover; margin-top: -44px; border: 4px solid #fff; }
        .desc { color: var(--color-muted); }
        .actions { display: flex; flex-direction: column; gap: 0.75rem; margin-top: 1.5rem; }
        .action-btn { min-height: 52px; border-radius: 14px; border: 1px solid var(--color-border); background: #fff; font-weight: 600; cursor: pointer; }
        .campaign { margin-top: 0.75rem; text-align: left; }
        .campaign img { width: 100%; border-radius: 10px; margin-bottom: 0.5rem; }
      `}</style>
    </>
  );
}
