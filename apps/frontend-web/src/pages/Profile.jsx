import { useProfile, useProfileCompany } from '../hooks/useProfile';
import GuestProfileCard from '../components/GuestProfileCard';
import CompanyProfileCard from '../components/CompanyProfileCard';
import './Profile.css';

function Profile() {
  const { data: userProfile, isLoading: userLoading, error: userError } = useProfile();
  const { data: companyProfile, isLoading: companyLoading, error: companyError } = useProfileCompany();

  return (
    <div className="profile-page">
      <div className="container">
        <div className="profile-page__header">
          <h1>Mi Perfil</h1>
          <p>Administra tu información personal y de empresa</p>
        </div>

        <div className="profile-page__grid">
          <section className="profile-page__section">
            <GuestProfileCard 
              user={userProfile} 
              isLoading={userLoading} 
              error={userError}
            />
          </section>

          <section className="profile-page__section">
            <CompanyProfileCard 
              company={companyProfile} 
              isLoading={companyLoading} 
              error={companyError}
            />
          </section>
        </div>
      </div>
    </div>
  );
}

export default Profile;
