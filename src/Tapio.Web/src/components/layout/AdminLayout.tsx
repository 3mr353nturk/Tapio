import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const nav = [
  { to: '/admin', label: 'Genel Bakış', end: true },
  { to: '/admin/businesses', label: 'İşletmeler' },
  { to: '/admin/users', label: 'Kullanıcılar' },
  { to: '/admin/stands', label: 'Standlar' }
];

export function AdminLayout() {
  const { user, logout } = useAuth();
  return (
    <div className="dashboard">
      <aside className="sidebar card">
        <div className="sidebar-brand">Tapio</div>
        <div className="sidebar-sub">Yönetim paneli</div>
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
      <main className="dashboard-content container"><Outlet /></main>
    </div>
  );
}
