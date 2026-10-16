import { Link } from 'react-router-dom';

export function LandingPage() {
  return (
    <div className="container" style={{ paddingTop: '4rem', textAlign: 'center' }}>
      <h1>Tapio</h1>
      <p style={{ color: 'var(--color-muted)', maxWidth: 520, margin: '0 auto 2rem' }}>
        NFC + QR stand ile müşterilerinizi dijital sayfanıza yönlendirin.
      </p>
      <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
        <Link className="btn btn-primary" to="/register">Kayıt Ol</Link>
        <Link className="btn btn-secondary" to="/login">Giriş Yap</Link>
      </div>
    </div>
  );
}
