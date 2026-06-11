import React from 'react';
import { useForm } from 'react-hook-form';
import { useProfileCompany } from '../hooks/useProfile';
import { X, Loader } from 'lucide-react';
import { toast } from 'react-toastify';
import { useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import './EditCompanyProfileModal.css';

function EditCompanyProfileModal({ isOpen, onClose, onSuccess }) {
  const queryClient = useQueryClient();
  const { data: companyData, refetch } = useProfileCompany();
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm({
    defaultValues: {
      name: companyData?.name || '',
      description: companyData?.description || '',
      contactEmail: companyData?.contactEmail || '',
      websiteUrl: companyData?.websiteUrl || '',
      phoneNumber: companyData?.phoneNumber || '',
      location: companyData?.location || '',
    },
  });

  React.useEffect(() => {
    if (companyData) {
      reset({
        name: companyData?.name || '',
        description: companyData?.description || '',
        contactEmail: companyData?.contactEmail || '',
        websiteUrl: companyData?.websiteUrl || '',
        phoneNumber: companyData?.phoneNumber || '',
        location: companyData?.location || '',
      });
    }
  }, [companyData, reset]);

  const onSubmit = async (data) => {
    try {
      const payload = {
        name: data.name.trim(),
        description: data.description?.trim() || '',
        contactEmail: data.contactEmail.trim(),
        websiteUrl: data.websiteUrl?.trim() || '',
        phoneNumber: data.phoneNumber?.trim() || '',
        location: data.location?.trim() || '',
      };

      console.log('Enviando datos:', payload);
      await api.put('/api/companies/me/profile', payload);
      toast.success('Perfil de la empresa actualizado exitosamente');
      // Invalidar el caché de React Query para forzar refetch
      await queryClient.invalidateQueries({ queryKey: ['profile-company'] });
      onSuccess?.();
      onClose();
    } catch (error) {
      const errorMessage = error.response?.data?.detail || 'Error al actualizar el perfil de la empresa';
      toast.error(errorMessage);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="edit-company-modal__overlay" onClick={onClose}>
      <div className="edit-company-modal" onClick={(e) => e.stopPropagation()}>
        <div className="edit-company-modal__header">
          <h2>Editar Perfil de la Empresa</h2>
          <button className="edit-company-modal__close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="edit-company-modal__form">
          <div className="edit-company-modal__form-group">
            <label htmlFor="name">Nombre de la Empresa *</label>
            <input
              id="name"
              type="text"
              placeholder="Ej: Acme Corp"
              {...register('name', {
                required: 'El nombre de la empresa es requerido',
                minLength: { value: 3, message: 'Mínimo 3 caracteres' },
                maxLength: { value: 100, message: 'Máximo 100 caracteres' },
              })}
              className="edit-company-modal__input"
            />
            {errors.name && <span className="edit-company-modal__error">{errors.name.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label htmlFor="description">Descripción</label>
            <textarea
              id="description"
              placeholder="Descripción de tu empresa..."
              rows={4}
              {...register('description', {
                maxLength: { value: 500, message: 'Máximo 500 caracteres' },
              })}
              className="edit-company-modal__textarea"
            />
            {errors.description && <span className="edit-company-modal__error">{errors.description.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label htmlFor="contactEmail">Email de Contacto *</label>
            <input
              id="contactEmail"
              type="email"
              placeholder="contacto@empresa.com"
              {...register('contactEmail', {
                required: 'El email de contacto es requerido',
                pattern: {
                  value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                  message: 'Email inválido',
                },
              })}
              className="edit-company-modal__input"
            />
            {errors.contactEmail && <span className="edit-company-modal__error">{errors.contactEmail.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label htmlFor="websiteUrl">Sitio Web</label>
            <input
              id="websiteUrl"
              type="url"
              placeholder="https://ejemplo.com"
              {...register('websiteUrl', {
                pattern: {
                  value: /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/,
                  message: 'URL inválida',
                },
              })}
              className="edit-company-modal__input"
            />
            {errors.websiteUrl && <span className="edit-company-modal__error">{errors.websiteUrl.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label htmlFor="phoneNumber">Teléfono</label>
            <input
              id="phoneNumber"
              type="tel"
              placeholder="+34 912 345 678"
              {...register('phoneNumber', {
                maxLength: { value: 20, message: 'Máximo 20 caracteres' },
              })}
              className="edit-company-modal__input"
            />
            {errors.phoneNumber && <span className="edit-company-modal__error">{errors.phoneNumber.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label htmlFor="location">Ubicación</label>
            <input
              id="location"
              type="text"
              placeholder="Madrid, España"
              {...register('location', {
                maxLength: { value: 200, message: 'Máximo 200 caracteres' },
              })}
              className="edit-company-modal__input"
            />
            {errors.location && <span className="edit-company-modal__error">{errors.location.message}</span>}
          </div>

          <div className="edit-company-modal__form-group">
            <label>Verificación</label>
            <div className="edit-company-modal__verification-info">
              {companyData?.isVerified ? (
                <p className="edit-company-modal__verified">✓ Empresa verificada</p>
              ) : (
                <p className="edit-company-modal__not-verified">La empresa aún no está verificada</p>
              )}
            </div>
            <small className="edit-company-modal__help-text">El estado de verificación no puede ser modificado aquí</small>
          </div>

          <div className="edit-company-modal__actions">
            <button
              type="button"
              onClick={onClose}
              className="edit-company-modal__button edit-company-modal__button--secondary"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="edit-company-modal__button edit-company-modal__button--primary"
            >
              {isSubmitting ? (
                <>
                  <Loader size={18} className="spin" />
                  Guardando...
                </>
              ) : (
                'Guardar Cambios'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default EditCompanyProfileModal;
