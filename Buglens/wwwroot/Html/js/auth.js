
const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const rememberMe = document.getElementById('rememberMe')?.checked || false;

        await handleLogin(email, password, rememberMe);
    });
}


const signupForm = document.getElementById('signupForm');
if (signupForm) {
    signupForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const fullName = document.getElementById('fullName').value;
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

   
        if (password !== confirmPassword) {
            showError('Passwords do not match');
            return;
        }

        await handleSignup(fullName, email, password, confirmPassword);
    });

    const emailInput = document.getElementById('email');
    if (emailInput) {
        emailInput.addEventListener('blur', async function() {
            await checkEmailAvailability(this.value);
        });
    }

  
    const passwordInput = document.getElementById('password');
    if (passwordInput) {
        passwordInput.addEventListener('input', function() {
            checkPasswordStrength(this.value);
        });
    }


    const confirmPasswordInput = document.getElementById('confirmPassword');
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', function() {
            validateConfirmPassword();
        });
    }
}


async function handleLogin(email, password, rememberMe) {
    showLoading('loginBtn');
    hideError();

    try {
        const response = await fetch(`${API_CONFIG.BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                password: password,
                rememberMe: rememberMe
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.details || data.error || 'Login failed');
        }

       
        localStorage.setItem('authToken', data.token);
        localStorage.setItem('userEmail', data.email);
        localStorage.setItem('userName', data.fullName);
        localStorage.setItem('userId', data.userId);

 
        showSuccess('Login successful! Redirecting...');

      
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 1000);

    } catch (error) {
        showError(error.message);
    } finally {
        hideLoading('loginBtn');
    }
}


async function handleSignup(fullName, email, password, confirmPassword) {
    showLoading('signupBtn');
    hideError();

    try {
        const response = await fetch(`${API_CONFIG.BASE_URL}/Auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                fullName: fullName,
                email: email,
                password: password,
                confirmPassword: confirmPassword
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.details || data.error || 'Registration failed');
        }

        showSuccess('Account created successfully! Redirecting to dashboard...');

       
        localStorage.setItem('authToken', data.token);
        localStorage.setItem('userEmail', data.email);
        localStorage.setItem('userName', data.fullName);
        localStorage.setItem('userId', data.userId);

    
        setTimeout(() => {
            window.location.href = 'index.html';
        }, 2000);

    } catch (error) {
        showError(error.message);
    } finally {
        hideLoading('signupBtn');
    }
}


const googleLoginBtn = document.getElementById('googleLoginBtn');
const googleSignupBtn = document.getElementById('googleSignupBtn');
const githubLoginBtn = document.getElementById('githubLoginBtn');
const githubSignupBtn = document.getElementById('githubSignupBtn');

if (googleLoginBtn) {
    googleLoginBtn.addEventListener('click', async function() {
        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/OAuth/google/url`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.url) {
                window.location.href = data.url;
            } else {
                throw new Error('No OAuth URL returned');
            }
        } catch (error) {
            console.error('Google OAuth error:', error);
            showError(`Failed to initiate Google login: ${error.message}`);
        }
    });
}


if (googleSignupBtn) {
    googleSignupBtn.addEventListener('click', async function() {
        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/OAuth/google/url`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.url) {
                window.location.href = data.url;
            } else {
                throw new Error('No OAuth URL returned');
            }
        } catch (error) {
            console.error('Google OAuth error:', error);
            showError(`Failed to initiate Google signup: ${error.message}`);
        }
    });
}


if (githubLoginBtn) {
    githubLoginBtn.addEventListener('click', async function() {
        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/OAuth/github/url`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.url) {
                window.location.href = data.url;
            } else {
                throw new Error('No OAuth URL returned');
            }
        } catch (error) {
            console.error('GitHub OAuth error:', error);
            showError(`Failed to initiate GitHub login: ${error.message}`);
        }
    });
}


if (githubSignupBtn) {
    githubSignupBtn.addEventListener('click', async function() {
        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}/OAuth/github/url`);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.url) {
                window.location.href = data.url;
            } else {
                throw new Error('No OAuth URL returned');
            }
        } catch (error) {
            console.error('GitHub OAuth error:', error);
            showError(`Failed to initiate GitHub signup: ${error.message}`);
        }
    });
}


function setupPasswordToggle(toggleId, inputId) {
    const toggleBtn = document.getElementById(toggleId);
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function() {
            const input = document.getElementById(inputId);
            const icon = this.querySelector('i');

            if (input && icon) {
                if (input.type === 'password') {
                    input.type = 'text';
                    icon.classList.remove('bi-eye');
                    icon.classList.add('bi-eye-slash');
                } else {
                    input.type = 'password';
                    icon.classList.remove('bi-eye-slash');
                    icon.classList.add('bi-eye');
                }
            }
        });
    }
}

setupPasswordToggle('togglePassword', 'password');
setupPasswordToggle('toggleConfirmPassword', 'confirmPassword');


async function checkEmailAvailability(email) {
    if (!email || !email.includes('@')) return;

    try {
        const response = await fetch(`${API_CONFIG.BASE_URL}/Auth/check-email?email=${encodeURIComponent(email)}`);
        const data = await response.json();

        const feedback = document.getElementById('emailFeedback');
        if (feedback) {
            if (data.exists) {
                feedback.className = 'form-feedback feedback-error';
                feedback.innerHTML = '<i class="bi bi-x-circle"></i> This email is already registered';
            } else {
                feedback.className = 'form-feedback feedback-success';
                feedback.innerHTML = '<i class="bi bi-check-circle"></i> Email is available';
            }
        }
    } catch (error) {
        console.error('Error checking email:', error);
    }
}


function checkPasswordStrength(password) {
    const strengthBar = document.getElementById('passwordStrength');
    if (!strengthBar) return;

    let strength = 0;

    if (password.length >= 6) strength++;
    if (password.length >= 10) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;

    let strengthClass = '';
    let strengthText = '';

    if (strength <= 2) {
        strengthClass = 'strength-weak';
        strengthText = 'Weak';
    } else if (strength <= 4) {
        strengthClass = 'strength-medium';
        strengthText = 'Medium';
    } else {
        strengthClass = 'strength-strong';
        strengthText = 'Strong';
    }

    strengthBar.innerHTML = `<div class="password-strength-bar ${strengthClass}"><span>${strengthText}</span></div>`;
}

function validateConfirmPassword() {
    const password = document.getElementById('password');
    const confirmPassword = document.getElementById('confirmPassword');
    const feedback = document.getElementById('confirmPasswordFeedback');

    if (!password || !confirmPassword || !feedback) return;

    if (!confirmPassword.value) {
        feedback.innerHTML = '';
        return;
    }

    if (password.value === confirmPassword.value) {
        feedback.className = 'form-feedback feedback-success';
        feedback.innerHTML = '<i class="bi bi-check-circle"></i> Passwords match';
    } else {
        feedback.className = 'form-feedback feedback-error';
        feedback.innerHTML = '<i class="bi bi-x-circle"></i> Passwords do not match';
    }
}


function showLoading(btnId) {
    const btn = document.getElementById(btnId);
    if (!btn) return;

    btn.disabled = true;
    const btnText = btn.querySelector('.btn-text');
    const btnLoader = btn.querySelector('.btn-loader');

    if (btnText) btnText.style.display = 'none';
    if (btnLoader) btnLoader.style.display = 'flex';
}

function hideLoading(btnId) {
    const btn = document.getElementById(btnId);
    if (!btn) return;

    btn.disabled = false;
    const btnText = btn.querySelector('.btn-text');
    const btnLoader = btn.querySelector('.btn-loader');

    if (btnText) btnText.style.display = 'block';
    if (btnLoader) btnLoader.style.display = 'none';
}

function showError(message) {
    const errorAlert = document.getElementById('errorAlert');
    const errorMessage = document.getElementById('errorMessage');

    if (errorAlert && errorMessage) {
        errorMessage.textContent = message;
        errorAlert.style.display = 'flex';
        errorAlert.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

function hideError() {
    const errorAlert = document.getElementById('errorAlert');
    if (errorAlert) {
        errorAlert.style.display = 'none';
    }
}


function showSuccess(message) {
    const successAlert = document.getElementById('successAlert');
    if (successAlert) {
        const successMessage = document.getElementById('successMessage');
        if (successMessage) {
            successMessage.textContent = message;
            successAlert.style.display = 'flex';
            successAlert.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
}

const urlParams = new URLSearchParams(window.location.search);
const error = urlParams.get('error');
if (error) {
    showError(decodeURIComponent(error));
}


const token = urlParams.get('token');
const email = urlParams.get('email');
const fullName = urlParams.get('fullName');
const userId = urlParams.get('userId');

if (token && email) {
 
    localStorage.setItem('authToken', token);
    localStorage.setItem('userEmail', email);
    if (fullName) localStorage.setItem('userName', fullName);
    if (userId) localStorage.setItem('userId', userId);

   
    window.history.replaceState({}, document.title, window.location.pathname);
    showSuccess('Login successful! Redirecting...');
    setTimeout(() => {
        window.location.href = 'index.html';
    }, 1000);
}

