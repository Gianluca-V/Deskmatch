import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../lib/api';
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
      const response = await api.post('/api/auth/login', { email, password });
      login(response.data);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Credenciales incorrectas.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ minHeight: '70vh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '24px' }}>
      <div style={{ width: '100%', maxWidth: '440px', backgroundColor: 'var(--color-surface)', padding: '32px', borderRadius: '24px', boxShadow: '0 24px 60px rgba(3, 6, 8, 0.08)', border: '1px solid rgba(58, 149, 223, 0.12)' }}>
        <Link to="/" style={{ display: 'inline-block', marginBottom: '16px', color: 'var(--color-primary)', textDecoration: 'none', fontSize: '14px', fontWeight: 600 }}> Volver a inicio</Link>
        <h1 style={{ marginBottom: '16px', color: 'var(--color-text)' }}>Iniciar sesión</h1>
        <p style={{ marginBottom: '24px', color: 'var(--color-muted)' }}>Introduce tu correo electrónico y contraseña para acceder.</p>
        {error && (
          <p style={{ marginBottom: '16px', color: '#dc2626', fontSize: '14px', backgroundColor: '#fef2f2', padding: '10px 14px', borderRadius: '8px' }}>
            {error}
          </p>
        )}
        <form>
          <label style={{ display: 'block', marginBottom: '12px', color: 'var(--color-muted-strong)' }}>
            Correo electrónico
            <input
              type="email"
              placeholder="usuario@ejemplo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              style={{ width: '100%', marginTop: '8px', padding: '12px 14px', borderRadius: '12px', border: '1px solid var(--color-border)', backgroundColor: '#ffffff' }}
            />
          </label>
          <label style={{ display: 'block', marginBottom: '20px', color: 'var(--color-muted-strong)' }}>
            Contraseña
            <input
              type="password"
              placeholder="********"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              style={{ width: '100%', marginTop: '8px', padding: '12px 14px', borderRadius: '12px', border: '1px solid var(--color-border)', backgroundColor: '#ffffff' }}
            />
          </label>
          <button
            type="button"
            onClick={handleLogin}
            disabled={loading}
            style={{ width: '100%', padding: '14px 18px', borderRadius: '12px', border: 'none', backgroundColor: 'var(--color-primary)', color: '#ffffff', fontWeight: 700, cursor: loading ? 'not-allowed' : 'pointer', opacity: loading ? 0.7 : 1 }}
          >
            {loading ? 'Ingresando...' : 'Ingresar'}
          </button>
          <p style={{ marginTop: '20px', color: 'var(--color-muted)', textAlign: 'center' }}>
            ¿No tienes cuenta? <Link to="/register" style={{ color: 'var(--color-primary)', fontWeight: 700, textDecoration: 'none' }}>Regístrate</Link>
          </p>
        </form>
      </div>
    </div>
  );
}

export default Login;
