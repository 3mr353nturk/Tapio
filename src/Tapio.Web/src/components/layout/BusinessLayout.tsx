import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const nav = [
  { to: '/app', label: 'Genel Bakış', end: true },
  { to: '/app/share', label: 'Paylaşım' },
  { to: '/app/profile', label: 'Profil' },
  { to: '/app/actions', label: 'Aksiyonlar' },
  { to: '/app/campaigns', label: 'Kampanyalar' },
  { to: '/app/stands', label: 'Standlar' },
  { to: '/app/analytics', label: 'Analitik' }
];

export function BusinessLayout() {
  const { user, logout } = useAuth();

  return (
    <div className="dashboard">
      <aside className="sidebar card">
        <div className="sidebar-brand">Tapio</div>
        <div className="sidebar-sub">İşletme paneli</div>
        <nav className="sidebar-nav">
          {nav.map(item => (
            <NavLink key={item.to} to={item.to} end={item.end} className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div>{user?.email}</div>
          <button className="btn btn-secondary" style={{ width: '100%', marginTop: '0.5rem' }} onClick={() => logout()}>Çıkış</button>
        </div>
      </aside>
      <main className="dashboard-content container">
        <Outlet />
      </main>
      <nav className="mobile-nav">
        {nav.slice(0, 5).map(item => (
          <NavLink key={item.to} to={item.to} end={item.end} className={({ isActive }) => isActive ? 'active' : ''}>{item.label}</NavLink>
        ))}
      </nav>
    </div>
  );
}
