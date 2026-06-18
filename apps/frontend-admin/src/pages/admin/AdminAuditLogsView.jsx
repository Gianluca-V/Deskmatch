import { DataGrid } from '@mui/x-data-grid';
import { adminAuditLogsRows } from '../../mock/adminData';

const columns = [
  { field: 'id', headerName: 'ID', width: 60, align: 'center', headerAlign: 'center' },
  { field: 'user', headerName: 'Usuario', flex: 1, minWidth: 140, align: 'center', headerAlign: 'center' },
  { field: 'action', headerName: 'Acción', flex: 1, minWidth: 160, align: 'center', headerAlign: 'center' },
  { field: 'entity', headerName: 'Entidad', flex: 1, minWidth: 100, align: 'center', headerAlign: 'center' },
  { field: 'entityId', headerName: 'ID Entidad', width: 100, align: 'center', headerAlign: 'center' },
  { field: 'ipAddress', headerName: 'IP', width: 130, align: 'center', headerAlign: 'center' },
  { field: 'timestamp', headerName: 'Fecha', width: 160, align: 'center', headerAlign: 'center' },
];

function AdminAuditLogsView() {
  return (
    <section>
      <h1 style={{ fontSize: '22px', fontWeight: 600, color: '#1e2a3a', marginBottom: '4px' }}>Historial de Auditoría</h1>
      <p style={{ fontSize: '13px', color: '#64748b', marginBottom: '24px' }}>Registro de acciones realizadas en el sistema</p>
      <div style={{ width: '100%', background: '#fff', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <DataGrid
          autoHeight
          rows={adminAuditLogsRows}
          columns={columns}
          pageSizeOptions={[5, 10, 25]}
          initialState={{ pagination: { paginationModel: { pageSize: 10, page: 0 } } }}
          disableRowSelectionOnClick
          sx={{
            border: 'none',
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: '#f8fafc',
              borderBottom: '1px solid #e2e8f0',
            },
            '& .MuiDataGrid-cell': {
              borderBottom: '1px solid #f1f5f9',
            },
          }}
        />
      </div>
    </section>
  );
}

export default AdminAuditLogsView;
