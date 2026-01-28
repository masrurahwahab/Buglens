
(function() {
    'use strict';

    function isAuthenticated() {
        const token = localStorage.getItem('authToken');

        if (!token) {
            return false;
        }

      
        try {
            const payload = parseJwt(token);
            const currentTime = Math.floor(Date.now() / 1000);

           
            if (payload.exp) {
                console.log('Token expiry:', new Date(payload.exp * 1000));
                console.log('Current time:', new Date());

                if (payload.exp < currentTime) {
                    console.log('Token expired');
                    localStorage.removeItem('authToken');
                    return false;
                }
            }

            return true;
        } catch (error) {
            console.error('Invalid token:', error);
            localStorage.removeItem('authToken');
            return false;
        }
    }

 
    function parseJwt(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(
                atob(base64)
                    .split('')
                    .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
            );

            const parsed = JSON.parse(jsonPayload);
            console.log('Parsed JWT payload:', parsed); 
            return parsed;
        } catch (error) {
            throw new Error('Invalid token format');
        }
    }


    function protectPage() {
        if (!isAuthenticated()) {
            console.log('Not authenticated, redirecting to login...');
           
            const hasBeenAuthenticated = localStorage.getItem('hasBeenAuthenticated');
            if (hasBeenAuthenticated === 'true') {
                alert('Your session has expired. Please login again.');
            }
            sessionStorage.setItem('redirectAfterLogin', window.location.pathname);
            window.location.href = 'login.html';
        } else {
            console.log('User authenticated, loading page...');
            localStorage.setItem('hasBeenAuthenticated', 'true');
            loadUserInfo();
        }
    }

  
    function loadUserInfo() {
        const userName = localStorage.getItem('userName');
        const userEmail = localStorage.getItem('userEmail');

        
        const userNameElement = document.getElementById('userName');
        if (userNameElement && userName) {
            userNameElement.textContent = userName;
        }

       
        const userEmailElement = document.getElementById('userEmail');
        if (userEmailElement && userEmail) {
            userEmailElement.textContent = userEmail;
        }

       
        const userDetailsName = document.querySelector('.user-details .user-name');
        if (userDetailsName && userName) {
            userDetailsName.textContent = userName;
        }
    }

  
    function setupLogoutButton() {
        const logoutBtn = document.getElementById('logoutBtn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', function(e) {
                e.preventDefault();
                if (confirm('Are you sure you want to logout?')) {
                   
                    localStorage.removeItem('authToken');
                    localStorage.removeItem('userEmail');
                    localStorage.removeItem('userName');
                    localStorage.removeItem('userId');
                    localStorage.removeItem('hasBeenAuthenticated');
                    sessionStorage.clear();

                 
                    window.location.href = 'login.html';
                }
            });
        }
    }

  
    function setActiveNavigation() {
        const currentPage = window.location.pathname.split('/').pop() || 'index.html';

       
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
        });

      
        if (currentPage.includes('statistics')) {
            const statsNav = document.querySelector('a[href="statistics.html"]');
            if (statsNav) statsNav.classList.add('active');
        } else if (currentPage.includes('settings')) {
            const settingsNav = document.querySelector('a[href="settings.html"]');
            if (settingsNav) settingsNav.classList.add('active');
        } else if (currentPage.includes('history')) {
            const historyNav = document.querySelector('a[href="history.html"]');
            if (historyNav) historyNav.classList.add('active');
        } else if (currentPage.includes('index') || currentPage === '') {
            const dashboardNav = document.querySelector('a[href="index.html"]');
            if (dashboardNav) dashboardNav.classList.add('active');
        }
    }

    function setupTokenRefresh() {
        setInterval(() => {
            const token = localStorage.getItem('authToken');
            if (token) {
                try {
                    const payload = parseJwt(token);
                    const currentTime = Math.floor(Date.now() / 1000);

                    if (payload.exp && payload.exp < currentTime) {
                        console.log('Session expired during refresh check');
                        localStorage.setItem('hasBeenAuthenticated', 'true');
                        alert('Your session has expired. Please login again.');
                        window.location.href = 'login.html';
                    }
                } catch (error) {
                    console.error('Token validation error:', error);
                }
            }
        }, 5 * 60 * 1000); 
    }

    function initialize() {
        console.log('Auth Guard: Checking authentication...');

       
        protectPage();

      
        setupLogoutButton();

        
        setActiveNavigation();

     
        setupTokenRefresh();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    } else {
        initialize();
    }

   
    window.AuthGuard = {
        isAuthenticated: isAuthenticated,
        logout: function() {
            localStorage.removeItem('authToken');
            localStorage.removeItem('userEmail');
            localStorage.removeItem('userName');
            localStorage.removeItem('userId');
            localStorage.removeItem('hasBeenAuthenticated');
            window.location.href = 'login.html';
        },
        getToken: function() {
            return localStorage.getItem('authToken');
        },
        getUserInfo: function() {
            return {
                email: localStorage.getItem('userEmail'),
                name: localStorage.getItem('userName'),
                id: localStorage.getItem('userId')
            };
        }
    };
})();