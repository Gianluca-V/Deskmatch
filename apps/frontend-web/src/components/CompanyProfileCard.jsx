import { useState } from 'react';
import { Building2, Mail, Phone, MapPin, CheckCircle, Globe, LogOut, Settings, LayoutGrid } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import EditCompanyProfileModal from './EditCompanyProfileModal';
import './CompanyProfileCard.css';

function CompanyProfileCard({ company, isLoading, error }) {
  const navigate = useNavigate();
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigate('/login');
  };

  if (isLoading) {
    return (
      <div className="company-profile-card">
        <div className="company-profile-card__header-section">
          <div className="company-profile-card__avatar skeleton-avatar"></div>
          <div>
            <div className="skeleton-line skeleton-line--name"></div>
            <div className="skeleton-line skeleton-line--email"></div>
          </div>
        </div>
        <div className="company-profile-card__content">
          <div className="skeleton-line"></div>
          <div className="skeleton-line"></div>
          <div className="skeleton-line"></div>
        </div>
      </div>
    );
  }

  if (error || !company) {
    return (
      <div className="company-profile-card company-profile-card--error">
        <div className="company-profile-card__error-message">
          <p>Sin empresa asociada</p>
          <small>Aún no has registrado una empresa. Crea una para publicar espacios.</small>
        </div>
      </div>
    );
  }

  const companyName = company?.name || 'Mi Empresa';
  const email = company?.email || 'No especificado';
  const phone = company?.phone || 'No especificado';
  const location = company?.location || 'No especificado';
  const description = company?.description || '';
  const website = company?.website || '';
  const isVerified = company?.isVerified || false;

  return (
    <>
      <div className="company-profile-card">
        <div className="company-profile-card__header-top">
          <div className="company-profile-card__header-section">
            <div className="company-profile-card__avatar">
              {company?.profileImage ? (
                <img src={company.profileImage} alt={companyName} />
              ) : (
                <Building2 size={32} />
              )}
            </div>
            <div>
              <div className="company-profile-card__title-wrapper">
                <h2 className="company-profile-card__name">{companyName}</h2>
                {isVerified && (
                  <div className="company-profile-card__badge">
                    <CheckCircle size={14} />
                    <span>Verificado</span>
                  </div>
                )}
              </div>
              <p className="company-profile-card__email">{email}</p>
            </div>
          </div>
          <div className="company-profile-card__actions">
            <button
              onClick={() => setIsEditModalOpen(true)}
              className="company-profile-card__action-button company-profile-card__action-button--config"
              title="Editar perfil"
            >
              <Settings size={18} />
              <span>Editar Perfil</span>
            </button>
            <button
              onClick={() => navigate('/dashboard/spaces')}
              className="company-profile-card__action-button company-profile-card__action-button--config"
              title="Gestionar espacios"
            >
              <LayoutGrid size={18} />
              <span>Gestionar Espacios</span>
            </button>
            <button
              onClick={handleLogout}
              className="company-profile-card__action-button company-profile-card__action-button--logout"
              title="Cerrar sesión"
            >
              <LogOut size={18} />
              <span>Cerrar Sesión</span>
            </button>
          </div>
        </div>

        <div className="company-profile-card__divider"></div>

        <div className="company-profile-card__section-title">
          <Building2 size={20} />
          <h3>Información de la Empresa</h3>
        </div>

        <div className="company-profile-card__content">
          <div className="company-profile-card__field">
            <div className="company-profile-card__field-icon">
              <Building2 size={18} />
            </div>
            <div>
              <label className="company-profile-card__field-label">Nombre de empresa</label>
              <p className="company-profile-card__field-value">{companyName}</p>
            </div>
          </div>

          {description && (
            <div className="company-profile-card__field">
              <div className="company-profile-card__field-icon">
                <Building2 size={18} />
              </div>
              <div>
                <label className="company-profile-card__field-label">Descripción</label>
                <p className="company-profile-card__field-value company-profile-card__field-value--description">
                  {description}
                </p>
              </div>
            </div>
          )}

          <div className="company-profile-card__field">
            <div className="company-profile-card__field-icon">
              <Mail size={18} />
            </div>
            <div>
              <label className="company-profile-card__field-label">Correo de contacto</label>
              <p className={`company-profile-card__field-value ${!company?.email ? 'company-profile-card__field-value--empty' : ''}`}>
                {email}
              </p>
            </div>
          </div>

          <div className="company-profile-card__field">
            <div className="company-profile-card__field-icon">
              <Phone size={18} />
            </div>
            <div>
              <label className="company-profile-card__field-label">Teléfono</label>
              <p className={`company-profile-card__field-value ${!company?.phone ? 'company-profile-card__field-value--empty' : ''}`}>
                {phone}
              </p>
            </div>
          </div>

          <div className="company-profile-card__field">
            <div className="company-profile-card__field-icon">
              <MapPin size={18} />
            </div>
            <div>
              <label className="company-profile-card__field-label">Ubicación</label>
              <p className={`company-profile-card__field-value ${!company?.location ? 'company-profile-card__field-value--empty' : ''}`}>
                {location}
              </p>
            </div>
          </div>

          {website && (
            <div className="company-profile-card__field">
              <div className="company-profile-card__field-icon">
                <Globe size={18} />
              </div>
              <div>
                <label className="company-profile-card__field-label">Sitio web</label>
                <a href={website} target="_blank" rel="noopener noreferrer" className="company-profile-card__field-value company-profile-card__field-value--link">
                  {website}
                </a>
              </div>
            </div>
          )}
        </div>
      </div>

      <EditCompanyProfileModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
      />
    </>
  );
}

export default CompanyProfileCard;
