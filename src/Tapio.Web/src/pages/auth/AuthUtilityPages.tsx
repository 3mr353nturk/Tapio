import { FormEvent, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '../../services/authService';

export function VerifyEmailPage() {
  const [search] = useSearchParams();
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const verify = async () => {
    const token = search.get('token');
    if (!token) { setError('Token bulunamadı.'); return; }
    try {
      await authService.verifyEmail(token);
      setMessage('E-posta doğrulandı.');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Doğrulama başarısız.');
    }
  };

  return (
    <div className="container card" style={{ maxWidth: 420, marginTop: '3rem' }}>
      <h2>E-posta Doğrulama</h2>
      <button className="btn btn-primary" onClick={verify}>Doğrula</button>
      {message && <p>{message}</p>}
      {error && <p className="error-text">{error}</p>}
    </div>
  );
}

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    await authService.forgotPassword(email);
    setMessage('Şifre sıfırlama bağlantısı gönderildi.');
  };
  return (
    <div className="container card" style={{ maxWidth: 420, marginTop: '3rem' }}>
      <h2>Şifremi Unuttum</h2>
      <form onSubmit={onSubmit}>
        <input className="input" type="email" value={email} onChange={e => setEmail(e.target.value)} required />
        <button className="btn btn-primary" style={{ marginTop: '1rem' }} type="submit">Gönder</button>
      </form>
      {message && <p>{message}</p>}
      <Link to="/login">Girişe dön</Link>
    </div>
  );
}

export function ResetPasswordPage() {
  const [search] = useSearchParams();
  const navigate = useNavigate();
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const token = search.get('token');
    if (!token) { setError('Token bulunamadı.'); return; }
    try {
      await authService.resetPassword(token, password);
      navigate('/login');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'İşlem başarısız.');
    }
  };
  return (
    <div className="container card" style={{ maxWidth: 420, marginTop: '3rem' }}>
      <h2>Yeni Şifre</h2>
      <form onSubmit={onSubmit}>
        <input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={8} />
        {error && <p className="error-text">{error}</p>}
        <button className="btn btn-primary" style={{ marginTop: '1rem' }} type="submit">Güncelle</button>
      </form>
    </div>
  );
}

