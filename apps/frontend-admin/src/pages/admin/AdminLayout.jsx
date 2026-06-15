import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Building2, Users, Shield, LogOut } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

function AdminLayout() {
  const [expanded, setExpanded] = useState(false);
  const [hoveredPath, setHoveredPath] = useState(null);
  const [logoutHovered, setLogoutHovered] = useState(false);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const initials = user?.name
    ? user.name.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
    : '';

  const items = [
    { to: '/admin/companies', icon: Building2, label: 'Gestión de Empresas', subtext: 'KYB' },
    { to: '/admin/users', icon: Users, label: 'Gestión de Usuarios' },
    { to: '/admin/audit-logs', icon: Shield, label: 'Historial de Auditoría', subtext: 'Solo lectura' },
  ];

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <aside
        onMouseEnter={() => setExpanded(true)}
        onMouseLeave={() => setExpanded(false)}
        style={{
          width: expanded ? '220px' : '60px',
          transition: 'width 220ms cubic-bezier(0.4, 0, 0.2, 1)',
          overflow: 'hidden',
          background: '#ffffff',
          borderRight: '1px solid #e2e8f0',
          display: 'flex',
          flexDirection: 'column',
          height: '100vh',
          flexShrink: 0,
        }}
      >
        <div style={{ height: '60px', display: 'flex', alignItems: 'center', paddingLeft: '16px', borderBottom: '1px solid #e2e8f0' }}>
          <div style={{ opacity: expanded ? 1 : 0, transition: 'opacity 150ms', whiteSpace: 'nowrap' }}>
            <div style={{ fontWeight: 700, fontSize: '14px', color: '#1a6fb5' }}>DeskMatch</div>            
          </div>
        </div>

        <div style={{ padding: '20px 16px 6px', opacity: expanded ? 1 : 0, transition: 'opacity 100ms', whiteSpace: 'nowrap' }}>
          <span style={{ color: '#94a3b8', fontSize: '10px', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.09em' }}>Administración</span>
        </div>

        <nav style={{ flex: 1, padding: '4px 8px' }}>
          {items.map((item) => (
            <NavLink key={item.to} to={item.to}>
              {({ isActive }) => (
                <div
                  onMouseEnter={() => setHoveredPath(item.to)}
                  onMouseLeave={() => setHoveredPath(null)}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '12px',
                    padding: '10px 8px',
                    borderRadius: '8px',
                    marginBottom: '2px',
                    background: isActive ? '#dbeafe' : hoveredPath === item.to ? '#f1f5f9' : 'transparent',
                    overflow: 'hidden',
                    whiteSpace: 'nowrap',
                  }}
                >
                  <item.icon size={18} style={{ color: isActive ? '#3a95df' : '#64748b', flexShrink: 0 }} />
                  <div style={{ opacity: expanded ? 1 : 0, transition: 'opacity 120ms' }}>
                    <div style={{ color: '#334155', fontSize: '13.5px', fontWeight: 400 }}>{item.label}</div>
                    {item.subtext && <div style={{ color: '#94a3b8', fontSize: '11px' }}>{item.subtext}</div>}
                  </div>
                </div>
              )}
            </NavLink>
          ))}
        </nav>

        <div style={{ padding: '8px', borderTop: '1px solid #e2e8f0' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px', padding: '8px' }}>
            <div style={{ width: '30px', height: '30px', borderRadius: '50%', background: '#3a95df', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '11px', fontWeight: 700, color: '#fff', flexShrink: 0 }}>
              {initials}
            </div>
            <div style={{ opacity: expanded ? 1 : 0, transition: 'opacity 120ms', whiteSpace: 'nowrap' }}>
              <div style={{ fontSize: '12.5px', fontWeight: 500, color: '#334155' }}>{user?.name}</div>
              <div style={{ fontSize: '11px', color: '#94a3b8' }}>Administrador</div>
            </div>
          </div>
          <button
            onClick={handleLogout}
            onMouseEnter={() => setLogoutHovered(true)}
            onMouseLeave={() => setLogoutHovered(false)}
            style={{
              width: '100%',
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
              padding: '8px',
              borderRadius: '8px',
              background: logoutHovered ? '#f1f5f9' : 'transparent',
              border: 'none',
              cursor: 'pointer',
            }}
          >
            <LogOut size={16} style={{ color: '#94a3b8', flexShrink: 0 }} />
            <span style={{ opacity: expanded ? 1 : 0, transition: 'opacity 120ms', color: '#64748b', fontSize: '12.5px', whiteSpace: 'nowrap' }}>
              Cerrar sesión
            </span>
          </button>
        </div>
      </aside>
      <main style={{ flex: 1, padding: '32px', backgroundColor: '#f0f4f8', overflow: 'hidden', minWidth: 0 }}>
        <Outlet />
      </main>
    </div>
  );
}

export default AdminLayout;
