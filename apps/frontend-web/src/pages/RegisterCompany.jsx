import { Link, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

function RegisterCompany() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ companyName: '', email: '', website: '', password: '' });
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    setError('');
  };

  const getErrorMessage = (error) => {
    if (typeof error === 'string') {
      const lowerError = error.toLowerCase();

      if (lowerError.includes('password')) {
        return 'La contraseña debe incluir al menos 8 caracteres, una letra mayúscula, una minúscula, un número y un carácter especial como: ! @ # $ % ^ & *';
      }
      if (lowerError.includes('email')) {
        return 'El correo electrónico es inválido o ya está registrado';
      }
    }

    return error || 'Error al crear la cuenta';
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');

    try {
      if (!formData.companyName.trim() || !formData.email.trim() || !formData.password.trim()) {
        setError('Todos los campos son requeridos');
        setLoading(false);
        return;
      }

      if (formData.password.length < 8) {
        setError('La contraseña debe incluir al menos 8 caracteres, una letra mayúscula, una minúscula, un número y un carácter especial como: ! @ # $ % ^ & *');
        setLoading(false);
        return;
      }

      const registerRes = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Name: formData.companyName, Email: formData.email, Password: formData.password, Role: 'Manager', Website: formData.website })
      });

      const registerData = await registerRes.json().catch(() => ({}));

      if (registerRes.ok || registerRes.status === 201) {
        // Loguear automáticamente después del registro
        const loginRes = await fetch('/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Email: formData.email, Password: formData.password })
        });

        const loginData = await loginRes.json().catch(() => ({}));

        if (loginRes.ok && loginData.accessToken) {
          // Crear la empresa automáticamente
          const companyRes = await fetch('/api/companies', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${loginData.accessToken}`
            },
            body: JSON.stringify({
              Name: formData.companyName,
              Description: '',
              WebsiteUrl: formData.website || ''
            })
          });

          if (companyRes.ok || companyRes.status === 201) {
            // Guardar sesión y redirigir al dashboard
            localStorage.setItem('dm_session', JSON.stringify(loginData));
            setSuccess('¡Cuenta y empresa creadas exitosamente! Redirigiendo...');
            setTimeout(() => navigate('/profile/company'), 1500);
          } else {
            // Si falla crear la empresa, igual loguear
            localStorage.setItem('dm_session', JSON.stringify(loginData));
            setSuccess('Cuenta creada. Por favor crea tu empresa desde el dashboard.');
            setTimeout(() => navigate('/dashboard'), 1500);
          }
        } else {
          setSuccess('¡Cuenta creada exitosamente! Redirigiendo al login...');
          setTimeout(() => navigate('/login'), 1800);
        }
      } else if (registerRes.status === 409) {
        setError('El correo electrónico ya está registrado. Intenta con otro.');
      } else if (registerRes.status === 400 && registerData.errors) {
        const errorMessages = Object.entries(registerData.errors)
          .map(([key, messages]) => {
            if (Array.isArray(messages) && messages.length > 0) {
              return getErrorMessage(messages[0]);
            }
            return getErrorMessage(key);
          })
          .filter(msg => msg);

        setError(errorMessages.length > 0 ? errorMessages[0] : 'Error al crear la cuenta');
      } else {
        setError(registerData.message || 'Error al crear la cuenta');
      }
    } catch (err) {
      setError('Error al conectar con el servidor: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

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
          <form onSubmit={handleSubmit}>
            {error && (
              <div style={{ padding: '12px', marginBottom: '16px', backgroundColor: '#fee', color: '#c00', borderRadius: '6px', fontSize: '14px' }}>
                ⚠️ {error}
              </div>
            )}
            {success && (
              <div style={{ padding: '12px', marginBottom: '16px', backgroundColor: '#efe', color: '#060', borderRadius: '6px', fontSize: '14px' }}>
                ✓ {success}
              </div>
            )}
            <div className="form-group auth-card__field" style={{ marginTop: '24px' }}>
              <label htmlFor="company-name">Nombre de la empresa</label>
              <input id="company-name" name="companyName" value={formData.companyName} onChange={handleChange} disabled={loading} type="text" placeholder="Mi Empresa S.A." required />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-email">Correo electrónico</label>
              <input id="company-email" name="email" value={formData.email} onChange={handleChange} disabled={loading} type="email" placeholder="empresa@ejemplo.com" required />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-website">Sitio web</label>
              <input id="company-website" name="website" value={formData.website} onChange={handleChange} disabled={loading} type="url" placeholder="https://miempresa.com" />
            </div>
            <div className="form-group auth-card__field">
              <label htmlFor="company-password">Contraseña</label>
              <div style={{ position: 'relative' }}>
                <input id="company-password" name="password" value={formData.password} onChange={handleChange} disabled={loading} type={showPassword ? 'text' : 'password'} placeholder="********" required style={{ paddingRight: '44px' }} />
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
            <button type="submit" className="btn btn-primary" style={{ width: '100%' }} disabled={loading}>{loading ? 'Creando cuenta...' : 'Crear cuenta'}</button>
          </form>
          <p className="auth-card__foot">¿Ya tenés cuenta? <Link to="/login" className="auth-link">Inicia sesión</Link></p>
        </div>
      </div>
    </div>
  );
}

export default RegisterCompany;