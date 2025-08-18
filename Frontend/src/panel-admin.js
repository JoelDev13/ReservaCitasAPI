const API_BASE_URL = 'http://localhost:5256/api';

// Variables globales
let configuracionActual = null;
let citasRecientes = [];
let systemLogs = [];

// Inicialización
document.addEventListener('DOMContentLoaded', () => {
    verificarAutenticacionAdmin();
    inicializarPanelAdmin();
    setupEventListeners();
    iniciarActualizacionPeriodica();
});

// 🔹 Verificar rol y token
function verificarAutenticacionAdmin() {
    const token = localStorage.getItem('jwt_token');
    const userRole = localStorage.getItem('user_role'); // guardado como "0" o "1"

    if (!token || userRole !== "1") {
        // Si no es admin => vuelve al login
        window.location.href = '../index.html';
        return;
    }

    // Mostrar nombre en header
    const payload = parseJwt(token);
    if (payload) {
        document.querySelector('header span').textContent = 
            payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || "Admin";
    }
}

function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
}

// 🔹 Inicializar datos del panel
async function inicializarPanelAdmin() {
    try {
        await cargarDashboardStats();
        await cargarConfiguracionesActivas();
        // Este endpoint puede no existir aún; se ignora error en catch interno
        await cargarCitasRecientes();
        await cargarTurnos();
        cargarSystemLogs();
    } catch (error) {
        console.error('Error inicializando panel admin:', error);
        showNotification('Error cargando datos del panel', 'error');
    }
}

function setupEventListeners() {
    // Cerrar sesión
    document.querySelector('button[class*="bg-red-500"]').addEventListener('click', cerrarSesion);
    
    // Formulario de configuración
    setupConfiguracionForm();

    // Botones
    document.getElementById('btnAgregarFecha')?.addEventListener('click', agregarFechaDisponible);
    document.getElementById('btnGuardarEstaciones')?.addEventListener('click', guardarConfiguracionEstaciones);
    document.getElementById('btnGenerarSlots')?.addEventListener('click', generarSlotsDialog);

    // Filtros
    setupFiltrosTabla();
}

// Placeholders para funciones de filtros (no-op si no existen elementos)
function setupFiltrosTabla() {}

// ===================== Datos del dashboard =====================
async function cargarDashboardStats() {
    // Sin endpoint específico; se muestran placeholders seguros
    const citasHoy = document.getElementById('citasHoy');
    if (citasHoy) citasHoy.textContent = '-';
    const cupos = document.getElementById('cuposDisponibles');
    if (cupos) cupos.textContent = '-';
}

// ===================== Configuraciones activas =====================
async function cargarConfiguracionesActivas() {
    try {
        const resp = await fetch(`${API_BASE_URL}/Configuraciones/activas`, { headers: getAuthHeaders() });
        if (!resp.ok) return;
        const data = await resp.json();
        configuracionActual = Array.isArray(data) && data.length > 0 ? data[0] : null;
        // Actualizar dashboard básico
        const cupos = document.getElementById('cuposDisponibles');
        if (cupos) cupos.textContent = String(Array.isArray(data) ? data.length : 0);
    } catch {}
}

// ===================== Turnos =====================
async function cargarTurnos() {
    try {
        await fetch(`${API_BASE_URL}/Citas/turnos`, { headers: getAuthHeaders() });
    } catch {}
}

// ===================== Logs =====================
function cargarSystemLogs() {
    // Aun sin endpoint de logs centralizado en frontend
}

// ===================== Generar slots =====================
async function generarSlotsDialog() {
    if (!configuracionActual || !configuracionActual.id) {
        showNotification('No hay configuración seleccionada', 'warning');
        return;
    }
    try {
        const resp = await fetch(`${API_BASE_URL}/Citas/generar-slots/${configuracionActual.id}`, {
            method: 'POST',
            headers: getAuthHeaders()
        });
        if (!resp.ok) throw new Error(await resp.text());
        showNotification('Slots generados correctamente', 'success');
    } catch (e) {
        showNotification('Error generando slots', 'error');
        console.error(e);
    }
}

// ============================================================================
// =========================  📌 PANEL DE CITAS  ==============================
// ============================================================================

async function cargarCitasRecientes() {
    try {
        const response = await fetch(`${API_BASE_URL}/Admin/citas-recientes`, {
            headers: getAuthHeaders()
        });
        
        if (response.ok) {
            const citas = await response.json();
            citasRecientes = citas;
            mostrarCitasEnTabla(citas);
        } else {
            mostrarCitasEnTabla([]);
        }
    } catch (error) {
        console.error('Error cargando citas:', error);
    }
}

function mostrarCitasEnTabla(citas) {
    const tbody = document.getElementById('citasTableBody');
    if (!tbody) return;

    tbody.innerHTML = citas.map(cita => {
        const fecha = new Date(cita.fechaHora);
        const iniciales = cita.nombreUsuario.split(' ').map(n => n[0]).join('');

        return `
            <tr>
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                        <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                            <span class="text-blue-600 font-medium">${iniciales}</span>
                        </div>
                        <div class="ml-4">
                            <div class="text-sm font-medium text-gray-900">${cita.nombreUsuario}</div>
                            <div class="text-sm text-gray-500">${cita.emailUsuario}</div>
                        </div>
                    </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-purple-100 text-purple-800">
                        ${mapearTramite(cita.tipoTramite)}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    ${fecha.toLocaleDateString()}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    ${fecha.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' })}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getEstadoClasses(cita.estado)}">
                        ${cita.estado}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button class="text-blue-600 hover:text-blue-900 mr-3" onclick="editarCitaAdmin(${cita.id})">
                        Editar
                    </button>
                    <button class="text-red-600 hover:text-red-900" onclick="cancelarCitaAdmin(${cita.id})">
                        Cancelar
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

// 🔹 Mapear el tipo de trámite válido
function mapearTramite(tramite) {
    switch (tramite) {
        case 1: return "Renovación";
        case 2: return "PrimeraVez";
        case 3: return "Duplicado";
        case 4: return "CambioDeCategoria";
        default: return "Desconocido";
    }
}

// 🔹 Estados visuales
function getEstadoClasses(estado) {
    switch (estado) {
        case 'Confirmada': return 'bg-green-100 text-green-800';
        case 'Reservada': return 'bg-blue-100 text-blue-800';
        case 'Cancelada': return 'bg-red-100 text-red-800';
        case 'Pendiente': return 'bg-yellow-100 text-yellow-800';
        default: return 'bg-gray-100 text-gray-800';
    }
}

// ============================================================================
// =========================  📌 UTILIDADES  =================================
// ============================================================================

function cerrarSesion() {
    localStorage.clear();
    window.location.href = 'index.html';
}

function getAuthHeaders() {
    const token = localStorage.getItem('jwt_token');
    return token ? {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
    } : { 'Content-Type': 'application/json' };
}

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
