import { useState } from 'react';
import { Building2, Mail, Phone, MapPin, CheckCircle, Edit } from 'lucide-react';
import EditCompanyProfileModal from './EditCompanyProfileModal';
import './CompanyProfileCard.css';

function CompanyProfileCard({ company, isLoading, error }) {
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  if (isLoading) {
    return (
      <div className="company-profile-card">
        <div className="company-profile-card__header">
          <div className="company-profile-card__avatar skeleton-avatar"></div>
          <div style={{ flex: 1 }}>
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

  // Tratar errores como "no hay empresa asociada"
  if (error || !company) {
    return (
      <div className="company-profile-card company-profile-card--empty">
        <div className="company-profile-card__empty-message">
          <Building2 size={48} />
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
  const isVerified = company?.isVerified || false;

  return (
    <>
      <div className="company-profile-card">
        <div className="company-profile-card__header">
          <div className="company-profile-card__avatar">
            <Building2 size={32} />
          </div>
          <div style={{ flex: 1 }}>
            <div className="company-profile-card__title-wrapper">
              <h3 className="company-profile-card__name">{companyName}</h3>
              {isVerified && (
                <div className="company-profile-card__badge">
                  <CheckCircle size={16} />
                  <span>Verificado</span>
                </div>
              )}
            </div>
            <p className="company-profile-card__subtitle">Información de la Empresa</p>
          </div>
          <button
            onClick={() => setIsEditModalOpen(true)}
            className="company-profile-card__edit-button"
            title="Editar perfil"
          >
            <Edit size={18} />
          </button>
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
