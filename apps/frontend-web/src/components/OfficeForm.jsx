import { useState } from 'react';
import { X } from 'lucide-react';
import ImageUpload from './ImageUpload';
import AddressAutocomplete from './AddressAutocomplete';

const AMENITIES = [
  { key: 'WiFi', label: 'WiFi' },
  { key: 'Coffee', label: 'Café' },
  { key: 'Parking', label: 'Parking' },
  { key: 'Meeting Rooms', label: 'Salas de reuniones' },
  { key: 'Printing', label: 'Impresión' },
  { key: '24/7 Access', label: 'Acceso 24/7' },
  { key: 'Cafeteria', label: 'Cafetería' },
  { key: 'Gym', label: 'Gimnasio' },
  { key: 'Lounge', label: 'Lounge' },
  { key: 'Bike Storage', label: 'Guardabicicletas' },
  { key: 'Phone Booths', label: 'Cabinas telefónicas' },
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

export default function OfficeForm({ form, onChange, onAmenityToggle, onAmenityAdd, onAmenityRemove, onImagesChange, onLocationSelect, onSubmit, onCancel, errors = {}, isPending, isError, errorMessage }) {
  const [customAmenity, setCustomAmenity] = useState('');

  const amenitiesLower = form.amenities.map((a) => a.toLowerCase());
  const presetAmenities = form.amenities.filter((a) => PRESET_LOWER_VALUES.has(a.toLowerCase()));
  const customAmenities = form.amenities.filter((a) => !PRESET_LOWER_VALUES.has(a.toLowerCase()));

  function handleAdd() {
    const val = customAmenity.trim();
    if (!val || form.amenities.some((a) => a.toLowerCase() === val.toLowerCase())) {
      setCustomAmenity('');
      return;
    }
    onAmenityAdd(val);
    setCustomAmenity('');
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAdd();
    }
  }

  return (
    <form onSubmit={onSubmit} noValidate>

      <div className="form-section">
        <p className="form-section__title">Información básica</p>
        <div className="form-group">
          <label htmlFor="name">Nombre del espacio *</label>
          <input id="name" name="name" type="text" value={form.name} onChange={onChange} placeholder="Ej: Oficina Central Plaza" maxLength={100} />
          {errors.name && <p className="form-error" role="alert">{errors.name}</p>}
        </div>
        <div className="form-group">
          <label htmlFor="description">Descripción</label>
          <textarea id="description" name="description" value={form.description} onChange={onChange} placeholder="Describí el espacio, sus características, ambiente..." maxLength={500} />
        </div>
      </div>

      <div className="form-section">
        <p className="form-section__title">Ubicación</p>
        <div className="form-group">
          <label htmlFor="address">Dirección *</label>
          <AddressAutocomplete
            value={form.address}
            onChange={(val) => onChange({ target: { name: 'address', value: val } })}
            onSelect={onLocationSelect}
            placeholder="Calle, número, piso o nombre del lugar"
          />
          {errors.address && <p className="form-error" role="alert">{errors.address}</p>}
        </div>
        <div className="form-row form-row--2">
          <div className="form-group">
            <label htmlFor="city">Ciudad *</label>
            <input id="city" name="city" type="text" value={form.city} onChange={onChange} placeholder="Ej: Buenos Aires" />
            {errors.city && <p className="form-error" role="alert">{errors.city}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="country">País *</label>
            <input id="country" name="country" type="text" value={form.country} onChange={onChange} placeholder="Ej: Argentina" />
            {errors.country && <p className="form-error" role="alert">{errors.country}</p>}
          </div>
        </div>
        <div className="form-row form-row--2">
          <div className="form-group">
            <label htmlFor="latitude">Latitud</label>
            <input id="latitude" name="latitude" type="number" value={form.latitude} onChange={onChange} placeholder="-34.603722" step="any" min={-90} max={90} disabled />
          </div>
          <div className="form-group">
            <label htmlFor="longitude">Longitud</label>
            <input id="longitude" name="longitude" type="number" value={form.longitude} onChange={onChange} placeholder="-58.381592" step="any" min={-180} max={180} disabled />
          </div>
        </div>
      </div>

      <div className="form-section">
        <p className="form-section__title">Detalles del espacio</p>
        <div className="form-group">
          <label htmlFor="capacity">Capacidad (personas) *</label>
          <input id="capacity" name="capacity" type="number" value={form.capacity} onChange={onChange} placeholder="Ej: 10" min={1} max={1000} />
          {errors.capacity && <p className="form-error" role="alert">{errors.capacity}</p>}
        </div>
      </div>

      <div className="form-section">
        <p className="form-section__title">Precios y seña</p>
        <div className="form-row form-row--3">
          <div className="form-group">
            <label htmlFor="pricePerHour">Por hora *</label>
            <input id="pricePerHour" name="pricePerHour" type="number" value={form.pricePerHour} onChange={onChange} placeholder="0.00" min={0} step="0.01" />
            {errors.pricePerHour && <p className="form-error" role="alert">{errors.pricePerHour}</p>}
          </div>
          <div className="form-group">
            <label htmlFor="pricePerDay">Por día</label>
            <input id="pricePerDay" name="pricePerDay" type="number" value={form.pricePerDay} onChange={onChange} placeholder="0.00" min={0} step="0.01" />
          </div>
          <div className="form-group">
            <label htmlFor="pricePerMonth">Por mes</label>
            <input id="pricePerMonth" name="pricePerMonth" type="number" value={form.pricePerMonth} onChange={onChange} placeholder="0.00" min={0} step="0.01" />
          </div>
        </div>
        <div className="form-group">
          <label htmlFor="depositPercentage">Seña (%)</label>
          <input id="depositPercentage" name="depositPercentage" type="number" value={form.depositPercentage} onChange={onChange} placeholder="30" min={0} max={100} />
          {errors.depositPercentage && <p className="form-error" role="alert">{errors.depositPercentage}</p>}
        </div>
      </div>

      <div className="form-section">
        <p className="form-section__title">Imágenes</p>
        <ImageUpload files={form.images} onChange={onImagesChange} />
      </div>

      <div className="form-section">
        <p className="form-section__title">Amenidades</p>

        <div className="amenities-custom">
          <input
            type="text"
            placeholder="Agregar otra amenidad..."
            value={customAmenity}
            onChange={(e) => setCustomAmenity(e.target.value)}
            onKeyDown={handleKeyDown}
          />
          <button type="button" onClick={handleAdd} disabled={!customAmenity.trim()}>
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

      {Object.keys(errors).length > 0 && (
        <div className="form-validation-banner" role="alert">
          <span className="form-validation-banner__icon">!</span>
          <span>Campos incompletos: {Object.values(errors).join(' · ')}</span>
        </div>
      )}

      {isError && (
        <p className="form-api-error" role="alert">
          {errorMessage ?? 'Ocurrió un error. Intentá de nuevo.'}
        </p>
      )}

      <div className="modal__footer">
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isPending}>
          Cancelar
        </button>
        <button type="submit" className="btn btn-primary" disabled={isPending}>
          {isPending ? 'Guardando...' : 'Guardar'}
        </button>
      </div>

    </form>
  );
}