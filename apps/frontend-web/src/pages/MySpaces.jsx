import { useMemo, useState } from 'react';
import OfficeModal from '../components/OfficeModal';
import { useMyCompany } from '../hooks/useMyCompany';
import { useWorkspacesByCompany } from '../hooks/useWorkspacesByCompany';

const STATUS_LABELS = {
  true: 'Activo',
  false: 'Inactivo',
};

const STATUS_CLASS = {
  true: 'my-spaces__badge--active',
  false: 'my-spaces__badge--inactive',
};

const STATUS_FILTER_MAP = {
  active: true,
  inactive: false,
};

function MySpaces() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [modalOpen, setModalOpen] = useState(false);

  const { data: company } = useMyCompany();
  const companyId = company?.id;

  const { data: spaces = [], isLoading } = useWorkspacesByCompany(companyId);

  const filteredSpaces = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return spaces.filter((space) => {
      const matchesQuery =
        !query ||
        space.name.toLowerCase().includes(query) ||
        (space.address ?? '').toLowerCase().includes(query);
      const matchesStatus =
        statusFilter === 'all' || space.isActive === STATUS_FILTER_MAP[statusFilter];
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
          <strong>—</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Ingresos del mes</p>
          <strong>—</strong>
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
          {filteredSpaces.map((space) => (
            <article key={space.id} className="my-spaces__card">
              <div className="my-spaces__image" aria-label="Vista previa del espacio">
                {space.images?.length > 0
                  ? <img src={space.images[0]} alt={space.name} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                  : <span>🏢</span>
                }
              </div>

              <div className="my-spaces__card-body">
                <div className="my-spaces__card-topline">
                  <span className={`my-spaces__badge ${STATUS_CLASS[space.isActive]}`}>
                    {STATUS_LABELS[space.isActive]}
                  </span>
                  <span className="my-spaces__price">$ {space.pricePerHour.toLocaleString('es-AR')}/h</span>
                </div>

                <h2 className="my-spaces__card-title">{space.name}</h2>
                <p className="my-spaces__address">📍 {space.address ?? '—'} · {space.city}, {space.country}</p>

                {space.description && (
                  <p className="my-spaces__description">{space.description}</p>
                )}

                <div className="my-spaces__meta-grid">
                  <span>👥 {space.capacity} personas</span>
                  <span>📅 — reservas</span>
                  <span>⭐ —</span>
                </div>

                <div className="my-spaces__meta-grid" style={{ marginTop: '4px' }}>
                  {space.pricePerDay && <span>💰 $ {space.pricePerDay.toLocaleString('es-AR')}/día</span>}
                  {space.pricePerMonth && <span>💰 $ {space.pricePerMonth.toLocaleString('es-AR')}/mes</span>}
                  {space.amenities?.length > 0 && <span>✨ {space.amenities.length} amenities</span>}
                </div>

                <div className="my-spaces__actions">
                  <button type="button" className="btn btn-secondary">Editar</button>
                  <button type="button" className="btn btn-primary">Ver reservas</button>
                </div>
              </div>
            </article>
          ))}
        </section>
      )}

      <OfficeModal isOpen={modalOpen} onClose={() => setModalOpen(false)} companyId={companyId ?? ''} />
    </section>
  );
}

export default MySpaces;
