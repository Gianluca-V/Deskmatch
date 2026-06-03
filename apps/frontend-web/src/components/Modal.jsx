import { useEffect } from 'react';
import { createPortal } from 'react-dom';

export default function Modal({ isOpen, onClose, title, children }) {
  useEffect(() => {
    if (!isOpen) return;
    const handleKey = (e) => { if (e.key === 'Escape') onClose(); };
    document.addEventListener('keydown', handleKey);
    document.body.style.overflow = 'hidden';
    return () => {
      document.removeEventListener('keydown', handleKey);
      document.body.style.overflow = '';
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return createPortal(
    <div
      className="modal-overlay"
      onClick={(e) => { if (e.target === e.currentTarget) onClose(); }}
    >
      <div role="dialog" aria-modal="true" aria-labelledby="modal-title" className="modal">
        <div className="modal__header">
          <h2 id="modal-title" className="modal__title">{title}</h2>
          <button onClick={onClose} aria-label="Cerrar" className="modal__close">✕</button>
        </div>
        <div className="modal__body">
          {children}
        </div>
      </div>
    </div>,
    document.body
  );
}
