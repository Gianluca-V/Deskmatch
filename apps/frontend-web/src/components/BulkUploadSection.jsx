import { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { AlertTriangle, CheckCircle2, Download, HelpCircle, Loader2 } from 'lucide-react';
import { downloadBulkTemplate } from '../api/offices';
import { useBulkPreview, useBulkConfirm } from '../hooks/useBulkCreateWorkspaces';
import BulkUploadHelpModal from './BulkUploadHelpModal';
import BulkConfirmModal from './BulkConfirmModal';

export default function BulkUploadSection() {
  const [excelFile, setExcelFile] = useState(null);
  const [images, setImages] = useState([]);
  const [showHelp, setShowHelp] = useState(false);
  const [preview, setPreview] = useState(null);
  const [results, setResults] = useState(null);
  const [showErrors, setShowErrors] = useState(false);
  const [previewError, setPreviewError] = useState(null);

  const { mutate: requestPreview, isPending: isPreviewing } = useBulkPreview({
    onSuccess: (data) => {
      setPreview(data);
      setPreviewError(null);
    },
    onError: (err) => {
      const msg = err?.response?.data?.errors?.[0]?.message || err?.message || 'Error al procesar el archivo';
      setPreviewError(msg);
      setPreview(null);
    },
  });

  const { mutate: confirmCreate, isPending: isConfirming } = useBulkConfirm({
    onSuccess: (data) => {
      setPreview(null);
      setResults(data);
    },
    onError: (err) => {
      const msg = err?.response?.data?.errors?.[0]?.message || err?.message || 'Error al crear los espacios';
      setPreview(null);
      setResults({ createdCount: 0, totalRows: 0, errors: [{ officeName: '', message: msg }] });
    },
  });

  const onExcelDrop = useCallback((acceptedFiles) => {
    if (acceptedFiles.length > 0) {
      setExcelFile(acceptedFiles[0]);
      setResults(null);
      setPreview(null);
      setPreviewError(null);
    }
  }, []);

  const onImagesDrop = useCallback((acceptedFiles) => {
    setImages((prev) => {
      const existing = new Set(prev.map((f) => f.name));
      const newFiles = acceptedFiles.filter((f) => !existing.has(f.name));
      return [...prev, ...newFiles];
    });
    setResults(null);
  }, []);

  const removeImage = (name) => {
    setImages((prev) => prev.filter((f) => f.name !== name));
  };

  const handleProcess = () => {
    if (!excelFile) return;

    const formData = new FormData();
    formData.append('file', excelFile);

    requestPreview(formData);
  };

  const handleConfirm = (formData) => {
    confirmCreate(formData);
  };

  const handleCancelConfirm = () => {
    setPreview(null);
  };

  const handleReset = () => {
    setExcelFile(null);
    setImages([]);
    setResults(null);
    setPreview(null);
    setPreviewError(null);
    setShowErrors(false);
  };

  const excelDropzone = useDropzone({
    onDrop: onExcelDrop,
    accept: { 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'] },
    maxFiles: 1,
    disabled: isPreviewing || isConfirming,
  });

  const imagesDropzone = useDropzone({
    onDrop: onImagesDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp'],
    },
    multiple: true,
    disabled: isPreviewing || isConfirming,
  });

  const hasErrors = results?.errors?.length > 0;
  const allCreated = results && !hasErrors && results.createdCount > 0;

  return (
    <section className="bulk-upload">
      <div className="bulk-upload__header">
        <h2 className="bulk-upload__title">Carga masiva de espacios</h2>
        <div className="bulk-upload__header-actions">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => downloadBulkTemplate()}
            disabled={isPreviewing || isConfirming}
          >
            <Download size={16} />
            Descargar plantilla
          </button>
          <button
            type="button"
            className="bulk-upload__help-btn"
            onClick={() => setShowHelp(true)}
            title="Ayuda"
          >
            <HelpCircle size={18} />
          </button>
        </div>
      </div>

      {results ? (
        <div className="bulk-upload__results">
          {allCreated && (
            <div className="bulk-upload__result-banner bulk-upload__result-banner--success">
              <CheckCircle2 size={20} />
              Se crearon {results.createdCount} espacios correctamente.
            </div>
          )}

          {hasErrors && (
            <>
              <div className="bulk-upload__result-banner bulk-upload__result-banner--warning">
                <AlertTriangle size={20} />
                {results.errors.length} espacios tienen errores —{' '}
                <button
                  type="button"
                  className="bulk-upload__toggle-errors"
                  onClick={() => setShowErrors(!showErrors)}
                >
                  Ver detalle
                </button>
              </div>

              {showErrors && (
                <div className="bulk-upload__errors-list">
                  {results.errors.map((err, i) => (
                    <div key={i} className="bulk-upload__error-item">
                      <strong>{err.officeName || `Espacio ${err.row}`}:</strong> {err.message}
                    </div>
                  ))}
                </div>
              )}

              {results.createdCount > 0 && (
                <p className="bulk-upload__partial-success">
                  <CheckCircle2 size={16} />
                  {results.createdCount} espacios se crearon correctamente.
                </p>
              )}
            </>
          )}

          <button type="button" className="btn btn-primary" onClick={handleReset}>
            Cargar otro archivo
          </button>
        </div>
      ) : (
        <>
          <div
            {...excelDropzone.getRootProps()}
            className={`bulk-upload__dropzone ${excelDropzone.isDragActive ? 'bulk-upload__dropzone--active' : ''} ${excelFile ? 'bulk-upload__dropzone--filled' : ''}`}
          >
            <input {...excelDropzone.getInputProps()} />
            {excelFile ? (
              <div className="bulk-upload__file-info">
                <CheckCircle2 size={20} className="bulk-upload__file-check" />
                <span className="bulk-upload__file-name">{excelFile.name}</span>
              </div>
            ) : (
              <p>Arrastra el archivo Excel (.xlsx) o hace clic para seleccionar</p>
            )}
          </div>

          {previewError && (
            <div className="bulk-upload__result-banner bulk-upload__result-banner--warning">
              <AlertTriangle size={16} />
              {previewError}
            </div>
          )}

          <div
            {...imagesDropzone.getRootProps()}
            className={`bulk-upload__dropzone ${imagesDropzone.isDragActive ? 'bulk-upload__dropzone--active' : ''}`}
          >
            <input {...imagesDropzone.getInputProps()} />
            {images.length > 0 ? (
              <div className="bulk-upload__images-info">
                <div className="bulk-upload__images-badge">{images.length} imagenes seleccionadas</div>
                <div className="bulk-upload__images-thumbs">
                  {images.map((img) => (
                    <div key={img.name} className="bulk-upload__thumb">
                      <img src={URL.createObjectURL(img)} alt={img.name} />
                      <button
                        type="button"
                        className="bulk-upload__thumb-remove"
                        onClick={(e) => {
                          e.stopPropagation();
                          removeImage(img.name);
                        }}
                      >
                        <svg width="10" height="10" viewBox="0 0 10 10" fill="none"><path d="M1 1L9 9M9 1L1 9" stroke="currentColor" strokeWidth="2"/></svg>
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            ) : (
              <p>Arrastra las imagenes o hace clic para seleccionar (.jpg, .png, .webp)</p>
            )}
          </div>

          <button
            type="button"
            className="btn btn-primary bulk-upload__submit"
            onClick={handleProcess}
            disabled={!excelFile || isPreviewing}
          >
            {isPreviewing ? (
              <>
                <Loader2 size={16} className="bulk-confirm__spinner" />
                Procesando... (puede llevar unos segundos)
              </>
            ) : (
              'Procesar'
            )}
          </button>
        </>
      )}

      {preview && (
        <BulkConfirmModal
          preview={preview}
          images={images}
          onConfirm={handleConfirm}
          onCancel={handleCancelConfirm}
          isPending={isConfirming}
        />
      )}

      <BulkUploadHelpModal isOpen={showHelp} onClose={() => setShowHelp(false)} />
    </section>
  );
}
