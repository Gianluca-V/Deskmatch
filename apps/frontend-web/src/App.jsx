import { Routes, Route, Navigate } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import Login from './pages/Login';
import Register from './pages/Register';
import RegisterCompany from './pages/RegisterCompany';
import RegisterType from './pages/RegisterType';
import Home from './pages/Home';
import Offices from './pages/Offices';
import MySpaces from './pages/MySpaces';
import Profile from './pages/Profile';
import Reservations from './pages/Reservations';
import WorkspaceDetail from './pages/WorkspaceDetail';

function Dashboard() {
  return (
    <section>
      <h1>Dashboard</h1>
      <p>Resumen de tu actividad y accesos recientes.</p>
    </section>
  );
}

function Spaces() {
  return (
    <section>
      <h1>Mis Espacios</h1>
      <p>Gestiona los espacios de tu empresa.</p>
    </section>
  );
}

function ManageCompany() {
  return (
    <section>
      <h1>Gestionar Empresa</h1>
      <p>Administra la información de tu empresa.</p>
    </section>
  );
}

function Analytics() {
  return (
    <section>
      <h1>Analytics</h1>
      <p>Métricas y análisis de tu negocio.</p>
    </section>
  );
}

function App() {
  const { isAuthenticated } = useAuth();
  
  const ProtectedRoute = ({ children }) => {
    return isAuthenticated ? children : <Navigate to="/login" replace />;
  };
  
  return (
    <>
      <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh', backgroundColor: 'var(--color-bg)' }}>
        <Navbar />
        <main style={{ flex: 1, padding: '24px' }}>
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/offices" element={<Offices />} />
            <Route path="/offices/:id" element={<WorkspaceDetail />} />
            <Route path="/workspaces/:id" element={<WorkspaceDetail />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<RegisterType />} />
            <Route path="/register/user" element={<Register />} />
            <Route path="/register/company" element={<RegisterCompany />} />
            <Route path="/dashboard" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
            <Route path="/my-spaces" element={<ProtectedRoute><MySpaces /></ProtectedRoute>} />
            <Route path="/spaces" element={<ProtectedRoute><Spaces /></ProtectedRoute>} />
            <Route path="/manage-company" element={<ProtectedRoute><ManageCompany /></ProtectedRoute>} />
            <Route path="/analytics" element={<ProtectedRoute><Analytics /></ProtectedRoute>} />
            <Route path="/reservations" element={<ProtectedRoute><Reservations /></ProtectedRoute>} />
            <Route path="/profile/user" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
            <Route path="/profile/company" element={<ProtectedRoute><Profile /></ProtectedRoute>} />
          </Routes>
        </main>
        <footer style={{ padding: '12px 16px', textAlign: 'center', backgroundColor: 'transparent', color: 'var(--color-muted)' }}>
          <p style={{ fontSize: '13px', margin: 0 }}>DeskMatch - Sitio web de búsqueda de espacios</p>
        </footer>
      </div>
      <ToastContainer
        position="bottom-right"
        autoClose={3000}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
      />
    </>
  );
}

export default App;
