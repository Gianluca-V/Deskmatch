import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../context/AuthContext';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async () => {
    setError('');
    setLoading(true);
    try {
      const response = await axios.post('http://localhost:5000/api/auth/login', {
        email,
        password,
      });
      login(response.data);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Credenciales incorrectas.');
    } finally {
      setLoading(false);
    }
  };

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
          {error && (
            <div style={{ margin: '0 0 16px', color: '#dc2626', fontSize: '14px', backgroundColor: '#fef2f2', padding: '10px 14px', borderRadius: '8px' }}>
              {error}
            </div>
          )}
          <form>
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
              onClick={handleLogin}
              disabled={loading}
            >
              {loading ? 'Ingresando...' : 'Ingresar'}
            </button>
          </form>
          <p className="auth-card__foot">¿No tienes cuenta? <Link to="/register" className="auth-link">Regístrate</Link></p>
        </div>
      </div>
    </div>
  );
}

export default Login;
