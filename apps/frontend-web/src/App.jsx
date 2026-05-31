import { Routes, Route, Link } from 'react-router-dom';

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

function Login() {
  return (
    <section>
      <h1>Iniciar sesión</h1>
      <p>Ingresa tus credenciales para continuar.</p>
    </section>
  );
}

function Register() {
  return (
    <section>
      <h1>Registrarse</h1>
      <p>Crea una nueva cuenta para usar DeskMatch.</p>
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
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <header style={{ padding: '16px', backgroundColor: '#0f172a', color: '#f8fafc' }}>
        <nav style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
          <Link to="/" style={{ color: '#f8fafc', textDecoration: 'none' }}>Home</Link>
          <Link to="/offices" style={{ color: '#f8fafc', textDecoration: 'none' }}>Oficinas</Link>
          <Link to="/dashboard" style={{ color: '#f8fafc', textDecoration: 'none' }}>Dashboard</Link>
          <Link to="/login" style={{ color: '#f8fafc', textDecoration: 'none' }}>Login</Link>
          <Link to="/register" style={{ color: '#f8fafc', textDecoration: 'none' }}>Registrarse</Link>
        </nav>
      </header>
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
      <footer style={{ padding: '16px', textAlign: 'center', backgroundColor: '#e2e8f0' }}>
        <p>DeskMatch - Sitio web de búsqueda de espacios</p>
      </footer>
    </div>
  );
}

export default App;
