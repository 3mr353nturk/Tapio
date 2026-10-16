import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { CategorySelect } from '../../components/forms/CategorySelect';
import { LocationFields } from '../../components/forms/LocationFields';
import { authService } from '../../services/authService';
import { setAccessToken } from '../../services/apiClient';

export function RegisterPage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    firstName: '', lastName: '', email: '', phone: '', password: '',
    businessName: '', category: '', businessPhone: '', address: '', city: '', district: ''
  });

  const onChange = (key: string, value: string) => setForm(prev => ({ ...prev, [key]: value }));

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const result = await authService.register(form);
      setAccessToken(result.accessToken);
      navigate('/app');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kayıt başarısız.');
    }
  };

  return (
    <div className="auth-shell">
      <div className="auth-card card">
        <h2 style={{ marginTop: 0 }}>İşletme kaydı</h2>
        <p className="page-description" style={{ marginBottom: '1.25rem' }}>Hesabınızı ve işletmenizi birkaç adımda oluşturun.</p>
        <form onSubmit={onSubmit}>
          <h3 className="card-section-title">Hesap bilgileri</h3>
          <div className="grid-2">
            <div className="form-group"><label>Ad</label><input className="input" value={form.firstName} onChange={e => onChange('firstName', e.target.value)} required /></div>
            <div className="form-group"><label>Soyad</label><input className="input" value={form.lastName} onChange={e => onChange('lastName', e.target.value)} required /></div>
            <div className="form-group"><label>E-posta</label><input className="input" type="email" value={form.email} onChange={e => onChange('email', e.target.value)} required /></div>
            <div className="form-group"><label>Telefon</label><input className="input" value={form.phone} onChange={e => onChange('phone', e.target.value)} required /></div>
            <div className="form-group"><label>Şifre</label><input className="input" type="password" value={form.password} onChange={e => onChange('password', e.target.value)} required minLength={8} /></div>
          </div>
          <h3 className="card-section-title">İşletme bilgileri</h3>
          <div className="grid-2">
            <div className="form-group"><label>İşletme adı</label><input className="input" value={form.businessName} onChange={e => onChange('businessName', e.target.value)} required /></div>
            <CategorySelect value={form.category} onChange={v => onChange('category', v)} required />
            <div className="form-group"><label>İşletme telefonu</label><input className="input" value={form.businessPhone} onChange={e => onChange('businessPhone', e.target.value)} required /></div>
            <LocationFields
              city={form.city}
              district={form.district}
              onCityChange={v => onChange('city', v)}
              onDistrictChange={v => onChange('district', v)}
              required
            />
          </div>
          <div className="form-group"><label>Adres</label><textarea className="textarea" value={form.address} onChange={e => onChange('address', e.target.value)} required /></div>
          {error && <p className="error-text">{error}</p>}
          <button className="btn btn-primary" type="submit">Kayıt ol</button>
        </form>
        <p style={{ marginTop: '1rem' }}>Zaten hesabınız var mı? <Link to="/login">Giriş yapın</Link></p>
      </div>
    </div>
  );
}
