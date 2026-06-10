import { useNavigate } from 'react-router-dom';
import { Calendar } from 'lucide-react';
import './ReservationSummary.css';

function ReservationSummary() {
  const navigate = useNavigate();
  // TODO: Obtener datos de reservas desde API
  const reservations = {
    active: 0,
    pending: 1,
    completed: 1,
  };

  const handleViewAll = () => {
    navigate('/reservations');
  };

  return (
    <div className="reservation-summary">
      <div className="reservation-summary__header">
        <Calendar size={20} />
        <h3>Resumen de Reservas</h3>
      </div>

      <div className="reservation-summary__stats">
        <div className="reservation-summary__stat reservation-summary__stat--active">
          <label>Activas</label>
          <p className="reservation-summary__count">{reservations.active}</p>
        </div>

        <div className="reservation-summary__stat reservation-summary__stat--pending">
          <label>Pendientes</label>
          <p className="reservation-summary__count">{reservations.pending}</p>
          <span className="reservation-summary__badge">{reservations.pending}</span>
        </div>

        <div className="reservation-summary__stat reservation-summary__stat--completed">
          <label>Completadas</label>
          <p className="reservation-summary__count">{reservations.completed}</p>
          <span className="reservation-summary__count-badge">{reservations.completed}</span>
        </div>
      </div>

      <button className="reservation-summary__view-all" onClick={handleViewAll}>
        Ver Todas las Reservas
      </button>
    </div>
  );
}

export default ReservationSummary;
