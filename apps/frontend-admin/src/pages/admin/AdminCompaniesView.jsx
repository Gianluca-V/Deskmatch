import { useCallback, useEffect, useRef, useState } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import api from '../../lib/api';

const ActionsCell = ({ row, onVerify, onRevoke }) => {
  return row.isVerified ? (
    <button
      onClick={() => onRevoke(row)}
      style={{
        background: 'transparent',
        color: '#991b1b',
        border: '1.5px solid #fca5a5',
        borderRadius: '4px',
        padding: '4px 12px',
        cursor: 'pointer',
        fontSize: '12px',
        fontWeight: 600,
      }}
    >
      Revocar
    </button>
  ) : (
    <button
      onClick={() => onVerify(row)}
      style={{
        background: '#3a95df',
        color: '#fff',
        border: 'none',
        borderRadius: '4px',
        padding: '4px 12px',
        cursor: 'pointer',
        fontSize: '12px',
        fontWeight: 600,
      }}
    >
      Verificar
    </button>
  );
};

function AdminCompaniesView() {
  const containerRef = useRef(null);
  const [, forceRender] = useState(0);
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
  const [modal, setModal] = useState({ open: false, company: null, action: 'verify' });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-companies', paginationModel],
    queryFn: async () => {
      const res = await api.get('/api/admin/companies', {
        params: {
          skip: paginationModel.page * paginationModel.pageSize,
          take: paginationModel.pageSize,
        },
      });
      return res.data;
    },
  });

  const rows = data?.items ?? [];
  const rowCount = data?.total ?? 0;

  const queryClient = useQueryClient();

  const toggleMutation = useMutation({
    mutationFn: async ({ id }) => {
      const res = await api.put(`/api/admin/companies/${id}/toggle-verification`, null);
      return res.data;
    },
    onSuccess: (data) => {
      toast.success(data.isVerified ? 'Empresa verificada' : 'Verificación revocada');
      queryClient.invalidateQueries({ queryKey: ['admin-companies'] });
      setModal({ open: false, company: null, action: 'verify' });
      setIsSubmitting(false);
    },
    onError: (error) => {
      if (error.response?.status === 401 || error.response?.status === 403) {
        toast.error('No tenés permisos para realizar esta acción');
      } else if (error.response?.status === 404) {
        toast.error('Empresa no encontrada');
      } else {
        toast.error('Error al cambiar la verificación de la empresa');
      }
      setIsSubmitting(false);
    },
  });

  const handleVerify = useCallback((company) => {
    setModal({ open: true, company, action: 'verify' });
  }, []);

  const handleRevoke = useCallback((company) => {
    setModal({ open: true, company, action: 'revoke' });
  }, []);

  const handleConfirm = useCallback(() => {
    setIsSubmitting(true);
    toggleMutation.mutate({ id: modal.company.id });
  }, [modal]);

  const handleCloseModal = useCallback(() => {
    if (isSubmitting) return;
    setModal({ open: false, company: null, action: 'verify' });
  }, [isSubmitting]);

  const columns = [
    { field: 'id', headerName: 'ID', width: 70, align: 'center', headerAlign: 'center' },
    {
      field: 'name',
      headerName: 'Nombre',
      flex: 1,
      minWidth: 140,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <span style={{ color: '#1e2a3a', fontWeight: 600, fontSize: '13.5px' }}>{params.value}</span>
      ),
    },
    // TODO: agregar columna Owner cuando el backend incluya el nombre del propietario
    {
      field: 'contactEmail',
      headerName: 'Contacto',
      flex: 1,
      minWidth: 150,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <span style={{ color: '#475569', fontSize: '13px' }}>{params.value ?? '-'}</span>
      ),
    },
    {
      field: 'verification',
      headerName: 'Verificación',
      width: 150,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <span className={`badge ${params.row.isVerified ? 'badge--verified' : 'badge--unverified'}`}>
          {params.row.isVerified ? 'Verificada' : 'No Verificada'}
        </span>
      ),
    },
    {
      field: 'kybSubmittedAt',
      headerName: 'Creado',
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => {
        if (!params.value) return <span style={{ color: '#94a3b8' }}>-</span>;
        const d = new Date(params.value);
        const day = String(d.getDate()).padStart(2, '0');
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const year = d.getFullYear();
        return <span style={{ color: '#64748b', fontSize: '13px' }}>{`${day}/${month}/${year}`}</span>;
      },
    },
    {
      field: 'acciones',
      headerName: 'Acciones',
      width: 110,
      sortable: false,
      filterable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <ActionsCell row={params.row} onVerify={handleVerify} onRevoke={handleRevoke} />
      ),
    },
  ];

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    const observer = new ResizeObserver(() => forceRender((n) => n + 1));
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  const isVerify = modal.action === 'verify';

  return (
    <section className="admin-view">
      <div className="admin-view__title-label">
        KYB — VERIFICACIÓN
      </div>
      <h1 className="admin-view__title">
        Gestión de Empresas
      </h1>
      <p className="admin-view__subtitle">
        Empresas registradas y su estado de verificación
      </p>
      <div ref={containerRef} className="admin-table-container">
        <DataGrid
          autoHeight
          rows={rows}
          columns={columns}
          rowCount={rowCount}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[5, 10, 25]}
          disableRowSelectionOnClick
          disableColumnFilter
          disableColumnMenu
          hideFooterSelectedRowCount
          loading={isLoading}
          getRowId={(row) => row.id}
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

      {modal.open && (
        <div
          style={{
            position: 'fixed',
            inset: 0,
            background: 'rgba(0,0,0,0.4)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1300,
          }}
          onClick={handleCloseModal}
        >
          <div
            style={{
              background: '#fff',
              borderRadius: '8px',
              padding: '24px',
              minWidth: '400px',
              boxShadow: '0 4px 24px rgba(0,0,0,0.15)',
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <h2 style={{ margin: '0 0 16px', fontSize: '18px', fontWeight: 700, color: '#1e2a3a' }}>
              {isVerify ? 'Verificar empresa' : 'Revocar verificación'}
            </h2>
            <p style={{ margin: '0 0 24px', fontSize: '14px', color: '#475569' }}>
              ¿Estás seguro de que deseas {isVerify ? 'verificar' : 'revocar la verificación de'}{' '}
              <strong>{modal.company?.name}</strong>?
            </p>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <button
                onClick={handleCloseModal}
                disabled={isSubmitting}
                style={{
                  background: 'transparent',
                  color: '#64748b',
                  border: '1px solid #cbd5e1',
                  borderRadius: '6px',
                  padding: '8px 20px',
                  cursor: 'pointer',
                  fontSize: '14px',
                  fontWeight: 600,
                }}
              >
                Cancelar
              </button>
              <button
                onClick={handleConfirm}
                disabled={isSubmitting}
                style={{
                  background: isVerify ? '#3a95df' : '#991b1b',
                  color: '#fff',
                  border: 'none',
                  borderRadius: '6px',
                  padding: '8px 20px',
                  cursor: 'pointer',
                  fontSize: '14px',
                  fontWeight: 600,
                  opacity: isSubmitting ? 0.6 : 1,
                }}
              >
                {isSubmitting ? 'Procesando...' : 'Confirmar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}

export default AdminCompaniesView;
