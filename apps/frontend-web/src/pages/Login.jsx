import { Link } from 'react-router-dom';

function Login() {
  return (
    <div style={{ minHeight: '70vh', display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '24px' }}>
      <div style={{ width: '100%', maxWidth: '440px', backgroundColor: 'var(--color-surface)', padding: '32px', borderRadius: '24px', boxShadow: '0 24px 60px rgba(3, 6, 8, 0.08)', border: '1px solid rgba(58, 149, 223, 0.12)' }}>
        <h1 style={{ marginBottom: '16px', color: 'var(--color-text)' }}>Iniciar sesión</h1>
        <p style={{ marginBottom: '24px', color: 'var(--color-muted)' }}>Introduce tu correo electrónico y contraseña para acceder.</p>
        <form>
          <label style={{ display: 'block', marginBottom: '12px', color: 'var(--color-muted-strong)' }}>
            Correo electrónico
            <input type="email" placeholder="usuario@ejemplo.com" style={{ width: '100%', marginTop: '8px', padding: '12px 14px', borderRadius: '12px', border: '1px solid var(--color-border)', backgroundColor: '#ffffff' }} />
          </label>
          <label style={{ display: 'block', marginBottom: '20px', color: 'var(--color-muted-strong)' }}>
            Contraseña
            <input type="password" placeholder="********" style={{ width: '100%', marginTop: '8px', padding: '12px 14px', borderRadius: '12px', border: '1px solid var(--color-border)', backgroundColor: '#ffffff' }} />
          </label>
          <button type="button" style={{ width: '100%', padding: '14px 18px', borderRadius: '12px', border: 'none', backgroundColor: 'var(--color-primary)', color: '#ffffff', fontWeight: 700, cursor: 'pointer' }}>Ingresar</button>
          <p style={{ marginTop: '20px', color: 'var(--color-muted)', textAlign: 'center' }}>¿No tienes cuenta? <Link to="/register" style={{ color: 'var(--color-primary)', fontWeight: 700, textDecoration: 'none' }}>Regístrate</Link></p>
        </form>
      </div>
    </div>
  );
}

export default Login;
