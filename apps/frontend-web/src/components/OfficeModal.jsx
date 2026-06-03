import { useState } from 'react';
import Modal from './Modal';
import OfficeForm from './OfficeForm';
import { useCreateOffice } from '../hooks/useCreateOffice';

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
  images: '',
};

function validate(form) {
  const errors = {};
  if (!form.companyId.trim()) errors.companyId = 'La empresa es requerida';
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

export default function OfficeModal({ isOpen, onClose }) {
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});

  const { mutate, isPending, isError, error } = useCreateOffice({
    onSuccess: () => {
      setForm(EMPTY_FORM);
      setErrors({});
      onClose();
    },
  });

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: '' }));
  }

  function handleAmenityToggle(key) {
    setForm((prev) => ({
      ...prev,
      amenities: prev.amenities.includes(key)
        ? prev.amenities.filter((a) => a !== key)
        : [...prev.amenities, key],
    }));
  }

  function handleSubmit(e) {
    e.preventDefault();
    const errs = validate(form);
    if (Object.keys(errs).length) { setErrors(errs); return; }

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
      depositPercentage: Number(form.depositPercentage),
      amenities: form.amenities,
      images: form.images.map((f) => f.url).filter(Boolean),
    });
  }

  function handleClose() {
    if (!isPending) {
      setForm(EMPTY_FORM);
      setErrors({});
      onClose();
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Nuevo Espacio">
      <OfficeForm
        form={form}
        onChange={handleChange}
        onAmenityToggle={handleAmenityToggle}
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
