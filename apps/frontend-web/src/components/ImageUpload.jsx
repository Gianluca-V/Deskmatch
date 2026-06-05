import { useRef, useState } from 'react';

const MAX_FILES = 5;
const MAX_SIZE_MB = 5;

export default function ImageUpload({ files = [], onChange }) {
  const inputRef = useRef();
  const [dragging, setDragging] = useState(false);
  const [error, setError] = useState('');

  function addFiles(rawFiles) {
    setError('');
    const slots = MAX_FILES - files.length;
    if (slots <= 0) return;

    const valid = Array.from(rawFiles)
      .filter(f => {
        if (!f.type.startsWith('image/')) { setError('Solo se aceptan imágenes.'); return false; }
        if (f.size > MAX_SIZE_MB * 1024 * 1024) { setError(`Máximo ${MAX_SIZE_MB}MB por imagen.`); return false; }
        return true;
      })
      .slice(0, slots)
      .map(f => ({ file: f, preview: URL.createObjectURL(f) }));

    if (valid.length) onChange([...files, ...valid]);
  }

  function remove(index) {
    if (files[index].file) URL.revokeObjectURL(files[index].preview);
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
            <div key={f.preview} className="image-thumb">
              <img src={f.preview} alt={f.file?.name ?? 'imagen'} className="image-thumb__img" />
              <button
                type="button"
                className="image-thumb__remove"
                onClick={() => remove(i)}
                aria-label="Eliminar imagen"
              >
                ✕
              </button>
            </div>
          ))}
          {files.length < MAX_FILES && (
            <button
              type="button"
              className="image-thumb image-thumb--add"
              onClick={() => inputRef.current.click()}
              title="Agregar imagen"
            >
              +
            </button>
          )}
        </div>
      ) : (
        <div
          className={`image-upload__zone${dragging ? ' image-upload__zone--active' : ''}`}
          onClick={() => inputRef.current.click()}
          onDrop={handleDrop}
          onDragOver={(e) => { e.preventDefault(); setDragging(true); }}
          onDragLeave={() => setDragging(false)}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => e.key === 'Enter' && inputRef.current.click()}
        >
          <div className="image-upload__icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2" />
              <circle cx="8.5" cy="8.5" r="1.5" />
              <path d="M21 15l-5-5L5 21" />
            </svg>
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
