import { useState, useCallback, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { MapContainer, TileLayer, Marker } from 'react-leaflet';
import L from 'leaflet';
import {
  MapPin, Users, Star, Clock, ArrowLeft,
  Building2, ChevronRight, Maximize2
} from 'lucide-react';
import api from '../lib/api';
import Modal from '../components/Modal';
import BookingWidget from '../components/BookingWidget';
import 'leaflet/dist/leaflet.css';
import './WorkspaceDetail.css';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

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
  const [activeIndex, setActiveIndex] = useState(0);
  const [galleryOpen, setGalleryOpen] = useState(false);
  const [galleryIndex, setGalleryIndex] = useState(0);
  const [mapOpen, setMapOpen] = useState(false);

  const allImages = workspace?.images || [];

  const openGallery = useCallback((idx) => {
    setGalleryIndex(idx);
    setGalleryOpen(true);
  }, []);

  const closeGallery = useCallback(() => setGalleryOpen(false), []);

  const galleryPrev = useCallback(() => {
    setGalleryIndex((prev) => (prev > 0 ? prev - 1 : allImages.length - 1));
  }, [allImages.length]);

  const galleryNext = useCallback(() => {
    setGalleryIndex((prev) => (prev < allImages.length - 1 ? prev + 1 : 0));
  }, [allImages.length]);

  useEffect(() => {
    if (!galleryOpen) return;
    const handleKey = (e) => {
      if (e.key === 'ArrowLeft') { e.preventDefault(); galleryPrev(); }
      if (e.key === 'ArrowRight') { e.preventDefault(); galleryNext(); }
    };
    document.addEventListener('keydown', handleKey);
    return () => document.removeEventListener('keydown', handleKey);
  }, [galleryOpen, galleryPrev, galleryNext]);

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

  const heroImage = allImages[activeIndex];
  const maxThumbs = 5;
  const visibleThumbs = allImages.slice(0, maxThumbs);
  const extraCount = allImages.length - maxThumbs;
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
            <img
              src={heroImage}
              alt={workspace.name}
              className="wd-hero__img"
              onClick={() => openGallery(activeIndex)}
            />
          ) : (
            <div className="wd-hero__placeholder">
              <Building2 size={64} />
            </div>
          )}
          {allImages.length > 1 && (
            <div className="wd-hero__thumbs">
              {visibleThumbs.map((img, i) => (
                <button
                  key={i}
                  className={`wd-hero__thumb ${i === activeIndex ? 'wd-hero__thumb--active' : ''}`}
                  onClick={() => setActiveIndex(i)}
                >
                  <img src={img} alt={`${workspace.name} ${i + 1}`} />
                </button>
              ))}
              {extraCount > 0 && (
                <button className="wd-hero__thumb wd-hero__thumb--more" onClick={() => openGallery(activeIndex)}>
                  +{extraCount}
                </button>
              )}
            </div>
          )}
        </div>

        {/* Full-screen gallery modal */}
        <Modal isOpen={galleryOpen} onClose={closeGallery} className="wd-gallery-modal">
          <div className="wd-gallery">
            <div className="wd-gallery__counter">
              {galleryIndex + 1} / {allImages.length}
            </div>
            <button className="wd-gallery__nav wd-gallery__nav--prev" onClick={galleryPrev}>
              <ChevronLeft size={28} />
            </button>
            <img
              className="wd-gallery__img"
              src={allImages[galleryIndex]}
              alt={`${workspace.name} ${galleryIndex + 1}`}
            />
            <button className="wd-gallery__nav wd-gallery__nav--next" onClick={galleryNext}>
              <ChevronRightIcon size={28} />
            </button>
          </div>
        </Modal>

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
                {workspace.latitude != null && workspace.longitude != null && (
                  <>
                    <button
                      type="button"
                      className="wd-location__map-toggle"
                      onClick={() => setMapOpen((prev) => !prev)}
                    >
                      <Maximize2 size={14} />
                      {mapOpen ? 'Ocultar mapa' : 'Ver en mapa'}
                    </button>
                    {mapOpen && (
                      <div className="wd-location__map">
                        <MapContainer
                          center={[workspace.latitude, workspace.longitude]}
                          zoom={15}
                          className="wd-location__leaflet"
                          zoomControl={true}
                          scrollWheelZoom={true}
                        >
                          <TileLayer
                            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                          />
                          <Marker position={[workspace.latitude, workspace.longitude]} />
                        </MapContainer>
                      </div>
                    )}
                  </>
                )}
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
