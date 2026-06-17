import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  MapPin, Users, Star, Wifi, Clock, ArrowLeft,
  Building2, ChevronRight
} from 'lucide-react';
import api from '../lib/api';
import BookingWidget from '../components/BookingWidget';
import './WorkspaceDetail.css';

function useWorkspace(id) {
  return useQuery({
    queryKey: ['workspace', id],
    queryFn: () => api.get(`/api/workspaces/${id}`).then((r) => r.data),
    enabled: !!id,
  });
}

const AMENITY_ICONS = {
  WiFi: '📶', Coffee: '☕', Parking: '🅿️', AC: '❄️',
  Kitchen: '🍽️', Projector: '📽️', Whiteboard: '📋',
  Printer: '🖨️', Phone: '📞', Reception: '🏢',
};

function AmenityPill({ name }) {
  return (
    <span className="wd-amenity">
      <span>{AMENITY_ICONS[name] ?? '✓'}</span>
      {name}
    </span>
  );
}

function SkeletonDetail() {
  return (
    <div className="wd-skeleton">
      <div className="wd-skeleton__hero" />
      <div className="wd-skeleton__body">
        <div className="wd-skeleton__main">
          <div className="wd-skeleton__line wd-skeleton__line--title" />
          <div className="wd-skeleton__line" />
          <div className="wd-skeleton__line wd-skeleton__line--short" />
        </div>
        <div className="wd-skeleton__widget" />
      </div>
    </div>
  );
}

export default function WorkspaceDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: workspace, isLoading, error } = useWorkspace(id);

  if (isLoading) return <div className="wd-page"><SkeletonDetail /></div>;

  if (error || !workspace) {
    return (
      <div className="wd-page">
        <div className="wd-error">
          <Building2 size={48} />
          <h2>Espacio no encontrado</h2>
          <p>El workspace que buscás no existe o fue desactivado.</p>
          <button className="btn btn-primary" onClick={() => navigate(-1)}>
            Volver
          </button>
        </div>
      </div>
    );
  }

  const heroImage = workspace.images?.[0];
  const location = [workspace.city, workspace.country].filter(Boolean).join(', ');

  return (
    <div className="wd-page">
      <div className="container">

        {/* Breadcrumb */}
        <nav className="wd-breadcrumb">
          <button className="wd-breadcrumb__back" onClick={() => navigate(-1)}>
            <ArrowLeft size={16} />
            Volver
          </button>
          <ChevronRight size={14} className="wd-breadcrumb__sep" />
          <span className="wd-breadcrumb__current">{workspace.name}</span>
        </nav>

        {/* Hero */}
        <div className="wd-hero">
          {heroImage ? (
            <img src={heroImage} alt={workspace.name} className="wd-hero__img" />
          ) : (
            <div className="wd-hero__placeholder">
              <Building2 size={64} />
            </div>
          )}
          {workspace.images?.length > 1 && (
            <div className="wd-hero__thumbnails">
              {workspace.images.slice(1, 4).map((img, i) => (
                <img key={i} src={img} alt={`${workspace.name} ${i + 2}`} className="wd-hero__thumb" />
              ))}
            </div>
          )}
        </div>

        {/* Layout: info + widget */}
        <div className="wd-layout">

          {/* Main info */}
          <div className="wd-main">
            <div className="wd-main__header">
              <div>
                <h1 className="wd-main__title">{workspace.name}</h1>
                {location && (
                  <p className="wd-main__location">
                    <MapPin size={15} />
                    {location}
                    {workspace.address && ` · ${workspace.address}`}
                  </p>
                )}
              </div>
              <div className="wd-main__badges">
                {workspace.rating > 0 && (
                  <span className="wd-badge wd-badge--rating">
                    <Star size={13} fill="currentColor" />
                    {workspace.rating?.toFixed(1)}
                    <span className="wd-badge__sub">({workspace.reviewCount})</span>
                  </span>
                )}
                <span className={`wd-badge ${workspace.isActive ? 'wd-badge--active' : 'wd-badge--inactive'}`}>
                  {workspace.isActive ? 'Disponible' : 'No disponible'}
                </span>
              </div>
            </div>

            <div className="wd-stats">
              <div className="wd-stat">
                <Users size={18} />
                <div>
                  <strong>{workspace.capacity}</strong>
                  <span>Capacidad</span>
                </div>
              </div>
              <div className="wd-stat">
                <Clock size={18} />
                <div>
                  <strong>${workspace.pricePerHour?.toFixed(2)}</strong>
                  <span>por hora</span>
                </div>
              </div>
              {workspace.pricePerDay && (
                <div className="wd-stat">
                  <Clock size={18} />
                  <div>
                    <strong>${workspace.pricePerDay?.toFixed(2)}</strong>
                    <span>por día</span>
                  </div>
                </div>
              )}
            </div>

            {workspace.description && (
              <section className="wd-section">
                <h2 className="wd-section__title">Sobre este espacio</h2>
                <p className="wd-section__text">{workspace.description}</p>
              </section>
            )}

            {workspace.amenities?.length > 0 && (
              <section className="wd-section">
                <h2 className="wd-section__title">Amenities</h2>
                <div className="wd-amenities">
                  {workspace.amenities.map((a) => (
                    <AmenityPill key={a} name={a} />
                  ))}
                </div>
              </section>
            )}

            {workspace.address && (
              <section className="wd-section">
                <h2 className="wd-section__title">Ubicación</h2>
                <div className="wd-location-card">
                  <MapPin size={16} />
                  <div>
                    <p>{workspace.address}</p>
                    {location && <p className="wd-location-card__sub">{location}</p>}
                  </div>
                </div>
              </section>
            )}
          </div>

          {/* Booking Widget */}
          <aside className="wd-aside">
            <BookingWidget workspace={workspace} />
          </aside>
        </div>
      </div>
    </div>
  );
}
