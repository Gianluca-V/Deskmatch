import { useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Calendar, Eye, Search, X } from 'lucide-react';
import { useCompanyReservations, useWorkspaceReservations } from '../hooks/useCompanyReservations';
import './Reservations.css';

const STATUS_MAP = {
  1: { label: 'Confirmada', cls: 'reservation-card__badge--confirmed', key: 'confirmed' },
  2: { label: 'Cancelada', cls: 'reservation-card__badge--cancelled', key: 'cancelled' },
  3: { label: 'Completada', cls: 'reservation-card__badge--pending', key: 'completed' },
};

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('es-AR', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function ReservationSkeleton() {
  return (
    <div className="reservation-card" style={{ opacity: 0.6 }}>
      <div className="reservation-card__image">
        <div className="reservation-card__image-placeholder" style={{ animation: 'pulse 1.5s ease-in-out infinite' }} />
      </div>
      <div className="reservation-card__body">
        <div style={{ height: 16, background: '#e2e8f0', borderRadius: 6, marginBottom: 8, width: '60%' }} />
        <div style={{ height: 12, background: '#f1f5f9', borderRadius: 6, marginBottom: 16, width: '40%' }} />
        <div style={{ height: 12, background: '#f1f5f9', borderRadius: 6, width: '80%' }} />
      </div>
    </div>
  );
}

function CompanyReservationsPage() {
  const navigate = useNavigate();
  const { workspaceId } = useParams();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  const companyQuery = useCompanyReservations();
  const workspaceQuery = useWorkspaceReservations(workspaceId);
  const activeQuery = workspaceId ? workspaceQuery : companyQuery;
  const reservations = activeQuery.data ?? [];

  const filtered = useMemo(() => {
    const q = searchQuery.trim().toLowerCase();
    return reservations.filter((r) => {
      const statusInfo = STATUS_MAP[r.status];
      const matchQuery =
        !q ||
        r.workspaceName?.toLowerCase().includes(q) ||
        r.guestId?.toLowerCase().includes(q);
      const matchStatus = statusFilter === 'all' || statusInfo?.key === statusFilter;
      return matchQuery && matchStatus;
    });
  }, [reservations, searchQuery, statusFilter]);

  const countByKey = (key) =>
    reservations.filter((r) => STATUS_MAP[r.status]?.key === key).length;

  return (
    <div className="reservations-page">
      <div className="container">
        <div className="reservations-page__header">
          <div>
            <h1>{workspaceId ? 'Reservas del espacio' : 'Reservas recibidas'}</h1>
            <p>Consultá las reservas realizadas sobre los espacios de tu empresa.</p>
          </div>
        </div>

        <div className="reservations-page__tabs">
          {[
            { key: 'all', label: `Todas (${reservations.length})` },
            { key: 'confirmed', label: `Confirmadas (${countByKey('confirmed')})` },
            { key: 'completed', label: `Completadas (${countByKey('completed')})` },
            { key: 'cancelled', label: `Canceladas (${countByKey('cancelled')})` },
          ].map(({ key, label }) => (
            <button
              key={key}
              className={`reservations-page__tab ${statusFilter === key ? 'reservations-page__tab--active' : ''}`}
              onClick={() => setStatusFilter(key)}
            >
              {label}
            </button>
          ))}
        </div>

        <div className="reservations-page__search-bar">
          <Search size={18} />
          <input
            type="text"
            placeholder="Buscar por espacio o ID de huésped..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
          {searchQuery && (
            <button className="reservations-page__clear-search" onClick={() => setSearchQuery('')}>
              <X size={18} />
            </button>
          )}
        </div>

        <div className="reservations-page__content">
          {activeQuery.isLoading && (
            <div className="reservations-page__list">
              {[1, 2, 3].map((i) => <ReservationSkeleton key={i} />)}
            </div>
          )}

          {activeQuery.error && (
            <div className="reservations-page__empty">
              <Calendar size={48} />
              <h3>Error al cargar reservas</h3>
              <p>No pudimos obtener las reservas recibidas. Intentá recargar la página.</p>
            </div>
          )}

          {!activeQuery.isLoading && !activeQuery.error && filtered.length > 0 && (
            <div className="reservations-page__list">
              {filtered.map((r) => {
                const statusInfo = STATUS_MAP[r.status] ?? STATUS_MAP[3];
                return (
                  <div key={r.id} className="reservation-card">
                    <div className="reservation-card__image">
                      <div className="reservation-card__image-placeholder" />
                    </div>

                    <div className="reservation-card__body">
                      <div className="reservation-card__main">
                        <h3 className="reservation-card__name">
                          {r.workspaceName ?? 'Espacio de trabajo'}
                        </h3>
                        <p className="reservation-card__number">
                          Huésped: {r.guestId?.slice(0, 8).toUpperCase() ?? 'N/D'}
                        </p>

                        <div className="reservation-card__dates">
                          <div className="reservation-card__date-item">
                            <Calendar size={16} />
                            <div>
                              <label>Inicio</label>
                              <p>{formatDate(r.startTime)}</p>
                            </div>
                          </div>
                          <div className="reservation-card__date-item">
                            <Calendar size={16} />
                            <div>
                              <label>Fin</label>
                              <p>{formatDate(r.endTime)}</p>
                            </div>
                          </div>
                        </div>

                        <div className="reservation-card__price-section">
                          <label>Total</label>
                          <p className="reservation-card__price">
                            ${Number(r.totalPrice).toFixed(2)}
                          </p>
                        </div>
                      </div>

                      <div className="reservation-card__footer">
                        <button
                          className="reservation-card__btn reservation-card__btn--secondary"
                          onClick={() => navigate(`/workspaces/${r.workspaceId}`)}
                        >
                          <Eye size={16} />
                          Ver Espacio
                        </button>
                      </div>
                    </div>

                    <div className="reservation-card__side">
                      <span className={`reservation-card__badge ${statusInfo.cls}`}>
                        {statusInfo.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {!activeQuery.isLoading && !activeQuery.error && filtered.length === 0 && (
            <div className="reservations-page__empty">
              <Calendar size={48} />
              <h3>No hay reservas recibidas</h3>
              <p>
                {searchQuery
                  ? 'No se encontraron reservas que coincidan con tu búsqueda.'
                  : 'Todavía no hay reservas para mostrar.'}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default CompanyReservationsPage;
