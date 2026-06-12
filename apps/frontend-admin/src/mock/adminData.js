export const adminUsersColumns = [
  { field: 'id', headerName: 'ID', width: 80 },
  { field: 'name', headerName: 'Nombre', width: 180 },
  { field: 'email', headerName: 'Email', width: 220 },
  { field: 'role', headerName: 'Rol', width: 110 },
  { field: 'status', headerName: 'Estado', width: 110 },
  { field: 'createdAt', headerName: 'Creado', width: 120 },
];

export const adminUsersRows = [
  { id: 1, name: 'Carlos Mendoza', email: 'carlos@deskmatch.com', role: 'Admin', status: 'Activo', createdAt: '2025-01-15' },
  { id: 2, name: 'Ana López', email: 'ana@empresa-a.com', role: 'Manager', status: 'Activo', createdAt: '2025-02-20' },
  { id: 3, name: 'Pedro Ramírez', email: 'pedro@empresa-a.com', role: 'User', status: 'Activo', createdAt: '2025-03-10' },
  { id: 4, name: 'Laura Gómez', email: 'laura@empresa-b.com', role: 'Manager', status: 'Inactivo', createdAt: '2025-03-22' },
  { id: 5, name: 'Sofía Torres', email: 'sofia@empresa-b.com', role: 'User', status: 'Activo', createdAt: '2025-04-05' },
  { id: 6, name: 'Diego Fernández', email: 'diego@empresa-c.com', role: 'User', status: 'Activo', createdAt: '2025-04-18' },
  { id: 7, name: 'Valeria Castro', email: 'valeria@empresa-c.com', role: 'User', status: 'Inactivo', createdAt: '2025-05-01' },
  { id: 8, name: 'Jorge Ruiz', email: 'jorge@deskmatch.com', role: 'Admin', status: 'Activo', createdAt: '2025-05-12' },
];

export const adminCompaniesColumns = [
  { field: 'id', headerName: 'ID', width: 80 },
  { field: 'name', headerName: 'Empresa', width: 200 },
  { field: 'owner', headerName: 'Propietario', width: 180 },
  { field: 'email', headerName: 'Email', width: 220 },
  { field: 'verification', headerName: 'Verificación', width: 130 },
  { field: 'createdAt', headerName: 'Creado', width: 120 },
];

export const adminCompaniesRows = [
  { id: 1, name: 'TechSolutions SA', owner: 'Ana López', email: 'contacto@techsolutions.com', verification: 'Verificada', createdAt: '2025-02-20' },
  { id: 2, name: 'InnovaGroup', owner: 'Laura Gómez', email: 'info@innovagroup.com', verification: 'Verificada', createdAt: '2025-03-22' },
  { id: 3, name: 'DataCorp', owner: 'Diego Fernández', email: 'admin@datacorp.io', verification: 'Pendiente', createdAt: '2025-04-18' },
  { id: 4, name: 'GreenLogic', owner: 'María Torres', email: 'maria@greenlogic.com', verification: 'Verificada', createdAt: '2025-05-30' },
  { id: 5, name: 'NexusDigital', owner: 'Ricardo Paz', email: 'rpaz@nexusdigital.net', verification: 'Rechazada', createdAt: '2025-06-15' },
];

export const adminAuditLogsColumns = [
  { field: 'id', headerName: 'ID', width: 60 },
  { field: 'user', headerName: 'Usuario', width: 160 },
  { field: 'action', headerName: 'Acción', width: 180 },
  { field: 'entity', headerName: 'Entidad', width: 120 },
  { field: 'entityId', headerName: 'ID Entidad', width: 100 },
  { field: 'ipAddress', headerName: 'IP', width: 130 },
  { field: 'timestamp', headerName: 'Fecha', width: 160 },
];

export const adminAuditLogsRows = [
  { id: 1, user: 'Carlos Mendoza', action: 'Usuario creado', entity: 'User', entityId: '3', ipAddress: '192.168.1.10', timestamp: '2025-06-10 09:15:32' },
  { id: 2, user: 'Carlos Mendoza', action: 'Empresa verificada', entity: 'Company', entityId: '2', ipAddress: '192.168.1.10', timestamp: '2025-06-10 09:20:15' },
  { id: 3, user: 'Ana López', action: 'Reserva cancelada', entity: 'Reservation', entityId: '45', ipAddress: '192.168.1.25', timestamp: '2025-06-10 10:00:00' },
  { id: 4, user: 'Jorge Ruiz', action: 'Oficina creada', entity: 'Office', entityId: '12', ipAddress: '192.168.1.11', timestamp: '2025-06-09 14:30:45' },
  { id: 5, user: 'Carlos Mendoza', action: 'Usuario desactivado', entity: 'User', entityId: '4', ipAddress: '192.168.1.10', timestamp: '2025-06-09 11:22:10' },
  { id: 6, user: 'Jorge Ruiz', action: 'Empresa rechazada', entity: 'Company', entityId: '5', ipAddress: '192.168.1.11', timestamp: '2025-06-08 16:05:33' },
  { id: 7, user: 'Ana López', action: 'Perfil actualizado', entity: 'User', entityId: '2', ipAddress: '192.168.1.25', timestamp: '2025-06-08 08:45:00' },
  { id: 8, user: 'Sofía Torres', action: 'Reserva creada', entity: 'Reservation', entityId: '52', ipAddress: '192.168.1.30', timestamp: '2025-06-07 19:10:22' },
];
