import { History } from 'lucide-react';
import './RecentActivity.css';

function RecentActivity() {
  // TODO: Obtener datos de actividad reciente desde API
  const activities = [
    {
      id: 1,
      name: 'Oficina Moderna Centro',
      date: '4 jun 2026',
      status: 'confirmed',
      image: null,
    },
    {
      id: 2,
      name: 'Coworking Barcelona Tech',
      date: '14 jun 2026',
      status: 'pending',
      image: null,
    },
    {
      id: 3,
      name: 'Sala de Reuniones Premium',
      date: '19 may 2026',
      status: 'cancelled',
      image: null,
    },
  ];

  const getStatusBadge = (status) => {
    const badges = {
      confirmed: { label: 'Confirmada', class: 'activity-item__badge--confirmed' },
      pending: { label: 'Pendiente', class: 'activity-item__badge--pending' },
      cancelled: { label: 'Cancelada', class: 'activity-item__badge--cancelled' },
    };
    return badges[status] || badges.pending;
  };

  return (
    <div className="recent-activity">
      <div className="recent-activity__header">
        <History size={20} />
        <h3>Actividad Reciente</h3>
      </div>

      <div className="recent-activity__list">
        {activities.length > 0 ? (
          activities.map((activity) => {
            const badge = getStatusBadge(activity.status);
            return (
              <div key={activity.id} className="activity-item">
                <div className="activity-item__image">
                  {activity.image ? (
                    <img src={activity.image} alt={activity.name} />
                  ) : (
                    <div className="activity-item__image-placeholder"></div>
                  )}
                </div>
                <div className="activity-item__content">
                  <h4 className="activity-item__name">{activity.name}</h4>
                  <p className="activity-item__date">{activity.date}</p>
                </div>
                <span className={`activity-item__badge ${badge.class}`}>
                  {badge.label}
                </span>
              </div>
            );
          })
        ) : (
          <div className="recent-activity__empty">
            <p>No hay actividad reciente</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default RecentActivity;
