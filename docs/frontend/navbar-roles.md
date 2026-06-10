# Navbar Adaptable según Rol de Usuario

## Descripción General

La barra de navegación se adapta automáticamente según el tipo de usuario (Usuario Regular o Empresa/Admin). Esto garantiza que cada tipo de usuario vea solo las opciones relevantes para su rol.

## Estructura Adaptable

El componente `Navbar` verifica automáticamente el rol del usuario y muestra diferentes menús:

```javascript
const isCompany = user?.role === 'Company' || user?.role === 'Admin';
```

## Versión 1: Usuario Regular (Rol: "User")

**Caso de uso**: Usuarios individuales buscando espacios de oficina

**Navbar mostrada**:
```
┌─────────────────────────────────────────────────────────┐
│ DeskMatch  │  Oficinas | Dashboard | Mis Espacios  │ 👤 │
└─────────────────────────────────────────────────────────┘
```

**Links disponibles**:
- 🏢 **Oficinas** → `/offices` - Explorar espacios disponibles
- 📊 **Dashboard** → `/dashboard` - Ver resumen de actividad
- 📦 **Mis Espacios** → `/my-spaces` - Mis reservas/espacios alquilados

**UserMenu Badge**: 👤 USUARIO

### Flujo de Usuario Regular

1. Ingresa y ve "Oficinas" para buscar espacios
2. Va a "Dashboard" para ver su actividad
3. En "Mis Espacios" gestiona sus reservas
4. En "Mi Perfil" actualiza su información personal

---

## Versión 2: Empresa/Admin (Rol: "Company" o "Admin")

**Caso de uso**: Empresas publicando y gestionando espacios de oficina

**Navbar mostrada**:
```
┌──────────────────────────────────────────────────────────────┐
│ DeskMatch  │  Espacios | Dashboard | Gestionar Empresa | Analytics  │ 🏢 │
└──────────────────────────────────────────────────────────────┘
```

**Links disponibles**:
- 🏢 **Espacios** → `/spaces` - Gestionar espacios publicados
- 📊 **Dashboard** → `/dashboard` - Métricas generales
- 🏪 **Gestionar Empresa** → `/manage-company` - Datos de la empresa
- 📈 **Analytics** → `/analytics` - Análisis detallado del negocio

**UserMenu Badge**: 🏢 EMPRESA (o 👑 ADMIN si es administrador)

### Flujo de Usuario Empresa

1. Ingresa y ve "Espacios" para publicar/editar sus oficinas
2. Va a "Dashboard" para ver métricas generales
3. En "Gestionar Empresa" actualiza datos corporativos
4. En "Analytics" analiza su desempeño comercial
5. En "Mi Perfil" actualiza información de contacto

---

## Implementación

### Componente Navbar.jsx

```jsx
function Navbar() {
  const location = useLocation();
  const { isAuthenticated, user } = useAuth();

  // Determinar el tipo de usuario
  const isCompany = user?.role === 'Company' || user?.role === 'Admin';

  return (
    <header className="navbar">
      <nav className="navbar__container">
        <Link to="/" className="navbar__logo">
          DeskMatch
        </Link>

        {isAuthenticated && (
          <div className="navbar__links">
            {isCompany ? (
              <>
                <Link to="/spaces" className="navbar__link">Espacios</Link>
                <Link to="/dashboard" className="navbar__link">Dashboard</Link>
                <Link to="/manage-company" className="navbar__link">Gestionar Empresa</Link>
                <Link to="/analytics" className="navbar__link">Analytics</Link>
              </>
            ) : (
              <>
                <Link to="/offices" className="navbar__link">Oficinas</Link>
                <Link to="/dashboard" className="navbar__link">Dashboard</Link>
                <Link to="/my-spaces" className="navbar__link">Mis Espacios</Link>
              </>
            )}
          </div>
        )}

        <div className="navbar__actions">
          {isAuthenticated ? (
            <UserMenu />
          ) : (
            {/* Links de autenticación */}
          )}
        </div>
      </nav>
    </header>
  );
}
```

### UserMenu con Badge de Rol

El UserMenu también muestra un badge identificando el rol del usuario:

```jsx
{user?.role && (
  <span className="user-menu__role-badge">
    {user.role === 'Company' ? '🏢 Empresa' : 
     user.role === 'Admin' ? '👑 Admin' : 
     '👤 Usuario'}
  </span>
)}
```

**Badges mostrados**:
- `👤 USUARIO` - Usuario regular
- `🏢 EMPRESA` - Usuario de empresa
- `👑 ADMIN` - Administrador

---

## Rutas Protegidas

Todas las rutas nuevas están protegidas por `ProtectedRoute`:

```jsx
<Route path="/spaces" element={<ProtectedRoute><Spaces /></ProtectedRoute>} />
<Route path="/manage-company" element={<ProtectedRoute><ManageCompany /></ProtectedRoute>} />
<Route path="/analytics" element={<ProtectedRoute><Analytics /></ProtectedRoute>} />
```

Esto garantiza que solo usuarios autenticados puedan acceder a estas páginas.

---

## Responsividad

### Desktop (1024px+)
- Todos los links visibles
- Nombre de usuario completo en navbar
- Avatar sin nombre en mobile

### Tablet (768px - 1023px)
- Links ocultos (se muestran en UserMenu)
- Navbar compacta

### Mobile (< 640px)
- Solo avatar en navbar
- Links en UserMenu
- Navbar muy compacta

---

## Cambio de Rol

Si un usuario tiene ambos roles (simultáneamente), se prioriza "Company":

```javascript
const isCompany = user?.role === 'Company' || user?.role === 'Admin';
```

**Orden de prioridad**:
1. Si `role === 'Company'` → Mostrar navbar de empresa
2. Si `role === 'Admin'` → Mostrar navbar de admin
3. En cualquier otro caso → Mostrar navbar de usuario regular

---

## Cómo Probar

### Prueba como Usuario Regular:
1. Registrarse desde `/register/user`
2. Se asigna automáticamente role = "User"
3. Ver navbar con: Oficinas | Dashboard | Mis Espacios
4. Badge en UserMenu: 👤 USUARIO

### Prueba como Empresa:
1. Registrarse desde `/register/company`
2. Se asigna automáticamente role = "Company"
3. Ver navbar con: Espacios | Dashboard | Gestionar Empresa | Analytics
4. Badge en UserMenu: 🏢 EMPRESA

---

## Archivos Modificados

- **Navbar.jsx** - Lógica de rol + renderizado condicional
- **App.jsx** - Rutas para /spaces, /manage-company, /analytics
- **UserMenu.jsx** - Badge de rol agregado
- **UserMenu.css** - Estilos para badge de rol

---

## Próximos Pasos

- [ ] Implementar contenido real para cada página
- [ ] Agregar validaciones por rol en el backend
- [ ] Crear formularios para gestionar empresa y espacios
- [ ] Implementar analytics dashboard
- [ ] Agregar permisos granulares por rol
