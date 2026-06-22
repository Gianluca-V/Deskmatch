import { useNavigate } from 'react-router-dom';
import { MapPin, Euro } from 'lucide-react';
import './CompanySpaces.css';

function CompanySpaces({ spaces, isLoading }) {
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <div className="company-spaces">
        <div className="company-spaces__header">
          <h3 className="company-spaces__title">Resumen de Espacios</h3>
          <button className="company-spaces__view-all" disabled>
            Ver Dashboard Completo
          </button>
        </div>
        <div className="company-spaces__grid">
          {[1, 2, 3].map((i) => (
            <div key={i} className="company-spaces__card skeleton">
              <div className="company-spaces__image"></div>
              <div className="company-spaces__info">
                <div className="skeleton-line" style={{ height: '16px', marginBottom: '8px' }}></div>
                <div className="skeleton-line" style={{ height: '12px', width: '60%' }}></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (!spaces || spaces.length === 0) {
    return (
      <div className="company-spaces">
        <div className="company-spaces__header">
          <h3 className="company-spaces__title">Resumen de Espacios</h3>
        </div>
        <div className="company-spaces__empty">
          <p>Aún no tienes espacios publicados</p>
        </div>
      </div>
    );
  }

  return (
    <div className="company-spaces">
      <div className="company-spaces__header">
        <h3 className="company-spaces__title">Resumen de Espacios</h3>
        <button
          className="company-spaces__view-all"
          onClick={() => navigate('/dashboard')}
        >
          Ver Dashboard Completo
        </button>
      </div>
      <div className="company-spaces__grid">
        {spaces.slice(0, 3).map((space) => (
          <div
            key={space.id}
            className="company-spaces__card"
            onClick={() => navigate(`/workspaces/${space.id}`)}
          >
            <div className="company-spaces__image">
              {space.images && space.images.length > 0 ? (
                <img src={space.images[0]} alt={space.name} />
              ) : (
                <div className="company-spaces__image-placeholder"></div>
              )}
            </div>
            <div className="company-spaces__info">
              <h4 className="company-spaces__name">{space.name}</h4>
              <div className="company-spaces__location">
                <MapPin size={14} />
                <span>{space.city || space.location}</span>
              </div>
              <div className="company-spaces__type">
                <span className="company-spaces__type-badge">{space.type}</span>
              </div>
              <div className="company-spaces__price">
                <Euro size={14} />
                <span>€ {space.pricePerDay}</span>
                <span className="company-spaces__price-period"> por día</span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default CompanySpaces;
