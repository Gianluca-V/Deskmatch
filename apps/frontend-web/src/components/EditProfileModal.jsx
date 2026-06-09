import React from 'react';
import { useForm } from 'react-hook-form';
import { useAuth } from '../context/AuthContext';
import { useProfile } from '../hooks/useProfile';
import { X, Loader } from 'lucide-react';
import { toast } from 'react-toastify';
import api from '../lib/api';
import './EditProfileModal.css';

function EditProfileModal({ isOpen, onClose, onSuccess }) {
  const { user } = useAuth();
  const { data: profileData, refetch } = useProfile();
  const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm({
    defaultValues: {
      fullName: profileData?.firstName ? `${profileData.firstName} ${profileData.lastName || ''}`.trim() : '',
      phoneNumber: profileData?.phoneNumber || '',
      location: profileData?.location || '',
    },
  });

  React.useEffect(() => {
    if (profileData) {
      reset({
        fullName: profileData?.firstName ? `${profileData.firstName} ${profileData.lastName || ''}`.trim() : '',
        phoneNumber: profileData?.phoneNumber || '',
        location: profileData?.location || '',
      });
    }
  }, [profileData, reset]);

  const onSubmit = async (data) => {
    try {
      const [firstName, ...lastNameParts] = data.fullName.trim().split(' ');
      const lastName = lastNameParts.join(' ') || '';

      const payload = {
        fullName: data.fullName.trim(),
        phoneNumber: data.phoneNumber?.trim() || null,
        location: data.location?.trim() || null,
      };

      await api.put('/api/users/me/profile', payload);
      toast.success('Perfil actualizado exitosamente');
      refetch();
      onSuccess?.();
      onClose();
    } catch (error) {
      const errorMessage = error.response?.data?.detail || 'Error al actualizar el perfil';
      toast.error(errorMessage);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="edit-profile-modal__overlay" onClick={onClose}>
      <div className="edit-profile-modal" onClick={(e) => e.stopPropagation()}>
        <div className="edit-profile-modal__header">
          <h2>Editar Mi Perfil</h2>
          <button className="edit-profile-modal__close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="edit-profile-modal__form">
          <div className="edit-profile-modal__form-group">
            <label>Correo Electrónico</label>
            <input
              type="email"
              value={user?.email}
              disabled
              className="edit-profile-modal__input edit-profile-modal__input--disabled"
            />
            <small className="edit-profile-modal__help-text">El correo no puede ser modificado</small>
          </div>

          <div className="edit-profile-modal__form-group">
            <label htmlFor="fullName">Nombre Completo *</label>
            <input
              id="fullName"
              type="text"
              placeholder="Ej: Juan Pérez"
              {...register('fullName', {
                required: 'El nombre completo es requerido',
                minLength: { value: 3, message: 'Mínimo 3 caracteres' },
                maxLength: { value: 100, message: 'Máximo 100 caracteres' },
              })}
              className="edit-profile-modal__input"
            />
            {errors.fullName && <span className="edit-profile-modal__error">{errors.fullName.message}</span>}
          </div>

          <div className="edit-profile-modal__form-group">
            <label htmlFor="phoneNumber">Teléfono</label>
            <input
              id="phoneNumber"
              type="tel"
              placeholder="+54 9 11 1234 5678"
              {...register('phoneNumber', {
                pattern: {
                  value: /^[+]?[(]?[0-9]{1,4}[)]?[-\s.]?[0-9]{1,9}$/,
                  message: 'Teléfono inválido',
                },
                maxLength: { value: 20, message: 'Máximo 20 caracteres' },
              })}
              className="edit-profile-modal__input"
            />
            {errors.phoneNumber && <span className="edit-profile-modal__error">{errors.phoneNumber.message}</span>}
          </div>

          <div className="edit-profile-modal__form-group">
            <label htmlFor="location">Ubicación</label>
            <input
              id="location"
              type="text"
              placeholder="Ej: Buenos Aires, Argentina"
              {...register('location', {
                maxLength: { value: 100, message: 'Máximo 100 caracteres' },
              })}
              className="edit-profile-modal__input"
            />
            {errors.location && <span className="edit-profile-modal__error">{errors.location.message}</span>}
          </div>

          <div className="edit-profile-modal__actions">
            <button
              type="button"
              onClick={onClose}
              className="edit-profile-modal__button edit-profile-modal__button--secondary"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="edit-profile-modal__button edit-profile-modal__button--primary"
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

export default EditProfileModal;
