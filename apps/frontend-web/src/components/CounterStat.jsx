import { useState, useEffect, useRef, useCallback } from 'react';

function CounterStat({ targetValue, label, suffix = '' }) {
  const [currentValue, setCurrentValue] = useState(0);
  const elementRef = useRef(null);
  const hasAnimatedRef = useRef(false);

    useEffect(() => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && !hasAnimatedRef.current) {
            hasAnimatedRef.current = true;
            animateCounter();
            observer.disconnect();
          }
        },
        { threshold: 0.5 }
      );

      if (elementRef.current) {
        observer.observe(elementRef.current);
      }

      return () => observer.disconnect();
    }, [animateCounter]);

    const animateCounter = useCallback(() => {
    const numericValue = parseFloat(targetValue.toString().replace(/[^0-9.]/g, ''));
    const duration = 2000;
    const steps = 60;
    const stepDuration = duration / steps;

    let currentStep = 0;

    const interval = setInterval(() => {
      currentStep++;
      const progress = currentStep / steps;
      const easeOutValue = 1 - Math.pow(1 - progress, 3);
      const newValue = numericValue * easeOutValue;

      if (currentStep >= steps) {
        setCurrentValue(numericValue);
        clearInterval(interval);
      } else {
        setCurrentValue(newValue);
      }
    }, stepDuration);
  }, [targetValue]);

  // Formatear el valor para mostrar
  const formatValue = (value) => {
    if (suffix === '★') {
      return value.toFixed(1) + suffix;
    }
    if (suffix === '+') {
      return Math.round(value).toLocaleString() + suffix;
    }
    return Math.round(value).toLocaleString();
  };

  return (
    <div className="home-stats__item" ref={elementRef}>
      <h3>{formatValue(currentValue)}</h3>
      <p>{label}</p>
    </div>
  );
}

export default CounterStat;
