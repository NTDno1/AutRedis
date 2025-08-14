// Global variables
let currentUser = null;
let authToken = localStorage.getItem('authToken');

// API base URL
const API_BASE_URL = window.location.origin;

// Utility functions
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} fade-in`;
    alertDiv.textContent = message;
    
    const container = document.querySelector('.main-content') || document.body;
    container.insertBefore(alertDiv, container.firstChild);
    
    setTimeout(() => {
        alertDiv.style.opacity = '0';
        alertDiv.style.transform = 'translateY(-20px)';
        setTimeout(() => alertDiv.remove(), 300);
    }, 5000);
}

function setLoading(button, isLoading) {
    if (isLoading) {
        button.disabled = true;
        button.innerHTML = '<span class="loading"></span> Đang xử lý...';
    } else {
        button.disabled = false;
        button.innerHTML = button.dataset.originalText || 'Submit';
    }
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN');
}

function truncateText(text, maxLength = 50) {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

function getDeviceIcon(userAgent) {
    if (userAgent.includes('Mobile')) return '📱';
    if (userAgent.includes('Tablet')) return '📱';
    if (userAgent.includes('Windows') || userAgent.includes('Mac')) return '💻';
    return '🖥️';
}

// API functions
async function apiCall(endpoint, options = {}) {
    const url = `${API_BASE_URL}/api/${endpoint}`;
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
        },
        ...options
    };

    if (authToken) {
        defaultOptions.headers.Authorization = `Bearer ${authToken}`;
    }

    try {
        const response = await fetch(url, defaultOptions);
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('API call failed:', error);
        throw error;
    }
}

// Authentication functions
async function login(username, password) {
    try {
        const response = await apiCall('auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });

        if (response.success) {
            authToken = response.token;
            localStorage.setItem('authToken', authToken);
            currentUser = {
                username: response.username,
                token: response.token
            };
            
            showAlert('✅ Đăng nhập thành công!', 'success');
            setTimeout(() => {
                showDashboard();
            }, 1000);
        } else {
            showAlert('❌ ' + (response.message || 'Đăng nhập thất bại'), 'error');
        }
        
        return response;
    } catch (error) {
        showAlert('🚨 Lỗi kết nối: ' + error.message, 'error');
        throw error;
    }
}

async function logout() {
    try {
        await apiCall('auth/logout', { method: 'POST' });
        
        authToken = null;
        currentUser = null;
        localStorage.removeItem('authToken');
        
        showAlert('👋 Đăng xuất thành công!', 'success');
        showLoginForm();
    } catch (error) {
        showAlert('🚨 Lỗi đăng xuất: ' + error.message, 'error');
    }
}

async function register(username, email, password) {
    try {
        const response = await apiCall('user/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });

        showAlert('🎉 Đăng ký thành công! Vui lòng đăng nhập.', 'success');
        showLoginForm();
        
        return response;
    } catch (error) {
        showAlert('🚨 Lỗi đăng ký: ' + error.message, 'error');
        throw error;
    }
}

async function getUserProfile() {
    try {
        const user = await apiCall('user/profile');
        return user;
    } catch (error) {
        showAlert('🚨 Lỗi lấy thông tin user: ' + error.message, 'error');
        throw error;
    }
}

async function getActiveSessions() {
    try {
        const sessions = await apiCall('auth/active-sessions');
        return sessions;
    } catch (error) {
        showAlert('🚨 Lỗi lấy danh sách session: ' + error.message, 'error');
        throw error;
    }
}

async function logoutOtherDevices() {
    try {
        await apiCall('auth/logout-other-devices', { method: 'POST' });
        showAlert('🔒 Đã đăng xuất tất cả thiết bị khác!', 'success');
        loadActiveSessions();
    } catch (error) {
        showAlert('🚨 Lỗi đăng xuất thiết bị khác: ' + error.message, 'error');
    }
}

async function validateSession() {
    try {
        const response = await apiCall('auth/validate-session');
        return response.valid;
    } catch (error) {
        return false;
    }
}

// UI functions
function showLoginForm() {
    const mainContent = document.querySelector('.main-content');
    mainContent.innerHTML = `
        <div class="container">
            <div class="auth-container slide-in">
                <div class="text-center mb-3">
                    <div class="device-icon desktop">🔐</div>
                </div>
                <h1 class="auth-title">Đăng nhập</h1>
                <form id="loginForm">
                    <div class="form-group">
                        <label for="username" class="form-label">👤 Tên đăng nhập</label>
                        <input type="text" id="username" class="form-input" required placeholder="Nhập tên đăng nhập">
                    </div>
                    <div class="form-group">
                        <label for="password" class="form-label">🔒 Mật khẩu</label>
                        <input type="password" id="password" class="form-input" required placeholder="Nhập mật khẩu">
                    </div>
                    <button type="submit" class="form-button hover-lift" data-original-text="🚀 Đăng nhập">🚀 Đăng nhập</button>
                </form>
                <div class="text-center mt-3">
                    <a href="#" onclick="showRegisterForm()" class="btn btn-secondary">📝 Chưa có tài khoản? Đăng ký ngay</a>
                </div>
            </div>
        </div>
    `;

    document.getElementById('loginForm').addEventListener('submit', handleLogin);
}

function showRegisterForm() {
    const mainContent = document.querySelector('.main-content');
    mainContent.innerHTML = `
        <div class="container">
            <div class="auth-container slide-in">
                <div class="text-center mb-3">
                    <div class="device-icon desktop">📝</div>
                </div>
                <h1 class="auth-title">Đăng ký</h1>
                <form id="registerForm">
                    <div class="form-group">
                        <label for="regUsername" class="form-label">👤 Tên đăng nhập</label>
                        <input type="text" id="regUsername" class="form-input" required placeholder="Nhập tên đăng nhập">
                    </div>
                    <div class="form-group">
                        <label for="regEmail" class="form-label">📧 Email</label>
                        <input type="email" id="regEmail" class="form-input" required placeholder="Nhập email">
                    </div>
                    <div class="form-group">
                        <label for="regPassword" class="form-label">🔒 Mật khẩu</label>
                        <input type="password" id="regPassword" class="form-input" required placeholder="Nhập mật khẩu">
                    </div>
                    <button type="submit" class="form-button hover-lift" data-original-text="🎉 Đăng ký">🎉 Đăng ký</button>
                </form>
                <div class="text-center mt-3">
                    <a href="#" onclick="showLoginForm()" class="btn btn-secondary">🔐 Đã có tài khoản? Đăng nhập ngay</a>
                </div>
            </div>
        </div>
    `;

    document.getElementById('registerForm').addEventListener('submit', handleRegister);
}

function showDashboard() {
    const mainContent = document.querySelector('.main-content');
    mainContent.innerHTML = `
        <div class="container">
            <div class="dashboard fade-in">
                <div class="dashboard-header">
                    <h1 class="dashboard-title">📊 Dashboard</h1>
                    <div class="user-info">
                        <div class="user-name" id="userName">⏳ Loading...</div>
                        <div class="user-email" id="userEmail">⏳ Loading...</div>
                    </div>
                </div>
                
                <div class="mb-3">
                    <button class="btn btn-primary hover-lift" onclick="loadActiveSessions()">
                        🔄 Làm mới danh sách session
                    </button>
                    <button class="btn btn-danger hover-lift" onclick="logoutOtherDevices()">
                        🚫 Đăng xuất tất cả thiết bị khác
                    </button>
                </div>
                
                <div id="sessionsContainer">
                    <div class="text-center">
                        <span class="loading"></span> Đang tải danh sách session...
                    </div>
                </div>
            </div>
        </div>
    `;

    loadUserProfile();
    loadActiveSessions();
}

function updateNavigation() {
    const navMenu = document.querySelector('.nav-menu');
    if (currentUser) {
        navMenu.innerHTML = `
            <li><a href="#" onclick="showDashboard()">📊 Dashboard</a></li>
            <li><a href="#" onclick="logout()">🚪 Đăng xuất</a></li>
        `;
    } else {
        navMenu.innerHTML = `
            <li><a href="#" onclick="showLoginForm()">🔐 Đăng nhập</a></li>
            <li><a href="#" onclick="showRegisterForm()">📝 Đăng ký</a></li>
        `;
    }
}

async function loadUserProfile() {
    try {
        const user = await getUserProfile();
        document.getElementById('userName').innerHTML = `👤 ${user.username}`;
        document.getElementById('userEmail').innerHTML = `📧 ${user.email}`;
    } catch (error) {
        console.error('Failed to load user profile:', error);
    }
}

async function loadActiveSessions() {
    try {
        const sessions = await getActiveSessions();
        displaySessions(sessions);
    } catch (error) {
        console.error('Failed to load sessions:', error);
        document.getElementById('sessionsContainer').innerHTML = 
            '<div class="alert alert-error">🚨 Không thể tải danh sách session</div>';
    }
}

function displaySessions(sessions) {
    const container = document.getElementById('sessionsContainer');
    
    if (!sessions || sessions.length === 0) {
        container.innerHTML = '<div class="text-center">📭 Không có session nào đang hoạt động</div>';
        return;
    }

    const sessionsHtml = sessions.map(session => {
        const deviceIcon = getDeviceIcon(session.userAgent);
        const isActive = session.isActive && session.expiryTime > new Date();
        
        return `
            <div class="session-card hover-lift">
                <div class="session-header">
                    <h3>${deviceIcon} Session ${session.deviceId.substring(0, 8)}...</h3>
                    <span class="session-status ${isActive ? 'active' : 'expired'}">
                        ${isActive ? '✅ Hoạt động' : '❌ Hết hạn'}
                    </span>
                </div>
                <div class="session-details">
                    <div class="session-detail">
                        <span class="session-detail-label">🖥️ Thiết bị:</span>
                        <span class="session-detail-value">${truncateText(session.userAgent, 40)}</span>
                    </div>
                    <div class="session-detail">
                        <span class="session-detail-label">🌐 IP Address:</span>
                        <span class="session-detail-value">${session.ipAddress}</span>
                    </div>
                    <div class="session-detail">
                        <span class="session-detail-label">⏰ Thời gian đăng nhập:</span>
                        <span class="session-detail-value">${formatDate(session.loginTime)}</span>
                    </div>
                    <div class="session-detail">
                        <span class="session-detail-label">⏳ Hết hạn:</span>
                        <span class="session-detail-value">${formatDate(session.expiryTime)}</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');

    container.innerHTML = `
        <h2 class="mb-2">📱 Danh sách Session đang hoạt động (${sessions.length})</h2>
        <div class="sessions-grid">
            ${sessionsHtml}
        </div>
    `;
}

// Event handlers
async function handleLogin(event) {
    event.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const submitButton = event.target.querySelector('button[type="submit"]');
    
    setLoading(submitButton, true);
    
    try {
        await login(username, password);
    } catch (error) {
        console.error('Login failed:', error);
    } finally {
        setLoading(submitButton, false);
    }
}

async function handleRegister(event) {
    event.preventDefault();
    
    const username = document.getElementById('regUsername').value;
    const email = document.getElementById('regEmail').value;
    const password = document.getElementById('regPassword').value;
    const submitButton = event.target.querySelector('button[type="submit"]');
    
    setLoading(submitButton, true);
    
    try {
        await register(username, email, password);
    } catch (error) {
        console.error('Registration failed:', error);
    } finally {
        setLoading(submitButton, false);
    }
}

// Initialize app
async function initApp() {
    updateNavigation();
    
    if (authToken) {
        try {
            const isValid = await validateSession();
            if (isValid) {
                showDashboard();
            } else {
                localStorage.removeItem('authToken');
                showLoginForm();
            }
        } catch (error) {
            localStorage.removeItem('authToken');
            showLoginForm();
        }
    } else {
        showLoginForm();
    }
}

// Start the app when DOM is loaded
document.addEventListener('DOMContentLoaded', initApp); 