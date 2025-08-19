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


   const email = document.getElementById('loginEmail').value.trim();
   const contrasenaHash = document.getElementById('loginPassword').value.trim();


   if (!email || !contrasenaHash) {
       mostrarNotificacion('Por favor completa todos los campos', 'error');
       return;
   }


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
           mostrarNotificacion('Credenciales inválidas', 'error');
           return;
       }


       const data = await res.json();


       // Guardar token
       localStorage.setItem('jwt_token', data.token);


       // Extraer rol del endpoint
       const rawRole = data.rol; // 1=admin, 0=usuario
       const roleNum = String(rawRole) === '1' ? '1' : '0';
       localStorage.setItem('user_role', roleNum);


       // Guardar id de usuario
       if (data.usuarioId) localStorage.setItem('user_id', String(data.usuarioId));


       mostrarNotificacion('¡Login exitoso!', 'success');


       setTimeout(() => {
           redirigirSegunRol(roleNum);
       }, 1000);


   } catch (err) {
       console.error('Error en login:', err);
       mostrarNotificacion('Error de conexión. Intenta nuevamente.', 'error');
   } finally {
       submitBtn.innerHTML = originalText;
       submitBtn.disabled = false;
   }
});


// ------------------- REDIRECCIÓN SEGÚN ROL -------------------
function redirigirSegunRol(rol) {
   if (rol === '0') {
       window.location.href = '/src/PanelUsuario.html';
   } else if (rol === '1') {
       window.location.href = '/src/PanelAdmin.html';
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
           const payload = JSON.parse(atob(token.split('.')[1]));
           const exp = payload.exp * 1000;


           if (Date.now() >= exp) {
               localStorage.removeItem('jwt_token');
               localStorage.removeItem('user_role');
               localStorage.removeItem('user_id');
               mostrarLogin();
               return;
           }


           // Normalizar rol
           const rol = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                       || payload['role']
                       || localStorage.getItem('user_role');
           const roleNum = (String(rol).toLowerCase().includes('admin') || String(rol) === '1') ? '1' : '0';


           // Redirigir si estamos en index
           if (window.location.pathname.endsWith('index.html') || window.location.pathname === '/' || window.location.pathname.endsWith('/')) {
               mostrarNotificacion('Sesión activa encontrada, redirigiendo...', 'info');
               setTimeout(() => redirigirSegunRol(roleNum), 1000);
           }


       } catch (error) {
           console.error('Error procesando token:', error);
           localStorage.removeItem('jwt_token');
           localStorage.removeItem('user_role');
           localStorage.removeItem('user_id');
           mostrarLogin();
       }
   } else {
       if (window.location.pathname.endsWith('index.html') || window.location.pathname === '/' || window.location.pathname.endsWith('/')) {
           mostrarLogin();
       }
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


// ------------------- SISTEMA DE NOTIFICACIONES -------------------
function mostrarNotificacion(mensaje, tipo = 'info') {
   const existingNotifications = document.querySelectorAll('.notification-toast');
   existingNotifications.forEach(n => n.remove());


   const notification = document.createElement('div');
   notification.className = `notification-toast fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg max-w-sm transition-all duration-300 transform translate-x-full`;


   let bgColor, textColor, icon;
   switch (tipo) {
       case 'success':
           bgColor = 'bg-green-500'; textColor = 'text-white'; icon = 'fas fa-check-circle'; break;
       case 'error':
           bgColor = 'bg-red-500'; textColor = 'text-white'; icon = 'fas fa-exclamation-circle'; break;
       case 'warning':
           bgColor = 'bg-yellow-500'; textColor = 'text-white'; icon = 'fas fa-exclamation-triangle'; break;
       default:
           bgColor = 'bg-blue-500'; textColor = 'text-white'; icon = 'fas fa-info-circle';
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
   setTimeout(() => notification.classList.remove('translate-x-full'), 100);
   setTimeout(() => {
       if (notification.parentNode) {
           notification.classList.add('translate-x-full');
           setTimeout(() => notification.remove(), 300);
       }
   }, 5000);
}
