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
        role: 'Admin',
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
          <p className="auth-card__brand">DeskMatch Admin</p>
          <h2 className="auth-card__aside-title">Registrá un administrador</h2>
          <p className="auth-card__aside-copy">Creá acceso para gestionar todo tu entorno administrativo desde reservas hasta usuarios y empresas.</p>
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
            <p className="auth-card__subtitle">Creá un usuario administrativo para empezar a gestionar las operaciones de DeskMatch.</p>
          </div>
          {error && (
            <div style={{ margin: '0 0 16px', color: '#dc2626', fontSize: '14px', backgroundColor: '#fef2f2', padding: '10px 14px', borderRadius: '8px' }}>
              {error}
            </div>
          )}
          <form>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginTop: '24px' }}>
              <div className="form-group auth-card__field">
                <label htmlFor="admin-first-name">Nombre</label>
                <input
                  id="admin-first-name"
                  type="text"
                  placeholder="Juan"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                />
              </div>
              <div className="form-group auth-card__field">
                <label htmlFor="admin-last-name">Apellido</label>
                <input
                  id="admin-last-name"
                  type="text"
                  placeholder="Pérez"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                />
              </div>
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-email">Correo electrónico</label>
              <input
                id="admin-email"
                type="email"
                placeholder="admin@deskmatch.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="admin-password">Contraseña</label>
              <input
                id="admin-password"
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
