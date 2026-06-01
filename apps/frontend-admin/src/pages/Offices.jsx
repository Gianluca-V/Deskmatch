import { useState } from 'react';
import OfficeModal from '../components/OfficeModal';

export default function Offices() {
  const [modalOpen, setModalOpen] = useState(false);

  return (
    <section>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: '24px' }}>
        <h1>Oficinas</h1>
        <button className="btn btn-primary" onClick={() => setModalOpen(true)}>
          + Nuevo Espacio
        </button>
      </div>
      <p>Gestión de oficinas disponibles.</p>
      <OfficeModal isOpen={modalOpen} onClose={() => setModalOpen(false)} />
    </section>
  );
}
