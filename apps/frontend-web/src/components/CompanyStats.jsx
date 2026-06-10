import { Building2, Calendar, Star } from 'lucide-react';
import './CompanyStats.css';

function CompanyStats({ stats, isLoading }) {
  if (isLoading) {
    return (
      <div className="company-stats">
        <div className="company-stats__stat">
          <div className="skeleton-line" style={{ width: '40px', height: '12px' }}></div>
          <div className="skeleton-line" style={{ width: '60px', height: '28px', marginTop: '12px' }}></div>
        </div>
        <div className="company-stats__stat">
          <div className="skeleton-line" style={{ width: '40px', height: '12px' }}></div>
          <div className="skeleton-line" style={{ width: '60px', height: '28px', marginTop: '12px' }}></div>
        </div>
        <div className="company-stats__stat">
          <div className="skeleton-line" style={{ width: '40px', height: '12px' }}></div>
          <div className="skeleton-line" style={{ width: '60px', height: '28px', marginTop: '12px' }}></div>
        </div>
      </div>
    );
  }

  const publishedSpaces = stats?.publishedSpaces || 0;
  const totalReservations = stats?.totalReservations || 0;
  const averageRating = stats?.averageRating || 0;
  const totalReviews = stats?.totalReviews || 0;

  return (
    <div className="company-stats">
      <div className="company-stats__stat">
        <div className="company-stats__stat-icon">
          <Building2 size={20} />
        </div>
        <label className="company-stats__stat-label">Espacios Publicados</label>
        <p className="company-stats__stat-value">{publishedSpaces}</p>
      </div>

      <div className="company-stats__stat company-stats__stat--highlight">
        <div className="company-stats__stat-icon">
          <Calendar size={20} />
        </div>
        <label className="company-stats__stat-label">Total de Reservas</label>
        <p className="company-stats__stat-value">{totalReservations}</p>
      </div>

      <div className="company-stats__stat">
        <div className="company-stats__stat-icon">
          <Star size={20} />
        </div>
        <label className="company-stats__stat-label">Puntuación</label>
        <div className="company-stats__stat-rating">
          <p className="company-stats__stat-value">{averageRating.toFixed(1)}</p>
          <small className="company-stats__stat-reviews">({totalReviews} reseñas)</small>
        </div>
      </div>
    </div>
  );
}

export default CompanyStats;
