# Navbar y UserMenu - Sistema de Navegación Profesional

## Descripción General

Se ha implementado un sistema de navegación profesional para DeskMatch con dos nuevos componentes:

1. **Navbar** - Barra de navegación principal
2. **UserMenu** - Menú desplegable del usuario (solo para usuarios autenticados)

## Componentes

### Navbar

**Ubicación**: `src/components/Navbar.jsx`

Barra de navegación principal que se adapta según el estado de autenticación del usuario.

**Estructura**:
```
[Logo]  [Opciones de menú]  [Acciones de usuario/Auth]
```

**Características**:
- Logo/Home clickeable al lado izquierdo
- Links de navegación centrados (solo para usuarios autenticados)
- Acciones de usuario al lado derecho
- Sticky (se queda fijo al scroll)
- Sombra y borde inferior
- Responsive design

**Estado No Autenticado**:
- Muestra: Logo + "Iniciar Sesión" + "Registrarse"

**Estado Autenticado**:
- Muestra: Logo + [Oficinas | Dashboard | Mis Espacios] + UserMenu

**Links**:
- `/offices` - Explorar oficinas disponibles
- `/dashboard` - Panel de control personal
- `/my-spaces` - Gestionar espacios propios

### UserMenu

**Ubicación**: `src/components/UserMenu.jsx`

Menú desplegable con información del usuario y acciones rápidas.

**Características**:
- Avatar circular con inicial del usuario
- Nombre y email del usuario
- Dropdown con opciones:
  - Mi Perfil (link a `/profile`)
  - Configuración (link a `/profile`)
  - Cerrar sesión (logout)
- Cierre automático al hacer clic fuera
- Animación suave de apertura/cierre
- Responsive (oculta nombre en mobile)

**Estructura del Dropdown**:
```
┌─────────────────────────┐
│ [Avatar] Nombre Completo│ ← Header con info
│         email@user.com  │
├─────────────────────────┤
│ 👤 Mi Perfil           │
│ ⚙️  Configuración       │
├─────────────────────────┤
│ 🚪 Cerrar sesión       │ ← Estilo rojo
└─────────────────────────┘
```

## Uso

### Acceso Automático

El componente Navbar se integra automáticamente en `App.jsx` y maneja todo el flujo de navegación:

```jsx
import Navbar from './components/Navbar';

function App() {
  return (
    <>
      <Navbar />  {/* Se muestra automáticamente */}
      <main>...</main>
    </>
  );
}
```

### UserMenu Independiente

El UserMenu se puede usar de forma independiente si es necesario:

```jsx
import UserMenu from './components/UserMenu';
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { isAuthenticated, user } = useAuth();
  
  if (!isAuthenticated) return null;
  
  return <UserMenu />;
}
```

## Estilos

### Colores Utilizados

- **Primary**: `var(--color-primary)` - Fondo de navbar
- **Surface**: `var(--color-surface)` - Fondo de dropdown
- **Text**: `var(--color-text)` - Texto principal
- **Muted**: `var(--color-muted)` - Texto secundario
- **Border**: `var(--color-border)` - Bordes

### Breakpoints Responsive

- **Desktop**: 1024px+
  - Navbar completa con todos los links
  - UserMenu con nombre visible

- **Tablet**: 768px - 1023px
  - Navbar completa
  - UserMenu compacto

- **Mobile**: < 640px
  - Links de navegación ocultos (solo para auth)
  - Avatar sin nombre
  - Menu dropdown más compacto
  - Botones más pequeños

## Comportamiento

### Navbar - No Autenticado
- Muestra: Logo + "Iniciar Sesión" + "Registrarse"
- Click en logo → `/`
- Click "Iniciar Sesión" → `/login`
- Click "Registrarse" → `/register`

### Navbar - Autenticado
- Muestra: Logo + [Oficinas | Dashboard | Mis Espacios] + UserMenu
- Links con hover effect (underline)
- UserMenu disponible al lado derecho

### UserMenu - Dropdown
- Se abre/cierra al click del trigger
- Se cierra automáticamente al:
  - Hacer clic fuera del dropdown
  - Seleccionar una opción
  - Hacer logout

## Iconos Utilizados (lucide-react)

En UserMenu:
- `User` - Para "Mi Perfil"
- `Settings` - Para "Configuración"
- `LogOut` - Para "Cerrar sesión"
- `ChevronDown` - Indicador de dropdown

## Datos del Usuario

El UserMenu obtiene los datos del usuario del `AuthContext`:

```javascript
{
  firstName: string,
  lastName: string,
  email: string
}
```

**Fallback**:
- Si no hay `firstName`: muestra "U" en el avatar
- Si no hay nombre completo: muestra email

## Archivos Modificados

**App.jsx**:
- Removida la navbar inline
- Importado componente Navbar
- Simplificado el código principal

## Archivos Creados

1. `Navbar.jsx` - Componente de navegación principal
2. `Navbar.css` - Estilos de navbar
3. `UserMenu.jsx` - Componente de menú de usuario
4. `UserMenu.css` - Estilos de menú de usuario

## Animaciones

### Navbar
- Hover en links: subrayado suave
- Hover en botones: cambio de opacidad

### UserMenu
- Apertura/cierre del dropdown: slide-in suave (150ms)
- Hover en items: cambio de background
- Hover en logout: color rojo
- Rotación del chevron (180deg)

## Accesibilidad

✅ Atributo `aria-expanded` en UserMenu trigger
✅ Etiqueta `aria-label` en botón UserMenu
✅ Estructura semántica con `<nav>`, `<header>`, `<main>`
✅ Contraste de colores adecuado
✅ Elementos clickeables suficientemente grandes (min 44px)

## Próximos Pasos (Opcional)

- [ ] Agregar foto de perfil real del usuario (avatar image)
- [ ] Agregar notificaciones en navbar
- [ ] Crear submenu para "Configuración" con más opciones
- [ ] Agregar búsqueda global en navbar
- [ ] Implementar menu mobile hamburguesa
- [ ] Agregar breadcrumbs en ciertas páginas
