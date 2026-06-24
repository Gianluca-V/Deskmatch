import { useState, useCallback } from 'react';
import { MapContainer, TileLayer, Marker, Circle, useMapEvents } from 'react-leaflet';
import L from 'leaflet';
import { Navigation, X, MapPin, Search } from 'lucide-react';
import 'leaflet/dist/leaflet.css';
import AddressAutocomplete from './AddressAutocomplete';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

const RADII = [
  { label: '1 km', value: 1 },
  { label: '5 km', value: 5 },
  { label: '10 km', value: 10 },
  { label: '25 km', value: 25 },
  { label: '50 km', value: 50 },
];

function ClickHandler({ onMapClick }) {
  useMapEvents({
    click(e) {
      onMapClick({ lat: e.latlng.lat, lng: e.latlng.lng });
    },
  });
  return null;
}

export default function LocationFilter({ value, onChange }) {
  const [center, setCenter] = useState(value?.lat && value?.lon ? { lat: value.lat, lng: value.lon } : null);
  const [radius, setRadius] = useState(value?.radius || 10);
  const [locating, setLocating] = useState(false);
  const [geoError, setGeoError] = useState(null);
  const [searchText, setSearchText] = useState('');

  const handleMove = useCallback(({ lat, lng }) => {
    const pos = { lat, lng };
    setCenter(pos);
    onChange({ lat, lon: lng, radius });
  }, [onChange, radius]);

  const handleRadius = useCallback((r) => {
    setRadius(r);
    if (center) onChange({ lat: center.lat, lon: center.lng, radius: r });
  }, [onChange, center]);

  const handleMapClick = useCallback(({ lat, lng }) => {
    const pos = { lat, lng };
    setCenter(pos);
    setSearchText('');
    onChange({ lat, lon: lng, radius });
  }, [onChange, radius]);

  const handleSearchSelect = useCallback((result) => {
    const pos = { lat: result.latitude, lng: result.longitude };
    setCenter(pos);
    setSearchText(result.displayName);
    onChange({ lat: result.latitude, lon: result.longitude, radius });
  }, [onChange, radius]);

  const locateMe = useCallback(() => {
    if (!navigator.geolocation) { setGeoError('Geolocalización no soportada'); return; }
    setLocating(true);
    setGeoError(null);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        const p = { lat: pos.coords.latitude, lng: pos.coords.longitude };
        setCenter(p);
        onChange({ lat: p.lat, lon: p.lng, radius });
        setLocating(false);
      },
      () => { setGeoError('No se pudo obtener tu ubicación'); setLocating(false); },
      { enableHighAccuracy: true, timeout: 10000 },
    );
  }, [onChange, radius]);

  const clearLocation = useCallback(() => {
    setCenter(null);
    setRadius(10);
    setSearchText('');
    onChange({ lat: undefined, lon: undefined, radius: undefined });
    setGeoError(null);
  }, [onChange]);

  return (
    <div className="location-filter">
      <div className="location-filter__actions">
        <button
          type="button"
          className="location-filter__btn location-filter__btn--locate"
          onClick={locateMe}
          disabled={locating}
        >
          <Navigation size={14} />
          {locating ? 'Obteniendo...' : 'Cerca de mí'}
        </button>
        {center && (
          <button
            type="button"
            className="location-filter__btn location-filter__btn--clear"
            onClick={clearLocation}
          >
            <X size={14} />
            Quitar ubicación
          </button>
        )}
      </div>

      <div className="location-filter__search">
        <AddressAutocomplete
          value={searchText}
          onChange={setSearchText}
          onSelect={handleSearchSelect}
          placeholder="Buscar dirección o ciudad..."
        />
      </div>

      {geoError && <p className="location-filter__error">{geoError}</p>}

      {center && (
        <div className="location-filter__radii">
          {RADII.map((r) => (
            <button
              key={r.value}
              type="button"
              className={`location-filter__radius-btn ${radius === r.value ? 'location-filter__radius-btn--active' : ''}`}
              onClick={() => handleRadius(r.value)}
            >
              {r.label}
            </button>
          ))}
        </div>
      )}

      <div className="location-filter__map">
        <MapContainer
          key={center ? `${center.lat}-${center.lng}` : 'default'}
          center={center || { lat: -34.6037, lng: -58.3816 }}
          zoom={center ? 13 : 4}
          className="location-filter__leaflet"
          zoomControl={true}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <ClickHandler onMapClick={handleMapClick} />
          {center && (
            <Marker
              draggable
              position={center}
              eventHandlers={{
                dragend(e) {
                  const { lat, lng } = e.target.getLatLng();
                  handleMove({ lat, lng });
                },
              }}
            />
          )}
          {center && radius && (
            <Circle center={center} radius={radius * 1000} pathOptions={{ color: '#0066ff', fillOpacity: 0.08, weight: 2 }} />
          )}
        </MapContainer>
      </div>

      {center && (
        <p className="location-filter__coords">
          <MapPin size={12} />
          {center.lat.toFixed(5)}, {center.lng.toFixed(5)} — {radius} km
        </p>
      )}
    </div>
  );
}
