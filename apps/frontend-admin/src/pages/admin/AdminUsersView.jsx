import { useEffect, useRef, useState } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { adminUsersRows } from '../../mock/adminData';

const columns = [
  { field: 'id', headerName: 'ID', width: 70 },
  {
    field: 'name',
    headerName: 'Nombre',
    flex: 1,
    minWidth: 140,
    renderCell: (params) => (
      <span style={{ color: '#1e2a3a', fontWeight: 600, fontSize: '13.5px' }}>{params.value}</span>
    ),
  },
  {
    field: 'email',
    headerName: 'Email',
    flex: 1,
    minWidth: 180,
    renderCell: (params) => (
      <span style={{ color: '#475569', fontSize: '13px' }}>{params.value}</span>
    ),
  },
  {
    field: 'role',
    headerName: 'Rol',
    flex: 1,
    minWidth: 100,
    renderCell: (params) => (
      <span className="badge badge--role">{params.value}</span>
    ),
  },
  {
    field: 'status',
    headerName: 'Estado',
    flex: 1,
    minWidth: 120,
    renderCell: (params) => {
      const isActive = params.value === 'Activo';
      return (
        <span className={`badge ${isActive ? 'badge--verified' : 'badge--unverified'}`}>
          {isActive ? 'Activo' : 'Suspendido'}
        </span>
      );
    },
  },
];

function AdminUsersView() {
  const containerRef = useRef(null);
  const [, forceRender] = useState(0);

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    const observer = new ResizeObserver(() => forceRender((n) => n + 1));
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <section className="admin-view">
      <div className="admin-view__title-label">
        ADMINISTRACIÓN
      </div>
      <h1 className="admin-view__title">
        Gestión de Usuarios
      </h1>
      <p className="admin-view__subtitle">
        Usuarios registrados en la plataforma
      </p>
      <div ref={containerRef} className="admin-table-container">
        <DataGrid
          autoHeight
          rows={adminUsersRows}
          columns={columns}
          pageSizeOptions={[5, 10, 25]}
          initialState={{ pagination: { paginationModel: { pageSize: 10, page: 0 } } }}
          disableRowSelectionOnClick
          disableColumnFilter
          disableColumnMenu
          hideFooterSelectedRowCount
          sx={{
            border: 'none',
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: '#f8fafc',
              borderBottom: '1px solid #e2e8f0',
            },
            '& .MuiDataGrid-columnHeaderTitle': {
              color: '#64748b',
              fontWeight: 700,
              fontSize: '11px',
              textTransform: 'uppercase',
              letterSpacing: '0.05em',
            },
            '& .MuiDataGrid-cell': {
              borderBottom: '1px solid #f1f5f9',
            },
            '& .MuiDataGrid-row:hover': {
              backgroundColor: '#f8fafc',
            },
          }}
        />
      </div>
    </section>
  );
}

export default AdminUsersView;
