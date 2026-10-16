import { FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const user = await login(email, password);
      navigate(user.role === 'Admin' ? '/admin' : '/app');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giriş başarısız.');
    }
  };

  return (
    <div className="auth-shell">
      <div className="auth-card card" style={{ maxWidth: 420 }}>
        <h2 style={{ marginTop: 0 }}>Giriş yap</h2>
        <form onSubmit={onSubmit}>
          <div className="form-group">
            <label>E-posta</label>
            <input className="input" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
          </div>
          <div className="form-group">
            <label>Şifre</label>
            <input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)} required />
          </div>
          {error && <p className="error-text">{error}</p>}
          <button className="btn btn-primary" style={{ width: '100%' }} type="submit">Giriş</button>
        </form>
        <p style={{ marginTop: '1rem' }}>
          <Link to="/forgot-password">Şifremi unuttum</Link>
        </p>
        <p>Hesabınız yok mu? <Link to="/register">Kayıt olun</Link></p>
      </div>
    </div>
  );
}
