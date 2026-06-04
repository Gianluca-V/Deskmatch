import { useEffect, useMemo, useState } from 'react';

const MOCK_SPACES = [
  { id: 1, name: 'Oficina Central Buenos Aires', address: 'Av. Corrientes 1234, CABA', pricePerHour: 2500, capacity: 10, status: 'active', reservationsThisMonth: 24, rating: 4.8 },
  { id: 2, name: 'Sala de Reuniones Palermo', address: 'Thames 456, Palermo', pricePerHour: 1800, capacity: 6, status: 'active', reservationsThisMonth: 18, rating: 4.6 },
  { id: 3, name: 'Coworking Microcentro', address: 'Florida 789, Microcentro', pricePerHour: 1200, capacity: 20, status: 'inactive', reservationsThisMonth: 0, rating: 4.2 },
  { id: 4, name: 'Espacio Creativo Belgrano', address: 'Cabildo 321, Belgrano', pricePerHour: 2000, capacity: 8, status: 'draft', reservationsThisMonth: 0, rating: 0 },
];

const STATUS_LABELS = {
  active: 'Activo',
  inactive: 'Inactivo',
  draft: 'Borrador',
};

const STATUS_CLASS = {
  active: 'my-spaces__badge--active',
  inactive: 'my-spaces__badge--inactive',
  draft: 'my-spaces__badge--draft',
};

function MySpaces() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  useEffect(() => {
    // TODO: Reemplazar MOCK_SPACES por un fetch real a la API del propietario.
  }, []);

  const filteredSpaces = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();

    return MOCK_SPACES.filter((space) => {
      const matchesQuery =
        !query ||
        space.name.toLowerCase().includes(query) ||
        space.address.toLowerCase().includes(query);

      const matchesStatus = statusFilter === 'all' || space.status === statusFilter;

      return matchesQuery && matchesStatus;
    });
  }, [searchQuery, statusFilter]);

  return (
    <section className="my-spaces page-container">
      <header className="my-spaces__header">
        <div>
          <p className="my-spaces__eyebrow">Panel de empresa</p>
          <h1 className="my-spaces__title">Mis Espacios</h1>
          <p className="my-spaces__subtitle">Gestiona tus oficinas y espacios publicados desde un solo lugar.</p>
        </div>
        <button type="button" className="btn btn-primary">
          Publicar nuevo espacio
        </button>
      </header>

      <section className="my-spaces__metrics" aria-label="Resumen de espacios">
        <article className="my-spaces__metric-card">
          <p>Total de espacios</p>
          <strong>{MOCK_SPACES.length}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Espacios activos</p>
          <strong>{MOCK_SPACES.filter((space) => space.status === 'active').length}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Reservas este mes</p>
          <strong>{MOCK_SPACES.reduce((sum, space) => sum + space.reservationsThisMonth, 0)}</strong>
        </article>
        <article className="my-spaces__metric-card">
          <p>Ingresos del mes</p>
          <strong>$ {MOCK_SPACES.reduce((sum, space) => sum + space.pricePerHour * space.reservationsThisMonth, 0).toLocaleString('es-AR')}</strong>
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
            <option value="draft">Borrador</option>
          </select>
        </label>
      </section>

      {filteredSpaces.length === 0 ? (
        <article className="my-spaces__empty">
          <div className="my-spaces__empty-icon">📭</div>
          <h2>No hay espacios para mostrar</h2>
          <p>Intenta cambiar el filtro o publica un nuevo espacio para empezar.</p>
          <button type="button" className="btn btn-primary">Publicar nuevo espacio</button>
        </article>
      ) : (
        <section className="my-spaces__grid" aria-label="Listado de espacios">
          {filteredSpaces.map((space) => (
            <article key={space.id} className="my-spaces__card">
              <div className="my-spaces__image" aria-label="Vista previa del espacio">
                <span>🏢</span>
              </div>

              <div className="my-spaces__card-body">
                <div className="my-spaces__card-topline">
                  <span className={`my-spaces__badge ${STATUS_CLASS[space.status]}`}>
                    {STATUS_LABELS[space.status]}
                  </span>
                  <span className="my-spaces__price">$ {space.pricePerHour.toLocaleString('es-AR')}/h</span>
                </div>

                <h2 className="my-spaces__card-title">{space.name}</h2>
                <p className="my-spaces__address">📍 {space.address}</p>

                <div className="my-spaces__meta-grid">
                  <span>👥 {space.capacity} personas</span>
                  <span>📅 {space.reservationsThisMonth} reservas</span>
                  <span>⭐ {space.rating.toFixed(1)}</span>
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
    </section>
  );
}

export default MySpaces;
