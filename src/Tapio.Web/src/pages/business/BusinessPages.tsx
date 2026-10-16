import { FormEvent, useEffect, useState } from 'react';
import { useAuth } from '../../hooks/useAuth';
import { businessService } from '../../services/businessService';
import type { AnalyticsSummary, BusinessAction, BusinessProfile, Campaign, OnboardingResponse, Stand } from '../../types/api.types';
import { OnboardingChecklist } from '../../components/business/OnboardingChecklist';
import { CategorySelect } from '../../components/forms/CategorySelect';
import { ImageUploadField } from '../../components/forms/ImageUploadField';
import { LocationFields } from '../../components/forms/LocationFields';
import { Badge } from '../../components/shared/Badge';
import { DataTable } from '../../components/shared/DataTable';
import { PageHeader } from '../../components/shared/PageHeader';
import { StatCard } from '../../components/shared/StatCard';
import { EmptyState, ErrorState, LoadingState } from '../../components/shared/StateViews';
import { PublicSharePanel } from '../../components/business/PublicSharePanel';
import { actionTypeLabel, actionTypeOptions, formatDateTime, profileFieldLabels } from '../../utils/labels';
import { publicAssetUrl, rewriteToPublicBase } from '../../utils/publicUrl';

export function DashboardPage() {
  const { user } = useAuth();
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null);
  const [onboarding, setOnboarding] = useState<OnboardingResponse | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([businessService.getAnalyticsSummary(), businessService.getOnboarding()])
      .then(([s, o]) => { setSummary(s); setOnboarding(o); })
      .catch(err => setError(err instanceof Error ? err.message : 'Veri yüklenemedi.'));
  }, []);

  if (error) return <ErrorState message={error} />;
  if (!summary || !onboarding) return <LoadingState />;

  return (
    <div>
      <PageHeader
        title="Genel bakış"
        description="İşletmenizin performansı ve kurulum adımları."
      />
      {user?.businessSlug && (
        <div style={{ marginBottom: '1rem' }}>
          <PublicSharePanel slug={user.businessSlug} />
        </div>
      )}
      <OnboardingChecklist data={onboarding} />
      <div className="grid-3" style={{ marginTop: '1rem' }}>
        <StatCard label="Görüntülenme" value={summary.pageViews} />
        <StatCard label="Aksiyon tıklama" value={summary.actionClicks} />
        <StatCard label="En çok tıklanan" value={summary.topAction ?? '—'} />
        <StatCard label="Aktif kampanya" value={summary.activeCampaigns} />
        <StatCard label="Aktif stand" value={summary.activeStands} />
      </div>
    </div>
  );
}

export function SharePage() {
  const { user } = useAuth();
  if (!user?.businessSlug) return <ErrorState message="İşletme slug bulunamadı." />;
  return (
    <div>
      <PageHeader title="Paylaşım" description="Public link, QR indirme ve NFC için aynı URL." />
      <PublicSharePanel slug={user.businessSlug} />
    </div>
  );
}

export function ProfilePage() {
  const [profile, setProfile] = useState<BusinessProfile | null>(null);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    businessService.getProfile().then(setProfile).catch(err => setError(err instanceof Error ? err.message : 'Profil yüklenemedi.'));
  }, []);

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!profile) return;
    try {
      const updated = await businessService.updateProfile(profile);
      setProfile(updated);
      setSaved(true);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Güncelleme başarısız.');
    }
  };

  if (error && !profile) return <ErrorState message={error} />;
  if (!profile) return <LoadingState />;

  const textFields = ['name', 'phone', 'email', 'whatsAppNumber', 'instagramUrl', 'googleMapsUrl', 'googleReviewUrl', 'menuUrl'] as const;

  return (
    <div>
      <PageHeader title="Profil" description="Public sayfanızda görünen işletme bilgileri." />
      <form className="card" onSubmit={onSubmit}>
        <h3 className="card-section-title">Temel bilgiler</h3>
        <div className="grid-2">
          <div className="form-group">
            <label>{profileFieldLabels.name}</label>
            <input className="input" value={profile.name} onChange={e => setProfile({ ...profile, name: e.target.value })} required />
          </div>
          <CategorySelect value={profile.category ?? ''} onChange={v => setProfile({ ...profile, category: v })} />
          <div className="form-group" style={{ gridColumn: '1 / -1' }}>
            <label>{profileFieldLabels.description}</label>
            <textarea className="textarea" value={profile.description ?? ''} onChange={e => setProfile({ ...profile, description: e.target.value })} />
          </div>
        </div>

        <h3 className="card-section-title">İletişim</h3>
        <div className="grid-2">
          {textFields.filter(k => k !== 'name').slice(0, 6).map(key => (
            <div className="form-group" key={key}>
              <label>{profileFieldLabels[key]}</label>
              <input className="input" value={profile[key] ?? ''} onChange={e => setProfile({ ...profile, [key]: e.target.value })} />
            </div>
          ))}
        </div>

        <h3 className="card-section-title">Görseller</h3>
        <div className="grid-2">
          <ImageUploadField
            label="Logo"
            variant="logo"
            value={profile.logoUrl}
            onChange={url => setProfile({ ...profile, logoUrl: url })}
            onUpload={file => businessService.uploadLogo(file)}
            hint="Kare veya yuvarlak kırpılacak şekilde yükleyin (max 5 MB)."
          />
          <ImageUploadField
            label="Kapak görseli"
            variant="cover"
            value={profile.coverImageUrl}
            onChange={url => setProfile({ ...profile, coverImageUrl: url })}
            onUpload={file => businessService.uploadCover(file)}
            hint="Geniş yatay görsel (max 8 MB)."
          />
          <div className="form-group" style={{ gridColumn: '1 / -1' }}>
            <label>{profileFieldLabels.menuUrl}</label>
            <input className="input" value={profile.menuUrl ?? ''} onChange={e => setProfile({ ...profile, menuUrl: e.target.value })} placeholder="https://.../menus/kafe.html" />
            <p style={{ fontSize: '0.8rem', color: 'var(--color-muted)', marginTop: '0.35rem' }}>
              Menü aksiyonunda da aynı tam linki kullanın.{' '}
              <button type="button" className="btn btn-secondary btn-sm" style={{ marginTop: '0.35rem' }} onClick={() => setProfile({ ...profile, menuUrl: publicAssetUrl('/menus/kafe.html') })}>
                {publicAssetUrl('/menus/kafe.html')} linkini kullan
              </button>
              {profile.menuUrl?.includes('localhost') && (
                <button
                  type="button"
                  className="btn btn-secondary btn-sm"
                  style={{ marginTop: '0.35rem', marginLeft: '0.35rem' }}
                  onClick={() => setProfile({ ...profile, menuUrl: rewriteToPublicBase(profile.menuUrl ?? '') })}
                >
                  localhost → demo URL
                </button>
              )}
            </p>
          </div>
        </div>

        <h3 className="card-section-title">Konum</h3>
        <div className="grid-2">
          <LocationFields
            city={profile.city ?? ''}
            district={profile.district ?? ''}
            onCityChange={city => setProfile({ ...profile, city, district: '' })}
            onDistrictChange={district => setProfile({ ...profile, district })}
          />
          <div className="form-group" style={{ gridColumn: '1 / -1' }}>
            <label>{profileFieldLabels.address}</label>
            <textarea className="textarea" value={profile.address ?? ''} onChange={e => setProfile({ ...profile, address: e.target.value })} />
          </div>
        </div>

        {error && <p className="error-text">{error}</p>}
        {saved && <p className="success-text">Kaydedildi.</p>}
        <button className="btn btn-primary" type="submit">Kaydet</button>
      </form>
    </div>
  );
}

export function ActionsPage() {
  const [actions, setActions] = useState<BusinessAction[]>([]);
  const [error, setError] = useState('');
  const [form, setForm] = useState({ type: 'Menu', title: '', value: '', displayOrder: 1, isEnabled: true });

  const load = () => businessService.getActions().then(setActions).catch(err => setError(err instanceof Error ? err.message : 'Yüklenemedi.'));
  useEffect(() => { load(); }, []);

  const create = async (e: FormEvent) => {
    e.preventDefault();
    await businessService.createAction(form as BusinessAction);
    setForm({ type: 'Menu', title: '', value: '', displayOrder: 1, isEnabled: true });
    await load();
  };

  if (error && actions.length === 0) return <ErrorState message={error} />;

  return (
    <div>
      <PageHeader title="Aksiyonlar" description="Public sayfadaki hızlı aksiyon butonları." />
      <form className="card form-panel" onSubmit={create}>
        <div className="grid-2">
          <div className="form-group">
            <label>Tip</label>
            <select className="select" value={form.type} onChange={e => setForm({ ...form, type: e.target.value })}>
              {actionTypeOptions.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>
          <div className="form-group"><label>Başlık</label><input className="input" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} required /></div>
          <div className="form-group">
            <label>Değer (tam link veya telefon)</label>
            <input className="input" value={form.value} onChange={e => setForm({ ...form, value: e.target.value })} required placeholder="Profildeki menü URL veya https://..." />
            <p style={{ fontSize: '0.8rem', color: 'var(--color-muted)', marginTop: '0.35rem' }}>Menü aksiyonu için profildeki Menü URL ile aynı tam linki girin.</p>
          </div>
          <div className="form-group"><label>Sıra</label><input className="input" type="number" value={form.displayOrder} onChange={e => setForm({ ...form, displayOrder: Number(e.target.value) })} /></div>
        </div>
        <button className="btn btn-primary" type="submit">Aksiyon ekle</button>
      </form>

      <DataTable
        rows={actions}
        rowKey={a => a.id}
        emptyMessage="Henüz aksiyon yok."
        columns={[
          { key: 'title', header: 'Başlık', sortable: true, getSortValue: r => r.title, getSearchValue: r => `${r.title} ${r.type}`, render: r => <strong>{r.title}</strong> },
          { key: 'type', header: 'Tip', sortable: true, getSortValue: r => r.type, render: r => actionTypeLabel(r.type) },
          { key: 'value', header: 'Değer', sortable: true, getSortValue: r => r.value, render: r => <span className="mono">{r.value}</span> },
          { key: 'order', header: 'Sıra', sortable: true, getSortValue: r => r.displayOrder, render: r => r.displayOrder },
          { key: 'status', header: 'Durum', render: r => r.isEnabled ? <Badge variant="success">Aktif</Badge> : <Badge variant="muted">Pasif</Badge> },
          {
            key: 'actions',
            header: '',
            render: r => (
              <button className="btn btn-danger btn-sm" type="button" onClick={() => businessService.deleteAction(r.id).then(load)}>Sil</button>
            )
          }
        ]}
      />
    </div>
  );
}

export function CampaignsPage() {
  const [items, setItems] = useState<Campaign[]>([]);
  const [error, setError] = useState('');
  const [form, setForm] = useState({ title: '', description: '', imageUrl: '', startDate: '', endDate: '', isActive: true });

  const load = () => businessService.getCampaigns().then(setItems).catch(err => setError(err instanceof Error ? err.message : 'Yüklenemedi.'));
  useEffect(() => { load(); }, []);

  const create = async (e: FormEvent) => {
    e.preventDefault();
    await businessService.createCampaign({
      ...form,
      startDate: new Date(form.startDate).toISOString(),
      endDate: new Date(form.endDate).toISOString()
    } as Campaign);
    setForm({ title: '', description: '', imageUrl: '', startDate: '', endDate: '', isActive: true });
    await load();
  };

  if (error && items.length === 0) return <ErrorState message={error} />;

  return (
    <div>
      <PageHeader title="Kampanyalar" description="Public sayfada gösterilecek kampanyalar." />
      <form className="card form-panel" onSubmit={create}>
        <div className="grid-2">
          <div className="form-group"><label>Başlık</label><input className="input" value={form.title} onChange={e => setForm({ ...form, title: e.target.value })} required /></div>
          <div className="form-group"><label>Görsel URL</label><input className="input" value={form.imageUrl} onChange={e => setForm({ ...form, imageUrl: e.target.value })} /></div>
          <div className="form-group"><label>Başlangıç</label><input className="input" type="datetime-local" value={form.startDate} onChange={e => setForm({ ...form, startDate: e.target.value })} required /></div>
          <div className="form-group"><label>Bitiş</label><input className="input" type="datetime-local" value={form.endDate} onChange={e => setForm({ ...form, endDate: e.target.value })} required /></div>
        </div>
        <div className="form-group"><label>Açıklama</label><textarea className="textarea" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} /></div>
        <button className="btn btn-primary" type="submit">Kampanya ekle</button>
      </form>

      <DataTable
        rows={items}
        rowKey={c => c.id}
        emptyMessage="Henüz kampanya yok."
        columns={[
          { key: 'title', header: 'Başlık', sortable: true, getSortValue: r => r.title, getSearchValue: r => `${r.title} ${r.description ?? ''}`, render: r => <strong>{r.title}</strong> },
          { key: 'dates', header: 'Tarih aralığı', sortable: true, getSortValue: r => new Date(r.startDate).getTime(), render: r => `${formatDateTime(r.startDate)} – ${formatDateTime(r.endDate)}` },
          { key: 'status', header: 'Durum', sortable: true, getSortValue: r => (r.isActive ? 1 : 0), render: r => r.isActive ? <Badge variant="success">Aktif</Badge> : <Badge variant="muted">Pasif</Badge> }
        ]}
      />
    </div>
  );
}

export function StandsPage() {
  const [stands, setStands] = useState<Stand[]>([]);
  const [error, setError] = useState('');
  useEffect(() => {
    businessService.getStands().then(setStands).catch(err => setError(err instanceof Error ? err.message : 'Yüklenemedi.'));
  }, []);
  if (error) return <ErrorState message={error} />;
  if (!stands) return <LoadingState />;

  return (
    <div>
      <PageHeader title="Standlar" description="Stand ataması yönetici tarafından yapılır; bu ekran salt okunurdur." />
      {stands.length === 0 ? (
        <EmptyState message="Henüz atanmış stand yok." />
      ) : (
        <DataTable
          rows={stands}
          rowKey={s => s.id}
          columns={[
            { key: 'code', header: 'Stand kodu', sortable: true, getSortValue: r => r.standCode, getSearchValue: r => r.standCode, render: r => <strong>{r.standCode}</strong> },
            { key: 'status', header: 'Durum', sortable: true, getSortValue: r => (r.isActive ? 1 : 0), render: r => r.isActive ? <Badge variant="success">Aktif</Badge> : <Badge variant="muted">Pasif</Badge> },
            { key: 'created', header: 'Oluşturulma', sortable: true, getSortValue: r => new Date(r.createdDate).getTime(), render: r => formatDateTime(r.createdDate) }
          ]}
        />
      )}
    </div>
  );
}

export function AnalyticsPage() {
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null);
  const [error, setError] = useState('');
  useEffect(() => {
    businessService.getAnalyticsSummary().then(setSummary).catch(err => setError(err instanceof Error ? err.message : 'Yüklenemedi.'));
  }, []);
  if (error) return <ErrorState message={error} />;
  if (!summary) return <LoadingState />;
  return (
    <div>
      <PageHeader title="Analitik" description="Public sayfa ve aksiyon etkileşim özeti." />
      <div className="grid-2">
        <StatCard label="Sayfa görüntülenme" value={summary.pageViews} />
        <StatCard label="Aksiyon tıklama" value={summary.actionClicks} />
        <StatCard label="En çok tıklanan aksiyon" value={summary.topAction ?? '—'} />
        <StatCard label="Aktif kampanya / stand" value={`${summary.activeCampaigns} / ${summary.activeStands}`} />
      </div>
    </div>
  );
}
