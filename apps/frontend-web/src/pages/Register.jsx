import { Link } from 'react-router-dom';

function Register() {
  return (
    <div className="auth-page">
      <div className="auth-card">
        <aside className="auth-card__aside">
          <p className="auth-card__brand">DeskMatch</p>
          <h2 className="auth-card__aside-title">Regístrate y encuentra tu próximo espacio</h2>
          <p className="auth-card__aside-copy">Crea tu cuenta para acceder a reservas exclusivas, gestionar tu historial y descubrir las mejores oficinas cerca de ti.</p>
          <ul className="auth-card__benefits">
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Reserva con facilidad
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Gestiona tus preferencias
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Acceso rápido a nuevas ofertas
            </li>
          </ul>
        </aside>
        <div className="auth-card__body">
          <div className="auth-card__header">
            <p className="auth-card__brand auth-card__brand--secondary">Registro fácil</p>
            <h1 className="auth-card__title">Crea tu cuenta</h1>
            <p className="auth-card__subtitle">Completa este breve formulario para comenzar a reservar oficinas rápidamente.</p>
          </div>
          <form>
            <div className="form-group auth-card__field">
              <label htmlFor="full-name">Nombre completo</label>
              <input id="full-name" type="text" placeholder="Tu nombre" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="register-email">Correo electrónico</label>
              <input id="register-email" type="email" placeholder="usuario@ejemplo.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="register-password">Contraseña</label>
              <input id="register-password" type="password" placeholder="********" />
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
