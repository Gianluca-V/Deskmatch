import { useState, useRef, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { User, Settings, LogOut, ChevronDown, Calendar } from 'lucide-react';
import './UserMenu.css';

function UserMenu() {
  const { user, logout, isAuthenticated } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef(null);
  const navigate = useNavigate();

  // Cerrar dropdown al hacer clic fuera
  useEffect(() => {
    function handleClickOutside(event) {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  if (!isAuthenticated || !user) {
    return null;
  }

  const handleLogout = () => {
    logout();
    setIsOpen(false);
    navigate('/login');
  };

  const getProfileRoute = () => {
    // Manager = empresa/company user, User = regular user
    return user.role === 'Manager' || user.role === 'Company' || user.role === 'Admin'
      ? '/profile/company'
      : '/profile/user';
  };

  const isCompanyUser = user.role === 'Manager' || user.role === 'Company' || user.role === 'Admin';

  // Obtener inicial del usuario
  const initial = user.firstName ? user.firstName.charAt(0).toUpperCase() : 'U';
  const userName = user.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : user.email;
  const userEmail = user.email;

  return (
    <div className="user-menu" ref={menuRef}>
      <button 
        className="user-menu__trigger"
        onClick={() => setIsOpen(!isOpen)}
        aria-expanded={isOpen}
        aria-label="Menú de usuario"
      >
        <div className="user-menu__avatar">
          {initial}
        </div>
        <div className="user-menu__info">
          <span className="user-menu__name">{userName}</span>
          <ChevronDown size={16} className={`user-menu__chevron ${isOpen ? 'user-menu__chevron--open' : ''}`} />
        </div>
      </button>

      {isOpen && (
        <div className="user-menu__dropdown">
          <div className="user-menu__header">
            <div className="user-menu__header-avatar">
              {initial}
            </div>
            <div>
              <p className="user-menu__header-name">{userName}</p>
              <p className="user-menu__header-email">{userEmail}</p>
              {user?.role && (
                <span className="user-menu__role-badge">
                  {user.role === 'Manager' ? '🏢 Empresa' : user.role === 'Admin' ? '👑 Admin' : '👤 Usuario'}
                </span>
              )}
            </div>
          </div>

          <div className="user-menu__divider"></div>

          <Link
            to={getProfileRoute()}
            className="user-menu__item"
            onClick={() => setIsOpen(false)}
          >
            <User size={18} />
            <span>Mi Perfil</span>
          </Link>

          <Link
            to={isCompanyUser ? '/company/reservations' : '/reservations'}
            className="user-menu__item"
            onClick={() => setIsOpen(false)}
          >
            <Calendar size={18} />
            <span>{isCompanyUser ? 'Reservas recibidas' : 'Mis Reservas'}</span>
          </Link>

          <Link
            to={getProfileRoute()}
            className="user-menu__item"
            onClick={() => setIsOpen(false)}
          >
            <Settings size={18} />
            <span>Configuración</span>
          </Link>

          <div className="user-menu__divider"></div>

          <button 
            className="user-menu__item user-menu__item--logout"
            onClick={handleLogout}
          >
            <LogOut size={18} />
            <span>Cerrar sesión</span>
          </button>
        </div>
      )}
    </div>
  );
}

export default UserMenu;
