

class BugLensApp {
    constructor() {
        this.currentPage = this.detectCurrentPage();
        console.log('Current page:', this.currentPage);
        this.initializeApp();
    }

    detectCurrentPage() {
        const path = window.location.pathname;
        if (path.includes('statistics')) return 'statistics';
        if (path.includes('settings')) return 'settings';
        if (path.includes('login')) return 'login';
        if (path.includes('register')) return 'register';
        if (path.includes('index') || path === '/') return 'dashboard';
        return 'unknown';
    }

    initializeApp() {
 
        this.initializeCommonElements();

      
        switch(this.currentPage) {
            case 'dashboard':
                this.initializeDashboard();
                break;
            case 'statistics':
                
                break;
            case 'settings':
                this.initializeSettings();
                break;
            case 'login':
                this.initializeLogin();
                break;
            case 'register':
                this.initializeRegister();
                break;
        }
    }

    initializeCommonElements() {
        
        this.bindSidebarToggle();
    }

    bindSidebarToggle() {
        const toggleBtn = document.getElementById('sidebarToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => {
                const sidebar = document.querySelector('.sidebar');
                if (sidebar) {
                    sidebar.classList.toggle('show');
                }
            });
        }
    }

    initializeDashboard() {
        console.log('Initializing dashboard...');

     
        const languageSelect = document.getElementById('language');
        if (languageSelect) {
            languageSelect.addEventListener('change', (e) => {
                console.log('Language changed to:', e.target.value);
            });
        }

       
        const analyzeBtn = document.getElementById('analyzeBtn');
        if (analyzeBtn) {
            analyzeBtn.addEventListener('click', () => {
                this.handleAnalyze();
            });
        }

       
        const fileInput = document.getElementById('codeFile');
        if (fileInput) {
            fileInput.addEventListener('change', (e) => {
                this.handleFileUpload(e);
            });
        }
    }

    initializeSettings() {
        console.log('Initializing settings...');

       
        const saveBtn = document.getElementById('saveSettings');
        if (saveBtn) {
            saveBtn.addEventListener('click', () => {
                this.handleSaveSettings();
            });
        }
    }

    initializeLogin() {
        console.log('Initializing login...');

       
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.handleLogin();
            });
        }
    }

    initializeRegister() {
        console.log('Initializing register...');

       
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.handleRegister();
            });
        }
    }

    handleAnalyze() {
        console.log('Analyzing code...');
       
    }

    handleFileUpload(event) {
        const file = event.target.files[0];
        if (file) {
            console.log('File uploaded:', file.name);
            const reader = new FileReader();
            reader.onload = (e) => {
                const codeInput = document.getElementById('codeInput');
                if (codeInput) {
                    codeInput.value = e.target.result;
                }
            };
            reader.readAsText(file);
        }
    }

    handleSaveSettings() {
        console.log('Saving settings...');
       
    }

    handleLogin() {
        console.log('Logging in...');
      
    }

    handleRegister() {
        console.log('Registering...');
      
    }

  
    static getElement(id) {
        const element = document.getElementById(id);
        if (!element) {
            console.warn(`Element with id '${id}' not found`);
        }
        return element;
    }

  
    static setElementText(id, text) {
        const element = BugLensApp.getElement(id);
        if (element) {
            element.textContent = text;
        }
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.bugLensApp = new BugLensApp();
    });
} else {
    window.bugLensApp = new BugLensApp();
}

if (typeof module !== 'undefined' && module.exports) {
    module.exports = BugLensApp;
}