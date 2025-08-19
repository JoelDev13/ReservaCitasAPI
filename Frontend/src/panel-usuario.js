const API_BASE_URL = "http://localhost:5256/api";

// ==========================
// VARIABLES GLOBALES
// ==========================
let pasoActual = 1;
let datosReserva = {
    tramiteId: null,
    tramiteNombre: '',
    tramiteDuracion: 0,
    fecha: null,
    turnoId: null,
    turnoNombre: '',
    hora: null,
    fechaHora: null
};

// ==========================
// MANEJO DE PASOS
// ==========================
function mostrarPaso(paso) {
    document.querySelectorAll('[id^="paso-"]').forEach(el => el.classList.add('hidden'));
    document.getElementById(`paso-${getPasoName(paso)}`).classList.remove('hidden');
    actualizarProgreso(paso);

    document.getElementById('btnAnterior').classList.toggle('hidden', paso <= 1);
    document.getElementById('btnSiguiente').classList.toggle('hidden', paso >= 5);

    pasoActual = paso;
}

function getPasoName(paso) {
    const pasos = ['', 'tramite', 'fecha', 'turno', 'hora', 'confirmar'];
    return pasos[paso];
}

function actualizarProgreso(paso) {
    const pasos = ['', 'tramite', 'fecha', 'turno', 'hora', 'confirmar'];
    const lineas = ['', '', 'fecha', 'turno', 'hora', 'confirmar'];
    
    for (let i = 1; i <= 5; i++) {
        const stepEl = document.getElementById(`step-${pasos[i]}`);
        const lineEl = document.getElementById(`line-${lineas[i]}`);
        
        if (i < paso) {
            stepEl.className = 'w-8 h-8 md:w-10 md:h-10 bg-green-500 text-white rounded-full flex items-center justify-center';
            if (lineEl) lineEl.className = 'w-8 md:w-16 h-0.5 bg-green-500 flex-shrink-0';
        } else if (i === paso) {
            stepEl.className = 'w-8 h-8 md:w-10 md:h-10 bg-blue-500 text-white rounded-full flex items-center justify-center';
            if (lineEl) lineEl.className = 'w-8 md:w-16 h-0.5 bg-gray-300 flex-shrink-0';
        } else {
            stepEl.className = 'w-8 h-8 md:w-10 md:h-10 bg-gray-300 text-gray-500 rounded-full flex items-center justify-center';
            if (lineEl) lineEl.className = 'w-8 md:w-16 h-0.5 bg-gray-300 flex-shrink-0';
        }
    }
}

// ==========================
// TRÁMITES
// ==========================
document.querySelectorAll('.tramite-option').forEach(option => {
    option.addEventListener('click', function() {
        document.querySelectorAll('.tramite-option').forEach(opt => {
            opt.classList.remove('border-blue-500', 'bg-blue-50');
            opt.classList.add('border-gray-200');
        });
        
        this.classList.remove('border-gray-200');
        this.classList.add('border-blue-500', 'bg-blue-50');
        
        datosReserva.tramiteId = parseInt(this.dataset.tramiteId);
        datosReserva.tramiteNombre = this.dataset.tramiteNombre;
        datosReserva.tramiteDuracion = parseInt(this.dataset.duracion);
        
        document.getElementById('btnSiguiente').classList.remove('hidden');
    });
});

// ==========================
// NAVEGACIÓN
// ==========================
document.getElementById('btnSiguiente').addEventListener('click', function() {
    if (pasoActual < 5) {
        if (pasoActual === 1) {
            if (!datosReserva.tramiteId) {
                showNotification('Por favor selecciona un trámite', 'warning');
                return;
            }
            cargarFechasDisponibles();
        } else if (pasoActual === 2) {
            if (!datosReserva.fecha) {
                showNotification('Por favor selecciona una fecha', 'warning');
                return;
            }
            cargarTurnosDisponibles();
        } else if (pasoActual === 3) {
            if (!datosReserva.turnoId) {
                showNotification('Por favor selecciona un turno', 'warning');
                return;
            }
            cargarHorariosDisponibles();
        } else if (pasoActual === 4) {
            if (!datosReserva.hora) {
                showNotification('Por favor selecciona una hora', 'warning');
                return;
            }
            mostrarResumen();
        }

        mostrarPaso(pasoActual + 1);
    }
});

document.getElementById('btnAnterior').addEventListener('click', function() {
    if (pasoActual > 1) {
        mostrarPaso(pasoActual - 1);
    }
});

// ==========================
// LOGOUT
// ==========================
document.getElementById('cerrarSesionU').addEventListener('click', function() {
    localStorage.clear();
    window.location.href = '../index.html';
});

// ==========================
// AUTH Y HELPERS
// ==========================
function getAuthHeaders() {
    const token = localStorage.getItem('jwt_token');
    return token ? { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' } : { 'Content-Type': 'application/json' };
}

function parseJwt(token) {
    try { return JSON.parse(atob(token.split('.')[1])); } catch { return null; }
}

function formatoFechaCorta(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString('es-DO', { weekday: 'short', day: '2-digit', month: 'short' });
}

function formatoHora(h) {
    if (!h) return '';
    if (typeof h === 'string' && h.includes(':')) return h.substring(0,5);
    try { return new Date(h).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }); } catch { return String(h); }
}

// ==========================
// CARGA INICIAL Y SEGURIDAD
// ==========================
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('jwt_token');
    if (!token) {
        window.location.href = '../index.html';
        return;
    }

    const payload = parseJwt(token) || {};
    const nombre = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['name'] || payload['unique_name'] || 'Usuario';
    const inicial = nombre?.charAt(0)?.toUpperCase() || 'U';
    const userInitialEl = document.getElementById('userInitial');
    const userNameEl = document.getElementById('userName');
    if (userInitialEl) userInitialEl.textContent = inicial;
    if (userNameEl) userNameEl.textContent = nombre;

    // Paso inicial
    mostrarPaso(1);

    // Cargar mis citas
    cargarMisCitas();
});

// ==========================
// API: FECHAS, TURNOS, HORARIOS
// ==========================
async function cargarFechasDisponibles() {
    const cont = document.getElementById('fechasDisponibles');
    if (!cont) return;
    cont.innerHTML = '<div class="col-span-full text-center text-gray-500">Cargando fechas disponibles...</div>';

    try {
        const resp = await fetch(`${API_BASE_URL}/Configuraciones/fechas-disponibles`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar las fechas');
        const fechas = await resp.json();

        if (!fechas || fechas.length === 0) {
            cont.innerHTML = '<div class="col-span-full text-center text-red-500">No hay fechas disponibles configuradas</div>';
            return;
        }

        cont.innerHTML = fechas.map(f => {
            const turnosInfo = f.turnos.map(t => `${t.duracion}min, ${t.estaciones} estaciones`).join(' | ');
            return `
                <button data-fecha="${f.fecha}" class="p-4 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition text-left">
                    <div class="font-medium text-gray-800">${f.fechaFormateada}</div>
                    <div class="text-sm text-gray-500">${f.turnos.length} turno(s) disponible(s)</div>
                    <div class="text-xs text-gray-400">${turnosInfo}</div>
                </button>
            `;
        }).join('');

        // Selección
        cont.querySelectorAll('button[data-fecha]').forEach(btn => {
            btn.addEventListener('click', () => {
                cont.querySelectorAll('button').forEach(b => b.classList.remove('border-blue-500','bg-blue-50'));
                btn.classList.add('border-blue-500','bg-blue-50');
                datosReserva.fecha = btn.getAttribute('data-fecha');
                document.getElementById('btnSiguiente').classList.remove('hidden');
            });
        });
    } catch (e) {
        cont.innerHTML = '<div class="col-span-full text-center text-red-500">Error cargando fechas disponibles</div>';
        console.error(e);
    }
}

async function cargarTurnosDisponibles() {
    const cont = document.getElementById('turnosDisponibles');
    if (!cont) return;
    if (!datosReserva.fecha) { mostrarPaso(2); return; }

    cont.innerHTML = '<div class="col-span-full text-center text-gray-500">Cargando turnos...</div>';
    try {
        const resp = await fetch(`${API_BASE_URL}/Citas/turnos`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar los turnos');
        const turnos = await resp.json();

        if (!turnos || turnos.length === 0) {
            cont.innerHTML = '<div class="col-span-full text-center text-red-500">No hay turnos disponibles</div>';
            return;
        }

        cont.innerHTML = turnos.map(t => `
            <button data-turno="${t.id}" data-nombre="${t.nombre}" class="p-4 rounded-xl border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition text-left">
                <div class="text-gray-800 font-semibold">${t.nombre}</div>
                <div class="text-gray-500 text-sm">${t.rango || `${t.horaInicio} - ${t.horaFin}`}</div>
            </button>
        `).join('');

        cont.querySelectorAll('button[data-turno]').forEach(btn => {
            btn.addEventListener('click', () => {
                cont.querySelectorAll('button').forEach(b => b.classList.remove('border-blue-500','bg-blue-50'));
                btn.classList.add('border-blue-500','bg-blue-50');
                datosReserva.turnoId = parseInt(btn.getAttribute('data-turno'));
                datosReserva.turnoNombre = btn.getAttribute('data-nombre');
                document.getElementById('btnSiguiente').classList.remove('hidden');
            });
        });
    } catch (e) {
        cont.innerHTML = '<div class="col-span-full text-center text-red-500">Error cargando turnos</div>';
        console.error(e);
    }
}

async function cargarHorariosDisponibles() {
    const cont = document.getElementById('horariosDisponibles');
    if (!cont) return;
    if (!datosReserva.fecha || !datosReserva.turnoId) { mostrarPaso(3); return; }

    cont.innerHTML = '<div class="col-span-full text-center text-gray-500">Cargando horarios...</div>';
    try {
        const params = new URLSearchParams({ 
            fecha: datosReserva.fecha, 
            turnoId: String(datosReserva.turnoId) 
        });
        
        const resp = await fetch(`${API_BASE_URL}/Citas/horarios-disponibles?${params.toString()}`, { 
            headers: getAuthHeaders() 
        });
        
        if (!resp.ok) throw new Error('No se pudieron cargar los horarios');
        const horarios = await resp.json();

        if (!horarios || horarios.length === 0) {
            cont.innerHTML = '<div class="col-span-full text-center text-red-500">No hay horarios disponibles para esta fecha y turno</div>';
            return;
        }

        cont.innerHTML = horarios.map(h => {
            const hora = h.fechaHora || h.hora || h.horario || h;
            const cuposDisponibles = h.estacionesDisponibles ?? h.cuposDisponibles ?? h.capacidadDisponible ?? h.disponibles ?? 0;
            const cuposTotales = h.estacionesTotales ?? h.capacidadTotal ?? 4;
            const disabled = cuposDisponibles === 0;
            
            // Formateo la hora de manera más clara
            const horaFormateada = typeof hora === 'string' ? hora : hora.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
            
            const estadoCupos = disabled ? 'LLENO' : `${cuposDisponibles}/${cuposTotales} cupos`;
            const colorEstado = disabled ? 'text-red-500' : cuposDisponibles <= 1 ? 'text-orange-500' : 'text-green-500';
            
            return `
                <button data-hora="${hora}" ${disabled ? 'disabled' : ''}
                        class="p-4 rounded-lg border ${disabled ? 'bg-red-50 text-red-400 border-red-200 cursor-not-allowed' : 'border-gray-200 hover:border-blue-500 hover:bg-blue-50'} transition text-left">
                    <div class="font-medium text-gray-800 text-lg">${horaFormateada}</div>
                    <div class="text-sm ${colorEstado} font-medium mb-1">${estadoCupos}</div>
                    ${!disabled ? '<div class="text-xs text-gray-500">Disponible</div>' : '<div class="text-xs text-red-400">No disponible</div>'}
                </button>`;
        }).join('');

        cont.querySelectorAll('button[data-hora]').forEach(btn => {
            if (btn.disabled) return;
            btn.addEventListener('click', () => {
                cont.querySelectorAll('button').forEach(b => b.classList.remove('border-blue-500','bg-blue-50'));
                btn.classList.add('border-blue-500','bg-blue-50');
                datosReserva.hora = btn.getAttribute('data-hora');
                datosReserva.fechaHora = `${datosReserva.fecha}T${btn.querySelector('.font-medium').textContent}`;
                document.getElementById('btnSiguiente').classList.remove('hidden');
            });
        });
    } catch (e) {
        cont.innerHTML = '<div class="col-span-full text-center text-red-500">Error cargando horarios: ' + e.message + '</div>';
        console.error(e);
    }
}

// ==========================
// RESUMEN Y CONFIRMACIÓN
// ==========================
function mostrarResumen() {
    document.getElementById('resumen-tramite').textContent = datosReserva.tramiteNombre || '-';
    document.getElementById('resumen-fecha').textContent = new Date(datosReserva.fecha).toLocaleDateString();
    document.getElementById('resumen-turno').textContent = datosReserva.turnoNombre || '-';
    document.getElementById('resumen-hora').textContent = formatoHora(datosReserva.hora);
    document.getElementById('resumen-duracion').textContent = `${datosReserva.tramiteDuracion} minutos`;
}

document.getElementById('btnConfirmarReserva').addEventListener('click', confirmarReserva);

async function confirmarReserva() {
    const userId = localStorage.getItem('user_id');
    if (!userId) {
        showNotification('No se encontró el usuario. Inicie sesión nuevamente.', 'error');
        return;
    }

    // Validaciones adicionales
    if (!datosReserva.fecha || !datosReserva.turnoId || !datosReserva.hora) {
        showNotification('Faltan datos para confirmar la reserva', 'error');
        return;
    }

    const payload = {
        usuarioId: parseInt(userId),
        fechaHora: datosReserva.fechaHora,
        turnoId: datosReserva.turnoId,
        tipoTramite: datosReserva.tramiteId
    };

    const btn = document.getElementById('btnConfirmarReserva');
    const old = btn.innerHTML;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Reservando...';
    btn.disabled = true;
    
    try {
        const resp = await fetch(`${API_BASE_URL}/Citas/reservar`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(payload)
        });
        
        if (!resp.ok) {
            const msg = await resp.text();
            throw new Error(msg || 'No se pudo reservar la cita');
        }
        
        // Éxito
        showNotification('¡Cita reservada exitosamente! Revisa tu correo para la confirmación.', 'success');
        
        // Limpiar datos y volver al inicio
        datosReserva = {
            tramiteId: null,
            tramiteNombre: '',
            tramiteDuracion: 0,
            fecha: null,
            turnoId: null,
            turnoNombre: '',
            hora: null,
            fechaHora: null
        };
        
        // Actualizar mis citas
        await cargarMisCitas();
        
        // Volver al primer paso después de un delay
        setTimeout(() => {
            mostrarPaso(1);
            document.getElementById('btnSiguiente').classList.add('hidden');
        }, 2000);
        
    } catch (e) {
        showNotification(`Error: ${e.message}`, 'error');
        console.error('Error confirmando reserva:', e);
    } finally {
        btn.innerHTML = old;
        btn.disabled = false;
    }
}

// ==========================
// MIS CITAS
// ==========================
async function cargarMisCitas() {
    const userId = localStorage.getItem('user_id');
    const cont = document.getElementById('listaCitas');
    if (!userId || !cont) return;
    
    cont.innerHTML = `<div class="text-center py-8"><i class="fas fa-spinner fa-spin text-gray-400 text-2xl"></i></div>`;

    try {
        const resp = await fetch(`${API_BASE_URL}/Citas/mis-citas/${userId}`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar tus citas');
        const citas = await resp.json();
        
        if (!citas || citas.length === 0) {
            cont.innerHTML = `<div class="text-center py-8 text-gray-600">No tienes citas registradas.</div>`;
            return;
        }
        
        cont.innerHTML = citas.map(c => {
            const fecha = new Date(c.fechaHora || `${c.fecha}T${c.hora || '00:00'}`);
            const estado = c.estado || 'Reservada';
            const tramiteNombre = c.tramiteNombre || mapTramite(c.tipoTramite) || 'Trámite';
            
            // Verifica si se puede cancelar (minimo 2 horas antes)
            const ahora = new Date();
            const puedeCancelar = estado !== 'Cancelada' && 
                                 fecha > ahora && 
                                 fecha.getTime() - ahora.getTime() > 2 * 60 * 60 * 1000; // 2 horas
            
            // Verifica si se puede modificar (minimo 2 horas antes)
            const puedeModificar = estado !== 'Cancelada' && 
                                  fecha > ahora && 
                                  fecha.getTime() - ahora.getTime() > 2 * 60 * 60 * 1000; // 2 horas
            
            return `
                <div class="border border-gray-200 rounded-lg p-4">
                    <div class="flex items-center justify-between mb-3">
                        <div class="flex-1">
                            <div class="text-gray-800 font-semibold">${tramiteNombre}</div>
                            <div class="text-gray-600 text-sm">${fecha.toLocaleDateString()} - ${fecha.toLocaleTimeString('es-ES',{hour:'2-digit',minute:'2-digit'})}</div>
                            <div class="text-gray-500 text-xs">Estación: ${c.estacionNumero || 'N/A'}</div>
                            ${!puedeCancelar && estado !== 'Cancelada' ? '<div class="text-xs text-orange-500 mt-1">⚠️ Solo se puede modificar/cancelar hasta 2 horas antes de la cita</div>' : ''}
                        </div>
                        <div class="flex items-center space-x-2">
                            <span class="text-xs px-2 py-1 rounded-full ${estado==='Cancelada'?'bg-red-100 text-red-800':estado==='Confirmada'?'bg-green-100 text-green-800':'bg-blue-100 text-blue-800'}">${estado}</span>
                        </div>
                    </div>
                    
                    <div class="flex items-center space-x-2">
                        ${puedeModificar ? `
                            <button onclick="modificarCita(${c.id}, '${c.fechaHora || fecha.toISOString()}', '${tramiteNombre}')" 
                                    class="text-xs bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded-lg transition-colors">
                                <i class="fas fa-edit mr-1"></i>Modificar
                            </button>
                        ` : ''}
                        
                        ${puedeCancelar ? `
                            <button onclick="cancelarCita('${c.fechaHora || fecha.toISOString()}', ${c.id})" 
                                    class="text-xs bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded-lg transition-colors">
                                <i class="fas fa-times mr-1"></i>Cancelar
                            </button>
                        ` : ''}
                        
                        ${!puedeModificar && !puedeCancelar && estado !== 'Cancelada' ? `
                            <span class="text-xs text-gray-400 px-3 py-1">No se puede modificar</span>
                        ` : ''}
                    </div>
                </div>`;
        }).join('');
        
    } catch (e) {
        cont.innerHTML = `<div class="text-center py-8 text-red-500">Error cargando citas: ${e.message}</div>`;
    }
}

function mapTramite(t) {
    switch (t) {
        case 1: return 'Renovación de Licencia';
        case 2: return 'Primera Licencia';
        case 3: return 'Duplicado';
        case 4: return 'Cambio de Categoría';
        default: return 'Trámite';
    }
}

// ==========================
// CANCELACIÓN DE CITAS
// ==========================
async function cancelarCita(fechaHora, citaId) {
    if (!confirm('¿Estás seguro de que quieres cancelar esta cita?')) {
        return;
    }

    try {
        const userId = localStorage.getItem('user_id');
        if (!userId) {
            showNotification('No se encontró el usuario. Inicie sesión nuevamente.', 'error');
            return;
        }

        const payload = {
            usuarioId: parseInt(userId),
            fechaHora: fechaHora
        };

        const response = await fetch(`${API_BASE_URL}/Citas/cancelar`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            showNotification('Cita cancelada exitosamente. Revisa tu correo para la confirmación.', 'success');
            
            // Actualiza la lista de citas
            await cargarMisCitas();
        } else {
            const errorMsg = await response.text();
            throw new Error(errorMsg || 'No se pudo cancelar la cita');
        }
    } catch (error) {
        showNotification(`Error cancelando cita: ${error.message}`, 'error');
        console.error('Error cancelando cita:', error);
    }
}

// ==========================
// MODIFICACIÓN DE CITAS
// ==========================
async function modificarCita(citaId, fechaHoraActual, tramiteNombre) {
    // Creo un modal para modificar la cita
    const modal = document.createElement('div');
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
    modal.innerHTML = `
        <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Modificar Cita</h3>
            
            <div class="space-y-4">
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Cita Actual</label>
                    <div class="text-sm text-gray-600 bg-gray-50 p-3 rounded">
                        <div><strong>Trámite:</strong> ${tramiteNombre}</div>
                        <div><strong>Fecha/Hora:</strong> ${new Date(fechaHoraActual).toLocaleString('es-ES')}</div>
                    </div>
                </div>
                
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Nueva Fecha</label>
                    <input type="date" id="nuevaFecha" class="w-full border border-gray-300 rounded-lg px-3 py-2">
                </div>
                
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Nuevo Turno</label>
                    <select id="nuevoTurno" class="w-full border border-gray-300 rounded-lg px-3 py-2">
                        <option value="">Selecciona un turno</option>
                    </select>
                </div>
                
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Nueva Hora</label>
                    <select id="nuevaHora" class="w-full border border-gray-300 rounded-lg px-3 py-2">
                        <option value="">Primero selecciona fecha y turno</option>
                    </select>
                </div>
            </div>
            
            <div class="flex justify-end space-x-3 mt-6">
                <button onclick="this.closest('.fixed').remove()" 
                        class="px-4 py-2 text-gray-600 hover:text-gray-800 transition-colors">
                    Cancelar
                </button>
                <button onclick="confirmarModificacion(${citaId})" 
                        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors">
                    Confirmar Cambio
                </button>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Establecer fecha por defecto (mañana)
    const manana = new Date();
    manana.setDate(manana.getDate() + 1);
    document.getElementById('nuevaFecha').value = manana.toISOString().split('T')[0];
    
    // Cargar turnos cuando se selecciona fecha
    document.getElementById('nuevaFecha').addEventListener('change', cargarTurnosParaModificacion);
    document.getElementById('nuevoTurno').addEventListener('change', cargarHorariosParaModificacion);
    
    // Cargar turnos iniciales
    await cargarTurnosParaModificacion();
}

async function cargarTurnosParaModificacion() {
    const fecha = document.getElementById('nuevaFecha').value;
    const turnoSelect = document.getElementById('nuevoTurno');
    const horaSelect = document.getElementById('nuevaHora');
    
    if (!fecha) {
        turnoSelect.innerHTML = '<option value="">Selecciona una fecha primero</option>';
        horaSelect.innerHTML = '<option value="">Selecciona fecha y turno primero</option>';
        return;
    }
    
    try {
        const resp = await fetch(`${API_BASE_URL}/Citas/turnos`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar los turnos');
        const turnos = await resp.json();
        
        turnoSelect.innerHTML = '<option value="">Selecciona un turno</option>' + 
            turnos.map(t => `<option value="${t.id}">${t.nombre} (${t.rango || `${t.horaInicio} - ${t.horaFin}`})</option>`).join('');
        
        horaSelect.innerHTML = '<option value="">Selecciona un turno primero</option>';
    } catch (e) {
        turnoSelect.innerHTML = '<option value="">Error cargando turnos</option>';
        console.error(e);
    }
}

async function cargarHorariosParaModificacion() {
    const fecha = document.getElementById('nuevaFecha').value;
    const turnoId = document.getElementById('nuevoTurno').value;
    const horaSelect = document.getElementById('nuevaHora');
    
    if (!fecha || !turnoId) {
        horaSelect.innerHTML = '<option value="">Selecciona fecha y turno primero</option>';
        return;
    }
    
    try {
        const params = new URLSearchParams({ fecha, turnoId });
        const resp = await fetch(`${API_BASE_URL}/Citas/horarios-disponibles?${params.toString()}`, { 
            headers: getAuthHeaders() 
        });
        
        if (!resp.ok) throw new Error('No se pudieron cargar los horarios');
        const horarios = await resp.json();
        
        if (!horarios || horarios.length === 0) {
            horaSelect.innerHTML = '<option value="">No hay horarios disponibles</option>';
            return;
        }
        
        horaSelect.innerHTML = '<option value="">Selecciona una hora</option>' + 
            horarios.map(h => {
                const hora = h.fechaHora || h.hora || h.horario || h;
                const cuposDisponibles = h.estacionesDisponibles ?? h.cuposDisponibles ?? h.capacidadDisponible ?? h.disponibles ?? 0;
                const disabled = cuposDisponibles === 0;
                
                if (disabled) return '';
                
                const horaFormateada = typeof hora === 'string' ? hora : hora.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });
                return `<option value="${hora}">${horaFormateada} (${cuposDisponibles} cupos disponibles)</option>`;
            }).filter(opt => opt !== '').join('');
        
    } catch (e) {
        horaSelect.innerHTML = '<option value="">Error cargando horarios</option>';
        console.error(e);
    }
}

async function confirmarModificacion(citaId) {
    const nuevaFecha = document.getElementById('nuevaFecha').value;
    const nuevoTurno = document.getElementById('nuevoTurno').value;
    const nuevaHora = document.getElementById('nuevaHora').value;
    
    if (!nuevaFecha || !nuevoTurno || !nuevaHora) {
        showNotification('Por favor completa todos los campos', 'warning');
        return;
    }
    
    try {
        const nuevaFechaHora = `${nuevaFecha}T${nuevaHora}`;
        
        const payload = {
            citaId: citaId,
            usuarioId: parseInt(localStorage.getItem('user_id')),
            nuevaFechaHora: nuevaFechaHora,
            nuevoTurnoId: parseInt(nuevoTurno)
        };
        
        const resp = await fetch(`${API_BASE_URL}/Citas/editar`, {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify(payload)
        });
        
        if (!resp.ok) {
            const msg = await resp.text();
            throw new Error(msg || 'No se pudo modificar la cita');
        }
        
        showNotification('¡Cita modificada exitosamente! Revisa tu correo para la confirmación.', 'success');
        
        // Cerrar modal y actualizar lista
        document.querySelector('.fixed').remove();
        await cargarMisCitas();
        
    } catch (e) {
        showNotification(`Error modificando cita: ${e.message}`, 'error');
        console.error('Error modificando cita:', e);
    }
}

// ==========================
// NOTIFICACIONES
// ==========================
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