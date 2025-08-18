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
        
        datosReserva.tramiteId = this.dataset.tramiteId;
        datosReserva.tramiteNombre = this.dataset.tramiteNombre;
        datosReserva.tramiteDuracion = parseInt(this.dataset.duracion);
        
        mostrarInfoTramite();
        document.getElementById('btnSiguiente').classList.remove('hidden');
    });
});

function mostrarInfoTramite() {
    const nombreEl = document.getElementById('tramite-nombre');
    const duracionEl = document.getElementById('tramite-duracion');
    const iconEl = document.getElementById('tramite-icon');
    const infoEl = document.getElementById('info-tramite-seleccionado');

    nombreEl.textContent = datosReserva.tramiteNombre;
    duracionEl.textContent = `Duración estimada: ${datosReserva.tramiteDuracion} minutos`;

    const iconos = {
        '1': 'fa-sync-alt',        // Renovacion
        '2': 'fa-plus-circle',    // Primera vez
        '3': 'fa-copy',           // Duplicado
        '4': 'fa-exchange-alt'    // Cambio de categoría
    };
    
    iconEl.className = `fas ${iconos[datosReserva.tramiteId] || 'fa-clipboard-list'} text-blue-600 text-2xl`;
    infoEl.classList.remove('hidden');
}

// ==========================
// NAVEGACIÓN
// ==========================
document.getElementById('btnSiguiente').addEventListener('click', function() {
    if (pasoActual < 5) {
        if (pasoActual === 1) cargarFechasDisponibles();
        else if (pasoActual === 2) cargarTurnosDisponibles();
        else if (pasoActual === 3) cargarHorariosDisponibles();
        else if (pasoActual === 4) mostrarResumen();

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
    cont.innerHTML = '<div class="col-span-full text-center text-gray-500">Cargando fechas...</div>';

    try {
        const resp = await fetch(`${API_BASE_URL}/Configuraciones/activas`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar las fechas');
        const configs = await resp.json();

        // Extraer fechas únicas (propiedad flexible)
        const fechas = Array.from(new Set((configs || []).map(c => c.fecha || c.fechaHabilitada || c.dia || c.date)));
        cont.innerHTML = fechas.map(f => {
            const iso = new Date(f).toISOString().substring(0,10);
            return `
                <button data-fecha="${iso}" class="px-4 py-3 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition">
                    <span class="block font-medium text-gray-800">${formatoFechaCorta(iso)}</span>
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
        cont.innerHTML = '<div class="col-span-full text-center text-red-500">Error cargando fechas</div>';
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

        cont.innerHTML = (turnos || []).map(t => `
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
        const params = new URLSearchParams({ fecha: datosReserva.fecha, turnoId: String(datosReserva.turnoId) });
        const resp = await fetch(`${API_BASE_URL}/Citas/horarios-disponibles?${params.toString()}`, { headers: getAuthHeaders() });
        if (!resp.ok) throw new Error('No se pudieron cargar los horarios');
        const horarios = await resp.json();

        cont.innerHTML = (horarios || []).map(h => {
            const hora = h.hora || h.horario || h; // tolerante a distintos formatos
            const cupos = h.cuposDisponibles ?? h.capacidadDisponible ?? h.disponibles;
            const disabled = h.disponible === false || (typeof cupos === 'number' && cupos <= 0);
            return `
                <button data-hora="${hora}" ${disabled ? 'disabled' : ''}
                        class="px-4 py-3 rounded-lg border ${disabled ? 'bg-gray-100 text-gray-400 border-gray-200' : 'border-gray-200 hover:border-blue-500 hover:bg-blue-50'} transition">
                    <div class="font-medium">${formatoHora(hora)}</div>
                    ${typeof cupos === 'number' ? `<div class="text-xs text-gray-500">Cupos: ${cupos}</div>` : ''}
                </button>`;
        }).join('');

        cont.querySelectorAll('button[data-hora]').forEach(btn => {
            if (btn.disabled) return;
            btn.addEventListener('click', () => {
                cont.querySelectorAll('button').forEach(b => b.classList.remove('border-blue-500','bg-blue-50'));
                btn.classList.add('border-blue-500','bg-blue-50');
                datosReserva.hora = btn.getAttribute('data-hora');
                datosReserva.fechaHora = `${datosReserva.fecha}T${formatoHora(datosReserva.hora)}`;
                document.getElementById('btnSiguiente').classList.remove('hidden');
            });
        });
    } catch (e) {
        cont.innerHTML = '<div class="col-span-full text-center text-red-500">Error cargando horarios</div>';
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
        alert('No se encontró el usuario. Inicie sesión nuevamente.');
        return;
    }

    const payload = {
        usuarioId: parseInt(userId),
        fecha: datosReserva.fecha,
        turnoId: datosReserva.turnoId,
        hora: formatoHora(datosReserva.hora),
        tipoTramite: parseInt(datosReserva.tramiteId)
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
            throw new Error(msg || 'No se pudo reservar');
        }
        // Éxito
        alert('Cita reservada exitosamente');
        cargarMisCitas();
    } catch (e) {
        alert(`Error: ${e.message}`);
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
            return `
                <div class="border border-gray-200 rounded-lg p-4 flex items-center justify-between">
                    <div>
                        <div class="text-gray-800 font-semibold">${c.tramiteNombre || mapTramite(c.tipoTramite) || 'Trámite'}</div>
                        <div class="text-gray-600 text-sm">${fecha.toLocaleDateString()} - ${fecha.toLocaleTimeString('es-ES',{hour:'2-digit',minute:'2-digit'})}</div>
                    </div>
                    <span class="text-xs px-2 py-1 rounded-full ${estado==='Cancelada'?'bg-red-100 text-red-800':estado==='Confirmada'?'bg-green-100 text-green-800':'bg-blue-100 text-blue-800'}">${estado}</span>
                </div>`;
        }).join('');
    } catch (e) {
        cont.innerHTML = `<div class="text-center py-8 text-red-500">Error cargando citas</div>`;
    }
}

function mapTramite(t) {
    switch (t) {
        case 1: return 'Renovación de Licencia';
        case 2: return 'Primera Licencia';
        case 3: return 'Cambio de Categoría';
        case 4: return 'Duplicado';
        default: return '';
    }
}