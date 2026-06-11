import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { User, Mail, Phone, MapPin, Settings, LogOut } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import EditProfileModal from './EditProfileModal';
import './GuestProfileCard.css';

function GuestProfileCard({ user, isLoading, error }) {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (isLoading) {
    return (
      <div className="guest-profile-card">
        <div className="guest-profile-card__header-section">
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
        <div className="guest-profile-card__header-top">
          <div className="guest-profile-card__header-section">
            <div className="guest-profile-card__avatar">
              {user?.profilePictureUrl ? (
                <img src={user.profilePictureUrl} alt={`${firstName} ${lastName}`} />
              ) : (
                <User size={32} />
              )}
            </div>
            <div>
              <h2 className="guest-profile-card__name">
                {firstName} {lastName}
              </h2>
              <p className="guest-profile-card__email">{email}</p>
            </div>
          </div>
          <div className="guest-profile-card__actions">
            <button
              onClick={() => setIsEditModalOpen(true)}
              className="guest-profile-card__action-button guest-profile-card__action-button--config"
              title="Configuración"
            >
              <Settings size={18} />
              <span>Configuración</span>
            </button>
            <button
              onClick={handleLogout}
              className="guest-profile-card__action-button guest-profile-card__action-button--logout"
              title="Cerrar sesión"
            >
              <LogOut size={18} />
              <span>Cerrar Sesión</span>
            </button>
          </div>
        </div>

        <div className="guest-profile-card__divider"></div>

        <div className="guest-profile-card__section-title">
          <User size={20} />
          <h3>Información Personal</h3>
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
