import { useState, useEffect, useRef } from 'react';
import { geocode } from '../api/geocoding';

const DEBOUNCE_MS = 500;

export default function AddressAutocomplete({ value, onChange, onSelect, placeholder, maxLength }) {
  const [suggestions, setSuggestions] = useState([]);
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const timerRef = useRef(null);
  const wrapRef = useRef(null);
  const initialSearchDone = useRef(false);

  useEffect(() => {
    const handleClick = (e) => {
      if (wrapRef.current && !wrapRef.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  /* Buscar ubicación al cargar con una dirección pre-cargada */
  useEffect(() => {
    if (value && value.length >= 3 && !initialSearchDone.current) {
      initialSearchDone.current = true;
      setLoading(true);
      geocode(value)
        .then((results) => {
          if (results?.length > 0) {
            setSuggestions(results);
            setOpen(true);
          }
        })
        .catch(() => {})
        .finally(() => setLoading(false));
    }
  }, []);

  function handleChange(e) {
    const val = e.target.value;
    onChange(val);
    clearTimeout(timerRef.current);

    if (val.length < 3) { setSuggestions([]); setOpen(false); return; }

    timerRef.current = setTimeout(async () => {
      setLoading(true);
      try {
        const results = await geocode(val);
        setSuggestions(results ?? []);
        setOpen((results ?? []).length > 0);
      } catch {
        setSuggestions([]);
      } finally {
        setLoading(false);
      }
    }, DEBOUNCE_MS);
  }

  function handleSelect(result) {
    onSelect(result);
    setSuggestions([]);
    setOpen(false);
  }

  return (
    <div ref={wrapRef} className="address-autocomplete">
      <input
        type="text"
        value={value}
        onChange={handleChange}
        placeholder={placeholder}
        autoComplete="off"
        maxLength={maxLength}
      />
      {loading && <p className="address-autocomplete__loading">Buscando...</p>}
      {open && suggestions.length > 0 && (
        <ul className="address-autocomplete__list">
          {suggestions.map((s, i) => (
            <li
              key={i}
              className="address-autocomplete__item"
              onMouseDown={() => handleSelect(s)}
            >
              {s.displayName || [s.city, s.country].filter(Boolean).join(', ') || `${s.latitude}, ${s.longitude}`}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
