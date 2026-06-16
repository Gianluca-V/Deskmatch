import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import UserMenu from './UserMenu';
import './Navbar.css';

function Navbar() {
  const location = useLocation();
  const { isAuthenticated, user } = useAuth();

  const hideNavbar = ['/', '/login', '/register', '/register/user', '/register/company'].includes(location.pathname);

  if (hideNavbar) {
    return null;
  }

  // Determinar el tipo de usuario
  const isCompany = user?.role === 'Manager' || user?.role === 'Company' || user?.role === 'Admin';

  return (
    <header className="navbar">
      <nav className="navbar__container">
        {/* Logo/Home */}
        <Link to={isAuthenticated ? '/dashboard' : '/'} className="navbar__logo">
          DeskMatch
        </Link>

        {/* Navigation Links - Center */}
        <div className="navbar__links">
          <Link to="/offices" className="navbar__link">Oficinas</Link>
          {isAuthenticated && (
            <>
              {isCompany ? (
                <>
                  <Link to="/my-spaces" className="navbar__link">Mis Espacios</Link>
                  <Link to="/reservations" className="navbar__link">Reservas</Link>
                  <Link to="/dashboard" className="navbar__link">Dashboard</Link>
                  <Link to="/analytics" className="navbar__link">Analytics</Link>
                </>
              ) : (
                <Link to="/dashboard" className="navbar__link">Dashboard</Link>
              )}
            </>
          )}
        </div>

        {/* Auth Actions - Right */}
        <div className="navbar__actions">
          {isAuthenticated ? (
            <UserMenu />
          ) : (
            <>
              <Link to="/login" className="navbar__btn navbar__btn--text">
                Iniciar Sesión
              </Link>
              <Link to="/register" className="navbar__btn navbar__btn--primary">
                Registrarse
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
}

export default Navbar;
