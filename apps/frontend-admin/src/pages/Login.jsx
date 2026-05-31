import { Link } from 'react-router-dom';

function Login() {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <aside className="auth-card__aside">
          <p className="auth-card__brand">DeskMatch Admin</p>
          <h2 className="auth-card__aside-title">Tu panel administrativo seguro</h2>
          <p className="auth-card__aside-copy">Accede rápidamente a la gestión de reservas, empresas y usuarios con una experiencia pensada para equipos profesionales.</p>
          <ul className="auth-card__benefits">
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Control completo de tus operaciones
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Acceso a datos y métricas clave
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Seguridad con inicio de sesión protegido
            </li>
          </ul>
        </aside>
        <div className="auth-card__body">
          <div className="auth-card__header">
            <p className="auth-card__brand auth-card__brand--secondary">Panel seguro</p>
            <h1 className="auth-card__title">Acceso al panel</h1>
            <p className="auth-card__subtitle">Ingresa tus credenciales para administrar oficinas, reservas y usuarios desde un solo lugar.</p>
          </div>
          <form>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-email">Correo electrónico</label>
              <input id="admin-email" type="email" placeholder="admin@deskmatch.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-password">Contraseña</label>
              <input id="admin-password" type="password" placeholder="********" />
            </div>
            <button type="button" className="btn btn-primary" style={{ width: '100%' }}>Ingresar</button>
          </form>
          <p className="auth-card__foot">¿No tienes cuenta? <Link to="/register" className="auth-link">Regístrate</Link></p>
        </div>
      </div>
    </div>
  );
}

export default Login;
