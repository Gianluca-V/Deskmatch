import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Search, MapPin, Users, Clock, Star, SlidersHorizontal,
  X, ChevronLeft, ChevronRight, Building2, Wifi, Sparkles, Navigation
} from 'lucide-react';
import { useWorkspaces } from '../hooks/useWorkspaces';
import { aiSearch } from '../api/offices';
import LocationFilter from '../components/LocationFilter';
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
          <div className="ws-card__prices">
            {workspace.pricePerHour && (
              <span className="ws-card__price-item">
                ${workspace.pricePerHour?.toFixed(0)} / hora
              </span>
            )}
            {workspace.pricePerDay && (
              <span className="ws-card__price-item">
                ${workspace.pricePerDay?.toFixed(0)} / día
              </span>
            )}
          </div>
          <button className="ws-card__btn">Ver espacio</button>
        </div>
      </div>
    </article>
  );
}

export default function Offices() {
  const [showFilters, setShowFilters] = useState(false);
  const [search, setSearch] = useState('');
  const [aiInterpretation, setAiInterpretation] = useState(null);
  const [aiLoading, setAiLoading] = useState(false);
  const [customAmenity, setCustomAmenity] = useState('');
  const [filters, setFilters] = useState({
    q: '',
    city: '',
    minPrice: '',
    maxPrice: '',
    minCapacity: '',
    amenities: [],
    lat: undefined,
    lon: undefined,
    radius: undefined,
    page: 1,
    pageSize: 12,
  });

  const queryParams = {
    page: filters.page,
    pageSize: filters.pageSize,
    q: filters.q || undefined,
    city: filters.city || undefined,
    minPrice: filters.minPrice || undefined,
    maxPrice: filters.maxPrice || undefined,
    minCapacity: filters.minCapacity || undefined,
    amenities: filters.amenities.length > 0 ? filters.amenities.join(',') : undefined,
    lat: filters.lat,
    lon: filters.lon,
    radius: filters.radius,
  };

  const { data, isLoading, error } = useWorkspaces(queryParams);

  const items = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const totalPages = data?.totalPages ?? 1;
  const hasPrev = (data?.page ?? 1) > 1;
  const hasNext = (data?.page ?? 1) < (data?.totalPages ?? 1);

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

  function handleAddCustomAmenity() {
    const val = customAmenity.trim();
    if (!val || filters.amenities.includes(val)) {
      setCustomAmenity('');
      return;
    }
    setFilters((prev) => ({ ...prev, amenities: [...prev.amenities, val], page: 1 }));
    setCustomAmenity('');
  }

  function handleCustomAmenityKeyDown(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddCustomAmenity();
    }
  }

  function handleRemoveAmenity(a) {
    setFilters((prev) => ({
      ...prev,
      amenities: prev.amenities.filter((x) => x !== a),
      page: 1,
    }));
  }

  const isPresetAmenity = (a) => AMENITY_OPTIONS.includes(a);
  const presetAmenities = filters.amenities.filter(isPresetAmenity);
  const customAmenities = filters.amenities.filter((a) => !isPresetAmenity(a));

  function handleLocationChange({ lat, lon, radius }) {
    setFilters((prev) => ({ ...prev, lat, lon, radius, page: 1 }));
  }

  function clearFilters() {
    setFilters({ q: '', city: '', minPrice: '', maxPrice: '', minCapacity: '', amenities: [], lat: undefined, lon: undefined, radius: undefined, page: 1, pageSize: 12 });
    setSearch('');
    setAiInterpretation(null);
  }

  async function handleAiSearch() {
    const text = search.trim();
    if (!text || aiLoading) return;
    setAiLoading(true);
    setAiInterpretation(null);
    try {
      const result = await aiSearch(text, 1, 12);
      const ai = result.aiInterpretation;
      if (ai) {
        setAiInterpretation(ai);
        setFilters((prev) => ({
          ...prev,
          q: ai.q || text,
          city: ai.city || '',
          minPrice: ai.minPrice != null ? String(ai.minPrice) : '',
          maxPrice: ai.maxPrice != null ? String(ai.maxPrice) : '',
          minCapacity: ai.minCapacity != null ? String(ai.minCapacity) : '',
          amenities: Array.isArray(ai.amenities) ? ai.amenities : [],
          page: 1,
        }));
      }
    } catch {
      setAiInterpretation(null);
    } finally {
      setAiLoading(false);
    }
  }

  function handleSearchKeyDown(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      setFilter('q', search);
    }
  }

  const hasActiveFilters =
    filters.q || filters.city || filters.minPrice || filters.maxPrice ||
    filters.minCapacity || filters.amenities.length > 0 || aiInterpretation ||
    filters.lat != null;

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
        <div className="offices-page__search-row">
          <div className="offices-page__search">
            <Search size={18} />
            <input
              type="text"
              placeholder="Buscar por nombre..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={handleSearchKeyDown}
            />
            {search && (
              <button onClick={() => { setSearch(''); setFilter('q', ''); }}>
                <X size={16} />
              </button>
            )}
          </div>
          <button
            className="offices-page__ai-btn"
            onClick={handleAiSearch}
            disabled={aiLoading || !search.trim()}
            title="Buscar con IA"
          >
            <Sparkles size={18} />
            {aiLoading ? 'Analizando...' : 'IA'}
          </button>
        </div>

        {/* AI Interpretation Banner */}
        {aiInterpretation && (
          <div className="offices-page__ai-banner">
            <Sparkles size={15} />
            <span className="offices-page__ai-banner__label">Búsqueda IA:</span>
            <div className="offices-page__ai-chips">
              {aiInterpretation.city && (
                <span className="offices-page__ai-chip">
                  <MapPin size={11} /> {aiInterpretation.city}
                </span>
              )}
              {aiInterpretation.minPrice != null && (
                <span className="offices-page__ai-chip">
                  Mín ${aiInterpretation.minPrice}/h
                </span>
              )}
              {aiInterpretation.maxPrice != null && (
                <span className="offices-page__ai-chip">
                  Máx ${aiInterpretation.maxPrice}/h
                </span>
              )}
              {aiInterpretation.minCapacity != null && (
                <span className="offices-page__ai-chip">
                  <Users size={11} /> {aiInterpretation.minCapacity}+ pers.
                </span>
              )}
              {aiInterpretation.amenities?.map((a) => (
                <span className="offices-page__ai-chip" key={a}>
                  <Wifi size={11} /> {a}
                </span>
              ))}
            </div>
            <button className="offices-page__ai-banner__clear" onClick={clearFilters}>
              <X size={13} /> Limpiar
            </button>
          </div>
        )}

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

              {/* Ubicación */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <MapPin size={14} /> Ubicación
                </label>
                <LocationFilter
                  value={{ lat: filters.lat, lon: filters.lon, radius: filters.radius }}
                  onChange={handleLocationChange}
                />
              </div>

              {/* Amenities */}
              <div className="offices-filters__group">
                <label className="offices-filters__label">
                  <Wifi size={14} /> Amenities
                </label>

                <div className="offices-filters__amenity-add">
                  <input
                    type="text"
                    placeholder="Agregar amenity..."
                    value={customAmenity}
                    onChange={(e) => setCustomAmenity(e.target.value)}
                    onKeyDown={handleCustomAmenityKeyDown}
                  />
                  <button type="button" onClick={handleAddCustomAmenity} disabled={!customAmenity.trim()}>
                    + Agregar
                  </button>
                </div>

                {customAmenities.length > 0 && (
                  <div className="offices-filters__custom-amenities">
                    {customAmenities.map((a) => (
                      <span className="offices-filters__amenity-tag" key={a}>
                        {a}
                        <button onClick={() => handleRemoveAmenity(a)}>
                          <X size={12} />
                        </button>
                      </span>
                    ))}
                  </div>
                )}

                <div className="offices-filters__amenities">
                  {AMENITY_OPTIONS.map((a) => (
                    <label key={a} className="offices-filters__amenity">
                      <input
                        type="checkbox"
                        checked={presetAmenities.includes(a)}
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

            {!isLoading && !error && items.length === 0 && (
              <div className="offices-page__empty">
                <Building2 size={48} />
                <h3>Sin resultados</h3>
                <p>
                  {hasActiveFilters
                    ? 'No encontramos espacios con esos filtros. Probá con otros criterios.'
                    : 'No hay espacios disponibles por el momento.'}
                </p>
                {hasActiveFilters && (
                  <button className="btn btn-primary" onClick={clearFilters} style={{ marginTop: 12 }}>
                    Limpiar filtros
                  </button>
                )}
              </div>
            )}

            {!isLoading && !error && items.length > 0 && (
              <div className="offices-grid">
                {items.map((w) => <WorkspaceCard key={w.id} workspace={w} />)}
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
