import { Calendar, Euro } from 'lucide-react';
import './CompanyReservations.css';

function CompanyReservations({ reservations, isLoading }) {
  const getStatusBadgeClass = (status) => {
    const statusMap = {
      confirmed: 'company-reservations__status--confirmed',
      pending: 'company-reservations__status--pending',
      cancelled: 'company-reservations__status--cancelled',
      completed: 'company-reservations__status--completed',
    };
    return statusMap[status?.toLowerCase()] || 'company-reservations__status--pending';
  };

  const getStatusLabel = (status) => {
    const statusMap = {
      confirmed: 'Confirmada',
      pending: 'Pendiente',
      cancelled: 'Cancelada',
      completed: 'Completada',
    };
    return statusMap[status?.toLowerCase()] || status;
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
        <h3 className="company-reservations__title">Reservas Recientes</h3>
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
        <h3 className="company-reservations__title">Reservas Recientes</h3>
        <div className="company-reservations__empty">
          <p>No hay reservas recientes</p>
        </div>
      </div>
    );
  }

  return (
    <div className="company-reservations">
      <h3 className="company-reservations__title">Reservas Recientes</h3>
      <div className="company-reservations__list">
        {reservations.slice(0, 5).map((reservation) => (
          <div key={reservation.id} className="company-reservations__item">
            <div className="company-reservations__item-image">
              {reservation.space?.image ? (
                <img src={reservation.space.image} alt={reservation.space.name} />
              ) : (
                <div className="company-reservations__image-placeholder"></div>
              )}
            </div>
            <div className="company-reservations__item-content">
              <div className="company-reservations__header">
                <div>
                  <h4 className="company-reservations__space-name">
                    {reservation.space?.name || 'Espacio desconocido'}
                  </h4>
                  <p className="company-reservations__user-name">
                    {reservation.user?.firstName} {reservation.user?.lastName}
                  </p>
                </div>
                <span className={`company-reservations__status ${getStatusBadgeClass(reservation.status)}`}>
                  {getStatusLabel(reservation.status)}
                </span>
              </div>
              <div className="company-reservations__details">
                <div className="company-reservations__detail">
                  <Calendar size={14} />
                  <span>{formatDateRange(reservation.startDate, reservation.endDate)}</span>
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
    </div>
  );
}

export default CompanyReservations;
