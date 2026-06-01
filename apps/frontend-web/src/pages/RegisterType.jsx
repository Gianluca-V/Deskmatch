import { Link } from 'react-router-dom';
import { User, Building2, ArrowRight } from 'lucide-react';

function RegisterType() {
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
            <Link to="/register/user" className="reg-type__option">
                <div className="reg-type__option-icon">
                <User size={22} />
                </div>
                <div className="reg-type__option-content">
                <h3>Soy un profesional</h3>
                <p>Busco espacios para trabajar</p>
                </div>
                <ArrowRight size={20} className="reg-type__option-arrow" />
            </Link>

            <Link to="/register/company" className="reg-type__option reg-type__option--featured">
                <div className="reg-type__option-icon">
                <Building2 size={22} />
                </div>
                <div className="reg-type__option-content">
                <h3>Soy una empresa</h3>
                <p>Tengo espacios para publicar</p>
                </div>
                <ArrowRight size={20} className="reg-type__option-arrow" />
            </Link>
            </div>

                <p className="reg-type__foot">¿Ya tenés cuenta? <Link to="/login" className="auth-link">Iniciá sesión</Link></p>
        </div>
        </div>
    </div>
    );
}

export default RegisterType;