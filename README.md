# Sistema de Reserva de Citas API

## Descripción General

Este es un sistema completo de reserva de citas que permite a los administradores configurar horarios disponibles y a los usuarios reservar citas en tiempo real. El sistema incluye:

- **Panel de Administrador**: Para configurar fechas, turnos y generar slots automáticamente
- **Panel de Usuario**: Para seleccionar trámites y reservar citas
- **Backend API**: Con validaciones de negocio y control de concurrencia
- **Sistema de Emails**: Para confirmaciones automáticas

## Características Principales

### 🎯 Generación Automática de Slots
- Los administradores configuran fechas, duración de citas y número de estaciones
- El sistema genera automáticamente todos los horarios disponibles
- Los slots se dividen según la duración configurada (15, 30, 45, 60 minutos)
- Se crean cupos para cada estación de trabajo

### ⏰ Turnos Predefinidos
- **Turno Matutino**: 6:00 AM - 12:00 PM
- **Turno Vespertino**: 2:00 PM - 6:00 PM
- Los horarios se dividen automáticamente dentro de cada turno

### 🏢 Sistema de Estaciones
- Configuración flexible de estaciones de trabajo (1-10)
- Control de cupos por horario
- Asignación automática de estaciones disponibles

### 📧 Confirmaciones Automáticas
- Email de confirmación al reservar
- Email de cancelación al cancelar
- Email de modificación al editar

## Cómo Funciona

### 1. Configuración del Administrador

1. **Seleccionar Fecha**: El admin elige una fecha disponible
2. **Configurar Turnos**: Selecciona turno matutino, vespertino o ambos
3. **Establecer Duración**: Define cuánto dura cada cita (15, 30, 45, 60 min)
4. **Configurar Estaciones**: Define cuántas estaciones de trabajo usar
5. **Generar Slots**: El sistema crea automáticamente todos los horarios

### 2. Ejemplo de Generación

**Configuración:**
- Fecha: 15/01/2025
- Turnos: Matutino y Vespertino
- Duración: 30 minutos
- Estaciones: 4

**Resultado:**
- **Matutino (6:00 AM - 12:00 PM)**: 12 slots × 4 estaciones = 48 cupos
  - Horarios: 6:00, 6:30, 7:00, 7:30, 8:00, 8:30, 9:00, 9:30, 10:00, 10:30, 11:00, 11:30
- **Vespertino (2:00 PM - 6:00 PM)**: 8 slots × 4 estaciones = 32 cupos
  - Horarios: 14:00, 14:30, 15:00, 15:30, 16:00, 16:30, 17:00, 17:30

### 3. Reserva de Usuario

1. **Seleccionar Trámite**: Renovación, Primera Licencia, Duplicado, Cambio de Categoría
2. **Elegir Fecha**: Solo fechas con configuración activa
3. **Seleccionar Turno**: Matutino o Vespertino según disponibilidad
4. **Elegir Hora**: Horarios disponibles con cupos libres
5. **Confirmar**: Sistema valida y asigna estación automáticamente

## Estructura del Proyecto

```
ReservaCitasAPI/
├── Application/           # Lógica de negocio y servicios
├── Domain/               # Entidades y enums del dominio
├── Infrastructure/       # Persistencia y servicios externos
├── Presentation/         # Controllers y configuración de la API
└── Frontend/            # Interfaz de usuario (HTML/JS)
```

## Tecnologías Utilizadas

- **Backend**: .NET 6, Entity Framework Core
- **Frontend**: HTML5, CSS3 (Tailwind CSS), JavaScript ES6+
- **Base de Datos**: SQL Server (con migraciones automáticas)
- **Autenticación**: JWT (JSON Web Tokens)
- **Email**: Servicio SMTP configurable

## Endpoints Principales

### Configuraciones
- `POST /api/Configuraciones` - Crear configuración
- `GET /api/Configuraciones/activas` - Obtener configuraciones activas
- `GET /api/Configuraciones/fechas-disponibles` - Fechas con turnos

### Citas
- `POST /api/Citas/generar-slots/{configId}` - Generar slots automáticamente
- `GET /api/Citas/horarios-disponibles` - Horarios disponibles por fecha/turno
- `POST /api/Citas/reservar` - Reservar cita
- `POST /api/Citas/cancelar` - Cancelar cita
- `PUT /api/Citas/editar` - Editar cita

### Autenticación
- `POST /api/Auth/login` - Iniciar sesión
- `POST /api/Auth/registro` - Registrar usuario

## Validaciones de Negocio

### Control de Cupos
- Solo 1 cita activa por día por usuario
- Control de concurrencia para evitar sobrecupos
- Validación de horarios dentro del turno permitido
- Asignación automática de estaciones disponibles

### Restricciones de Tiempo
- No se pueden reservar citas en el pasado
- Cancelación solo hasta 2 horas antes de la cita
- Edición solo hasta 2 horas antes de la cita

## Instalación y Configuración

1. **Clonar el repositorio**
2. **Configurar la base de datos** en `appsettings.json`
3. **Ejecutar migraciones**: `dotnet ef database update`
4. **Configurar email** en `appsettings.json`
5. **Ejecutar el proyecto**: `dotnet run`

## Uso del Sistema

### Para Administradores
1. Acceder al panel de administrador
2. Configurar fechas disponibles
3. Generar slots automáticamente
4. Monitorear logs del sistema

### Para Usuarios
1. Registrarse o iniciar sesión
2. Seleccionar tipo de trámite
3. Elegir fecha y turno disponible
4. Seleccionar horario con cupo libre
5. Confirmar reserva

## Características de Seguridad

- Autenticación JWT con roles
- Validación de permisos por endpoint
- Control de acceso basado en roles
- Logs de auditoría para todas las operaciones
- Validación de datos en frontend y backend

## Monitoreo y Logs

El sistema mantiene logs detallados de:
- Configuraciones creadas
- Slots generados
- Reservas exitosas
- Cancelaciones y modificaciones
- Errores del sistema

## Soporte y Mantenimiento

Para soporte técnico o reportar problemas:
- Revisar logs del sistema
- Verificar configuración de base de datos
- Validar configuración de email
- Revisar permisos de usuario

---

**Desarrollado con ❤️ para la gestión eficiente de citas y trámites**
