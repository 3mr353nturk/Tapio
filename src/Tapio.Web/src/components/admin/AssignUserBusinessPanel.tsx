import { useEffect, useState } from 'react';
import { adminService } from '../../services/adminService';
import type { AdminBusiness, AdminUser } from '../../types/api.types';

type Props = {
  onAssigned?: () => void;
};

export function AssignUserBusinessPanel({ onAssigned }: Props) {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [businesses, setBusinesses] = useState<AdminBusiness[]>([]);
  const [userId, setUserId] = useState('');
  const [businessId, setBusinessId] = useState('');
  const [role, setRole] = useState<'Owner' | 'Manager' | 'Staff'>('Owner');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    Promise.all([
      adminService.getUsers({ page: 1, pageSize: 200 }),
      adminService.getBusinesses({ page: 1, pageSize: 200 })
    ]).then(([u, b]) => {
      setUsers(u.items.filter(x => x.role === 'BusinessUser'));
      setBusinesses(b.items);
    }).catch(() => undefined);
  }, []);

  const assign = async () => {
    setError('');
    setMessage('');
    if (!userId || !businessId) {
      setError('Kullanıcı ve işletme seçin.');
      return;
    }
    try {
      await adminService.assignUserToBusiness(userId, businessId, role);
      setMessage('Kullanıcı işletmeye atandı.');
      setUserId('');
      setBusinessId('');
      onAssigned?.();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Atama başarısız.');
    }
  };

  return (
    <div className="card form-panel">
      <h3 className="card-section-title">Kullanıcı — işletme ata</h3>
      <p style={{ color: 'var(--color-muted)', marginTop: 0 }}>
        Kayıtlı işletme kullanıcısını bir işletmeye bağlayın (davet yerine manuel atama).
      </p>
      <div className="grid-2">
        <div className="form-group">
          <label>İşletme kullanıcısı</label>
          <select className="select" value={userId} onChange={e => setUserId(e.target.value)}>
            <option value="">Kullanıcı seçin</option>
            {users.map(u => (
              <option key={u.id} value={u.id}>{u.firstName} {u.lastName} ({u.email})</option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label>İşletme</label>
          <select className="select" value={businessId} onChange={e => setBusinessId(e.target.value)}>
            <option value="">İşletme seçin</option>
            {businesses.map(b => (
              <option key={b.id} value={b.id}>{b.name} ({b.slug})</option>
            ))}
          </select>
        </div>
        <div className="form-group">
          <label>İşletme rolü</label>
          <select className="select" value={role} onChange={e => setRole(e.target.value as typeof role)}>
            <option value="Owner">Sahip (Owner)</option>
            <option value="Manager">Yönetici (Manager)</option>
            <option value="Staff">Personel (Staff)</option>
          </select>
        </div>
      </div>
      {error && <p className="error-text">{error}</p>}
      {message && <p className="success-text">{message}</p>}
      <button type="button" className="btn btn-secondary" onClick={assign}>Ata</button>
    </div>
  );
}
