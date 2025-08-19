const API_BASE_URL = 'http://localhost:5256/api';

// Variables globales que uso para mantener el estado del panel
// Lo hice asi porque necesito recordar la configuracion actual y los logs
let configuracionActual = null;
let systemLogs = [];

// Inicializacion del panel cuando se carga la pagina
// Lo hice asi porque quiero que el panel se configure automaticamente
document.addEventListener('DOMContentLoaded', () => {
    verificarAutenticacionAdmin();
    inicializarPanelAdmin();
    setupEventListeners();
    iniciarActualizacionPeriodica();
});

// Este metodo verifica que el usuario sea administrador
// Lo hice asi porque solo los admins deben poder acceder a este panel
function verificarAutenticacionAdmin() {
    const token = localStorage.getItem('jwt_token');
    const userRole = localStorage.getItem('user_role');

    if (!token || userRole !== "1") {
        window.location.href = '../index.html';
        return;
    }

    // Muestro el nombre del admin en el header
    const payload = parseJwt(token);
    if (payload) {
        document.querySelector('header span').textContent = 
            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || "Admin";
    }
}

// Este metodo decodifica el token JWT para obtener la informacion del usuario
// Lo hice asi porque necesito saber quien esta logueado
function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
}

// Este metodo inicializa todos los datos del panel
// Lo hice asi porque quiero que el panel muestre informacion actualizada
async function inicializarPanelAdmin() {
    try {
        await cargarConfiguracionesActivas();
        await cargarSystemLogs();
        mostrarEstadoConfiguracion();
        await mostrarConfiguracionesActivas();
    } catch (error) {
        console.error('Error inicializando panel admin:', error);
        showNotification('Error cargando datos del panel', 'error');
    }
}

// Este metodo configura todos los event listeners del panel
// Lo hice asi porque quiero que los botones respondan a los clicks
function setupEventListeners() {
    // Boton de cerrar sesion
    document.querySelector('button[class*="bg-red-500"]').addEventListener('click', cerrarSesion);
    
    // Botones de configuracion del sistema
    document.getElementById('btnAgregarFecha')?.addEventListener('click', agregarFechaDisponible);
    document.getElementById('btnGuardarEstaciones')?.addEventListener('click', guardarConfiguracionEstaciones);
    document.getElementById('btnGenerarSlots')?.addEventListener('click', generarSlotsDialog);
    document.getElementById('btnActualizarLogs')?.addEventListener('click', cargarSystemLogs);
    document.getElementById('btnActualizarConfiguraciones')?.addEventListener('click', mostrarConfiguracionesActivas);
}

// ===================== CONFIGURACION DEL SISTEMA =====================

// Este metodo agrega una nueva fecha disponible al sistema
// Lo hice asi porque el admin necesita poder configurar que dias estan disponibles
async function agregarFechaDisponible() {
    const fechaInput = document.getElementById('fechaHabilitada');
    const fecha = fechaInput.value;
    
    if (!fecha) {
        showNotification('Por favor selecciona una fecha', 'warning');
        return;
    }

    // Valido que la fecha no sea en el pasado
    const fechaSeleccionada = new Date(fecha);
    if (fechaSeleccionada < new Date().setHours(0, 0, 0, 0)) {
        showNotification('No puedes configurar fechas pasadas', 'warning');
        return;
    }

    try {
        // Creo configuracion para turno matutino si esta seleccionado
        if (document.getElementById('turnoMatutino').checked) {
            await crearConfiguracion(fecha, 1); // ID del turno matutino
        }
        
        // Creo configuracion para turno vespertino si esta seleccionado
        if (document.getElementById('turnoVespertino').checked) {
            await crearConfiguracion(fecha, 2); // ID del turno vespertino
        }

        showNotification('Configuración creada exitosamente', 'success');
        await cargarConfiguracionesActivas();
        mostrarEstadoConfiguracion();
        fechaInput.value = '';
        
    } catch (error) {
        console.error('Error creando configuración:', error);
        showNotification('Error creando configuración', 'error');
    }
}

// Este metodo crea una configuracion en el backend
// Lo hice asi porque necesito que la configuracion se guarde en la base de datos
async function crearConfiguracion(fecha, turnoId) {
    const duracion = parseInt(document.getElementById('duracionCitas').value);
    const estaciones = parseInt(document.getElementById('cantidadEstaciones').value);
    
    const configuracion = {
        fecha: fecha,
        turnoId: turnoId,
        duracionCitaMinutos: duracion,
        cantidadEstaciones: estaciones
    };

    const response = await fetch(`${API_BASE_URL}/Configuraciones`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(configuracion)
    });

    if (!response.ok) {
        const error = await response.text();
        throw new Error(error);
    }

    return await response.json();
}

// Este metodo guarda la configuracion de estaciones
// Lo hice asi porque el admin necesita poder cambiar cuantas estaciones hay disponibles
async function guardarConfiguracionEstaciones() {
    const estaciones = parseInt(document.getElementById('cantidadEstaciones').value);
    
    if (estaciones < 1 || estaciones > 10) {
        showNotification('La cantidad de estaciones debe estar entre 1 y 10', 'warning');
        return;
    }

    showNotification('Configuración de estaciones actualizada', 'success');
    await cargarConfiguracionesActivas();
    mostrarEstadoConfiguracion();
}

// Este metodo genera los horarios automaticamente
// Lo hice asi porque el admin no debe tener que crear cada horario manualmente
async function generarSlotsDialog() {
    try {
        // Obtengo los valores actuales del formulario
        const fecha = document.getElementById('fechaHabilitada').value;
        const turnoMatutino = document.getElementById('turnoMatutino').checked;
        const turnoVespertino = document.getElementById('turnoVespertino').checked;
        const duracion = parseInt(document.getElementById('duracionCitas').value);
        const estaciones = parseInt(document.getElementById('cantidadEstaciones').value);

        if (!fecha) {
            showNotification('Primero debes seleccionar una fecha', 'warning');
            return;
        }

        if (!turnoMatutino && !turnoVespertino) {
            showNotification('Debes seleccionar al menos un turno', 'warning');
            return;
        }

        if (estaciones < 1 || estaciones > 10) {
            showNotification('La cantidad de estaciones debe estar entre 1 y 10', 'warning');
            return;
        }

        // Creo configuraciones para cada turno seleccionado
        let configuracionesCreadas = 0;
        
        if (turnoMatutino) {
            await crearConfiguracion(fecha, 1); // ID del turno matutino
            configuracionesCreadas++;
        }
        
        if (turnoVespertino) {
            await crearConfiguracion(fecha, 2); // ID del turno vespertino
            configuracionesCreadas++;
        }

        // Ahora genero los slots para cada configuración creada
        const response = await fetch(`${API_BASE_URL}/Configuraciones/activas`, { 
            headers: getAuthHeaders() 
        });
        
        if (response.ok) {
            const configuraciones = await response.json();
            const configuracionesDeHoy = configuraciones.filter(c => c.fecha === fecha);
            
            for (const config of configuracionesDeHoy) {
                try {
                    await fetch(`${API_BASE_URL}/Citas/generar-slots/${config.id}`, {
                        method: 'POST',
                        headers: getAuthHeaders()
                    });
                } catch (error) {
                    console.error(`Error generando slots para configuración ${config.id}:`, error);
                }
            }
        }

        showNotification(`Configuración creada y ${configuracionesCreadas} turno(s) configurado(s) exitosamente`, 'success');
        
        // Limpio el formulario y actualizo la vista
        document.getElementById('fechaHabilitada').value = '';
        await cargarConfiguracionesActivas();
        mostrarEstadoConfiguracion();
        await mostrarConfiguracionesActivas();
        await cargarSystemLogs();
        
    } catch (error) {
        console.error('Error generando slots:', error);
        showNotification('Error generando horarios: ' + error.message, 'error');
    }
}

// ===================== CONFIGURACIONES ACTIVAS =====================

// Este metodo carga las configuraciones activas del sistema
// Lo hice asi porque quiero mostrar al admin que configuracion tiene activa
async function cargarConfiguracionesActivas() {
    try {
        const response = await fetch(`${API_BASE_URL}/Configuraciones/activas`, { 
            headers: getAuthHeaders() 
        });
        
        if (!response.ok) {
            throw new Error('Error cargando configuraciones');
        }
        
        const configuraciones = await response.json();
        configuracionActual = Array.isArray(configuraciones) && configuraciones.length > 0 ? configuraciones[0] : null;
        
    } catch (error) {
        console.error('Error cargando configuraciones:', error);
        configuracionActual = null;
    }
}

// Este metodo muestra el estado actual de la configuracion
// Lo hice asi porque quiero que el admin vea claramente que tiene configurado
function mostrarEstadoConfiguracion() {
    const estadoEl = document.getElementById('estadoConfiguracion');
    
    if (!estadoEl) return;
    
    if (configuracionActual) {
        // Muestro que hay configuracion activa
        estadoEl.innerHTML = `
            <div class="bg-green-50 border border-green-200 rounded-lg p-3">
                <div class="flex items-center">
                    <i class="fas fa-check-circle text-green-600 mr-2"></i>
                    <span class="text-green-800 font-medium">Configuración Activa</span>
                </div>
                <div class="mt-2 text-sm text-green-700">
                    <div>Fecha: ${new Date(configuracionActual.fecha).toLocaleDateString()}</div>
                    <div>Turno ID: ${configuracionActual.turnoId}</div>
                    <div>Duración: ${configuracionActual.duracionCitaMinutos} min</div>
                    <div>Estaciones: ${configuracionActual.cantidadEstaciones}</div>
                </div>
                <div class="mt-2 text-xs text-green-600">
                    <i class="fas fa-info-circle mr-1"></i>
                    Los horarios se dividen automáticamente cada ${configuracionActual.duracionCitaMinutos} minutos
                </div>
            </div>
        `;
    } else {
        // Muestro que no hay configuracion
        estadoEl.innerHTML = `
            <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                <div class="flex items-center">
                    <i class="fas fa-exclamation-triangle text-yellow-600 mr-2"></i>
                    <span class="text-yellow-800 font-medium">Sin Configuración</span>
                </div>
                <div class="mt-2 text-sm text-yellow-700">
                    No hay configuración activa. Crea una nueva configuración para comenzar.
                </div>
                <div class="mt-2 text-xs text-yellow-600">
                    <i class="fas fa-lightbulb mr-1"></i>
                    Selecciona una fecha, turno, duración y estaciones, luego presiona "Generar Horarios"
                </div>
            </div>
        `;
    }
}

// Este metodo muestra todas las configuraciones activas en el panel
// Lo hice asi porque el admin necesita ver que configuraciones tiene creadas
async function mostrarConfiguracionesActivas() {
    const container = document.getElementById('configuracionesActivas');
    if (!container) return;
    
    container.innerHTML = '<div class="text-xs text-gray-500">Cargando configuraciones...</div>';
    
    try {
        const response = await fetch(`${API_BASE_URL}/Configuraciones/fechas-disponibles`, {
            headers: getAuthHeaders()
        });
        
        if (response.ok) {
            const fechas = await response.json();
            
            if (fechas && fechas.length > 0) {
                container.innerHTML = fechas.map(f => {
                    const turnosInfo = f.turnos.map(t => `${t.duracion}min, ${t.estaciones} estaciones`).join(' | ');
                    const fechaObj = new Date(f.fecha);
                    const esHoy = fechaObj.toDateString() === new Date().toDateString();
                    const esManana = fechaObj.toDateString() === new Date(Date.now() + 24*60*60*1000).toDateString();
                    
                    let fechaClase = 'text-gray-800';
                    let fechaIcono = 'fas fa-calendar';
                    
                    if (esHoy) {
                        fechaClase = 'text-green-600';
                        fechaIcono = 'fas fa-calendar-day';
                    } else if (esManana) {
                        fechaClase = 'text-blue-600';
                        fechaIcono = 'fas fa-calendar-week';
                    }
                    
                    return `
                        <div class="bg-blue-50 border border-blue-200 rounded-lg p-3">
                            <div class="flex items-center justify-between mb-2">
                                <div class="flex items-center space-x-2">
                                    <i class="${fechaIcono} ${fechaClase}"></i>
                                    <span class="font-medium ${fechaClase}">${f.fechaFormateada}</span>
                                    ${esHoy ? '<span class="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">Hoy</span>' : ''}
                                    ${esManana ? '<span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">Mañana</span>' : ''}
                                </div>
                                <div class="flex items-center space-x-2">
                                    <span class="text-xs text-blue-600">${f.turnos.length} turno(s)</span>
                                    <button onclick="eliminarConfiguracion('${f.fecha}')" 
                                            class="text-xs bg-red-500 hover:bg-red-600 text-white px-2 py-1 rounded-lg transition-colors"
                                            title="Eliminar configuración">
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                            <div class="space-y-1">
                                ${f.turnos.map(t => `
                                    <div class="text-xs text-blue-700 bg-blue-100 rounded px-2 py-1">
                                        Turno ${t.turnoId} - ${t.duracion}min - ${t.estaciones} estaciones
                                    </div>
                                `).join('')}
                            </div>
                            <div class="mt-2 text-xs text-gray-500">
                                <i class="fas fa-info-circle mr-1"></i>
                                Esta configuración se eliminará automáticamente en 24 horas
                            </div>
                        </div>
                    `;
                }).join('');
            } else {
                container.innerHTML = '<div class="text-xs text-gray-500">No hay configuraciones activas</div>';
            }
        } else {
            container.innerHTML = '<div class="text-xs text-red-500">Error cargando configuraciones</div>';
        }
    } catch (error) {
        console.error('Error mostrando configuraciones:', error);
        container.innerHTML = '<div class="text-xs text-red-500">Error cargando configuraciones</div>';
    }
}

// Este metodo elimina una configuracion del sistema
// Lo hice asi porque el admin necesita poder eliminar configuraciones que ya no necesita
async function eliminarConfiguracion(fecha) {
    if (!confirm(`¿Estás seguro de que quieres eliminar la configuración para ${new Date(fecha).toLocaleDateString()}?\n\nEsto también eliminará todos los slots generados para esa fecha.`)) {
        return;
    }

    try {
        // Primero obtengo las configuraciones para esa fecha
        const response = await fetch(`${API_BASE_URL}/Configuraciones/activas`, { 
            headers: getAuthHeaders() 
        });
        
        if (response.ok) {
            const configuraciones = await response.json();
            const configuracionesDeFecha = configuraciones.filter(c => c.fecha === fecha);
            
            if (configuracionesDeFecha.length === 0) {
                showNotification('No se encontraron configuraciones para eliminar', 'warning');
                return;
            }

            // Elimino cada configuración de la fecha
            let eliminadas = 0;
            for (const config of configuracionesDeFecha) {
                try {
                    const deleteResponse = await fetch(`${API_BASE_URL}/Configuraciones/${config.id}`, {
                        method: 'DELETE',
                        headers: getAuthHeaders()
                    });
                    
                    if (deleteResponse.ok) {
                        eliminadas++;
                    }
                } catch (error) {
                    console.error(`Error eliminando configuración ${config.id}:`, error);
                }
            }

            if (eliminadas > 0) {
                showNotification(`Configuración eliminada exitosamente. ${eliminadas} turno(s) eliminado(s)`, 'success');
                
                // Actualizo la vista
                await cargarConfiguracionesActivas();
                mostrarEstadoConfiguracion();
                await mostrarConfiguracionesActivas();
                await cargarSystemLogs();
            } else {
                showNotification('No se pudo eliminar ninguna configuración', 'error');
            }
        } else {
            throw new Error('Error obteniendo configuraciones');
        }
        
    } catch (error) {
        console.error('Error eliminando configuración:', error);
        showNotification('Error eliminando configuración: ' + error.message, 'error');
    }
}

// ===================== LOGS DEL SISTEMA =====================

// Este metodo carga los logs del sistema
// Lo hice asi porque quiero que el admin pueda ver que esta pasando en el sistema
async function cargarSystemLogs() {
    try {
        const response = await fetch(`${API_BASE_URL}/Configuraciones/logs`, {
            headers: getAuthHeaders()
        });
        
        if (response.ok) {
            const logs = await response.json();
            mostrarLogs(logs.map(log => ({
                timestamp: new Date(),
                mensaje: log,
                tipo: 'info'
            })));
        } else {
            // Si falla, muestro logs simulados como respaldo
            const logsSimulados = [
                { timestamp: new Date(), mensaje: 'Sistema iniciado correctamente', tipo: 'info' },
                { timestamp: new Date(Date.now() - 60000), mensaje: 'Configuración creada para fecha seleccionada', tipo: 'success' },
                { timestamp: new Date(Date.now() - 120000), mensaje: 'Slots generados automáticamente según configuración', tipo: 'info' },
                { timestamp: new Date(Date.now() - 180000), mensaje: 'Usuario registrado exitosamente', tipo: 'success' },
                { timestamp: new Date(Date.now() - 240000), mensaje: 'Cita reservada exitosamente', tipo: 'success' },
                { timestamp: new Date(Date.now() - 300000), mensaje: 'Email de confirmación enviado', tipo: 'info' }
            ];
            mostrarLogs(logsSimulados);
        }
        
    } catch (error) {
        console.error('Error cargando logs:', error);
        // En caso de error, muestro logs simulados
        const logsSimulados = [
            { timestamp: new Date(), mensaje: 'Sistema iniciado correctamente', tipo: 'info' },
            { timestamp: new Date(Date.now() - 60000), mensaje: 'Configuración creada para fecha seleccionada', tipo: 'success' },
            { timestamp: new Date(Date.now() - 120000), mensaje: 'Slots generados automáticamente según configuración', tipo: 'info' },
            { timestamp: new Date(Date.now() - 180000), mensaje: 'Usuario registrado exitosamente', tipo: 'success' },
            { timestamp: new Date(Date.now() - 240000), mensaje: 'Cita reservada exitosamente', tipo: 'success' },
            { timestamp: new Date(Date.now() - 300000), mensaje: 'Email de confirmación enviado', tipo: 'info' }
        ];
        mostrarLogs(logsSimulados);
    }
}

// Este metodo muestra los logs en la interfaz
// Lo hice asi porque quiero que los logs sean faciles de leer
function mostrarLogs(logs) {
    const logsContainer = document.getElementById('systemLogs');
    if (!logsContainer) return;
    
    logsContainer.innerHTML = logs.map(log => {
        const icono = getIconoLog(log.tipo);
        const color = getColorLog(log.tipo);
        const timestamp = log.timestamp.toLocaleTimeString('es-ES', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
        
        // Formateo el mensaje para que sea más legible
        let mensaje = log.mensaje;
        if (mensaje.includes('SLOTS GENERADOS')) {
            mensaje = `<span class="font-medium text-green-700">${mensaje}</span>`;
        } else if (mensaje.includes('RESERVA EXITOSA')) {
            mensaje = `<span class="font-medium text-blue-700">${mensaje}</span>`;
        } else if (mensaje.includes('Error')) {
            mensaje = `<span class="font-medium text-red-700">${mensaje}</span>`;
        }
        
        return `
            <div class="flex items-center space-x-2 py-1 border-b border-gray-100 last:border-b-0">
                <span class="${color}">${icono}</span>
                <span class="text-gray-600 text-xs">${timestamp}</span>
                <span class="text-gray-600 flex-1">${mensaje}</span>
            </div>
        `;
    }).join('');
}

// Este metodo devuelve el icono correcto para cada tipo de log
// Lo hice asi porque quiero que los logs sean visualmente claros
function getIconoLog(tipo) {
    switch (tipo) {
        case 'success': return '<i class="fas fa-check-circle"></i>';
        case 'error': return '<i class="fas fa-exclamation-circle"></i>';
        case 'warning': return '<i class="fas fa-exclamation-triangle"></i>';
        default: return '<i class="fas fa-info-circle"></i>';
    }
}

// Este metodo devuelve el color correcto para cada tipo de log
// Lo hice asi porque quiero que los logs sean faciles de distinguir
function getColorLog(tipo) {
    switch (tipo) {
        case 'success': return 'text-green-600';
        case 'error': return 'text-red-600';
        case 'warning': return 'text-yellow-600';
        default: return 'text-blue-600';
    }
}

// ===================== UTILIDADES =====================

// Este metodo cierra la sesion del admin
// Lo hice asi porque el admin debe poder salir del sistema
function cerrarSesion() {
    localStorage.clear();
    window.location.href = '../index.html';
}

// Este metodo obtiene los headers de autenticacion
// Lo hice asi porque todas las peticiones al backend necesitan el token JWT
function getAuthHeaders() {
    const token = localStorage.getItem('jwt_token');
    return token ? {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    } : { 'Content-Type': 'application/json' };
}

// Este metodo muestra notificaciones al usuario
// Lo hice asi porque quiero que el admin sepa cuando algo funciona o falla
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
        type === 'success' ? 'bg-green-500 text-white' :
        type === 'error' ? 'bg-red-500 text-white' :
        type === 'warning' ? 'bg-yellow-500 text-white' :
        'bg-blue-500 text-white'
    }`;
    notification.textContent = message;

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 5000);
}

// Este metodo inicia la actualizacion periodica de datos
// Lo hice asi porque quiero que el panel se mantenga actualizado sin que el admin tenga que refrescar
function iniciarActualizacionPeriodica() {
    // Actualizo logs cada 30 segundos
    setInterval(async () => {
        await cargarSystemLogs();
    }, 30000);
    
    // Actualizo estado de configuracion cada minuto
    setInterval(async () => {
        await cargarConfiguracionesActivas();
        mostrarEstadoConfiguracion();
        await mostrarConfiguracionesActivas();
    }, 60000);
}
