import { useState } from 'react';
import { User, Mail, Phone, MapPin, Edit } from 'lucide-react';
import EditProfileModal from './EditProfileModal';
import './GuestProfileCard.css';

function GuestProfileCard({ user, isLoading, error }) {
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  if (isLoading) {
    return (
      <div className="guest-profile-card">
        <div className="guest-profile-card__header">
          <div className="guest-profile-card__avatar skeleton-avatar"></div>
          <div>
            <div className="skeleton-line skeleton-line--name"></div>
            <div className="skeleton-line skeleton-line--email"></div>
          </div>
        </div>
        <div className="guest-profile-card__content">
          <div className="skeleton-line"></div>
          <div className="skeleton-line"></div>
          <div className="skeleton-line"></div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="guest-profile-card guest-profile-card--error">
        <div className="guest-profile-card__error-message">
          <p>Error al cargar perfil</p>
          <small>No pudimos obtener tu información. Intenta nuevamente más tarde.</small>
        </div>
      </div>
    );
  }

  const firstName = user?.firstName || 'Usuario';
  const lastName = user?.lastName || '';
  const email = user?.email || 'No especificado';
  const phone = user?.phoneNumber || user?.phone || 'No especificado';
  const location = user?.location || 'No especificado';

  return (
    <>
      <div className="guest-profile-card">
        <div className="guest-profile-card__header">
          <div className="guest-profile-card__avatar">
            <User size={32} />
          </div>
          <div style={{ flex: 1 }}>
            <h3 className="guest-profile-card__name">
              {firstName} {lastName}
            </h3>
            <p className="guest-profile-card__subtitle">Información Personal</p>
          </div>
          <button
            onClick={() => setIsEditModalOpen(true)}
            className="guest-profile-card__edit-button"
            title="Editar perfil"
          >
            <Edit size={18} />
          </button>
        </div>

        <div className="guest-profile-card__content">
        <div className="guest-profile-card__field">
          <div className="guest-profile-card__field-icon">
            <User size={18} />
          </div>
          <div>
            <label className="guest-profile-card__field-label">Nombre completo</label>
            <p className="guest-profile-card__field-value">
              {firstName} {lastName}
            </p>
          </div>
        </div>

        <div className="guest-profile-card__field">
          <div className="guest-profile-card__field-icon">
            <Mail size={18} />
          </div>
          <div>
            <label className="guest-profile-card__field-label">Correo electrónico</label>
            <p className="guest-profile-card__field-value">{email}</p>
          </div>
        </div>

        <div className="guest-profile-card__field">
          <div className="guest-profile-card__field-icon">
            <Phone size={18} />
          </div>
          <div>
            <label className="guest-profile-card__field-label">Teléfono</label>
            <p className={`guest-profile-card__field-value ${!user?.phone ? 'guest-profile-card__field-value--empty' : ''}`}>
              {phone}
            </p>
          </div>
        </div>

        <div className="guest-profile-card__field">
          <div className="guest-profile-card__field-icon">
            <MapPin size={18} />
          </div>
          <div>
            <label className="guest-profile-card__field-label">Ubicación</label>
            <p className={`guest-profile-card__field-value ${!user?.location ? 'guest-profile-card__field-value--empty' : ''}`}>
              {location}
            </p>
          </div>
        </div>
        </div>
      </div>

      <EditProfileModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
      />
    </>
  );
}

export default GuestProfileCard;
