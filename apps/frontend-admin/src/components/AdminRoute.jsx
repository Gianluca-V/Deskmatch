import { Outlet } from 'react-router-dom';

function AdminRoute() {
  // TODO: reemplazar por verificación real de rol
  const isAdmin = true;
  if (!isAdmin) return null;
  return <Outlet />;
}

export default AdminRoute;
