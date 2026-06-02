import { Link } from 'react-router-dom';

function RegisterCompany() {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <aside className="auth-card__aside">
          <p className="auth-card__brand">DeskMatch</p>
          <h2 className="auth-card__aside-title">Regístrate y publica tus espacios de trabajo</h2>
          <p className="auth-card__aside-copy">Crea tu cuenta para publicar tus espacios de trabajo, gestionar reservas y conectar con profesionales que buscan el lugar ideal para trabajar.</p>
          <ul className="auth-card__benefits">
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Publicá tus espacios con facilidad
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Gestioná reservas en un solo lugar
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Llegá a miles de profesionales
            </li>
          </ul>
        </aside>
        <div className="auth-card__body">
          <div className="auth-card__header">
            <p className="auth-card__brand auth-card__brand--secondary">Registro fácil</p>
            <h1 className="auth-card__title">Crea tu cuenta</h1>
            <p className="auth-card__subtitle">Completá el formulario para comenzar a publicar tus espacios.</p>
          </div>
          <form>
            <div className="form-group auth-card__field" style={{ marginTop: '24px' }}>
              <label htmlFor="company-name">Nombre de la empresa</label>
              <input id="company-name" type="text" placeholder="Mi Empresa S.A." />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-email">Correo electrónico</label>
              <input id="company-email" type="email" placeholder="empresa@ejemplo.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-website">Sitio web</label>
              <input id="company-website" type="url" placeholder="https://miempresa.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-password">Contraseña</label>
              <input id="company-password" type="password" placeholder="********" />
            </div>
            <button type="button" className="btn btn-primary" style={{ width: '100%' }}>Crear cuenta</button>
          </form>
          <p className="auth-card__foot">¿Ya tenés cuenta? <Link to="/login" className="auth-link">Inicia sesión</Link></p>
        </div>
      </div>
    </div>
  );
}

export default RegisterCompany;