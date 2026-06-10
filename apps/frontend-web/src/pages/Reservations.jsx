import { useState, useMemo } from 'react';
import { Calendar, Search, X, Eye, Trash2 } from 'lucide-react';
import './Reservations.css';

function Reservations() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  // TODO: Obtener reservas desde API
  const reservations = [
    {
      id: 1,
      number: 'Reserva #1',
      name: 'Oficina Moderna Centro',
      checkInDate: '4 de junio de 2026',
      checkOutDate: '9 de junio de 2026',
      status: 'confirmed',
      image: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=300&fit=crop',
      totalPrice: '€1250',
    },
    {
      id: 2,
      number: 'Reserva #2',
      name: 'Coworking Barcelona Tech',
      checkInDate: '14 de junio de 2026',
      checkOutDate: '21 de junio de 2026',
      status: 'pending',
      image: 'https://images.unsplash.com/photo-1559027615-cd4628902d4a?w=400&h=300&fit=crop',
      totalPrice: '€980',
    },
    {
      id: 3,
      number: 'Reserva #3',
      name: 'Sala de Reuniones Premium',
      checkInDate: '19 de mayo de 2026',
      checkOutDate: '19 de mayo de 2026',
      status: 'cancelled',
      image: 'https://images.unsplash.com/photo-1554224311-beee415c15cb?w=400&h=300&fit=crop',
      totalPrice: '€250',
    },
  ];

  const filteredReservations = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return reservations.filter((reservation) => {
      const matchesQuery =
        !query ||
        reservation.name.toLowerCase().includes(query) ||
        reservation.checkInDate.toLowerCase().includes(query);
      const matchesStatus = statusFilter === 'all' || reservation.status === statusFilter;
      return matchesQuery && matchesStatus;
    });
  }, [searchQuery, statusFilter]);

  const getStatusBadge = (status) => {
    const badges = {
      confirmed: { label: 'Confirmada', class: 'reservation-card__badge--confirmed' },
      pending: { label: 'Pendiente', class: 'reservation-card__badge--pending' },
      cancelled: { label: 'Cancelada', class: 'reservation-card__badge--cancelled' },
    };
    return badges[status] || badges.pending;
  };

  const getTotalCount = (status) => {
    return reservations.filter(r => r.status === status).length;
  };

  return (
    <div className="reservations-page">
      <div className="container">
        <div className="reservations-page__header">
          <div>
            <h1>Mis Reservas</h1>
            <p>Gestiona y revisa tus espacios reservados</p>
          </div>
        </div>

        <div className="reservations-page__tabs">
          <button
            className={`reservations-page__tab ${statusFilter === 'all' ? 'reservations-page__tab--active' : ''}`}
            onClick={() => setStatusFilter('all')}
          >
            Todas ({reservations.length})
          </button>
          <button
            className={`reservations-page__tab ${statusFilter === 'confirmed' ? 'reservations-page__tab--active' : ''}`}
            onClick={() => setStatusFilter('confirmed')}
          >
            Confirmadas ({getTotalCount('confirmed')})
          </button>
          <button
            className={`reservations-page__tab ${statusFilter === 'pending' ? 'reservations-page__tab--active' : ''}`}
            onClick={() => setStatusFilter('pending')}
          >
            Pendientes ({getTotalCount('pending')})
          </button>
          <button
            className={`reservations-page__tab ${statusFilter === 'cancelled' ? 'reservations-page__tab--active' : ''}`}
            onClick={() => setStatusFilter('cancelled')}
          >
            Canceladas ({getTotalCount('cancelled')})
          </button>
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
            <button
              className="reservations-page__clear-search"
              onClick={() => setSearchQuery('')}
            >
              <X size={18} />
            </button>
          )}
        </div>

        <div className="reservations-page__content">
          {filteredReservations.length > 0 ? (
            <div className="reservations-page__list">
              {filteredReservations.map((reservation) => {
                const badge = getStatusBadge(reservation.status);
                return (
                  <div key={reservation.id} className="reservation-card">
                    <div className="reservation-card__image">
                      {reservation.image ? (
                        <img src={reservation.image} alt={reservation.name} />
                      ) : (
                        <div className="reservation-card__image-placeholder"></div>
                      )}
                    </div>

                    <div className="reservation-card__body">
                      <div className="reservation-card__main">
                        <h3 className="reservation-card__name">{reservation.name}</h3>
                        <p className="reservation-card__number">{reservation.number}</p>

                        <div className="reservation-card__dates">
                          <div className="reservation-card__date-item">
                            <Calendar size={16} />
                            <div>
                              <label>Entrada</label>
                              <p>{reservation.checkInDate}</p>
                            </div>
                          </div>
                          <div className="reservation-card__date-item">
                            <Calendar size={16} />
                            <div>
                              <label>Salida</label>
                              <p>{reservation.checkOutDate}</p>
                            </div>
                          </div>
                        </div>

                        <div className="reservation-card__price-section">
                          <label>Total pagado</label>
                          <p className="reservation-card__price">{reservation.totalPrice}</p>
                        </div>
                      </div>

                      <div className="reservation-card__footer">
                        <button className="reservation-card__btn reservation-card__btn--secondary">
                          <Eye size={16} />
                          Ver Espacio
                        </button>
                        <button className="reservation-card__btn reservation-card__btn--danger">
                          <Trash2 size={16} />
                          Cancelar
                        </button>
                      </div>
                    </div>

                    <div className="reservation-card__side">
                      <span className={`reservation-card__badge ${badge.class}`}>
                        {badge.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="reservations-page__empty">
              <Calendar size={48} />
              <h3>No hay reservas</h3>
              <p>
                {searchQuery
                  ? 'No se encontraron reservas que coincidan con tu búsqueda'
                  : 'Aún no tienes reservas. ¡Haz tu primera reserva hoy!'}
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Reservations;
