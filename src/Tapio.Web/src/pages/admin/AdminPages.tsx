import { FormEvent, useCallback, useEffect, useState } from 'react';
import { CategorySelect } from '../../components/forms/CategorySelect';
import { LocationFields } from '../../components/forms/LocationFields';
import { Badge } from '../../components/shared/Badge';
import { DataTable, type DataTableServerQuery } from '../../components/shared/DataTable';
import { PageHeader } from '../../components/shared/PageHeader';
import { StatCard } from '../../components/shared/StatCard';
import { ErrorState, LoadingState } from '../../components/shared/StateViews';
import { useDebouncedValue } from '../../hooks/useDebouncedValue';
import { adminService } from '../../services/adminService';
import type { AdminBusiness, AdminStand, AdminStats, AdminUser, PagedResult } from '../../types/api.types';
import { normalizeProvince } from '../../data/turkiyeLocations';
import { AssignUserBusinessPanel } from '../../components/admin/AssignUserBusinessPanel';
import { adminBusinessFormLabels, formatDateTime, registrationSourceLabel, userRoleLabel } from '../../utils/labels';

const defaultQuery: DataTableServerQuery = {
  page: 1,
  pageSize: 20,
  search: '',
  sortKey: 'createdDate',
  sortDir: 'desc'
};

export function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [error, setError] = useState('');
  useEffect(() => {
    adminService.getStats().then(setStats).catch(err => setError(err instanceof Error ? err.message : 'Yüklenemedi.'));
  }, []);
  if (error) return <ErrorState message={error} />;
  if (!stats) return <LoadingState />;
  return (
    <div>
      <PageHeader title="Genel bakış" description="Platform özeti ve operasyon metrikleri." />
      <div className="grid-3">
        <StatCard label="Toplam işletme" value={stats.totalBusinesses} />
        <StatCard label="Aktif işletme" value={stats.activeBusinesses} />
        <StatCard label="Toplam stand" value={stats.totalStands} />
        <StatCard label="Atanmamış stand" value={stats.unassignedStands} />
        <StatCard label="Bugünkü etkinlik" value={stats.totalEventsToday} />
      </div>
    </div>
  );
}

type BusinessForm = {
  name: string;
  category: string;
  phone: string;
  email: string;
  address: string;
  city: string;
  district: string;
  assignUserId: string;
};

const emptyBusinessForm: BusinessForm = {
  name: '', category: '', phone: '', email: '', address: '', city: '', district: '', assignUserId: ''
};

export function AdminBusinessesPage() {
  const [data, setData] = useState<PagedResult<AdminBusiness> | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<BusinessForm>(emptyBusinessForm);
  const [createdSlug, setCreatedSlug] = useState('');
  const [query, setQuery] = useState<DataTableServerQuery>({ ...defaultQuery, sortKey: 'createdDate' });
  const [businessUsers, setBusinessUsers] = useState<AdminUser[]>([]);
  const debouncedSearch = useDebouncedValue(query.search);

  useEffect(() => {
    adminService.getUsers({ page: 1, pageSize: 200 }).then(r => {
      setBusinessUsers(r.items.filter(u => u.role === 'BusinessUser'));
    }).catch(() => undefined);
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getBusinesses({
        page: query.page,
        pageSize: query.pageSize,
        search: debouncedSearch,
        sortBy: query.sortKey,
        sortDesc: query.sortDir === 'desc'
      });
      setData(result);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [query.page, query.pageSize, query.sortKey, query.sortDir, debouncedSearch]);

  useEffect(() => { load(); }, [load]);

  const create = async (e: FormEvent) => {
    e.preventDefault();
    const { assignUserId, ...payload } = form;
    const result = await adminService.createBusiness(payload) as { slug: string; businessId: string };
    if (assignUserId) {
      await adminService.assignUserToBusiness(assignUserId, result.businessId, 'Owner');
    }
    setCreatedSlug(result.slug);
    setForm(emptyBusinessForm);
    await load();
  };

  return (
    <div>
      <PageHeader title="İşletmeler" description="Admin işletme kayıtları; kullanıcı bağlantısı manuel atama ile yapılır." />
      <form className="card form-panel" onSubmit={create}>
        <h3 className="card-section-title">Yeni işletme</h3>
        <div className="grid-2">
          <div className="form-group">
            <label>{adminBusinessFormLabels.name}</label>
            <input className="input" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
          </div>
          <CategorySelect value={form.category} onChange={v => setForm({ ...form, category: v })} />
          <div className="form-group">
            <label>{adminBusinessFormLabels.phone}</label>
            <input className="input" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
          </div>
          <div className="form-group">
            <label>{adminBusinessFormLabels.email}</label>
            <input className="input" type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
          </div>
          <div className="form-group">
            <label>Bağlı kullanıcı (opsiyonel)</label>
            <select className="select" value={form.assignUserId} onChange={e => setForm({ ...form, assignUserId: e.target.value })}>
              <option value="">Sonra ata</option>
              {businessUsers.map(u => (
                <option key={u.id} value={u.id}>{u.firstName} {u.lastName} ({u.email})</option>
              ))}
            </select>
          </div>
          <LocationFields
            city={form.city}
            district={form.district}
            onCityChange={city => setForm(prev => ({ ...prev, city, district: normalizeProvince(prev.city) === normalizeProvince(city) ? prev.district : '' }))}
            onDistrictChange={district => setForm(prev => ({ ...prev, district }))}
          />
          <div className="form-group" style={{ gridColumn: '1 / -1' }}>
            <label>{adminBusinessFormLabels.address}</label>
            <textarea className="textarea" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
          </div>
        </div>
        <button className="btn btn-primary" type="submit">İşletme oluştur</button>
        {createdSlug && <p className="success-text" style={{ marginTop: '0.75rem' }}>Oluşturuldu: /t/{createdSlug}</p>}
      </form>

      <AssignUserBusinessPanel />

      {error && !data && <ErrorState message={error} />}
      <DataTable
        mode="server"
        loading={loading}
        rows={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        query={query}
        onQueryChange={setQuery}
        rowKey={b => b.id}
        emptyMessage="Henüz işletme yok."
        columns={[
          { key: 'name', header: 'İşletme', sortable: true, getSearchValue: r => `${r.name} ${r.slug}`, render: r => (<><strong>{r.name}</strong><div className="mono" style={{ color: 'var(--color-muted)' }}>{r.slug}</div></>) },
          { key: 'category', header: 'Kategori', sortable: true, getSortValue: r => r.category ?? '', render: r => r.category ?? '—' },
          { key: 'registrationSource', header: 'Kaynak', sortable: true, getSortValue: r => r.registrationSource, render: r => registrationSourceLabel(r.registrationSource) },
          { key: 'createdDate', header: 'Oluşturulma', sortable: true, getSortValue: r => new Date(r.createdDate).getTime(), render: r => formatDateTime(r.createdDate) },
          { key: 'isActive', header: 'Durum', sortable: true, getSortValue: r => (r.isActive ? 1 : 0), render: r => r.isActive ? <Badge variant="success">Aktif</Badge> : <Badge variant="danger">Pasif</Badge> },
          {
            key: 'public',
            header: 'Public',
            render: r => <a href={`/t/${r.slug}`} target="_blank" rel="noreferrer">/t/{r.slug}</a>
          }
        ]}
      />
    </div>
  );
}

export function AdminUsersPage() {
  const [data, setData] = useState<PagedResult<AdminUser> | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState<DataTableServerQuery>({ ...defaultQuery, sortKey: 'email' });
  const debouncedSearch = useDebouncedValue(query.search);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getUsers({
        page: query.page,
        pageSize: query.pageSize,
        search: debouncedSearch,
        sortBy: query.sortKey,
        sortDesc: query.sortDir === 'desc'
      });
      setData(result);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [query.page, query.pageSize, query.sortKey, query.sortDir, debouncedSearch]);

  useEffect(() => { load(); }, [load]);

  if (error && !data) return <ErrorState message={error} />;

  return (
    <div>
      <PageHeader title="Kullanıcılar" description="Platform kullanıcıları ve doğrulama durumları." />
      <AssignUserBusinessPanel onAssigned={load} />
      <DataTable
        mode="server"
        loading={loading}
        rows={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        query={query}
        onQueryChange={setQuery}
        rowKey={u => u.id}
        columns={[
          { key: 'email', header: 'E-posta', sortable: true, getSearchValue: r => `${r.email} ${r.firstName} ${r.lastName}`, render: r => r.email },
          { key: 'firstName', header: 'Ad', sortable: true, getSortValue: r => r.firstName, render: r => r.firstName },
          { key: 'lastName', header: 'Soyad', sortable: true, getSortValue: r => r.lastName, render: r => r.lastName },
          { key: 'role', header: 'Rol', sortable: true, getSortValue: r => r.role, render: r => userRoleLabel(r.role) },
          { key: 'emailVerified', header: 'E-posta', sortable: true, getSortValue: r => (r.emailVerified ? 1 : 0), render: r => r.emailVerified ? <Badge variant="success">Doğrulandı</Badge> : <Badge variant="warning">Bekliyor</Badge> },
          { key: 'isActive', header: 'Hesap', sortable: true, getSortValue: r => (r.isActive ? 1 : 0), render: r => r.isActive ? <Badge variant="success">Aktif</Badge> : <Badge variant="muted">Pasif</Badge> }
        ]}
      />
    </div>
  );
}

export function AdminStandsPage() {
  const [data, setData] = useState<PagedResult<AdminStand> | null>(null);
  const [businesses, setBusinesses] = useState<AdminBusiness[]>([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [newStandCode, setNewStandCode] = useState('');
  const [assignStandId, setAssignStandId] = useState('');
  const [assignBusinessId, setAssignBusinessId] = useState('');
  const [unassignedStandsOptions, setUnassignedStandsOptions] = useState<AdminStand[]>([]);
  const [unassignedOnly, setUnassignedOnly] = useState(false);
  const [query, setQuery] = useState<DataTableServerQuery>({ ...defaultQuery, sortKey: 'standCode', sortDir: 'asc' });
  const debouncedSearch = useDebouncedValue(query.search);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getStands({
        page: query.page,
        pageSize: query.pageSize,
        search: debouncedSearch,
        sortBy: query.sortKey,
        sortDesc: query.sortDir === 'desc',
        unassignedOnly
      });
      setData(result);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [query.page, query.pageSize, query.sortKey, query.sortDir, debouncedSearch, unassignedOnly]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    adminService.getBusinesses({ page: 1, pageSize: 200 }).then(r => setBusinesses(r.items)).catch(() => undefined);
    adminService.getStands({ page: 1, pageSize: 200, unassignedOnly: true }).then(r => setUnassignedStandsOptions(r.items)).catch(() => undefined);
  }, []);

  const createStand = async () => {
    const stand = await adminService.createStand();
    setNewStandCode(stand.standCode);
    await load();
    adminService.getStands({ page: 1, pageSize: 200, unassignedOnly: true }).then(r => setUnassignedStandsOptions(r.items)).catch(() => undefined);
  };

  const assign = async () => {
    if (!assignStandId || !assignBusinessId) return;
    await adminService.assignStand(assignStandId, assignBusinessId);
    setAssignStandId('');
    setAssignBusinessId('');
    await load();
  };

  return (
    <div>
      <PageHeader title="Standlar" description="StandCode otomatik üretilir; işletmelere buradan atayabilirsiniz." />
      <div className="grid-2 form-panel">
        <div className="card">
          <h3 className="card-section-title">Stand oluştur</h3>
          <p style={{ color: 'var(--color-muted)', marginTop: 0 }}>Kod formatı: TAP-000001 (SQL sequence).</p>
          <button className="btn btn-primary" type="button" onClick={createStand}>Stand oluştur</button>
          {newStandCode && <p className="success-text" style={{ marginTop: '1rem' }}>Oluşturulan kod: <strong>{newStandCode}</strong></p>}
        </div>
        <div className="card">
          <h3 className="card-section-title">Stand ata</h3>
          <div className="form-group">
            <label>Stand</label>
            <select className="select" value={assignStandId} onChange={e => setAssignStandId(e.target.value)}>
              <option value="">Stand seçin</option>
              {unassignedStandsOptions.map(s => (
                <option key={s.id} value={s.id}>{s.standCode}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label>İşletme</label>
            <select className="select" value={assignBusinessId} onChange={e => setAssignBusinessId(e.target.value)}>
              <option value="">İşletme seçin</option>
              {businesses.map(b => (
                <option key={b.id} value={b.id}>{b.name} ({b.slug})</option>
              ))}
            </select>
          </div>
          <button className="btn btn-secondary" type="button" onClick={assign} disabled={!assignStandId || !assignBusinessId}>Ata</button>
        </div>
      </div>

      {error && !data && <ErrorState message={error} />}
      <DataTable
        mode="server"
        loading={loading}
        rows={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        query={query}
        onQueryChange={setQuery}
        rowKey={s => s.id}
        toolbar={(
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem' }}>
            <input type="checkbox" checked={unassignedOnly} onChange={e => { setUnassignedOnly(e.target.checked); setQuery(q => ({ ...q, page: 1 })); }} />
            Yalnızca atanmamış
          </label>
        )}
        columns={[
          { key: 'standCode', header: 'Stand kodu', sortable: true, getSearchValue: r => `${r.standCode} ${r.businessName ?? ''}`, render: r => <strong>{r.standCode}</strong> },
          { key: 'businessName', header: 'İşletme', sortable: true, getSortValue: r => r.businessName ?? '', render: r => r.businessName ?? <Badge variant="warning">Atanmamış</Badge> },
          { key: 'createdDate', header: 'Oluşturulma', sortable: true, getSortValue: r => new Date(r.createdDate).getTime(), render: r => formatDateTime(r.createdDate) },
          { key: 'isActive', header: 'Durum', sortable: true, getSortValue: r => (r.isActive ? 1 : 0), render: r => r.isActive ? <Badge variant="success">Aktif</Badge> : <Badge variant="muted">Pasif</Badge> },
          {
            key: 'actions',
            header: '',
            render: r => r.businessId ? (
              <button className="btn btn-secondary btn-sm" type="button" onClick={() => adminService.unassignStand(r.id).then(load)}>Ayır</button>
            ) : null
          }
        ]}
      />
    </div>
  );
}
