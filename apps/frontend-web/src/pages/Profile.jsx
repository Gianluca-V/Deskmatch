import { useLocation } from 'react-router-dom';
import { useProfile, useProfileCompany } from '../hooks/useProfile';
import GuestProfileCard from '../components/GuestProfileCard';
import CompanyProfileCard from '../components/CompanyProfileCard';
import './Profile.css';

function Profile() {
  const location = useLocation();
  const isCompanyProfile = location.pathname.includes('/profile/company');

  const { data: userProfile, isLoading: userLoading, error: userError } = useProfile();
  const { data: companyProfile, isLoading: companyLoading, error: companyError } = useProfileCompany();

  return (
    <div className="profile-page">
      <div className="container">
        <div className="profile-page__header">
          <h1>{isCompanyProfile ? 'Perfil de la Empresa' : 'Mi Perfil'}</h1>
          <p>{isCompanyProfile ? 'Administra la información de tu empresa' : 'Administra tu información personal'}</p>
        </div>

        <div className="profile-page__grid">
          {!isCompanyProfile && (
            <section className="profile-page__section">
              <GuestProfileCard
                user={userProfile}
                isLoading={userLoading}
                error={userError}
              />
            </section>
          )}

          {isCompanyProfile && (
            <section className="profile-page__section">
              <CompanyProfileCard
                company={companyProfile}
                isLoading={companyLoading}
                error={companyError}
              />
            </section>
          )}
        </div>
      </div>
    </div>
  );
}

export default Profile;
