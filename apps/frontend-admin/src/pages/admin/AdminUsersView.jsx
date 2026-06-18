import { useCallback, useEffect, useRef, useState } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Button } from '@mui/material';
import { toast } from 'react-hot-toast';
import { adminUsersRows as initialRows } from '../../mock/adminData';

const ACTION_TYPES = { SUSPEND: 'suspend', ACTIVATE: 'activate' };

const modalStyle = {
  backdrop: {
    position: 'fixed',
    inset: 0,
    backgroundColor: 'rgba(0,0,0,0.45)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 1300,
  },
  dialog: {
    backgroundColor: '#fff',
    borderRadius: '12px',
    boxShadow: '0 20px 60px rgba(0,0,0,0.15)',
    width: '440px',
    maxWidth: '90vw',
    outline: 'none',
  },
  title: {
    fontSize: '18px',
    fontWeight: 700,
    color: '#1e2a3a',
    padding: '24px 24px 0',
    margin: 0,
  },
  content: {
    padding: '16px 24px',
    fontSize: '14px',
    color: '#475569',
  },
  textarea: {
    width: '100%',
    minHeight: '80px',
    padding: '10px 12px',
    fontSize: '13px',
    border: '1.5px solid #e2e8f0',
    borderRadius: '8px',
    resize: 'vertical',
    fontFamily: 'inherit',
    outline: 'none',
    boxSizing: 'border-box',
    marginTop: '12px',
  },
  actions: {
    padding: '16px 24px 24px',
    display: 'flex',
    justifyContent: 'flex-end',
    gap: '10px',
  },
};

function ConfirmModal({ modal, onClose, onConfirm, isSubmitting }) {
  if (!modal.open) return null;

  const isSuspend = modal.action === ACTION_TYPES.SUSPEND;
  const title = isSuspend ? 'Suspender usuario' : 'Reactivar usuario';
  const confirmColor = isSuspend ? '#dc2626' : '#3a95df';
  const canConfirm = !isSuspend || modal.reason.trim().length > 0;

  return (
    <div style={modalStyle.backdrop} onClick={onClose}>
      <div style={modalStyle.dialog} onClick={(e) => e.stopPropagation()}>
        <h2 style={modalStyle.title}>{title}</h2>
        <div style={modalStyle.content}>
          <p style={{ margin: 0 }}>¿Estás seguro de que querés {isSuspend ? 'suspender' : 'reactivar'} a <strong>{modal.user?.name}</strong>?</p>
          {isSuspend && (
            <div>
              <label style={{ display: 'block', marginTop: '16px', fontSize: '13px', fontWeight: 600, color: '#1e2a3a' }}>
                Motivo de la suspensión
              </label>
              <textarea
                style={modalStyle.textarea}
                placeholder="Describí el motivo..."
                value={modal.reason}
                onChange={(e) => modal.onReasonChange(e.target.value)}
              />
            </div>
          )}
        </div>
        <div style={modalStyle.actions}>
          <Button
            onClick={onClose}
            disabled={isSubmitting}
            sx={{ color: '#64748b', textTransform: 'none', fontWeight: 600 }}
          >
            Cancelar
          </Button>
          <Button
            onClick={onConfirm}
            disabled={!canConfirm || isSubmitting}
            variant="contained"
            sx={{
              backgroundColor: confirmColor,
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { backgroundColor: confirmColor, opacity: 0.9 },
              '&.Mui-disabled': { backgroundColor: '#e2e8f0', color: '#94a3b8' },
            }}
          >
            {isSubmitting ? 'Procesando...' : 'Confirmar'}
          </Button>
        </div>
      </div>
    </div>
  );
}

function AdminUsersView() {
  const containerRef = useRef(null);
  const [, forceRender] = useState(0);
  const [rows, setRows] = useState(initialRows);
  const [modal, setModal] = useState({ open: false, user: null, action: ACTION_TYPES.SUSPEND, reason: '' });
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;
    const observer = new ResizeObserver(() => forceRender((n) => n + 1));
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  const handleOpenModal = useCallback((user, action) => {
    setModal({ open: true, user, action, reason: '' });
  }, []);

  const handleCloseModal = useCallback(() => {
    if (isSubmitting) return;
    setModal({ open: false, user: null, action: ACTION_TYPES.SUSPEND, reason: '' });
  }, [isSubmitting]);

  const handleReasonChange = useCallback((reason) => {
    setModal((prev) => ({ ...prev, reason }));
  }, []);

  const handleConfirm = useCallback(() => {
    setIsSubmitting(true);
    // TODO: reemplazar por mutación real con TanStack Query, enviando { userId, reason } cuando el backend esté listo
    setTimeout(() => {
      setRows((prev) =>
        prev.map((u) =>
          u.id === modal.user.id
            ? { ...u, status: modal.action === ACTION_TYPES.SUSPEND ? 'Suspendido' : 'Activo' }
            : u
        )
      );
      toast.success(modal.action === ACTION_TYPES.SUSPEND ? 'Usuario suspendido' : 'Usuario reactivado');
      setIsSubmitting(false);
      setModal({ open: false, user: null, action: ACTION_TYPES.SUSPEND, reason: '' });
    }, 500);
  }, [modal]);

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
    {
      field: 'email',
      headerName: 'Email',
      flex: 1,
      minWidth: 180,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <span style={{ color: '#475569', fontSize: '13px' }}>{params.value}</span>
      ),
    },
    {
      field: 'role',
      headerName: 'Rol',
      flex: 1,
      minWidth: 100,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => (
        <span className="badge badge--role">{params.value}</span>
      ),
    },
    {
      field: 'status',
      headerName: 'Estado',
      flex: 1,
      minWidth: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => {
        const isActive = params.value === 'Activo';
        return (
          <span className={`badge ${isActive ? 'badge--verified' : 'badge--unverified'}`}>
            {isActive ? 'Activo' : 'Suspendido'}
          </span>
        );
      },
    },
    {
      field: 'actions',
      headerName: 'Acciones',
      width: 130,
      sortable: false,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params) => {
        const isActive = params.row.status === 'Activo';
        return (
          <button
            onClick={() => handleOpenModal(params.row, isActive ? ACTION_TYPES.SUSPEND : ACTION_TYPES.ACTIVATE)}
            style={{
              background: isActive ? 'transparent' : '#3a95df',
              color: isActive ? '#991b1b' : '#fff',
              border: isActive ? '1.5px solid #fca5a5' : 'none',
              borderRadius: '6px',
              padding: '4px 14px',
              fontSize: '12px',
              fontWeight: 600,
              cursor: 'pointer',
            }}
          >
            {isActive ? 'Suspender' : 'Activar'}
          </button>
        );
      },
    },
  ];

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
          rows={rows}
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
      <ConfirmModal
        modal={{ ...modal, onReasonChange: handleReasonChange }}
        onClose={handleCloseModal}
        onConfirm={handleConfirm}
        isSubmitting={isSubmitting}
      />
    </section>
  );
}

export default AdminUsersView;
