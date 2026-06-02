import { Link } from 'react-router-dom';

function Register() {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <aside className="auth-card__aside">
          <p className="auth-card__brand">DeskMatch Admin</p>
          <h2 className="auth-card__aside-title">Registra un administrador</h2>
          <p className="auth-card__aside-copy">Crea acceso para gestionar todo tu entorno administrativo desde reservas hasta usuarios y empresas.</p>
          <ul className="auth-card__benefits">
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Gestión de usuarios y permisos
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Acceso a informes de operaciones
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Inicio de sesión con protección adicional
            </li>
          </ul>
        </aside>
        <div className="auth-card__body">
          <div className="auth-card__header">
            <p className="auth-card__brand auth-card__brand--secondary">Registro seguro</p>
            <h1 className="auth-card__title">Registro de administrador</h1>
            <p className="auth-card__subtitle">Crea un usuario administrativo para empezar a gestionar las operaciones de DeskMatch.</p>
          </div>
          <form>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginTop: '24px' }}>
              <div className="form-group auth-card__field">
                <label htmlFor="admin-first-name">Nombre</label>
                <input id="admin-first-name" type="text" placeholder="Juan" />
              </div>
              <div className="form-group auth-card__field">
                <label htmlFor="admin-last-name">Apellido</label>
                <input id="admin-last-name" type="text" placeholder="Pérez" />
              </div>
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-email">Correo electrónico</label>
              <input id="admin-email" type="email" placeholder="admin@deskmatch.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-password">Contraseña</label>
              <input id="admin-password" type="password" placeholder="********" />
            </div>
            <button type="button" className="btn btn-primary" style={{ width: '100%' }}>Crear cuenta</button>
          </form>
          <p className="auth-card__foot">¿Ya tienes cuenta? <Link to="/login" className="auth-link">Inicia sesión</Link></p>
        </div>
      </div>
    </div>
  );
}

export default Register;
