import { useNavigate } from 'react-router-dom';
import { Calendar } from 'lucide-react';
import { useMyReservations } from '../hooks/useMyReservations';
import './ReservationSummary.css';

function ReservationSummary() {
  const navigate = useNavigate();
  const { data: reservations = [], isLoading } = useMyReservations();

  const confirmed = reservations.filter((r) => r.status === 1).length;
  const cancelled = reservations.filter((r) => r.status === 2).length;
  const completed = reservations.filter((r) => r.status === 3).length;

  return (
    <div className="reservation-summary">
      <div className="reservation-summary__header">
        <Calendar size={20} />
        <h3>Resumen de Reservas</h3>
      </div>

      <div className="reservation-summary__stats">
        <div className="reservation-summary__stat reservation-summary__stat--active">
          <label>Confirmadas</label>
          <p className="reservation-summary__count">
            {isLoading ? '—' : confirmed}
          </p>
        </div>

        <div className="reservation-summary__stat reservation-summary__stat--completed">
          <label>Completadas</label>
          <p className="reservation-summary__count">
            {isLoading ? '—' : completed}
          </p>
          {completed > 0 && (
            <span className="reservation-summary__count-badge">{completed}</span>
          )}
        </div>

        <div className="reservation-summary__stat reservation-summary__stat--pending">
          <label>Canceladas</label>
          <p className="reservation-summary__count">
            {isLoading ? '—' : cancelled}
          </p>
          {cancelled > 0 && (
            <span className="reservation-summary__badge">{cancelled}</span>
          )}
        </div>
      </div>

      <button className="reservation-summary__view-all" onClick={() => navigate('/reservations')}>
        Ver Todas las Reservas
      </button>
    </div>
  );
}

export default ReservationSummary;
