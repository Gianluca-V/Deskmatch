import { Routes, Route, Link, useLocation, Navigate } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Login from './pages/Login';
import Register from './pages/Register';
import RegisterCompany from './pages/RegisterCompany';
import RegisterType from './pages/RegisterType';
import Home from './pages/Home';
import Offices from './pages/Offices';


function OfficeDetail() {
  return (
    <section>
      <h1>Detalle de la Oficina</h1>
      <p>Información detallada de la oficina seleccionada.</p>
    </section>
  );
}

function Dashboard() {
  return (
    <section>
      <h1>Dashboard</h1>
      <p>Resumen de tu actividad y accesos recientes.</p>
    </section>
  );
}

function App() {
  const location = useLocation();  
  const hideNavbar = ['/', '/login', '/register', '/register/user', '/register/company'].includes(location.pathname);
  
  const ProtectedRoute = ({ children }) => {
    const { isAuthenticated } = useAuth();
    return isAuthenticated ? children : <Navigate to="/login" replace />;
  };
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh', backgroundColor: 'var(--color-bg)' }}>
      {!hideNavbar && (
        <header style={{ padding: '16px', backgroundColor: 'var(--color-primary)', color: '#ffffff' }}>
          <nav style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
            <Link to="/" style={{ color: '#ffffff', textDecoration: 'none' }}>Home</Link>
            <Link to="/offices" style={{ color: '#ffffff', textDecoration: 'none' }}>Oficinas</Link>
            <Link to="/dashboard" style={{ color: '#ffffff', textDecoration: 'none' }}>Dashboard</Link>
            <Link to="/login" style={{ color: '#ffffff', textDecoration: 'none' }}>Login</Link>
            <Link to="/register" style={{ color: '#ffffff', textDecoration: 'none' }}>Registrarse</Link>
          </nav>
        </header>
      )}
      <main style={{ flex: 1, padding: '24px' }}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/offices" element={<Offices />} />
          <Route path="/offices/:id" element={<OfficeDetail />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<RegisterType />} />
          <Route path="/register/user" element={<Register />} />
          <Route path="/register/company" element={<RegisterCompany />} />
          <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
        </Routes>
      </main>
      <footer style={{ padding: '12px 16px', textAlign: 'center', backgroundColor: 'transparent', color: 'var(--color-muted)' }}>
        <p style={{ fontSize: '13px', margin: 0 }}>DeskMatch - Sitio web de búsqueda de espacios</p>
      </footer>
    </div>
  );
}

export default App;
