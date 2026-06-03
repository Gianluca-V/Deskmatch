import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../lib/api';

function Register() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async () => {
    setError('');
    setLoading(true);
    try {
      await api.post('/api/auth/register', {
        firstName,
        lastName,
        name: `${firstName} ${lastName}`.trim(),
        email,
        password,
        role: 'User',
      });
      navigate('/login');
    } catch (err) {
      setError(err.response?.data?.message || 'No se pudo crear la cuenta. Revisá los datos e intentá de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <aside className="auth-card__aside">
          <p className="auth-card__brand">DeskMatch</p>
          <h2 className="auth-card__aside-title">Regístrate y encontrá tu próximo espacio</h2>
          <p className="auth-card__aside-copy">Creá tu cuenta para acceder a reservas exclusivas, gestionar tu historial y descubrir las mejores oficinas cerca de ti.</p>
          <ul className="auth-card__benefits">
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Reserva con facilidad
            </li>
            <li className="auth-card__benefit">
              <span className="auth-card__benefit-icon">✓</span>
              Gestioná tus preferencias
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
            <h1 className="auth-card__title">Creá tu cuenta</h1>
            <p className="auth-card__subtitle">Completá este breve formulario para comenzar a reservar oficinas rápidamente.</p>
          </div>
          {error && (
            <div style={{ margin: '0 0 16px', color: '#dc2626', fontSize: '14px', backgroundColor: '#fef2f2', padding: '10px 14px', borderRadius: '8px' }}>
              {error}
            </div>
          )}
          <form>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginTop: '24px' }}>
              <div className="form-group auth-card__field">
                <label htmlFor="first-name">Nombre</label>
                <input
                  id="first-name"
                  type="text"
                  placeholder="Juan"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                />
              </div>
              <div className="form-group auth-card__field">
                <label htmlFor="last-name">Apellido</label>
                <input
                  id="last-name"
                  type="text"
                  placeholder="Pérez"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                />
              </div>
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="register-email">Correo electrónico</label>
              <input
                id="register-email"
                type="email"
                placeholder="usuario@ejemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="register-password">Contraseña</label>
              <input
                id="register-password"
                type="password"
                placeholder="********"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
            <button
              type="button"
              className="btn btn-primary"
              style={{ width: '100%' }}
              onClick={handleRegister}
              disabled={loading}
            >
              {loading ? 'Creando cuenta...' : 'Crear cuenta'}
            </button>
          </form>
          <p className="auth-card__foot">¿Ya tenés cuenta? <Link to="/login" className="auth-link">Iniciá sesión</Link></p>
        </div>
      </div>
    </div>
  );
}

export default Register;
