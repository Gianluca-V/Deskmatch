import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import OfficeModal from '../components/OfficeModal';
import ConfirmDeleteModal from '../components/ConfirmDeleteModal';
import { useMyCompany } from '../hooks/useMyCompany';
import { useWorkspacesByCompany } from '../hooks/useWorkspacesByCompany';
import { useDeleteOffice } from '../hooks/useDeleteOffice';
import { useCompanyReservationsSummary } from '../hooks/useCompanyReservations';
import { useAuth } from '../context/AuthContext';

const STATUS_LABELS = {
  active: 'Activo',
  inactive: 'Inactivo',
};

const STATUS_CLASS = {
  active: 'my-spaces__badge--active',
  inactive: 'my-spaces__badge--inactive',
};

function MySpaces() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingSpace, setEditingSpace] = useState(null);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [spaceToDelete, setSpaceToDelete] = useState(null);

  function handleEdit(space) {
    setEditingSpace(space);
    setModalOpen(true);
  }

  function handleCloseModal() {
    setModalOpen(false);
    setEditingSpace(null);
  }

  function handleOpenDeleteModal(space) {
    setSpaceToDelete(space);
    setDeleteModalOpen(true);
  }

  function handleCloseDeleteModal() {
    setDeleteModalOpen(false);
    setSpaceToDelete(null);
  }

  function handleConfirmDelete() {
    if (spaceToDelete) {
      deleteSpace(spaceToDelete.id);
      handleCloseDeleteModal();
    }
  }

  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const canDelete = isAdmin || user?.role === 'Manager';
  const { data: company } = useMyCompany();
  const companyId = company?.id;

  const { data: spaces = [], isLoading } = useWorkspacesByCompany(companyId);
  const { data: reservationSummary } = useCompanyReservationsSummary();
  const { mutate: deleteSpace } = useDeleteOffice();

  const filteredSpaces = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return spaces.filter((space) => {
      const matchesQuery =
        !query ||
        space.name.toLowerCase().includes(query) ||
        (space.address ?? '').toLowerCase().includes(query);
      const matchesStatus =
        statusFilter === 'all' ||
        (statusFilter === 'active' && space.isActive) ||
        (statusFilter === 'inactive' && !space.isActive);
      return matchesQuery && matchesStatus;
    });
  }, [spaces, searchQuery, statusFilter]);

  const totalActive = spaces.filter((s) => s.isActive).length;

  return (
    <section className="my-spaces page-container">
      <header className="my-spaces__header">
        <div>
          <p className="my-spaces__eyebrow">Panel de empresa</p>
          <h1 className="my-spaces__title">Mis Espacios</h1>
          <p className="my-spaces__subtitle">Gestiona tus oficinas y espacios publicados desde un solo lugar.</p>
        </div>
        <button type="button" className="btn btn-primary" onClick={() => setModalOpen(true)}>
          Publicar nuevo espacio
        </button>
      </header>

      <section className="my-spaces__metrics" aria-label="Resumen de espacios">
        <article className="my-spaces__metric-card">
          <p>Total de espacios</p>
          <strong>{spaces.length}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Espacios activos</p>
          <strong>{totalActive}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Reservas este mes</p>
          <strong>{reservationSummary?.thisMonth ?? '—'}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Ingresos del mes</p>
          <strong>
            {reservationSummary ? `$ ${Number(reservationSummary.revenueThisMonth).toLocaleString('es-AR')}` : '—'}
          </strong>
        </article>
      </section>

      <section className="my-spaces__filters">
        <label className="my-spaces__filter-group">
          <span>Buscar</span>
          <input
            type="text"
            value={searchQuery}
            onChange={(event) => setSearchQuery(event.target.value)}
            placeholder="Nombre o dirección"
          />
        </label>

        <label className="my-spaces__filter-group">
          <span>Estado</span>
          <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
            <option value="all">Todos</option>
            <option value="active">Activo</option>
            <option value="inactive">Inactivo</option>
          </select>
        </label>
      </section>

      {isLoading ? (
        <p style={{ textAlign: 'center', color: 'var(--color-muted)', marginTop: '48px' }}>Cargando espacios...</p>
      ) : filteredSpaces.length === 0 ? (
        <article className="my-spaces__empty">
          <div className="my-spaces__empty-icon">📭</div>
          <h2>No hay espacios para mostrar</h2>
          <p>Intenta cambiar el filtro o publica un nuevo espacio para empezar.</p>
          <button type="button" className="btn btn-primary" onClick={() => setModalOpen(true)}>Publicar nuevo espacio</button>
        </article>
      ) : (
        <section className="my-spaces__grid" aria-label="Listado de espacios">
          {filteredSpaces.map((space) => {
            const status = space.isActive ? 'active' : 'inactive';
            return (
              <article key={space.id} className="my-spaces__card">
                <div className="my-spaces__image" aria-label="Vista previa del espacio" style={{ overflow: 'hidden', height: '280px' }}>
                  {space.images?.length > 0
                    ? <img src={space.images[0]} alt={space.name} style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
                    : <span>🏢</span>
                  }
                </div>

                <div className="my-spaces__card-body">
                  <div className="my-spaces__card-topline">
                    <span className={`my-spaces__badge ${STATUS_CLASS[status]}`}>
                      {STATUS_LABELS[status]}
                    </span>
                    <span className="my-spaces__price">$ {space.pricePerHour.toLocaleString('es-AR')}/h</span>
                  </div>

                  <h2 className="my-spaces__card-title">{space.name}</h2>
                  <p className="my-spaces__address">📍 {space.city}, {space.country}</p>

                  <div className="my-spaces__meta-grid">
                    <span>👥 {space.capacity} personas</span>
                    <span>📅 Próximamente</span>
                    <span>⭐ Próximamente</span>
                  </div>

                  <div className="my-spaces__actions">
                    <button type="button" className="btn btn-secondary" onClick={() => handleEdit(space)}>Editar</button>
                    {canDelete && (
                      <button type="button" className="btn btn-danger" onClick={() => handleOpenDeleteModal(space)}>Eliminar</button>
                    )}
                    <button
                      type="button"
                      className="btn btn-primary"
                      onClick={() => navigate(`/my-spaces/${space.id}/reservations`)}
                    >
                      Ver reservas
                    </button>
                  </div>
                </div>
              </article>
            );
          })}
        </section>
      )}

      <OfficeModal
        isOpen={modalOpen}
        onClose={handleCloseModal}
        companyId={companyId ?? ''}
        initialValues={editingSpace}
      />

      <ConfirmDeleteModal
        isOpen={deleteModalOpen}
        onClose={handleCloseDeleteModal}
        onConfirm={handleConfirmDelete}
        title="Eliminar espacio"
        message={spaceToDelete ? `¿Estás seguro de que querés eliminar "${spaceToDelete.name}"? Esta acción no se puede deshacer.` : ''}
        isPending={false}
      />
    </section>
  );
}

export default MySpaces;
