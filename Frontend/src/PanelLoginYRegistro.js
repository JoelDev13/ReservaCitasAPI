const API_BASE_URL = 'http://localhost:5256/api';

// Formularios y paneles
const loginForm = document.getElementById('loginForm');
const registroForm = document.getElementById('registroForm');
const recuperarForm = document.getElementById('recuperarForm');
const usuarioPanel = document.getElementById('usuarioPanel');
const adminPanel = document.getElementById('adminPanel');

// Botones de navegación
const mostrarRegistroBtn = document.getElementById('mostrarRegistro');
const mostrarLoginBtn = document.getElementById('mostrarLogin');
const mostrarRecuperarBtn = document.getElementById('mostrarRecuperar');
const volverLoginBtn = document.getElementById('volverLogin');

// Event listeners para navegación entre formularios
mostrarRegistroBtn?.addEventListener('click', mostrarRegistro);
mostrarLoginBtn?.addEventListener('click', mostrarLogin);
mostrarRecuperarBtn?.addEventListener('click', mostrarRecuperar);
volverLoginBtn?.addEventListener('click', mostrarLogin);

// ------------------- LOGIN -------------------
document.getElementById('formLogin').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const email = document.getElementById('loginEmail').value;
    const contrasenaHash = document.getElementById('loginPassword').value;
    
    // Validación básica
    if (!email || !contrasenaHash) {
        mostrarNotificacion('Por favor completa todos los campos', 'error');
        return;
    }
    
    // Mostrar loading
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Iniciando sesión...';
    submitBtn.disabled = true;
    
    try {
        const res = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, contrasenaHash })
        });
        
        if (!res.ok) {
            const errorText = await res.text();
            mostrarNotificacion('Credenciales inválidas', 'error');
            return;
        }
        
        const data = await res.json(); // RespuestaLoginDTO

        // Guardar token
        localStorage.setItem('jwt_token', data.token);

        // Decodificar JWT y extraer rol/usuario
        const payload = JSON.parse(atob(data.token.split('.')[1]));
        const rawRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
            || payload['role'] 
            || data.rol 
            || data.role 
            || data.rolUsuario;

        // Normalizar a string numérica: "0" usuario, "1" admin
        const roleNum = (rawRole === 1 || rawRole === '1' || rawRole === 'Administrador') ? '1' : '0';
        localStorage.setItem('user_role', roleNum);

        // Intentar persistir el id de usuario para 'mis citas'
        const userId = data.usuarioId 
            || data.userId 
            || data.idUsuario 
            || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] 
            || payload['nameid'] 
            || payload['sub'];
        if (userId) localStorage.setItem('user_id', String(userId));

        mostrarNotificacion('¡Login exitoso!', 'success');
        
        // Pequeño delay para mostrar el mensaje de éxito
        setTimeout(() => {
            redirigirSegunRol(roleNum);
        }, 1000);
        
    } catch (err) {
        console.error('Error en login:', err);
        mostrarNotificacion('Error de conexión. Intenta nuevamente.', 'error');
    } finally {
        // Restaurar botón
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
    }
});

// ------------------- REGISTRO -------------------
document.getElementById('formRegistro').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const nombre = document.getElementById('nombre').value.trim();
    const cedula = document.getElementById('cedula').value.trim();
    const email = document.getElementById('email').value.trim();
    const contrasenaHash = document.getElementById('password').value;
    
    // Validaciones
    if (!nombre || !cedula || !email || !contrasenaHash) {
        mostrarNotificacion('Por favor completa todos los campos', 'error');
        return;
    }
    
    if (cedula.length !== 11 || !/^\d{11}$/.test(cedula)) {
        mostrarNotificacion('La cédula debe tener exactamente 11 dígitos', 'error');
        return;
    }
    
    if (contrasenaHash.length < 6) {
        mostrarNotificacion('La contraseña debe tener al menos 6 caracteres', 'error');
        return;
    }
    
    // Mostrar loading
    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Registrando...';
    submitBtn.disabled = true;
    
    try {
        const res = await fetch(`${API_BASE_URL}/Auth/registrar`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                nombre,
                email,
                contrasenaHash,
                cedula,
                rolUsuario: 0 // 0 = Usuario por defecto
            })
        });
        
        if (!res.ok) {
            const errorText = await res.text();
            mostrarNotificacion('Error en el registro. Verifica que el email y cédula no estén en uso.', 'error');
            return;
        }
        
        mostrarNotificacion('¡Registro exitoso! Ya puedes iniciar sesión.', 'success');
        
        // Limpiar formulario
        document.getElementById('formRegistro').reset();
        
        // Cambiar a login después de un breve delay
        setTimeout(() => {
            mostrarLogin();
        }, 2000);
        
    } catch (err) {
        console.error('Error en registro:', err);
        mostrarNotificacion('Error de conexión. Intenta nuevamente.', 'error');
    } finally {
        // Restaurar botón
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
    }
});


// ------------------- FUNCIONES DE NAVEGACIÓN -------------------
function mostrarLogin() {
    loginForm?.classList.remove('hidden');
    registroForm?.classList.add('hidden');
    recuperarForm?.classList.add('hidden');
}

function mostrarRegistro() {
    registroForm?.classList.remove('hidden');
    loginForm?.classList.add('hidden');
    recuperarForm?.classList.add('hidden');
}

function mostrarRecuperar() {
    recuperarForm?.classList.remove('hidden');
    loginForm?.classList.add('hidden');
    registroForm?.classList.add('hidden');
}

function redirigirSegunRol(rol) {
    if (rol === 0 || rol === '0' || rol === 'Usuario') {
        // Redirigir al panel de usuario
        window.location.href = 'src/PanelUsuario.html';
    } else if (rol === 1 || rol === '1' || rol === 'Administrador') {
        // Redirigir al panel de administrador
        window.location.href = 'src/PanelAdmin.html';
    } else {
        console.error('Rol no reconocido:', rol);
        mostrarNotificacion('Error: Rol de usuario no válido', 'error');
    }
}

// ------------------- DETECCIÓN DE SESIÓN ACTIVA -------------------
document.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('jwt_token');
    
    if (token) {
        try {
            // Verificar si el token es válido
            const payload = JSON.parse(atob(token.split('.')[1]));
            const exp = payload.exp * 1000; // Convertir a milisegundos
            
            // Verifica si el token ha expirado
            if (Date.now() >= exp) {
                console.log('Token expirado, limpiando localStorage');
                localStorage.removeItem('jwt_token');
                localStorage.removeItem('user_role');
                mostrarLogin();
                return;
            }
            
            const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'];
            
            // Si estamos en index.html y hay sesión activa, redirigir
            if (window.location.pathname.endsWith('index.html') || 
                window.location.pathname === '/' || 
                window.location.pathname.endsWith('/')) {
                
                mostrarNotificacion('Sesión activa encontrada, redirigiendo...', 'info');
                setTimeout(() => {
                    // Normalizar rol por si viene como texto
                    const roleNum = (rol === 1 || rol === '1' || rol === 'Administrador') ? '1' : '0';
                    redirigirSegunRol(roleNum);
                }, 1000);
            }
            
        } catch (error) {
            console.error('Error procesando token:', error);
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('user_role');
            mostrarLogin();
        }
    } else {
        // No hay token, mostrar login si estamos en index
        if (window.location.pathname.endsWith('index.html') || 
            window.location.pathname === '/' || 
            window.location.pathname.endsWith('/')) {
            mostrarLogin();
        }
    }
});

// ------------------- SISTEMA DE NOTIFICACIONES -------------------
function mostrarNotificacion(mensaje, tipo = 'info') {
    // Remover notificaciones existentes
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());
    
    const notification = document.createElement('div');
    notification.className = `notification-toast fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg max-w-sm transition-all duration-300 transform translate-x-full`;
    
    let bgColor, textColor, icon;
    switch (tipo) {
        case 'success':
            bgColor = 'bg-green-500';
            textColor = 'text-white';
            icon = 'fas fa-check-circle';
            break;
        case 'error':
            bgColor = 'bg-red-500';
            textColor = 'text-white';
            icon = 'fas fa-exclamation-circle';
            break;
        case 'warning':
            bgColor = 'bg-yellow-500';
            textColor = 'text-white';
            icon = 'fas fa-exclamation-triangle';
            break;
        default:
            bgColor = 'bg-blue-500';
            textColor = 'text-white';
            icon = 'fas fa-info-circle';
    }
    
    notification.className += ` ${bgColor} ${textColor}`;
    notification.innerHTML = `
        <div class="flex items-center">
            <i class="${icon} mr-2"></i>
            <span>${mensaje}</span>
            <button class="ml-4 text-white hover:text-gray-200" onclick="this.parentElement.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    // Animar entrada
    setTimeout(() => {
        notification.classList.remove('translate-x-full');
    }, 100);
    
    // Auto-remove después de 5 segundos
    setTimeout(() => {
        if (notification.parentNode) {
            notification.classList.add('translate-x-full');
            setTimeout(() => {
                notification.remove();
            }, 300);
        }
    }, 5000);
}



// ------------------- PREVENIR ACCESO NO AUTORIZADO -------------------
function verificarAccesoPanel() {
    const token = localStorage.getItem('jwt_token');
    const userRole = localStorage.getItem('user_role');
    
    // Si estamos en una pagina de panel sin token, redirigir a login
    if (window.location.pathname.toLowerCase().includes('panel') && !token) {
        window.location.href = '../index.html';
        return false;
    }
    
    // Verificar rol específico para cada panel
    if (window.location.pathname.includes('PanelUsuario.html') && userRole !== '0' && userRole !== 'Usuario') {
        mostrarNotificacion('Acceso no autorizado', 'error');
        setTimeout(() => {
            window.location.href = '../index.html';
        }, 2000);
        return false;
    }
    
    if (window.location.pathname.includes('PanelAdmin.html') && userRole !== '1' && userRole !== 'Administrador') {
        mostrarNotificacion('Acceso no autorizado', 'error');
        setTimeout(() => {
            window.location.href = '../index.html';
        }, 2000);
        return false;
    }
    
    return true;
}

// Ejecuta verificacion al cargar la página
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', verificarAccesoPanel);
} else {
    verificarAccesoPanel();
}