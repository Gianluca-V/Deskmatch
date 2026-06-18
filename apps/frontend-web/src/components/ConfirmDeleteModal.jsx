import Modal from './Modal';

export default function ConfirmDeleteModal({ isOpen, onClose, onConfirm, title, message, isPending }) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title}>
      <div style={{ padding: '16px 0' }}>
        <p style={{ margin: '0 0 24px 0', color: 'var(--color-text)', lineHeight: '1.5' }}>
          {message}
        </p>
        <div style={{ display: 'flex', gap: '12px', justifyContent: 'flex-end' }}>
          <button
            onClick={onClose}
            className="btn btn-secondary"
            disabled={isPending}
          >
            Cancelar
          </button>
          <button
            onClick={onConfirm}
            className="btn btn-danger"
            disabled={isPending}
          >
            {isPending ? 'Eliminando...' : 'Eliminar'}
          </button>
        </div>
      </div>
    </Modal>
  );
}
