import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Eye, EyeOff } from 'lucide-react';
import api from '../lib/api';

function Register() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const navigate = useNavigate();

  const getErrorMessage = (error) => {
    if (typeof error === 'string') {
      const lowerError = error.toLowerCase();

      if (lowerError.includes('passwordrequiresnonalphanumeric') ||
          lowerError.includes('non alphanumeric')) {
        return 'La contraseña debe incluir al menos un carácter especial como: ! @ # $ % ^ & *';
      }
      if (lowerError.includes('passwordrequiresupper') ||
          lowerError.includes('uppercase')) {
        return 'La contraseña debe incluir al menos una letra mayúscula';
      }
      if (lowerError.includes('passwordrequireslower') ||
          lowerError.includes('lowercase')) {
        return 'La contraseña debe incluir al menos una letra minúscula';
      }
      if (lowerError.includes('passwordrequiresdigit') ||
          lowerError.includes('digit')) {
        return 'La contraseña debe incluir al menos un número';
      }
      if (lowerError.includes('email')) {
        return 'El correo electrónico es inválido o ya está registrado';
      }
    }

    return error || 'Error al crear la cuenta';
  };

  const handleRegister = async () => {
    setError('');
    setSuccess('');
    setLoading(true);

    if (!firstName.trim() || !lastName.trim() || !email.trim() || !password.trim()) {
      setError('Todos los campos son requeridos');
      setLoading(false);
      return;
    }

    if (password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres');
      setLoading(false);
      return;
    }

    try {
      const response = await api.post('/api/auth/register', {
        firstName,
        lastName,
        name: `${firstName} ${lastName}`.trim(),
        email,
        password,
        role: 'User',
      });

      if (response.status === 200 || response.status === 201) {
        setSuccess('¡Cuenta creada exitosamente! Redirigiendo...');
        setTimeout(() => navigate('/login'), 2000);
      }
    } catch (err) {
      const data = err.response?.data;
      const status = err.response?.status;

      if (status === 409) {
        setError('El correo electrónico ya está registrado. Intenta con otro.');
      } else if (status === 400 && data?.errors) {
        const errorMessages = Object.entries(data.errors)
          .map(([, messages]) => {
            if (Array.isArray(messages) && messages.length > 0) {
              return getErrorMessage(messages[0]);
            }
            return null;
          })
          .filter(Boolean);

        setError(errorMessages.length > 0 ? errorMessages[0] : 'Error al crear la cuenta');
      } else {
        setError(data?.message || 'No se pudo crear la cuenta. Revisá los datos e intentá de nuevo.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    handleRegister();
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
          {success && (
            <div style={{ margin: '0 0 16px', color: '#16a34a', fontSize: '14px', backgroundColor: '#f0fdf4', padding: '10px 14px', borderRadius: '8px' }}>
              ✓ {success}
            </div>
          )}
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginTop: '24px' }}>
              <div className="form-group auth-card__field">
                <label htmlFor="first-name">Nombre</label>
                <input
                  id="first-name"
                  type="text"
                  placeholder="Juan"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  disabled={loading}
                  required
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
                  disabled={loading}
                  required
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
                disabled={loading}
                required
              />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="register-password">Contraseña</label>
              <div style={{ position: 'relative' }}>
                <input
                  id="register-password"
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Mínimo 6 caracteres"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={loading}
                  required
                  style={{ paddingRight: '44px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  style={{ position: 'absolute', right: 12, top: '50%', transform: 'translateY(-50%)', background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-muted)', display: 'flex', alignItems: 'center', padding: 0 }}
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>
            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%' }}
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
