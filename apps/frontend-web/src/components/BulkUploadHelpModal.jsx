import { Download, Edit, Image, MapPin, Rocket } from 'lucide-react';
import Modal from './Modal';

const COLUMNS = [
  { name: 'Nombre', required: 'Sí', desc: 'Nombre único del espacio', example: '"Oficina Centro"' },
  { name: 'Descripción', required: 'No', desc: 'Descripción del espacio', example: '"Amplio piso con..."' },
  { name: 'Dirección', required: 'Sí', desc: 'Dirección (se usa para calcular coordenadas)', example: '"Av. Corrientes 1234"' },
  { name: 'Ciudad', required: 'Sí', desc: 'Ciudad', example: '"Buenos Aires"' },
  { name: 'País', required: 'Sí', desc: 'País', example: '"Argentina"' },
  { name: 'Capacidad', required: 'Sí', desc: 'Número de personas', example: '10' },
  { name: 'PrecioPorHora', required: 'Sí', desc: 'Precio por hora en USD', example: '25.00' },
  { name: 'PrecioPorDía', required: 'No', desc: 'Precio por día', example: '180.00' },
  { name: 'PrecioPorMes', required: 'No', desc: 'Precio por mes', example: '3500.00' },
  { name: 'Amenities', required: 'No', desc: 'Separados por coma', example: '"WiFi, Coffee, Parking"' },
  {
    name: 'Imagenes',
    required: 'No',
    desc: 'Nombres de archivo separados por coma (sin rutas). Luego arrastrás las imágenes en el paso de carga.',
    example: '"central1.jpg, central2.jpg"',
  },
];

export default function BulkUploadHelpModal({ isOpen, onClose }) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Cómo cargar espacios masivamente">
      <div className="bulk-help">
        <section className="bulk-help__step">
          <h3><Download size={18} /> Paso 1 — Descargar plantilla</h3>
          <p>
            Descargá la plantilla Excel con el botón de abajo. Tiene dos hojas: <strong>Datos</strong> (donde completás tus
            espacios) e <strong>Instrucciones</strong> (con el detalle de cada columna).
          </p>
        </section>

        <section className="bulk-help__step">
          <h3><Edit size={18} /> Paso 2 — Completar el Excel</h3>
          <p>
            Completá una fila por espacio. Los campos con * son obligatorios. La fila amarilla es de ejemplo —
            reemplazala o eliminala antes de subir.
          </p>

          <div className="bulk-help__table-wrapper">
            <table className="bulk-help__table">
              <thead>
                <tr>
                  <th>Columna</th>
                  <th>Requerido</th>
                  <th>Descripción</th>
                  <th>Ejemplo</th>
                </tr>
              </thead>
              <tbody>
                {COLUMNS.map((col) => (
                  <tr key={col.name}>
                    <td><code>{col.name}</code></td>
                    <td>{col.required}</td>
                    <td>{col.desc}</td>
                    <td><code>{col.example}</code></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>

        <section className="bulk-help__step">
          <h3><Image size={18} /> Paso 3 — Imágenes</h3>
          <p>
            En la columna <code>Imagenes</code> escribí solo el nombre del archivo (ej: <code>oficina1.jpg</code>).
            Si un espacio tiene varias fotos, separalas con coma.
          </p>
          <p>
            Después, en el paso de carga, arrastrá todas las imágenes juntas. El sistema las va a emparejar
            automáticamente.
          </p>
          <div className="bulk-help__table-wrapper">
            <table className="bulk-help__table">
              <thead>
                <tr>
                  <th>Columna Imagenes</th>
                  <th>Archivos a subir</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td><code>oficina1.jpg, sala.jpg</code></td>
                  <td><code>oficina1.jpg — sala.jpg — recepcion.jpg</code></td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <section className="bulk-help__step">
          <h3><MapPin size={18} /> Paso 4 — Ubicación automática</h3>
          <p>
            Si completás <strong>Dirección</strong>, <strong>Ciudad</strong> y <strong>País</strong>, el sistema calcula
            automáticamente las coordenadas. Si no se puede determinar la ubicación, el espacio igual se crea (sin
            coordenadas).
          </p>
        </section>

        <section className="bulk-help__step">
          <h3><Rocket size={18} /> Paso 5 — Subir y procesar</h3>
          <p>
            Arrastrá el Excel y las imágenes o seleccionalos con los botones. Hacé clic en <strong>Procesar</strong>.
            Al finalizar, vas a ver cuántos espacios se crearon y si hubo errores.
          </p>
        </section>
      </div>
    </Modal>
  );
}
