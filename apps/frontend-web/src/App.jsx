import { Routes, Route, Link, useLocation } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';

function Home() {
  return (
    <section>
      <h1>Bienvenido a DeskMatch</h1>
      <p>Encuentra el espacio de trabajo ideal para ti.</p>
    </section>
  );
}

function Offices() {
  return (
    <section>
      <h1>Oficinas</h1>
      <p>Aquí encontrarás tus opciones de oficinas disponibles.</p>
    </section>
  );
}

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
  const hideNavbar = ['/login', '/register'].includes(location.pathname);
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
          <Route path="/register" element={<Register />} />
          <Route path="/dashboard" element={<Dashboard />} />
        </Routes>
      </main>
      <footer style={{ padding: '16px', textAlign: 'center', backgroundColor: 'var(--color-secondary)', color: 'var(--color-text)' }}>
        <p>DeskMatch - Sitio web de búsqueda de espacios</p>
      </footer>
    </div>
  );
}

export default App;
