import { Link } from 'react-router-dom';
import { Search, MapPin, Calendar, Users, Zap, Shield, ArrowRight, Star, ChevronLeft, ChevronRight } from 'lucide-react';
import { useState } from 'react';

function Home() {
  const [searchQuery, setSearchQuery] = useState('');
  const [testimonialPage, setTestimonialPage] = useState(0);

  const testimonials = [
    {
      name: 'María García',
      role: 'Emprendedora',
      text: 'DeskMatch cambió cómo busco espacios. Ahora encuentro lo que necesito en minutos.',
      initials: 'MG'
    },
    {
      name: 'Carlos Rodríguez',
      role: 'Community Manager',
      text: 'Con DeskMatch gestionamos nuestros espacios de forma mucho más fácil y profesional.',
      initials: 'CR'
    },
    {
      name: 'Sofia Martínez',
      role: 'Freelancer',
      text: 'La mejor plataforma para encontrar mi próximo lugar de trabajo. Súper recomendado.',
      initials: 'SM'
    },
    {
      name: 'Juan López',
      role: 'Developer',
      text: 'Perfecto para trabajar desde diferentes lugares. La interfaz es intuitiva y rápida.',
      initials: 'JL'
    },
    {
      name: 'Andrea Sánchez',
      role: 'Diseñadora Gráfica',
      text: 'Encontré el espacio ideal para mi estudio. Proceso de reserva muy sencillo.',
      initials: 'AS'
    },
    {
      name: 'Diego Fernández',
      role: 'Gestor Empresarial',
      text: 'Excelente herramienta para maximizar el uso de nuestros espacios ociosos.',
      initials: 'DF'
    }
  ];

  const testimonialsPerPage = 3;
  const totalPages = testimonials.length - testimonialsPerPage + 1;

  const handlePrevTestimonials = () => {
    setTestimonialPage((prev) => (prev === 0 ? totalPages - 1 : prev - 1));
  };

  const handleNextTestimonials = () => {
    setTestimonialPage((prev) => (prev === totalPages - 1 ? 0 : prev + 1));
  };

  const visibleTestimonials = testimonials.slice(
    testimonialPage,
    testimonialPage + testimonialsPerPage
  );

  const features = [
    {
      icon: Search,
      title: 'Búsqueda inteligente',
      description: 'Encuentra espacios disponibles según tus necesidades y presupuesto'
    },
    {
      icon: MapPin,
      title: 'Ubicación perfecta',
      description: 'Filtra por zona, transporte público y servicios cercanos'
    },
    {
      icon: Calendar,
      title: 'Reservas flexibles',
      description: 'Por hora, día, semana o mes. Elige lo que mejor te convenga'
    },
    {
      icon: Shield,
      title: 'Seguro y confiable',
      description: 'Pagos protegidos y verificación de espacios y usuarios'
    },
    {
      icon: Zap,
      title: 'Gestión simple',
      description: 'Panel administrativo intuitivo para empresas y profesionales'
    },
    {
      icon: Users,
      title: 'Comunidad activa',
      description: 'Conecta con otros profesionales y expande tu red de contactos'
    }
  ];

  const steps = [
    {
      number: '01',
      title: 'Crea tu cuenta',
      description: 'Regístrate en segundos como profesional o empresa'
    },
    {
      number: '02',
      title: 'Busca o publica',
      description: 'Explora espacios o comparte los tuyos con la comunidad'
    },
    {
      number: '03',
      title: 'Reserva y disfruta',
      description: 'Confirma tu reserva y comienza a trabajar en tu nuevo espacio'
    }
  ];

  return (
    <div className="home">
      {/* Header with Auth Buttons */}
      <header className="home-header">
        <Link to="/" className="home-logo">DeskMatch</Link>
        <div className="home-header__actions">
          <Link to="/login" className="home-header__link">Iniciar sesión</Link>
          <Link to="/register" className="home-header__cta">Registrarse</Link>
        </div>
      </header>

      {/* Hero Section */}
      <section className="home-hero">
        <div className="home-hero__content">
          <div className="home-hero__badge">Flexible • Seguro • Accesible</div>
          <h1 className="home-hero__title">
            Encuentra el espacio perfecto para trabajar
          </h1>
          <p className="home-hero__subtitle">
            Conectamos profesionales con espacios de trabajo flexibles. Reserva por hora, día o mes. Simple, rápido y confiable.
          </p>

          {/* Search Bar */}
          <div className="home-search">
            <div className="home-search__input-group">
              <MapPin size={20} className="home-search__icon" />
              <input
                type="text"
                placeholder="¿Dónde quieres trabajar?"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="home-search__input"
              />
            </div>
            <Link to="/offices" className="home-search__button">
              <Search size={18} />
              Buscar
            </Link>
          </div>

          {/* CTAs */}
          <div className="home-hero__ctas">
            <Link to="/register" className="btn btn-primary home-hero__cta">
              Comenzar ahora
              <ArrowRight size={18} />
            </Link>
            <Link to="/offices" className="btn btn-secondary home-hero__cta">
              Explorar espacios
            </Link>
          </div>
        </div>

        <div className="home-hero__visual">
          <div className="home-hero__card home-hero__card--1">
            <div className="home-hero__card-icon">🏢</div>
            <p>Oficina en Palermo</p>
            <span>$56/hora</span>
          </div>
          <div className="home-hero__card home-hero__card--2">
            <div className="home-hero__card-icon">☕</div>
            <p>Café con escritorio</p>
            <span>$150/día</span>
          </div>
          <div className="home-hero__card home-hero__card--3">
            <div className="home-hero__card-icon">🎓</div>
            <p>Sala de reuniones</p>
            <span>$44/hora</span>
          </div>
          <div className="home-hero__card home-hero__card--4">
            <div className="home-hero__card-icon">🖥️</div>
            <p>Estudio compartido</p>
            <span>$280/día</span>
          </div>
          <div className="home-hero__card home-hero__card--5">
            <div className="home-hero__card-icon">💼</div>
            <p>Escritorio privado</p>
            <span>$40/hora</span>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="home-stats">
        <div className="home-stats__item">
          <h3>500+</h3>
          <p>Espacios disponibles</p>
        </div>
        <div className="home-stats__item">
          <h3>2,000+</h3>
          <p>Usuarios activos</p>
        </div>
        <div className="home-stats__item">
          <h3>50+</h3>
          <p>Ciudades</p>
        </div>
        <div className="home-stats__item">
          <h3>4.9★</h3>
          <p>Calificación promedio</p>
        </div>
      </section>

      {/* Features Grid */}
      <section className="home-section">
        <div className="home-section__header">
          <h2>¿Por qué elegir DeskMatch?</h2>
          <p>Todo lo que necesitas para encontrar tu próximo espacio de trabajo</p>
        </div>
        <div className="home-features">
          {features.map((feature, idx) => {
            const Icon = feature.icon;
            return (
              <div key={idx} className="home-feature">
                <div className="home-feature__icon">
                  <Icon size={28} />
                </div>
                <h3>{feature.title}</h3>
                <p>{feature.description}</p>
              </div>
            );
          })}
        </div>
      </section>

      {/* How it Works */}
      <section className="home-section home-how">
        <div className="home-section__header">
          <h2>Cómo funciona</h2>
          <p>Tres simples pasos para empezar</p>
        </div>
        <div className="home-steps">
          {steps.map((step, idx) => (
            <div key={idx} className="home-step">
              <div className="home-step__number">{step.number}</div>
              <h3>{step.title}</h3>
              <p>{step.description}</p>
              {idx < steps.length - 1 && <div className="home-step__arrow">→</div>}
            </div>
          ))}
        </div>
      </section>

      {/* Testimonials */}
      <section className="home-section home-testimonials">
        <div className="home-section__header">
          <h2>Lo que dicen nuestros usuarios</h2>
          <p>Historias reales de profesionales y empresas</p>
        </div>
        
        <div className="home-testimonials__wrapper">
          <button 
            className="home-testimonials__arrow home-testimonials__arrow--prev"
            onClick={handlePrevTestimonials}
            aria-label="Testimonios anteriores"
          >
            <ChevronLeft size={24} />
          </button>

          <div className="home-testimonials__carousel">
            {visibleTestimonials.map((testimonial, idx) => (
              <div key={idx} className="home-testimonial">
                <div className="home-testimonial__avatar">{testimonial.initials}</div>
                <div className="home-testimonial__stars">
                  {[...Array(5)].map((_, i) => (
                    <Star key={i} size={16} fill="currentColor" />
                  ))}
                </div>
                <p className="home-testimonial__text">"{testimonial.text}"</p>
                <p className="home-testimonial__author">
                  <strong>{testimonial.name}</strong>
                  <br />
                  <span>{testimonial.role}</span>
                </p>
              </div>
            ))}
          </div>

          <button 
            className="home-testimonials__arrow home-testimonials__arrow--next"
            onClick={handleNextTestimonials}
            aria-label="Testimonios siguientes"
          >
            <ChevronRight size={24} />
          </button>
        </div>

        <div className="home-testimonials__dots">
          {[...Array(totalPages)].map((_, idx) => (
            <button
              key={idx}
              className={`home-testimonials__dot ${idx === testimonialPage ? 'home-testimonials__dot--active' : ''}`}
              onClick={() => setTestimonialPage(idx)}
              aria-label={`Ver grupo ${idx + 1}`}
            />
          ))}
        </div>
      </section>

      {/* Final CTA */}
      <section className="home-cta-final">
        <h2>¿Listo para encontrar tu espacio ideal?</h2>
        <p>Únete a miles de profesionales y empresas que ya confían en DeskMatch</p>
        <div className="home-cta-final__actions">
          <Link to="/register" className="btn btn-primary btn-large">
            Crear cuenta gratis
          </Link>
          <Link to="/offices" className="btn btn-secondary btn-large">
            Ver espacios disponibles
          </Link>
        </div>
      </section>
    </div>
  );
}

export default Home;
