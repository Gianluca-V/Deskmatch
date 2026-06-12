import { DataGrid } from '@mui/x-data-grid';
import { adminAuditLogsColumns, adminAuditLogsRows } from '../../mock/adminData';

function AdminAuditLogsView() {
  return (
    <section>
      <h1 style={{ fontSize: '22px', fontWeight: 600, color: '#1e2a3a', marginBottom: '4px' }}>Historial de Auditoría</h1>
      <p style={{ fontSize: '13px', color: '#64748b', marginBottom: '24px' }}>Registro de acciones realizadas en el sistema</p>
      <div style={{ height: 480, width: '100%', background: '#fff', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
        <DataGrid
          rows={adminAuditLogsRows}
          columns={adminAuditLogsColumns}
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
