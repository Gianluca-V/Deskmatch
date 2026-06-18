import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Login from './pages/Login';
import Register from './pages/Register';
import Forbidden from './pages/Forbidden';
import AdminLayout from './pages/admin/AdminLayout';
import AdminUsersView from './pages/admin/AdminUsersView';
import AdminCompaniesView from './pages/admin/AdminCompaniesView';
import AdminAuditLogsView from './pages/admin/AdminAuditLogsView';
import AdminDashboard from './pages/admin/AdminDashboard';

function ProtectedRoute() {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (user?.role !== 'Admin') {
    return <Forbidden />;
  }

  return <Outlet />;
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AdminLayout />}>
          <Route path="/admin" element={<Navigate to="dashboard" replace />} />
          <Route path="/admin/dashboard" element={<AdminDashboard />} />
          <Route path="/admin/users" element={<AdminUsersView />} />
          <Route path="/admin/companies" element={<AdminCompaniesView />} />
          <Route path="/admin/audit-logs" element={<AdminAuditLogsView />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/admin" replace />} />
    </Routes>
  );
}

export default App;
