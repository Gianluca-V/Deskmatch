import { Calendar, Euro, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import './CompanyReservations.css';

function CompanyReservations({ reservations, isLoading }) {
  const navigate = useNavigate();

  const statusKey = (status) => {
    if (typeof status === 'number') {
      return {
        1: 'confirmed',
        2: 'cancelled',
        3: 'completed',
      }[status] ?? 'pending';
    }
    return status?.toLowerCase();
  };

  const getStatusBadgeClass = (status) => {
    const statusMap = {
      confirmed: 'company-reservations__status--confirmed',
      pending: 'company-reservations__status--pending',
      cancelled: 'company-reservations__status--cancelled',
      completed: 'company-reservations__status--completed',
    };
    return statusMap[statusKey(status)] || 'company-reservations__status--pending';
  };

  const getStatusLabel = (status) => {
    const statusMap = {
      confirmed: 'Confirmada',
      pending: 'Pendiente',
      cancelled: 'Cancelada',
      completed: 'Completada',
    };
    return statusMap[statusKey(status)] || status;
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', { month: 'short', day: 'numeric' });
  };

  const formatDateRange = (startDate, endDate) => {
    if (!startDate) return '';
    const start = formatDate(startDate);
    const end = endDate ? formatDate(endDate) : '';
    return `${start} ${end ? `- ${end}` : ''}`;
  };

  if (isLoading) {
    return (
      <div className="company-reservations">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 className="company-reservations__title">Reservas Recientes</h3>
          <button
            onClick={() => navigate('/reservations')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '10px 20px',
              background: 'linear-gradient(135deg, #7c3aed 0%, #ec4899 100%)',
              color: 'white',
              border: 'none',
              borderRadius: '8px',
              fontSize: '14px',
              fontWeight: '600',
              cursor: 'pointer',
            }}
          >
            Ver todas las reservas
          </button>
        </div>
        <div className="company-reservations__list">
          {[1, 2, 3].map((i) => (
            <div key={i} className="company-reservations__item skeleton">
              <div className="company-reservations__item-image"></div>
              <div className="company-reservations__item-content">
                <div className="skeleton-line" style={{ height: '14px', width: '150px', marginBottom: '8px' }}></div>
                <div className="skeleton-line" style={{ height: '12px', width: '200px' }}></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (!reservations || reservations.length === 0) {
    return (
      <div className="company-reservations">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <h3 className="company-reservations__title">Reservas Recientes</h3>
          <button
            onClick={() => navigate('/reservations')}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              padding: '10px 20px',
              background: 'linear-gradient(135deg, #7c3aed 0%, #ec4899 100%)',
              color: 'white',
              border: 'none',
              borderRadius: '8px',
              fontSize: '14px',
              fontWeight: '600',
              cursor: 'pointer',
            }}
          >
            Ver todas las reservas
          </button>
        </div>
        <div className="company-reservations__empty">
          <p>No hay reservas recientes</p>
          <button
            onClick={() => navigate('/reservations')}
            className="company-reservations__view-all-btn"
          >
            Ver todas las reservas
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="company-reservations">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h3 className="company-reservations__title">Reservas Recientes</h3>
        <button
          onClick={() => navigate('/reservations')}
          style={{
            padding: '10px 20px',
            background: 'linear-gradient(135deg, #7c3aed 0%, #ec4899 100%)',
            color: 'white',
            border: 'none',
            borderRadius: '8px',
            fontSize: '14px',
            fontWeight: '600',
            cursor: 'pointer',
          }}
        >
          Ver todas las reservas
        </button>
      </div>
      <div className="company-reservations__list">
        {reservations.slice(0, 5).map((reservation) => (
          <div key={reservation.id} className="company-reservations__item">
            <div className="company-reservations__item-image">
              {reservation.space?.image || reservation.workspaceImage ? (
                <img
                  src={reservation.space?.image ?? reservation.workspaceImage}
                  alt={reservation.space?.name ?? reservation.workspaceName}
                />
              ) : (
                <div className="company-reservations__image-placeholder"></div>
              )}
            </div>
            <div className="company-reservations__item-content">
              <div className="company-reservations__header">
                <div>
                  <h4 className="company-reservations__space-name">
                    {reservation.space?.name || reservation.workspaceName || 'Espacio desconocido'}
                  </h4>
                  <p className="company-reservations__user-name">
                    {reservation.user?.firstName
                      ? `${reservation.user.firstName} ${reservation.user.lastName ?? ''}`.trim()
                      : `Huésped ${reservation.guestId?.slice(0, 8).toUpperCase() ?? 'N/D'}`}
                  </p>
                </div>
                <span className={`company-reservations__status ${getStatusBadgeClass(reservation.status)}`}>
                  {getStatusLabel(reservation.status)}
                </span>
              </div>
              <div className="company-reservations__details">
                <div className="company-reservations__detail">
                  <Calendar size={14} />
                  <span>{formatDateRange(reservation.startDate ?? reservation.startTime, reservation.endDate ?? reservation.endTime)}</span>
                </div>
              </div>
              <div className="company-reservations__price">
                <Euro size={14} />
                <span>{reservation.totalPrice || 0}</span>
              </div>
            </div>
          </div>
        ))}
      </div>
      {reservations.length > 5 && (
        <button
          onClick={() => navigate('/reservations')}
          className="company-reservations__view-all-btn"
        >
          Ver todas las reservas
        </button>
      )}
    </div>
  );
}

export default CompanyReservations;
