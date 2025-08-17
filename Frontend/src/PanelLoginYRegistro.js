const API_BASE_URL = 'http://localhost:5256'; 

const registroForm = document.getElementById('registroForm');
const loginForm = document.getElementById('loginForm');
const adminPanel = document.getElementById('adminPanel');
const mostrarLoginBtn = document.getElementById('mostrarLogin');
const mostrarRegistroBtn = document.getElementById('mostrarRegistro');
const cerrarSesionBtn = document.getElementById('cerrarSesion');

// Verifica si ya hay una sesion activa al cargar la página
document.addEventListener('DOMContentLoaded', () => {
    checkAuthStatus();
    setupEventListeners();
});

// Configura event listeners
function setupEventListeners() {
    // Formulario de registro
    document.getElementById('formRegistro').addEventListener('submit', handleRegistro);
    
    // Formulario de login
    document.getElementById('formLogin').addEventListener('submit', handleLogin);
    
    // Botones de navegacion entre formularios
    mostrarLoginBtn.addEventListener('click', () => toggleForms('login'));
    mostrarRegistroBtn.addEventListener('click', () => toggleForms('registro'));
    
    // Boton de cerrar sesion
    cerrarSesionBtn.addEventListener('click', cerrarSesion);
}

// Maneja el formulario de registro
async function handleRegistro(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    const userData = {
        nombre: formData.get('nombre'),
        email: formData.get('email'),
        contrasenaHash: formData.get('password'), 
        rolUsuario: 0 
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/registrar`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            const result = await response.text(); 
            alert('Usuario registrado exitosamente');
            toggleForms('login');
            e.target.reset();
        } else {
            const error = await response.text(); 
            alert(`Error en el registro: ${error}`);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error de conexión. Verifica que el servidor esté funcionando.');
    }
}

// Maneja el formulario de login
async function handleLogin(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    const loginData = {
        email: formData.get('email'),
        contrasenaHash: formData.get('password') 
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(loginData)
        });
        
        if (response.ok) {
            const result = await response.json();
            
            // Guarda el token en localStorage
            localStorage.setItem('jwt_token', result.token);
            
            // Muestra el panel de administrador
            showAdminPanel();
        } else {
            const error = await response.text();
            alert(`Error en el login: ${error}`);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error de conexión. Verifica que el servidor esté funcionando.');
    }
}

// Panel de admin
function showAdminPanel() {
    // Oculta todos los formularios
    registroForm.classList.add('hidden');
    loginForm.classList.add('hidden');
    
    // Muestra el panel de admin
    adminPanel.classList.remove('hidden');
}

// Verifica el estado de la autenticacion
function checkAuthStatus() {
    const token = localStorage.getItem('jwt_token');
    
    if (token) {
        // Si hay token, muestra el panel de admin
        showAdminPanel();
    }
}

// Cambia entre formularios
function toggleForms(formType) {
    if (formType === 'login') {
        registroForm.classList.add('hidden');
        loginForm.classList.remove('hidden');
    } else {
        loginForm.classList.add('hidden');
        registroForm.classList.remove('hidden');
    }
}

// Cerrar sesion
function cerrarSesion() {
    // Limpiar localStorage
    localStorage.removeItem('jwt_token');
    
    // Oculta el panel de admin
    adminPanel.classList.add('hidden');
    
    // Muestra el formulario de registro
    registroForm.classList.remove('hidden');
    
    // Limpia formularios
    document.getElementById('formRegistro').reset();
    document.getElementById('formLogin').reset();
}