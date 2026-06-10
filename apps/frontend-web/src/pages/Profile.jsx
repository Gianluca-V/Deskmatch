import { useLocation } from 'react-router-dom';
import { useProfile, useProfileCompany } from '../hooks/useProfile';
import GuestProfileCard from '../components/GuestProfileCard';
import CompanyProfileCard from '../components/CompanyProfileCard';
import CompanyStats from '../components/CompanyStats';
import CompanySpaces from '../components/CompanySpaces';
import CompanyReservations from '../components/CompanyReservations';
import ReservationSummary from '../components/ReservationSummary';
import RecentActivity from '../components/RecentActivity';
import './Profile.css';

function Profile() {
  const location = useLocation();
  const isCompanyProfile = location.pathname.includes('/profile/company');

  const { data: userProfile, isLoading: userLoading, error: userError } = useProfile();
  const { data: companyProfile, isLoading: companyLoading, error: companyError } = useProfileCompany();

  const companyStats = companyProfile ? {
    publishedSpaces: companyProfile.spaces?.length || 0,
    totalReservations: companyProfile.reservations?.length || 0,
    averageRating: companyProfile.averageRating || 0,
    totalReviews: companyProfile.reviewCount || 0,
  } : null;

  return (
    <div className="profile-page">
      <div className="container">
        {!isCompanyProfile && (
          <>
            <section className="profile-page__section profile-page__section--full">
              <GuestProfileCard
                user={userProfile}
                isLoading={userLoading}
                error={userError}
              />
            </section>

            <div className="profile-page__grid">
              <section className="profile-page__section">
                <ReservationSummary />
              </section>

              <section className="profile-page__section">
                <RecentActivity />
              </section>
            </div>
          </>
        )}

        {isCompanyProfile && (
          <>
            <section className="profile-page__section profile-page__section--full">
              <CompanyProfileCard
                company={companyProfile}
                isLoading={companyLoading}
                error={companyError}
              />
            </section>

            {!companyError && companyProfile && (
              <>
                <section className="profile-page__section profile-page__section--full">
                  <CompanyStats
                    stats={companyStats}
                    isLoading={companyLoading}
                  />
                </section>

                <div className="profile-page__grid">
                  <section className="profile-page__section">
                    <CompanySpaces
                      spaces={companyProfile.spaces}
                      isLoading={companyLoading}
                    />
                  </section>

                  <section className="profile-page__section">
                    <CompanyReservations
                      reservations={companyProfile.reservations}
                      isLoading={companyLoading}
                    />
                  </section>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default Profile;


  
