import { Link, useNavigate } from 'react-router-dom';
import { User, Building2, ArrowRight } from 'lucide-react';
import { useState, useRef, useEffect } from 'react';

function RegisterType() {
    const navigate = useNavigate();
    const [hovered, setHovered] = useState(null);
    const timeoutRef = useRef(null);

    useEffect(() => {
        return () => {
            if (timeoutRef.current) clearTimeout(timeoutRef.current);
        };
    }, []);

    const handleMouseEnter = (type) => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        setHovered(type);
    };

    const handleMouseLeave = () => {
        timeoutRef.current = setTimeout(() => {
            setHovered(null);
        }, 200);
    };

    const handleFocus = (type) => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        setHovered(type);
    };

    const handleBlur = () => {
        timeoutRef.current = setTimeout(() => {
            setHovered(null);
        }, 200);
    };

    return (
    <div className="auth-page">
        <div className="auth-card reg-type-card">
        <aside className="reg-type__aside">
            <p className="reg-type__brand">DeskMatch</p>
            <h2 className="reg-type__title">Creá tu cuenta y empezá hoy</h2>
            <p className="reg-type__copy">Elegí el tipo de cuenta según lo que necesitás hacer en la plataforma.</p>
        </aside>

        <div className="reg-type__body">
            <div className="reg-type__header">
            <p className="reg-type__label">Registro fácil</p>
            <h1 className="reg-type__heading">¿Cómo querés registrarte?</h1>
            <p className="reg-type__subtitle">Seleccioná el tipo de cuenta que mejor te describe.</p>
            </div>

            <div className="reg-type__options">
            <Link
                to="/register/user"
                className={`reg-type__option ${hovered === 'user' ? 'reg-type__option--active' : ''}`}
                onMouseEnter={() => handleMouseEnter('user')}
                onMouseLeave={handleMouseLeave}
                onFocus={() => handleFocus('user')}
                onBlur={handleBlur}
                onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        navigate('/register/user');
                    }
                }}
                aria-label="Registrarme como profesional"
            >
                <div className="reg-type__option-icon">
                <User size={22} />
                </div>
                <div className="reg-type__option-content">
                <h3>Busco espacios para trabajar</h3>
                <p>Soy un profesional</p>
                </div>
                <ArrowRight size={20} className="reg-type__option-arrow" />
            </Link>

            <Link
                to="/register/company"
                className={`reg-type__option reg-type__option--featured ${hovered === 'company' ? 'reg-type__option--active' : ''}`}
                onMouseEnter={() => handleMouseEnter('company')}
                onMouseLeave={handleMouseLeave}
                onFocus={() => handleFocus('company')}
                onBlur={handleBlur}
                onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        navigate('/register/company');
                    }
                }}
                aria-label="Registrarme como empresa"
            >
                <div className="reg-type__option-icon">
                <Building2 size={22} />
                </div>
                <div className="reg-type__option-content">
                <h3>Tengo espacios para publicar</h3>
                <p>Soy una empresa</p>
                </div>
                <ArrowRight size={20} className="reg-type__option-arrow" />
            </Link>
            </div>

            <div className={`reg-type__preview ${hovered ? 'reg-type__preview--active' : ''}`} aria-live="polite">
                <div className={`reg-type__preview-content ${hovered === 'user' ? 'reg-type__preview-content--visible' : ''}`}>
                    <strong>Como profesional:</strong>
                    <p>Buscá y reservá espacios por hora o día, guardá tus favoritos y accedé a ofertas exclusivas.</p>
                </div>
                <div className={`reg-type__preview-content ${hovered === 'company' ? 'reg-type__preview-content--visible' : ''}`}>
                    <strong>Como empresa:</strong>
                    <p>Publicá tus espacios, gestioná reservas y recibí solicitudes desde el panel administrativo.</p>
                </div>
            </div>

                <p className="reg-type__foot">¿Ya tenés cuenta? <Link to="/login" className="auth-link">Iniciá sesión</Link></p>
        </div>
        </div>
    </div>
    );
}

export default RegisterType;