import { useState, useEffect } from 'react';
import Modal from './Modal';
import OfficeForm from './OfficeForm';
import { useCreateOffice } from '../hooks/useCreateOffice';
import { uploadImage, deleteImage } from '../api/storage';

const EMPTY_FORM = {
  companyId: '',
  name: '',
  description: '',
  address: '',
  city: '',
  country: '',
  latitude: '',
  longitude: '',
  capacity: '',
  pricePerHour: '',
  pricePerDay: '',
  pricePerMonth: '',
  depositPercentage: '30',
  amenities: [],
  images: [],
};

function validate(form) {
  const errors = {};
  if (!form.companyId.trim()) errors.companyId = 'No se encontró empresa asociada a tu cuenta';
  if (!form.name.trim()) errors.name = 'El nombre es requerido';
  if (!form.address.trim()) errors.address = 'La dirección es requerida';
  if (!form.city.trim()) errors.city = 'La ciudad es requerida';
  if (!form.country.trim()) errors.country = 'El país es requerido';
  if (!form.capacity || Number(form.capacity) < 1) errors.capacity = 'La capacidad debe ser mayor a 0';
  if (!form.pricePerHour || Number(form.pricePerHour) < 0) errors.pricePerHour = 'El precio por hora es requerido';
  const dep = Number(form.depositPercentage);
  if (isNaN(dep) || dep < 0 || dep > 100) errors.depositPercentage = 'Debe ser entre 0 y 100';
  return errors;
}

function fromWorkspace(w, companyId) {
  if (!w) return { ...EMPTY_FORM, companyId };
  return {
    companyId: w.companyId ?? companyId,
    name: w.name ?? '',
    description: w.description ?? '',
    address: w.address ?? '',
    city: w.city ?? '',
    country: w.country ?? '',
    latitude: w.latitude ?? '',
    longitude: w.longitude ?? '',
    capacity: w.capacity ?? '',
    pricePerHour: w.pricePerHour ?? '',
    pricePerDay: w.pricePerDay ?? '',
    pricePerMonth: w.pricePerMonth ?? '',
    depositPercentage: '30',
    amenities: w.amenities ?? [],
    images: [],
  };
}

export default function OfficeModal({ isOpen, onClose, companyId = '', initialValues = null }) {
  const isEditing = !!initialValues;
  const [form, setForm] = useState(() => fromWorkspace(initialValues, companyId));

  useEffect(() => {
    if (isOpen) setForm(fromWorkspace(initialValues, companyId));
  }, [isOpen, initialValues, companyId]);
  const [errors, setErrors] = useState({});

  const [rollback, setRollback] = useState(null);

  const { mutate, isPending, isError, error } = useCreateOffice({
    onSuccess: () => {
      setForm({ ...EMPTY_FORM, companyId });
      setErrors({});
      setRollback(null);
      onClose();
    },
    onError: () => {
      rollback?.();
      setRollback(null);
    },
  });

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function handleImagesChange(images) {
    setForm((prev) => ({ ...prev, images }));
  }

  function handleLocationSelect(result) {
    setForm((prev) => ({
      ...prev,
      address: result.displayName,
      city: result.city ?? prev.city,
      country: result.country ?? prev.country,
      latitude: result.latitude,
      longitude: result.longitude,
    }));
  }

  function handleAmenityToggle(key) {
    setForm((prev) => ({
      ...prev,
      amenities: prev.amenities.includes(key)
        ? prev.amenities.filter((a) => a !== key)
        : [...prev.amenities, key],
    }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const errs = validate(form);
    if (Object.keys(errs).length) { setErrors(errs); return; }

    const imageUrls = form.images.length
      ? await Promise.all(form.images.map((f) => uploadImage(f.file)))
      : [];

    setRollback(() => () => {
      imageUrls.forEach((url) => {
        try {
          const parts = new URL(url).pathname.split('/').slice(-2);
          if (parts.length === 2) deleteImage(parts[0], parts[1]);
        } catch { /* ignorar */ }
      });
    });

    mutate({
      companyId: form.companyId.trim(),
      name: form.name.trim(),
      description: form.description.trim() || undefined,
      address: form.address.trim(),
      city: form.city.trim(),
      country: form.country.trim(),
      latitude: form.latitude ? Number(form.latitude) : undefined,
      longitude: form.longitude ? Number(form.longitude) : undefined,
      capacity: Number(form.capacity),
      pricePerHour: Number(form.pricePerHour),
      pricePerDay: form.pricePerDay ? Number(form.pricePerDay) : undefined,
      pricePerMonth: form.pricePerMonth ? Number(form.pricePerMonth) : undefined,
      amenities: form.amenities,
      images: imageUrls,
    });
  }

  function handleClose() {
    if (!isPending) {
      setForm({ ...EMPTY_FORM, companyId });
      setErrors({});
      onClose();
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={isEditing ? 'Editar Espacio' : 'Nuevo Espacio'}>
      <OfficeForm
        form={form}
        onChange={handleChange}
        onAmenityToggle={handleAmenityToggle}
        onImagesChange={handleImagesChange}
        onLocationSelect={handleLocationSelect}
        onSubmit={handleSubmit}
        onCancel={handleClose}
        errors={errors}
        isPending={isPending}
        isError={isError}
        errorMessage={error?.response?.data?.detail}
      />
    </Modal>
  );
}
