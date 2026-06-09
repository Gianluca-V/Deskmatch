# Componentes de Perfil - DeskMatch Frontend

## Descripción General

Se han creado dos componentes React para mostrar la información del perfil del usuario en la página de Dashboard:

1. **GuestProfileCard** - Tarjeta para información personal del usuario
2. **CompanyProfileCard** - Tarjeta para información de la empresa asociada

## Componentes

### GuestProfileCard

**Ubicación**: `src/components/GuestProfileCard.jsx`

Tarjeta que muestra la información personal del usuario con los siguientes campos:
- Nombre completo (nombre + apellido)
- Correo electrónico
- Teléfono
- Ubicación

**Props**:
```javascript
{
  user: {
    firstName: string,
    lastName: string,
    email: string,
    phone: string,
    location: string
  },
  isLoading: boolean,
  error: Error | null
}
```

**Características**:
- Iconos de lucide-react (User, Mail, Phone, MapPin)
- Skeleton loader durante carga de datos
- Mensaje "No especificado" en gris para campos vacíos
- Mensaje de error amigable si falla la carga
- Diseño responsive (mobile-first)

### CompanyProfileCard

**Ubicación**: `src/components/CompanyProfileCard.jsx`

Tarjeta que muestra información de la empresa con los siguientes campos:
- Nombre de empresa
- Correo de contacto
- Teléfono
- Ubicación
- Badge de "Verificado" (si está verificada)

**Props**:
```javascript
{
  company: {
    name: string,
    email: string,
    phone: string,
    location: string,
    isVerified: boolean
  } | null,
  isLoading: boolean,
  error: Error | null
}
```

**Características**:
- Badge azul de verificación si `isVerified === true`
- Icono CheckCircle para indicar estado verificado
- Estado vacío amigable si el usuario no tiene empresa asociada
- Skeleton loader durante carga
- Mensaje de error personalizado

## Hooks Personalizados

### useProfile

**Ubicación**: `src/hooks/useProfile.js`

Hook para obtener el perfil del usuario actual.

```javascript
const { data: user, isLoading, error } = useProfile();
```

**Características**:
- Usa TanStack React Query para caching
- Reintentos automáticos (2 reintentos con 1 segundo de espera)
- Cache de 5 minutos
- Manejo de errores integrado

### useProfileCompany

**Ubicación**: `src/hooks/useProfile.js`

Hook para obtener la información de la empresa asociada.

```javascript
const { data: company, isLoading, error } = useProfileCompany();
```

**Características**:
- Retorna `null` si el usuario no tiene empresa (HTTP 404)
- Reintentos automáticos (1 reintento con 1 segundo de espera)
- Cache de 5 minutos
- Manejo gracioso de empresas no encontradas

## Página de Perfil

**Ubicación**: `src/pages/Profile.jsx`

Página completa que integra ambos componentes.

**Ruta**: `/profile` (protegida por autenticación)

**Características**:
- Grid responsive que se ajusta a diferentes tamaños de pantalla
- Muestra ambas tarjetas lado a lado (desktop) o apiladas (mobile)
- Manejo de estados de carga y error
- Estilos consistentes con el diseño del sistema

## Uso

### 1. Acceder a la página de perfil

```javascript
import { Link } from 'react-router-dom';

<Link to="/profile">Mi Perfil</Link>
```

### 2. Usar los componentes directamente

```javascript
import { useProfile, useProfileCompany } from '../hooks/useProfile';
import GuestProfileCard from '../components/GuestProfileCard';
import CompanyProfileCard from '../components/CompanyProfileCard';

function MyComponent() {
  const userQuery = useProfile();
  const companyQuery = useProfileCompany();

  return (
    <>
      <GuestProfileCard 
        user={userQuery.data}
        isLoading={userQuery.isLoading}
        error={userQuery.error}
      />
      <CompanyProfileCard 
        company={companyQuery.data}
        isLoading={companyQuery.isLoading}
        error={companyQuery.error}
      />
    </>
  );
}
```

## Estilos

Todos los componentes utilizan variables CSS del sistema de diseño:

- `--color-primary`: #3a95df (azul primario)
- `--color-surface`: #ffffff (fondo de tarjetas)
- `--color-text`: #030608 (texto principal)
- `--color-muted`: #475569 (texto secundario/deshabilitado)
- `--color-border`: #dbe0e8 (bordes)
- `--color-bg`: Color de fondo general

Los estilos son completamente responsive y optimizados para:
- Desktop (1024px+)
- Tablet (768px - 1023px)
- Mobile (< 768px)

## Endpoints de API Utilizados

- `GET /api/auth/profile` - Obtener información del usuario actual
- `GET /api/companies/me` - Obtener información de la empresa asociada

Ambos endpoints requieren autenticación Bearer token.

## Estados de Carga

### GuestProfileCard
- **Loading**: Muestra 3 líneas de skeleton animadas
- **Error**: Muestra mensaje rojo "Error al cargar perfil"
- **Exitoso**: Muestra tarjeta con datos del usuario

### CompanyProfileCard
- **Loading**: Muestra 3 líneas de skeleton animadas
- **Error**: Muestra mensaje rojo "Error al cargar información de empresa"
- **Sin empresa**: Muestra ícono de empresa gris + mensaje "Sin empresa asociada"
- **Exitoso**: Muestra tarjeta con datos de la empresa y badge de verificación (si aplica)

## Notas Importantes

✅ **Frontend-only** - No se realizaron cambios en el backend
✅ **Diseño pixel-perfect** - Estilos alineados con el sistema de diseño existente
✅ **Manejo robusto de errores** - Mensajes amigables para el usuario
✅ **Performance optimizado** - Cache de 5 minutos con reintentos automáticos
✅ **Accesibilidad** - Iconos con semántica clara y estructura HTML correcta
✅ **Responsive** - Optimizado para mobile, tablet y desktop
