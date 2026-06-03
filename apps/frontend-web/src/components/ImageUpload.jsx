import { useRef, useState } from 'react';
import { uploadImage } from '../api/storage';

const MAX_FILES = 5;
const MAX_SIZE_MB = 5;

export default function ImageUpload({ files = [], onChange }) {
  const inputRef = useRef();
  const [dragging, setDragging] = useState(false);
  const [error, setError] = useState('');
  const [uploading, setUploading] = useState(false);

  async function addFiles(rawFiles) {
    setError('');
    const slots = MAX_FILES - files.length;
    if (slots <= 0) return;

    const valid = Array.from(rawFiles)
      .filter(f => {
        if (!f.type.startsWith('image/')) { setError('Solo se aceptan imágenes.'); return false; }
        if (f.size > MAX_SIZE_MB * 1024 * 1024) { setError(`Máximo ${MAX_SIZE_MB}MB por imagen.`); return false; }
        return true;
      })
      .slice(0, slots);

    if (!valid.length) return;

    setUploading(true);
    try {
      const uploaded = await Promise.all(
        valid.map(async (f) => ({
          url: await uploadImage(f),
          preview: URL.createObjectURL(f),
          name: f.name,
        }))
      );
      onChange([...files, ...uploaded]);
    } catch {
      setError('Error al subir las imágenes. Intentá de nuevo.');
    } finally {
      setUploading(false);
    }
  }

  function remove(index) {
    URL.revokeObjectURL(files[index].preview);
    onChange(files.filter((_, i) => i !== index));
    setError('');
  }

  function handleDrop(e) {
    e.preventDefault();
    setDragging(false);
    addFiles(e.dataTransfer.files);
  }

  function handleInput(e) {
    addFiles(e.target.files);
    e.target.value = '';
  }

  return (
    <div className="image-upload">
      {files.length > 0 ? (
        <div className="image-upload__grid">
          {files.map((f, i) => (
            <div key={f.url} className="image-thumb">
              <img src={f.preview ?? f.url} alt={f.name} className="image-thumb__img" />
              <button
                type="button"
                className="image-thumb__remove"
                onClick={() => remove(i)}
                aria-label="Eliminar imagen"
                disabled={uploading}
              >
                ✕
              </button>
            </div>
          ))}
          {files.length < MAX_FILES && !uploading && (
            <button
              type="button"
              className="image-thumb image-thumb--add"
              onClick={() => inputRef.current.click()}
              title="Agregar imagen"
            >
              +
            </button>
          )}
          {uploading && (
            <div className="image-thumb image-thumb--loading">
              <span>...</span>
            </div>
          )}
        </div>
      ) : (
        <div
          className={`image-upload__zone${dragging ? ' image-upload__zone--active' : ''}${uploading ? ' image-upload__zone--loading' : ''}`}
          onClick={() => !uploading && inputRef.current.click()}
          onDrop={handleDrop}
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => e.key === 'Enter' && !uploading && inputRef.current.click()}
        >
          <div className="image-upload__icon">
            {uploading ? (
              <span style={{ fontSize: '14px', color: 'var(--color-primary)' }}>Subiendo...</span>
            ) : (
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                <rect x="3" y="3" width="18" height="18" rx="2" />
                <circle cx="8.5" cy="8.5" r="1.5" />
                <path d="M21 15l-5-5L5 21" />
              </svg>
            )}
          </div>
          <p className="image-upload__text">Arrastrá imágenes o hacé click para seleccionar</p>
          <p className="image-upload__hint">JPG, PNG, WebP · Máx. {MAX_FILES} imágenes · {MAX_SIZE_MB}MB c/u</p>
        </div>
      )}

      {error && <p className="form-error" role="alert">{error}</p>}

      <input
        ref={inputRef}
        type="file"
        accept="image/*"
        multiple
        onChange={handleInput}
        style={{ display: 'none' }}
      />
    </div>
  );
}
