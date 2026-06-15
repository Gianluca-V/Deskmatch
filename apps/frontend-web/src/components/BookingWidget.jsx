import { useState, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Calendar, Clock, AlertCircle, Loader2, CheckCircle2 } from 'lucide-react';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import { useCreateReservation } from '../hooks/useCreateReservation';
import './BookingWidget.css';

function roundToNext30(date) {
  const d = new Date(date);
  const m = d.getMinutes();
  const extra = m === 0 ? 0 : 30 - (m % 30);
  d.setMinutes(m + extra, 0, 0);
  return d;
}

function getMinTimeForDate(date) {
  const now = new Date();
  const isToday =
    date.getDate() === now.getDate() &&
    date.getMonth() === now.getMonth() &&
    date.getFullYear() === now.getFullYear();
  if (isToday) {
    const min = new Date(now);
    min.setMinutes(min.getMinutes() + 30, 0, 0);
    return min;
  }
  const dayStart = new Date(date);
  dayStart.setHours(0, 0, 0, 0);
  return dayStart;
}

function getMaxTime(date) {
  const d = new Date(date);
  d.setHours(23, 30, 0, 0);
  return d;
}

export default function BookingWidget({ workspace }) {
  const navigate = useNavigate();

  const defaultStart = useMemo(() => {
    const d = new Date();
    d.setHours(d.getHours() + 1);
    return roundToNext30(d);
  }, []);

  const defaultEnd = useMemo(() => {
    const d = new Date(defaultStart);
    d.setHours(d.getHours() + 1);
    return d;
  }, [defaultStart]);

  const [startDate, setStartDate] = useState(defaultStart);
  const [endDate, setEndDate] = useState(defaultEnd);
  const [endError, setEndError] = useState(null);
  const [conflictError, setConflictError] = useState(false);
  const [succeeded, setSucceeded] = useState(false);

  const durationHours = useMemo(() => {
    if (!startDate || !endDate) return 0;
    const diff = (endDate - startDate) / 3600000;
    return diff > 0 ? diff : 0;
  }, [startDate, endDate]);

  const estimatedPrice = useMemo(() => {
    if (!durationHours) return null;
    if (workspace.pricePerDay && durationHours >= 8) {
      const days = Math.ceil(durationHours / 24);
      return {
        amount: workspace.pricePerDay * days,
        unit: `${days} día${days > 1 ? 's' : ''}`,
      };
    }
    return {
      amount: workspace.pricePerHour * durationHours,
      unit: `${durationHours.toFixed(1)}h`,
    };
  }, [durationHours, workspace]);

  const isValid = useMemo(() => {
    if (!startDate || !endDate) return false;
    return startDate > new Date() && endDate > startDate && durationHours >= 1 && !endError;
  }, [startDate, endDate, durationHours, endError]);

  const validateEnd = useCallback(
    (end) => {
      if (!end || !startDate) return;
      if (end <= startDate) {
        setEndError('La fecha de fin debe ser posterior a la de inicio.');
        return;
      }
      const hours = (end - startDate) / 3600000;
      if (hours < 1) {
        setEndError('La duración mínima es de 1 hora.');
        return;
      }
      setEndError(null);
    },
    [startDate],
  );

  function handleStartChange(date) {
    setStartDate(date);
    setConflictError(false);
    if (date && endDate && date >= endDate) {
      const newEnd = new Date(date);
      newEnd.setHours(newEnd.getHours() + 1);
      setEndDate(newEnd);
      setEndError(null);
    } else {
      validateEnd(endDate);
    }
  }

  function handleEndChange(date) {
    setEndDate(date);
    setConflictError(false);
    validateEnd(date);
  }

  const { mutate, isPending } = useCreateReservation({
    onSuccess: () => {
      setSucceeded(true);
      toast.success('¡Reserva confirmada! Redirigiendo a tus reservas...');
      setTimeout(() => navigate('/reservations'), 1800);
    },
    onError: (err) => {
      const status = err?.response?.status;
      if (status === 409) {
        setConflictError(true);
        toast.error(
          'Este espacio no está disponible en las fechas seleccionadas o acaba de ser reservado.',
          { autoClose: 7000 },
        );
      } else if (status === 403) {
        toast.error('No podés reservar un espacio de tu propia empresa.');
      } else if (status === 400) {
        const detail = err?.response?.data?.detail || 'Las fechas ingresadas no son válidas.';
        toast.error(detail);
      } else {
        toast.error('Ocurrió un error al procesar la reserva. Intentá de nuevo.');
      }
    },
  });

  function handleSubmit(e) {
    e.preventDefault();
    if (!isValid) return;
    setConflictError(false);
    mutate({
      workspaceId: workspace.id,
      dto: {
        startTime: startDate.toISOString(),
        endTime: endDate.toISOString(),
      },
    });
  }

  const today = useMemo(() => {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d;
  }, []);

  if (succeeded) {
    return (
      <div className="booking-widget booking-widget--success">
        <CheckCircle2 size={48} className="booking-widget__success-icon" />
        <h3 className="booking-widget__success-title">¡Reserva confirmada!</h3>
        <p className="booking-widget__success-sub">Redirigiendo a Mis Reservas...</p>
      </div>
    );
  }

  return (
    <div className="booking-widget">
      <div className="booking-widget__header">
        <div className="booking-widget__price-main">
          <span className="booking-widget__price-amount">
            ${workspace.pricePerHour?.toFixed(2)}
          </span>
          <span className="booking-widget__price-unit">/ hora</span>
        </div>
        {workspace.pricePerDay && (
          <p className="booking-widget__price-alt">
            o ${workspace.pricePerDay?.toFixed(2)} / día completo
          </p>
        )}
      </div>

      <form className="booking-widget__form" onSubmit={handleSubmit} noValidate>

        <div className="booking-widget__field">
          <label className="booking-widget__label">
            <Calendar size={14} />
            Inicio
          </label>
          <DatePicker
            selected={startDate}
            onChange={handleStartChange}
            showTimeSelect
            timeFormat="HH:mm"
            timeIntervals={30}
            dateFormat="dd/MM/yyyy HH:mm"
            minDate={today}
            minTime={getMinTimeForDate(startDate)}
            maxTime={getMaxTime(startDate)}
            placeholderText="Seleccioná fecha y hora de inicio"
            className="booking-widget__input"
            wrapperClassName="booking-widget__picker-wrapper"
            popperPlacement="bottom-start"
            required
          />
        </div>

        <div className="booking-widget__field">
          <label className="booking-widget__label">
            <Clock size={14} />
            Fin
          </label>
          <DatePicker
            selected={endDate}
            onChange={handleEndChange}
            onCalendarClose={() => validateEnd(endDate)}
            showTimeSelect
            timeFormat="HH:mm"
            timeIntervals={30}
            dateFormat="dd/MM/yyyy HH:mm"
            minDate={startDate ?? today}
            minTime={startDate ? new Date(startDate.getTime() + 3600000) : getMinTimeForDate(endDate ?? today)}
            maxTime={getMaxTime(endDate ?? today)}
            placeholderText="Seleccioná fecha y hora de fin"
            className={`booking-widget__input${endError ? ' booking-widget__input--error' : ''}`}
            wrapperClassName="booking-widget__picker-wrapper"
            popperPlacement="bottom-start"
            required
          />
          {endError && (
            <p className="booking-widget__field-error">{endError}</p>
          )}
        </div>

        {estimatedPrice && durationHours >= 1 && (
          <div className="booking-widget__summary">
            <div className="booking-widget__summary-row">
              <span>{estimatedPrice.unit}</span>
              <span>${estimatedPrice.amount.toFixed(2)}</span>
            </div>
            <div className="booking-widget__summary-total">
              <span>Total estimado</span>
              <strong>${estimatedPrice.amount.toFixed(2)}</strong>
            </div>
            <p className="booking-widget__hint">El precio final lo calcula el servidor.</p>
          </div>
        )}

        {conflictError && (
          <div className="booking-widget__conflict">
            <AlertCircle size={16} />
            <span>
              Este espacio no está disponible en las fechas seleccionadas o acaba de ser reservado.
              Seleccioná otro período.
            </span>
          </div>
        )}

        <button
          type="submit"
          className="booking-widget__btn"
          disabled={!isValid || isPending}
        >
          {isPending ? (
            <>
              <Loader2 size={16} className="booking-widget__spinner" />
              Procesando...
            </>
          ) : (
            'Reservar Ahora'
          )}
        </button>
      </form>

      <p className="booking-widget__disclaimer">
        No se te cobrará hasta confirmar con el host.
      </p>
    </div>
  );
}
