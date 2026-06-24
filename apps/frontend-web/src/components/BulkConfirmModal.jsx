import { useState } from 'react';
import {
  AlertTriangle,
  CheckCircle2,
  ArrowLeft,
  MapPin,
  Users,
  Euro,
  Image,
  List,
  Loader2,
  Save,
  X,
} from 'lucide-react';
import Modal from './Modal';
import AddressAutocomplete from './AddressAutocomplete';

const AMENITIES = [
  { key: 'WiFi', label: 'WiFi' },
  { key: 'Coffee', label: 'Cafe' },
  { key: 'Parking', label: 'Parking' },
  { key: 'Meeting Rooms', label: 'Salas de reuniones' },
  { key: 'Printing', label: 'Impresion' },
  { key: '24/7 Access', label: 'Acceso 24/7' },
  { key: 'Cafeteria', label: 'Cafeteria' },
  { key: 'Gym', label: 'Gimnasio' },
  { key: 'Lounge', label: 'Lounge' },
  { key: 'Bike Storage', label: 'Guardabicicletas' },
  { key: 'Phone Booths', label: 'Cabinas telefonicas' },
  { key: 'Event Space', label: 'Espacio para eventos' },
  { key: 'Rooftop', label: 'Terraza' },
  { key: 'Pet Friendly', label: 'Mascotas permitidas' },
];

const PRESET_KEYS = AMENITIES.map((a) => a.key);
const PRESET_LOWER_VALUES = new Map();
AMENITIES.forEach(({ key, label }) => {
  PRESET_LOWER_VALUES.set(key.toLowerCase(), key);
  PRESET_LOWER_VALUES.set(label.toLowerCase(), key);
});

function EditPanel({ office, index, onSave, onBack }) {
  const [form, setForm] = useState({ ...office });
  const [dirty, setDirty] = useState(false);
  const [customAmenity, setCustomAmenity] = useState('');

  const amenities = form.amenities || [];
  const amenitiesLower = amenities.map((a) => a.toLowerCase());
  const presetAmenities = amenities.filter((a) => PRESET_LOWER_VALUES.has(a.toLowerCase()));
  const customAmenities = amenities.filter((a) => !PRESET_LOWER_VALUES.has(a.toLowerCase()));

  const set = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
    setDirty(true);
  };

  const handleSave = () => {
    onSave(index, form);
    onBack();
  };

  const onLocationSelect = (result) => {
    setForm((prev) => ({
      ...prev,
      address: result.displayName || prev.address,
      city: result.city || prev.city,
      country: result.country || prev.country,
      latitude: result.latitude ?? prev.latitude,
      longitude: result.longitude ?? prev.longitude,
    }));
    setDirty(true);
  };

  function onAmenityToggle(key) {
    const current = form.amenities || [];
    const label = AMENITIES.find((a) => a.key === key)?.label?.toLowerCase();
    const matchValues = [key.toLowerCase(), label].filter(Boolean);
    const hasMatch = current.some((a) => matchValues.includes(a.toLowerCase()));
    if (hasMatch) {
      set('amenities', current.filter((a) => !matchValues.includes(a.toLowerCase())));
    } else {
      set('amenities', [...current, key]);
    }
  }

  function onAmenityAdd(name) {
    const current = form.amenities || [];
    if (current.some((a) => a.toLowerCase() === name.toLowerCase())) return;
    set('amenities', [...current, name]);
  }

  function onAmenityRemove(name) {
    const nameLower = name.toLowerCase();
    set('amenities', (form.amenities || []).filter((a) => a.toLowerCase() !== nameLower));
  }

  function handleAddCustom() {
    const val = customAmenity.trim();
    if (!val || (form.amenities || []).some((a) => a.toLowerCase() === val.toLowerCase())) {
      setCustomAmenity('');
      return;
    }
    onAmenityAdd(val);
    setCustomAmenity('');
  }

  function handleCustomKeyDown(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddCustom();
    }
  }

  return (
    <div className="bulk-confirm__edit-panel">
      <div className="bulk-confirm__edit-header">
        <button type="button" className="bulk-confirm__back-btn" onClick={onBack}>
          <ArrowLeft size={18} />
          Volver
        </button>
        <h3 className="bulk-confirm__edit-title">{form.name}</h3>
      </div>

      <div className="bulk-confirm__edit-body">
        <div className="bulk-confirm__field">
          <label>Nombre</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => set('name', e.target.value)}
            maxLength={256}
          />
        </div>

        <div className="bulk-confirm__field">
          <label>Descripcion</label>
          <textarea
            value={form.description || ''}
            onChange={(e) => set('description', e.target.value)}
            rows={3}
            maxLength={500}
          />
        </div>

        <div className="bulk-confirm__field-row">
          <div className="bulk-confirm__field">
            <label>Direccion</label>
            <AddressAutocomplete
              value={form.address || ''}
              onChange={(val) => set('address', val)}
              onSelect={onLocationSelect}
              placeholder="Calle, numero, piso o nombre del lugar"
              maxLength={512}
            />
          </div>
          <div className="bulk-confirm__field">
            <label>Ciudad</label>
            <input
              type="text"
              value={form.city || ''}
              onChange={(e) => set('city', e.target.value)}
              maxLength={128}
            />
          </div>
          <div className="bulk-confirm__field">
            <label>Pais</label>
            <input
              type="text"
              value={form.country || ''}
              onChange={(e) => set('country', e.target.value)}
              maxLength={128}
            />
          </div>
        </div>

        <div className="bulk-confirm__field-row">
          <div className="bulk-confirm__field">
            <label>Capacidad</label>
            <input
              type="number"
              value={form.capacity}
              onChange={(e) => set('capacity', parseInt(e.target.value) || 0)}
            />
          </div>
          <div className="bulk-confirm__field">
            <label>Precio por hora</label>
            <input
              type="number"
              step="0.01"
              value={form.pricePerHour}
              onChange={(e) => set('pricePerHour', parseFloat(e.target.value) || 0)}
            />
          </div>
          <div className="bulk-confirm__field">
            <label>Precio por dia</label>
            <input
              type="number"
              step="0.01"
              value={form.pricePerDay ?? ''}
              onChange={(e) => set('pricePerDay', e.target.value ? parseFloat(e.target.value) : null)}
            />
          </div>
        </div>

        <div className="bulk-confirm__field-row">
          <div className="bulk-confirm__field">
            <label>Precio por mes</label>
            <input
              type="number"
              step="0.01"
              value={form.pricePerMonth ?? ''}
              onChange={(e) => set('pricePerMonth', e.target.value ? parseFloat(e.target.value) : null)}
            />
          </div>
          <div className="bulk-confirm__field">
            <label>Imagenes (separadas por coma)</label>
            <input
              type="text"
              value={(form.imageFileNames || []).join(', ')}
              onChange={(e) =>
                set(
                  'imageFileNames',
                  e.target.value.split(',').map((s) => s.trim()).filter(Boolean)
                )
              }
            />
          </div>
        </div>

        <div className="form-section">
          <p className="form-section__title">Amenidades</p>

          <div className="amenities-custom">
            <input
              type="text"
              placeholder="Agregar otra amenidad..."
              value={customAmenity}
              onChange={(e) => setCustomAmenity(e.target.value)}
              onKeyDown={handleCustomKeyDown}
            />
            <button type="button" onClick={handleAddCustom} disabled={!customAmenity.trim()}>
              + Agregar
            </button>
          </div>

          {customAmenities.length > 0 && (
            <div className="amenities-tags">
              {customAmenities.map((a) => (
                <span className="amenities-tag" key={a}>
                  {a}
                  <button type="button" onClick={() => onAmenityRemove(a)}>
                    <X size={12} />
                  </button>
                </span>
              ))}
            </div>
          )}

          <div className="amenities-grid">
            {AMENITIES.map(({ key, label }) => (
              <label key={key} className="amenity-item">
                <input type="checkbox" checked={amenitiesLower.includes(key.toLowerCase()) || amenitiesLower.includes(AMENITIES.find((a) => a.key === key)?.label?.toLowerCase() ?? '')} onChange={() => onAmenityToggle(key)} />
                {label}
              </label>
            ))}
          </div>
        </div>

        <div className="bulk-confirm__edit-coords">
          {form.latitude != null && form.longitude != null ? (
            <span>
              <MapPin size={14} /> {form.latitude.toFixed(5)}, {form.longitude.toFixed(5)}
            </span>
          ) : (
            <span className="bulk-confirm__edit-coords--missing">
              <MapPin size={14} /> Sin coordenadas
            </span>
          )}
        </div>

        {office.warnings.length > 0 && (
          <div className="bulk-confirm__warnings">
            {office.warnings.map((w, i) => (
              <div key={i} className="bulk-confirm__warning-item">
                <AlertTriangle size={14} />
                {w}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="bulk-confirm__edit-footer">
        <button type="button" className="btn btn-primary" onClick={handleSave}>
          <Save size={16} />
          Guardar cambios
        </button>
        <button type="button" className="btn btn-secondary" onClick={onBack}>
          Cancelar
        </button>
      </div>
    </div>
  );
}

function OfficeCard({ office, onClick, imageKeys }) {
  const imageWarnings = (office.imageFileNames || []).filter((fname) => {
    const dot = fname.lastIndexOf('.');
    const key = (dot > 0 ? fname.substring(0, dot) : fname).toLowerCase();
    return !imageKeys.has(key);
  }).map((fname) => `La imagen "${fname}" no coincide con ningun archivo subido.`);

  const allWarnings = [...(office.warnings || []), ...imageWarnings];
  const hasWarnings = allWarnings.length > 0;

  return (
    <div
      className={`bulk-confirm__office ${hasWarnings ? 'bulk-confirm__office--warning' : ''}`}
      onClick={() => onClick()}
    >
      <div className="bulk-confirm__office-header">
        <div className="bulk-confirm__office-summary">
          {hasWarnings ? (
            <AlertTriangle size={18} className="bulk-confirm__icon-warning" />
          ) : (
            <CheckCircle2 size={18} className="bulk-confirm__icon-ok" />
          )}
          <span className="bulk-confirm__office-name">{office.name}</span>
          <span className="bulk-confirm__office-meta">
            <Users size={14} />
            {office.capacity}
            <MapPin size={14} />
            {office.city}
          </span>
          {hasWarnings && (
            <span className="bulk-confirm__warning-badge">{allWarnings.length}</span>
          )}
        </div>
      </div>

      {hasWarnings && (
        <div className="bulk-confirm__warnings">
          {allWarnings.map((w, i) => (
            <div key={i} className="bulk-confirm__warning-item">
              <AlertTriangle size={14} />
              {w}
            </div>
          ))}
        </div>
      )}

      <div className="bulk-confirm__office-body">
        <div className="bulk-confirm__data-row">
          <span><MapPin size={14} /> {office.address || '—'}</span>
          <span><Euro size={14} /> ${office.pricePerHour}/h</span>
          <span><Users size={14} /> {office.capacity} pers.</span>
        </div>
        {office.description && (
          <p className="bulk-confirm__description">{office.description}</p>
        )}
        {office.amenities && office.amenities.length > 0 && (
          <div className="bulk-confirm__tags">
            <List size={14} />
            {office.amenities.map((a) => (
              <span key={a} className="bulk-confirm__tag">{a}</span>
            ))}
          </div>
        )}
        {(office.imageFileNames && office.imageFileNames.length > 0) && (
          <div className="bulk-confirm__files">
            <Image size={14} />
            {office.imageFileNames.join(', ')}
          </div>
        )}
      </div>
    </div>
  );
}

export default function BulkConfirmModal({ preview, images, onConfirm, onCancel, isPending }) {
  const [offices, setOffices] = useState(preview.offices);
  const [selectedIndex, setSelectedIndex] = useState(null);

  const imageKeys = new Set(
    images.map((img) => {
      const name = img.name;
      const dot = name.lastIndexOf('.');
      return (dot > 0 ? name.substring(0, dot) : name).toLowerCase();
    })
  );

  const hasWarnings = offices.some((o) => {
    if (o.warnings && o.warnings.length > 0) return true;
    return (o.imageFileNames || []).some((fname) => {
      const dot = fname.lastIndexOf('.');
      const key = (dot > 0 ? fname.substring(0, dot) : fname).toLowerCase();
      return !imageKeys.has(key);
    });
  });

  const handleOfficeChange = (index, updated) => {
    setOffices((prev) =>
      prev.map((o, i) => {
        if (i !== index) return o;
        const hasCoords = updated.latitude != null && updated.longitude != null;
        const warnings = (updated.warnings || []).filter(
          (w) => !hasCoords || !w.includes('coordenadas')
        );
        return { ...updated, warnings };
      })
    );
  };

  const handleConfirm = () => {
    const formData = new FormData();
    formData.append(
      'offices',
      JSON.stringify(
        offices.map((o) => ({
          tempId: o.tempId,
          name: o.name,
          description: o.description,
          address: o.address,
          city: o.city,
          country: o.country,
          latitude: o.latitude,
          longitude: o.longitude,
          capacity: o.capacity,
          pricePerHour: o.pricePerHour,
          pricePerDay: o.pricePerDay,
          pricePerMonth: o.pricePerMonth,
          amenities: o.amenities,
          imageFileNames: o.imageFileNames,
        }))
      )
    );
    images.forEach((img) => formData.append('images', img));
    onConfirm(formData);
  };

  return (
    <Modal isOpen onClose={onCancel} className="bulk-confirm-modal">
      {selectedIndex != null ? (
        <EditPanel
          office={offices[selectedIndex]}
          index={selectedIndex}
          onSave={handleOfficeChange}
          onBack={() => setSelectedIndex(null)}
        />
      ) : (
        <div className="bulk-confirm">
          <p className="bulk-confirm__intro">
            Se detectaron {preview.offices.length} espacios en el archivo.
            {hasWarnings &&
              ' Algunos tienen advertencias que podes revisar antes de confirmar.'}
          </p>

          <div className="bulk-confirm__list">
            {offices.map((office, i) => (
              <OfficeCard
                key={office.tempId}
                office={office}
                imageKeys={imageKeys}
                onClick={() => setSelectedIndex(i)}
              />
            ))}
          </div>

          <div className="bulk-confirm__footer">
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleConfirm}
              disabled={isPending}
            >
              {isPending ? (
                <>
                  <Loader2 size={16} className="bulk-confirm__spinner" />
                  Creando espacios...
                </>
              ) : (
                `Confirmar creacion (${offices.length} espacios)`
              )}
            </button>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onCancel}
              disabled={isPending}
            >
              Cancelar
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
