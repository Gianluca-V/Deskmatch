import { useNavigate } from 'react-router-dom';
import { History, Calendar } from 'lucide-react';
import { useMyReservations } from '../hooks/useMyReservations';
import './RecentActivity.css';

const STATUS_BADGE = {
  1: { label: 'Confirmada', cls: 'activity-item__badge--confirmed' },
  2: { label: 'Cancelada', cls: 'activity-item__badge--cancelled' },
  3: { label: 'Completada', cls: 'activity-item__badge--pending' },
};

function formatDate(iso) {
  return new Date(iso).toLocaleDateString('es-AR', {
    day: 'numeric', month: 'short', year: 'numeric',
  });
}

function RecentActivity() {
  const navigate = useNavigate();
  const { data: reservations = [], isLoading } = useMyReservations();

  const recent = reservations.slice(0, 3);

  return (
    <div className="recent-activity">
      <div className="recent-activity__header">
        <History size={20} />
        <h3>Actividad Reciente</h3>
      </div>

      <div className="recent-activity__list">
        {isLoading && (
          <>
            {[1, 2, 3].map((i) => (
              <div key={i} className="activity-item">
                <div className="activity-item__image">
                  <div className="activity-item__image-placeholder" />
                </div>
                <div className="activity-item__content">
                  <div style={{ height: 13, background: '#e2e8f0', borderRadius: 5, marginBottom: 6, width: '70%' }} />
                  <div style={{ height: 11, background: '#f1f5f9', borderRadius: 5, width: '45%' }} />
                </div>
              </div>
            ))}
          </>
        )}

        {!isLoading && recent.length > 0 &&
          recent.map((r) => {
            const badge = STATUS_BADGE[r.status] ?? STATUS_BADGE[3];
            return (
              <div
                key={r.id}
                className="activity-item activity-item--clickable"
                onClick={() => navigate(`/workspaces/${r.workspaceId}`)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => e.key === 'Enter' && navigate(`/workspaces/${r.workspaceId}`)}
              >
                <div className="activity-item__image">
                  <div className="activity-item__image-placeholder">
                    <Calendar size={16} />
                  </div>
                </div>
                <div className="activity-item__content">
                  <h4 className="activity-item__name">
                    Reserva #{r.id.slice(0, 6).toUpperCase()}
                  </h4>
                  <p className="activity-item__date">{formatDate(r.startTime)}</p>
                </div>
                <span className={`activity-item__badge ${badge.cls}`}>
                  {badge.label}
                </span>
              </div>
            );
          })
        }

        {!isLoading && recent.length === 0 && (
          <div className="recent-activity__empty">
            <p>No hay actividad reciente</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default RecentActivity;
