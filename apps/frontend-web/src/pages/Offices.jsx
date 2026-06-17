import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search, MapPin, Users, Clock, Star, SlidersHorizontal,
  X, ChevronLeft, ChevronRight, Building2, Wifi
} from 'lucide-react';
import { useWorkspaces } from '../hooks/useWorkspaces';
import './Offices.css';

const AMENITY_OPTIONS = [
  'WiFi', 'AC', 'Proyector', 'Pizarra', 'Café', 'TV',
  'Videoconferencia', 'Estacionamiento', 'Recepción', 'Impresora',
  'Cocina', 'Baños', 'Acceso 24 hs', 'Equipamiento de audio',
  'Escritorios individuales', 'Sillas ergonómicas',
];

const CITY_OPTIONS = [
  '', 'Buenos Aires', 'Rosario', 'Córdoba', 'Mendoza', 'La Plata',
];

const CAPACITY_OPTIONS = [
  { label: 'Cualquier capacidad', value: '' },
  { label: '2+ personas', value: 2 },
  { label: '4+ personas', value: 4 },
  { label: '8+ personas', value: 8 },
  { label: '12+ personas', value: 12 },
  { label: '20+ personas', value: 20 },
  { label: '50+ personas', value: 50 },
];

function WorkspaceSkeleton() {
  return (
    <div className="ws-card ws-card--skeleton">
      <div className="ws-card__media ws-card__media--skeleton" />
      <div className="ws-card__body">
        <div className="ws-skeleton-line ws-skeleton-line--title" />
        <div className="ws-skeleton-line ws-skeleton-line--short" />
        <div className="ws-skeleton-line" />
      </div>
    </div>
  );
}

function WorkspaceCard({ workspace }) {
  const navigate = useNavigate();
  const amenityPreview = workspace.amenities?.slice(0, 3) ?? [];
  const extraCount = (workspace.amenities?.length ?? 0) - amenityPreview.length;

  return (
    <article
      className="ws-card"
      onClick={() => navigate(`/workspaces/${workspace.id}`)}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => e.key === 'Enter' && navigate(`/workspaces/${workspace.id}`)}
    >
      <div className="ws-card__media">
        {workspace.images?.[0]
          ? <img src={workspace.images[0]} alt={workspace.name} />
          : <div className="ws-card__media-placeholder"><Building2 size={32} /></div>
        }
        {workspace.rating > 0 && (
          <span className="ws-card__rating">
            <Star size={11} fill="currentColor" />
            {workspace.rating?.toFixed(1)}
          </span>
        )}
      </div>

      <div className="ws-card__body">
        <h3 className="ws-card__name">{workspace.name}</h3>

        {workspace.city && (
          <p className="ws-card__location">
            <MapPin size={13} />
            {workspace.city}{workspace.address ? ` · ${workspace.address}` : ''}
          </p>
        )}

        <div className="ws-card__meta">
          <span className="ws-card__meta-item">
            <Users size={13} />
            {workspace.capacity} pers.
          </span>
          {amenityPreview.length > 0 && (
            <span className="ws-card__meta-item">
              <Wifi size={13} />
              {amenityPreview.join(', ')}
              {extraCount > 0 && ` +${extraCount}`}
            </span>
          )}
        </div>

        <div className="ws-card__footer">
          <div className="ws-card__price">
            <strong>${workspace.pricePerHour?.toFixed(0)}</strong>
            <span>/ hora</span>
          </div>
          {workspace.pricePerDay && (
            <div className="ws-card__price-alt">
              ${workspace.pricePerDay?.toFixed(0)} / día
            </div>
          )}
          <button className="ws-card__btn">Ver espacio</button>
        </div>
      </div>
    </article>
  );
}

export default function Offices() {
  const [showFilters, setShowFilters] = useState(false);
  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState({
    city: '',
    minPrice: '',
    maxPrice: '',
    minCapacity: '',
    amenities: [],
    page: 1,
    pageSize: 12,
  });

  const queryParams = {
    page: filters.page,
    pageSize: filters.pageSize,
    city: filters.city || undefined,
    minPrice: filters.minPrice || undefined,
    maxPrice: filters.maxPrice || undefined,
    minCapacity: filters.minCapacity || undefined,
    amenities: filters.amenities.length > 0 ? filters.amenities.join(',') : undefined,
  };

  const { data, isLoading, error } = useWorkspaces(queryParams);

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 1;
  const hasPrev = data?.hasPreviousPage ?? false;
  const hasNext = data?.hasNextPage ?? false;

  // client-side search on name (servidor no tiene query by name aún)
  const displayed = search.trim()
    ? items.filter((w) => w.name.toLowerCase().includes(search.toLowerCase()))
    : items;

  const setFilter = useCallback((key, value) => {
    setFilters((prev) => ({ ...prev, [key]: value, page: 1 }));
  }, []);

  function toggleAmenity(a) {
    setFilters((prev) => {
      const cur = prev.amenities;
      const next = cur.includes(a) ? cur.filter((x) => x !== a) : [...cur, a];
      return { ...prev, amenities: next, page: 1 };
    });
  }

  function clearFilters() {
    setFilters({ city: '', minPrice: '', maxPrice: '', minCapacity: '', amenities: [], page: 1, pageSize: 12 });
    setSearch('');
  }

  const hasActiveFilters =
    filters.city || filters.minPrice || filters.maxPrice ||
    filters.minCapacity || filters.amenities.length > 0;

  return (
    <div className="offices-page">
      <div className="container">

        {/* Header */}
        <div className="offices-page__header">
          <div>
            <h1>Oficinas y espacios</h1>
            <p>{totalCount > 0 ? `${totalCount} espacios disponibles` : 'Explorá todos los espacios disponibles'}</p>
          </div>
          <button
            className={`offices-page__filter-btn ${showFilters ? 'offices-page__filter-btn--active' : ''}`}
            onClick={() => setShowFilters((v) => !v)}
          >
            <SlidersHorizontal size={16} />
            Filtros
            {hasActiveFilters && <span className="offices-page__filter-dot" />}
          </button>
        </div>

        {/* Search bar */}
        <div className="offices-page__search">
          <Search size={18} />
          <input
            type="text"
            placeholder="Buscar por nombre..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button onClick={() => setSearch('')}><X size={16} /></button>
          )}
        </div>

        <div className="offices-page__layout">

          {/* Filters panel */}
          {showFilters && (
            <aside className="offices-filters">
              <div className="offices-filters__header">
                <h3>Filtros</h3>
                {hasActiveFilters && (
                  <button className="offices-filters__clear" onClick={clearFilters}>
                    <X size={14} /> Limpiar
                  </button>
                )}
              </div>

              {/* Ciudad */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <MapPin size={14} /> Ciudad
                </label>
                <select
                  className="offices-filters__select"
                  value={filters.city}
                  onChange={(e) => setFilter('city', e.target.value)}
                >
                  {CITY_OPTIONS.map((c) => (
                    <option key={c} value={c}>{c || 'Todas las ciudades'}</option>
                  ))}
                </select>
              </div>

              {/* Precio */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <Clock size={14} /> Precio por hora
                </label>
                <div className="offices-filters__range">
                  <input
                    type="number"
                    className="offices-filters__input"
                    placeholder="Mín $"
                    min="0"
                    value={filters.minPrice}
                    onChange={(e) => setFilter('minPrice', e.target.value)}
                  />
                  <span>—</span>
                  <input
                    type="number"
                    className="offices-filters__input"
                    placeholder="Máx $"
                    min="0"
                    value={filters.maxPrice}
                    onChange={(e) => setFilter('maxPrice', e.target.value)}
                  />
                </div>
              </div>

              {/* Capacidad */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <Users size={14} /> Capacidad mínima
                </label>
                <select
                  className="offices-filters__select"
                  value={filters.minCapacity}
                  onChange={(e) => setFilter('minCapacity', e.target.value)}
                >
                  {CAPACITY_OPTIONS.map(({ label, value }) => (
                    <option key={value} value={value}>{label}</option>
                  ))}
                </select>
              </div>

              {/* Amenities */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <Wifi size={14} /> Amenities
                </label>
                <div className="offices-filters__amenities">
                  {AMENITY_OPTIONS.map((a) => (
                    <label key={a} className="offices-filters__amenity">
                      <input
                        type="checkbox"
                        checked={filters.amenities.includes(a)}
                        onChange={() => toggleAmenity(a)}
                      />
                      {a}
                    </label>
                  ))}
                </div>
              </div>
            </aside>
          )}

          {/* Grid */}
          <div className="offices-page__content">
            {isLoading && (
              <div className="offices-grid">
                {Array.from({ length: 6 }).map((_, i) => <WorkspaceSkeleton key={i} />)}
              </div>
            )}

            {error && (
              <div className="offices-page__empty">
                <Building2 size={48} />
                <h3>Error al cargar espacios</h3>
                <p>No pudimos conectar con el servidor. Intentá recargar la página.</p>
              </div>
            )}

            {!isLoading && !error && displayed.length === 0 && (
              <div className="offices-page__empty">
                <Building2 size={48} />
                <h3>Sin resultados</h3>
                <p>
                  {hasActiveFilters || search
                    ? 'No encontramos espacios con esos filtros. Probá con otros criterios.'
                    : 'No hay espacios disponibles por el momento.'}
                </p>
                {(hasActiveFilters || search) && (
                  <button className="btn btn-primary" onClick={clearFilters} style={{ marginTop: 12 }}>
                    Limpiar filtros
                  </button>
                )}
              </div>
            )}

            {!isLoading && !error && displayed.length > 0 && (
              <div className="offices-grid">
                {displayed.map((w) => <WorkspaceCard key={w.id} workspace={w} />)}
              </div>
            )}

            {/* Pagination */}
            {!isLoading && totalPages > 1 && (
              <div className="offices-page__pagination">
                <button
                  className="offices-page__page-btn"
                  disabled={!hasPrev}
                  onClick={() => setFilters((p) => ({ ...p, page: p.page - 1 }))}
                >
                  <ChevronLeft size={16} /> Anterior
                </button>
                <span className="offices-page__page-info">
                  Página {filters.page} de {totalPages}
                </span>
                <button
                  className="offices-page__page-btn"
                  disabled={!hasNext}
                  onClick={() => setFilters((p) => ({ ...p, page: p.page + 1 }))}
                >
                  Siguiente <ChevronRight size={16} />
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
