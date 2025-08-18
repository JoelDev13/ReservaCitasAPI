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
