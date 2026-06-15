function Forbidden() {

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '60vh',
      color: '#1e2a3a',
      textAlign: 'center',
      padding: '24px',
    }}>
      <h1 style={{ fontSize: '72px', margin: '0 0 8px', fontWeight: 700 }}>
        403
      </h1>
      <h2 style={{ fontSize: '24px', margin: '0 0 12px', fontWeight: 600 }}>
        Acceso Denegado
      </h2>
      <p style={{ fontSize: '16px', margin: '0 0 32px', color: '#555' }}>
        No tenés permisos para ver esta página
      </p>      
    </div>
  );
}

export default Forbidden;
