import { Routes, Route, Navigate, Link } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Login from './pages/Login';
import Register from './pages/Register';

function Sidebar() {
  return (
    <aside style={{ width: '220px', padding: '24px', backgroundColor: 'var(--color-accent)', color: '#ffffff' }}>
      <h2>Admin</h2>
      <nav style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginTop: '24px' }}>
        <Link to="/" style={{ color: '#ffffff', textDecoration: 'none' }}>Dashboard</Link>
        <Link to="/companies" style={{ color: '#ffffff', textDecoration: 'none' }}>Empresas</Link>
        <Link to="/offices" style={{ color: '#ffffff', textDecoration: 'none' }}>Oficinas</Link>
        <Link to="/reservations" style={{ color: '#ffffff', textDecoration: 'none' }}>Reservas</Link>
        <Link to="/users" style={{ color: '#ffffff', textDecoration: 'none' }}>Usuarios</Link>
      </nav>
    </aside>
  );
}

function Dashboard() {
  return (
    <section>
      <h1>Dashboard</h1>
      <p>Bienvenido al panel de administración de DeskMatch.</p>
    </section>
  );
}

function Companies() {
  return (
    <section>
      <h1>Empresas</h1>
      <p>Lista de empresas y datos principales.</p>
    </section>
  );
}

function CompanyDetail() {
  return (
    <section>
      <h1>Detalle de Empresa</h1>
      <p>Información detallada de la empresa seleccionada.</p>
    </section>
  );
}

function Offices() {
  return (
    <section>
      <h1>Oficinas</h1>
      <p>Gestión de oficinas disponibles.</p>
    </section>
  );
}

function OfficeDetail() {
  return (
    <section>
      <h1>Detalle de Oficina</h1>
      <p>Información detallada de la oficina seleccionada.</p>
    </section>
  );
}

function OfficeNew() {
  return (
    <section>
      <h1>Nueva Oficina</h1>
      <p>Formulario para registrar una nueva oficina.</p>
    </section>
  );
}

function Reservations() {
  return (
    <section>
      <h1>Reservas</h1>
      <p>Control y gestión de reservas.</p>
    </section>
  );
}

function Users() {
  return (
    <section>
      <h1>Usuarios</h1>
      <p>Administración de usuarios del sistema.</p>
    </section>
  );
}

function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <Sidebar />
      <main style={{ flex: 1, padding: '32px', backgroundColor: 'var(--color-bg)' }}>
        {children}
      </main>
    </div>
  );
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
      <Route path="/companies" element={<ProtectedRoute><Companies /></ProtectedRoute>} />
      <Route path="/companies/:id" element={<ProtectedRoute><CompanyDetail /></ProtectedRoute>} />
      <Route path="/offices" element={<ProtectedRoute><Offices /></ProtectedRoute>} />
      <Route path="/offices/new" element={<ProtectedRoute><OfficeNew /></ProtectedRoute>} />
      <Route path="/offices/:id" element={<ProtectedRoute><OfficeDetail /></ProtectedRoute>} />
      <Route path="/reservations" element={<ProtectedRoute><Reservations /></ProtectedRoute>} />
      <Route path="/users" element={<ProtectedRoute><Users /></ProtectedRoute>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
