import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Calendar, Search, X, Eye, Trash2, Loader2 } from 'lucide-react';
import { useMyReservations } from '../hooks/useMyReservations';
import { useCancelReservation } from '../hooks/useCancelReservation';
import './Reservations.css';

const STATUS_MAP = {
  1: { label: 'Confirmada', cls: 'reservation-card__badge--confirmed', key: 'confirmed' },
  2: { label: 'Cancelada', cls: 'reservation-card__badge--cancelled', key: 'cancelled' },
  3: { label: 'Completada', cls: 'reservation-card__badge--pending', key: 'completed' },
};

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('es-AR', {
    day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit',
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

function Reservations() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [cancellingId, setCancellingId] = useState(null);

  const { data: reservations = [], isLoading, error } = useMyReservations();

  const { mutate: cancelReservation } = useCancelReservation({
    onSuccess: () => {
      toast.success('Reserva cancelada correctamente.');
      setCancellingId(null);
    },
    onError: (err) => {
      const detail = err?.response?.data?.detail;
      toast.error(detail || 'No se pudo cancelar la reserva.');
      setCancellingId(null);
    },
  });

  function handleCancel(id) {
    const toastId = toast(
      ({ closeToast }) => (
        <div>
          <p style={{ margin: '0 0 12px', fontWeight: 600 }}>
            ¿Cancelar esta reserva?
          </p>
          <p style={{ margin: '0 0 16px', fontSize: 13, color: '#64748b' }}>
            Esta acción no se puede deshacer.
          </p>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              onClick={() => {
                closeToast();
                setCancellingId(id);
                cancelReservation(id);
              }}
              style={{
                flex: 1, padding: '7px 0', background: '#ef4444', color: '#fff',
                border: 'none', borderRadius: 7, fontWeight: 700, cursor: 'pointer', fontSize: 13,
              }}
            >
              Sí, cancelar
            </button>
            <button
              onClick={closeToast}
              style={{
                flex: 1, padding: '7px 0', background: '#f1f5f9', color: '#334155',
                border: 'none', borderRadius: 7, fontWeight: 600, cursor: 'pointer', fontSize: 13,
              }}
            >
              No, volver
            </button>
          </div>
        </div>
      ),
      { autoClose: false, closeOnClick: false, draggable: false }
    );
    void toastId;
  }

  const filtered = useMemo(() => {
    const q = searchQuery.trim().toLowerCase();
    return reservations.filter((r) => {
      const statusInfo = STATUS_MAP[r.status];
      const matchQuery = !q || r.workspaceName?.toLowerCase().includes(q);
      const matchStatus =
        statusFilter === 'all' ||
        statusInfo?.key === statusFilter;
      return matchQuery && matchStatus;
    });
  }, [reservations, searchQuery, statusFilter]);

  const countByKey = (key) =>
    reservations.filter((r) => STATUS_MAP[r.status]?.key === key).length;

  const canCancel = (r) =>
    r.status === 1 && new Date(r.startTime) > new Date();

  return (
    <div className="reservations-page">
      <div className="container">
        <div className="reservations-page__header">
          <div>
            <h1>Mis Reservas</h1>
            <p>Gestiona y revisá tus espacios reservados</p>
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
            placeholder="Buscar reservas..."
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
          {isLoading && (
            <div className="reservations-page__list">
              {[1, 2, 3].map((i) => <ReservationSkeleton key={i} />)}
            </div>
          )}

          {error && (
            <div className="reservations-page__empty">
              <Calendar size={48} />
              <h3>Error al cargar reservas</h3>
              <p>No pudimos obtener tus reservas. Intentá recargar la página.</p>
            </div>
          )}

          {!isLoading && !error && filtered.length > 0 && (
            <div className="reservations-page__list">
              {filtered.map((r) => {
                const statusInfo = STATUS_MAP[r.status] ?? STATUS_MAP[3];
                const isCancelling = cancellingId === r.id;

                return (
                  <div key={r.id} className="reservation-card">
                    <div className="reservation-card__image">
                      {r.workspaceImage
                        ? <img src={r.workspaceImage} alt={r.workspaceName ?? 'Espacio'} style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
                        : <div className="reservation-card__image-placeholder" />}
                    </div>

                    <div className="reservation-card__body">
                      <div className="reservation-card__main">
                        <h3 className="reservation-card__name">
                          {r.workspaceName ?? 'Espacio de trabajo'}
                        </h3>
                        <p className="reservation-card__number">
                          ID: {r.id.slice(0, 8).toUpperCase()}
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
                        {canCancel(r) && (
                          <button
                            className="reservation-card__btn reservation-card__btn--danger"
                            onClick={() => handleCancel(r.id)}
                            disabled={isCancelling}
                          >
                            {isCancelling
                              ? <Loader2 size={16} style={{ animation: 'spin 0.8s linear infinite' }} />
                              : <Trash2 size={16} />}
                            Cancelar
                          </button>
                        )}
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

          {!isLoading && !error && filtered.length === 0 && (
            <div className="reservations-page__empty">
              <Calendar size={48} />
              <h3>No hay reservas</h3>
              <p>
                {searchQuery
                  ? 'No se encontraron reservas que coincidan con tu búsqueda.'
                  : 'Aún no tenés reservas. ¡Explorá los espacios disponibles!'}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Reservations;
