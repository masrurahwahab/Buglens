const API_BASE_URL = 'http://localhost:5112/api';

const sidebarToggle = document.getElementById('sidebarToggle');

sidebarToggle.addEventListener('click', function () {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('hide');
});

function getAuthToken() {
    return localStorage.getItem('authToken');
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
        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error parsing JWT:', error);
        return null;
    }
}

function checkAuth() {
    const token = getAuthToken();
    if (!token) {
        console.log('No token found, redirecting to login...');
        window.location.href = 'login.html';
        return false;
    }

   
    try {
        const payload = parseJwt(token);
        if (!payload) {
            console.log('Invalid token format, redirecting to login...');
            localStorage.removeItem('authToken');
            window.location.href = 'login.html';
            return false;
        }

        const currentTime = Math.floor(Date.now() / 1000);

        console.log('Token expiration:', payload.exp);
        console.log('Current time:', currentTime);
        console.log('Token expires at:', new Date(payload.exp * 1000));
        console.log('Current time:', new Date());

        if (payload.exp && payload.exp < currentTime) {
            console.log('Token expired, redirecting to login...');
            localStorage.removeItem('authToken');
            localStorage.removeItem('userEmail');
            localStorage.removeItem('userName');
            localStorage.removeItem('userId');
            alert('Your session has expired. Please login again.');
            window.location.href = 'login.html';
            return false;
        }

        console.log('Token is valid');
        return true;
    } catch (error) {
        console.error('Error validating token:', error);
        localStorage.removeItem('authToken');
        window.location.href = 'login.html';
        return false;
    }
}

function debugToken() {
    const token = getAuthToken();
    console.log('=== TOKEN DEBUG ===');
    console.log('Raw token:', token);

    if (token) {
        const payload = parseJwt(token);
        console.log('Decoded payload:', payload);
        console.log('Token claims:', {
            sub: payload.sub,
            email: payload.email,
            name: payload.name,
            role: payload.role,
            exp: payload.exp,
            iss: payload.iss,
            aud: payload.aud,
            jti: payload.jti
        });

        // Check expiration
        const now = Math.floor(Date.now() / 1000);
        const expiresIn = payload.exp - now;
        console.log(`Token expires in ${expiresIn} seconds (${Math.floor(expiresIn / 3600)} hours)`);
    } else {
        console.log('No token found!');
    }
    console.log('===================');
}

async function apiRequest(endpoint, options = {}) {
    const token = getAuthToken();

    if (!token) {
        window.location.href = 'login.html';
        throw new Error('No authentication token');
    }

    const defaultOptions = {
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        });

        if (response.status === 401) {
            console.error('401 Unauthorized - token invalid or expired');
            localStorage.removeItem('authToken');
            localStorage.removeItem('userEmail');
            localStorage.removeItem('userName');
            localStorage.removeItem('userId');
            alert('Session expired. Please login again.');
            window.location.href = 'login.html';
            throw new Error('Unauthorized');
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.details || errorData.error || `Request failed with status ${response.status}`);
        }

        return response.json();
    } catch (error) {
        console.error('API Request failed:', error);
        throw error;
    }
}

async function loadDashboardStats() {
    try {
        console.log('Loading dashboard statistics from backend...');

        
        const history = await apiRequest('/Analysis?limit=100');

        console.log('Analysis history:', history);

       
        const totalAnalyses = history.length || 0;
        const resolvedBugs = totalAnalyses;


        let avgTime = 0;
        if (history.length > 0) {
            const times = history.map(item => {
                if (item.createdAt && item.updatedAt) {
                    const created = new Date(item.createdAt);
                    const updated = new Date(item.updatedAt);
                    return (updated - created) / 1000;
                }
                return 2.5;
            });
            avgTime = (times.reduce((a, b) => a + b, 0) / times.length).toFixed(1);
        }


        updateStatsUI({
            totalAnalyses,
            resolvedBugs,
            avgTime
        });

    } catch (error) {
        console.error('Error loading dashboard stats:', error);
        // Set stats to 0 on error
        updateStatsUI({
            totalAnalyses: 0,
            resolvedBugs: 0,
            avgTime: '0'
        });
    }
}

function updateStatsUI(stats) {
    const totalEl = document.getElementById('totalAnalyses');
    const resolvedEl = document.getElementById('resolvedBugs');
    const avgTimeEl = document.getElementById('avgTime');
    const languagesEl = document.getElementById('languages');

    if (totalEl) totalEl.textContent = stats.totalAnalyses;
    if (resolvedEl) resolvedEl.textContent = stats.resolvedBugs;
    if (avgTimeEl) avgTimeEl.textContent = stats.avgTime + 's';
    if (languagesEl) languagesEl.textContent = '50+'; 
}


const PROGRAMMING_LANGUAGES = [
    { name: 'Ada', category: 'Compiled', icon: 'code-square' },
    { name: 'Assembly', category: 'Low-level', icon: 'cpu' },
    { name: 'Bash', category: 'Shell', icon: 'terminal' },
    { name: 'C', category: 'Compiled', icon: 'code-square' },
    { name: 'C#', category: 'Compiled', icon: 'code-square' },
    { name: 'C++', category: 'Compiled', icon: 'code-square' },
    { name: 'Clojure', category: 'Functional', icon: 'braces' },
    { name: 'COBOL', category: 'Compiled', icon: 'code-square' },
    { name: 'Crystal', category: 'Compiled', icon: 'code-square' },
    { name: 'Dart', category: 'Compiled', icon: 'code-square' },
    { name: 'Delphi', category: 'Compiled', icon: 'code-square' },
    { name: 'Elixir', category: 'Functional', icon: 'braces' },
    { name: 'Erlang', category: 'Functional', icon: 'braces' },
    { name: 'F#', category: 'Functional', icon: 'braces' },
    { name: 'Fortran', category: 'Compiled', icon: 'code-square' },
    { name: 'Go', category: 'Compiled', icon: 'code-square' },
    { name: 'Groovy', category: 'Scripting', icon: 'code-slash' },
    { name: 'Haskell', category: 'Functional', icon: 'braces' },
    { name: 'Java', category: 'Compiled', icon: 'code-square' },
    { name: 'JavaScript', category: 'Scripting', icon: 'code-slash' },
    { name: 'Julia', category: 'Mathematical', icon: 'calculator' },
    { name: 'Kotlin', category: 'Compiled', icon: 'code-square' },
    { name: 'Lua', category: 'Scripting', icon: 'code-slash' },
    { name: 'MATLAB', category: 'Mathematical', icon: 'calculator' },
    { name: 'Nim', category: 'Compiled', icon: 'code-square' },
    { name: 'Objective-C', category: 'Compiled', icon: 'code-square' },
    { name: 'OCaml', category: 'Functional', icon: 'braces' },
    { name: 'Pascal', category: 'Compiled', icon: 'code-square' },
    { name: 'Perl', category: 'Scripting', icon: 'code-slash' },
    { name: 'PHP', category: 'Scripting', icon: 'code-slash' },
    { name: 'PL/SQL', category: 'Query', icon: 'database' },
    { name: 'PowerShell', category: 'Shell', icon: 'terminal' },
    { name: 'Python', category: 'Scripting', icon: 'code-slash' },
    { name: 'R', category: 'Statistical', icon: 'graph-up' },
    { name: 'Racket', category: 'Functional', icon: 'braces' },
    { name: 'Ruby', category: 'Scripting', icon: 'code-slash' },
    { name: 'Rust', category: 'Compiled', icon: 'code-square' },
    { name: 'Scala', category: 'Compiled', icon: 'code-square' },
    { name: 'Scheme', category: 'Functional', icon: 'braces' },
    { name: 'Shell', category: 'Shell', icon: 'terminal' },
    { name: 'SQL', category: 'Query', icon: 'database' },
    { name: 'Swift', category: 'Compiled', icon: 'code-square' },
    { name: 'T-SQL', category: 'Query', icon: 'database' },
    { name: 'TypeScript', category: 'Scripting', icon: 'code-slash' },
    { name: 'V', category: 'Compiled', icon: 'code-square' },
    { name: 'VB.NET', category: 'Compiled', icon: 'code-square' },
    { name: 'Verilog', category: 'Hardware', icon: 'cpu' },
    { name: 'VHDL', category: 'Hardware', icon: 'cpu' },
    { name: 'Visual Basic', category: 'Compiled', icon: 'code-square' },
    { name: 'Zig', category: 'Compiled', icon: 'code-square' }
];

let selectedLanguage = 'C#';

function initLanguageDropdown() {
    const languageSelect = document.getElementById('language');
    if (!languageSelect) return;

   
    languageSelect.innerHTML = '';

  
    PROGRAMMING_LANGUAGES.forEach(lang => {
        const option = document.createElement('option');
        option.value = lang.name;
        option.textContent = `${lang.name} (${lang.category})`;

        
        if (lang.name === selectedLanguage) {
            option.selected = true;
        }

        languageSelect.appendChild(option);
    });

    console.log(`✅ Loaded ${PROGRAMMING_LANGUAGES.length} programming languages`);
}


const debugForm = document.getElementById('debugForm');
if (debugForm) {
    debugForm.addEventListener('submit', async function(e) {
        e.preventDefault();

        const language = document.getElementById('language')?.value;
        const errorLogs = document.getElementById('errorLogs')?.value;
        const sourceCode = document.getElementById('sourceCode')?.value;

       
        if (!language || !errorLogs || !sourceCode) {
            showNotification('Please fill in all required fields', 'warning');
            return;
        }

        const analyzeBtn = document.getElementById('analyzeBtn');
        const originalHTML = analyzeBtn?.innerHTML;
        if (analyzeBtn) {
            analyzeBtn.disabled = true;
            analyzeBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Analyzing...';
        }

   
        const resultsSection = document.getElementById('resultsSection');
        if (resultsSection) resultsSection.style.display = 'none';

        try {
            console.log('Sending analysis request...');

            const data = await apiRequest('/Analysis', {
                method: 'POST',
                body: JSON.stringify({
                    language,
                    errorLogs,
                    sourceCode
                })
            });

            console.log('Analysis completed:', data);

          
            displayAnalysisResults(data);

         
            await loadDashboardStats();

            showNotification('Analysis completed successfully!', 'success');

        } catch (error) {
            console.error('Analysis error:', error);
            showNotification(error.message || 'Analysis failed. Please try again.', 'danger');
        } finally {
            
            if (analyzeBtn) {
                analyzeBtn.disabled = false;
                analyzeBtn.innerHTML = originalHTML;
            }
        }
    });
}
function displayAnalysisResults(data) {
    console.log('=== ANALYSIS RESULTS DEBUG ===');
    console.log('Full response:', data);
    console.log('CorrectedCode raw:', data.correctedCode?.substring(0, 200));
    console.log('==============================');

    const rootCauseEl = document.getElementById('rootCause');
    const explanationEl = document.getElementById('explanation');
    const fixEl = document.getElementById('fix');
    const originalCodeEl = document.getElementById('originalCode');
    const correctedCodeEl = document.getElementById('correctedCode');

    
    if (rootCauseEl) rootCauseEl.textContent = data.rootCause || 'No root cause identified';
    if (explanationEl) explanationEl.textContent = data.explanation || 'No explanation provided';
    if (fixEl) fixEl.textContent = data.fix || 'No fix suggested';

   
    if (originalCodeEl) {
        originalCodeEl.innerText = data.sourceCode || 'No original code';
    }

    if (correctedCodeEl) {
        if (data.correctedCode && data.correctedCode.trim().length > 0) {
           
            let correctedCode = data.correctedCode;

          
            if (correctedCode.includes('\\n')) {
                correctedCode = correctedCode.replace(/\\n/g, '\n');
            }

         
            if (correctedCode.includes('\\t')) {
                correctedCode = correctedCode.replace(/\\t/g, '    ');
            }

            correctedCodeEl.innerText = correctedCode;
            console.log('✅ Corrected code set, first 200 chars:', correctedCode.substring(0, 200));
        } else {
            correctedCodeEl.innerText = '// No corrected code provided by AI';
            console.warn('⚠️ No corrected code in response');
        }
    }

    document.querySelectorAll('.code-tab').forEach(tab => {
        tab.addEventListener('click', () => {
            const target = tab.dataset.target;

            const originalPanel = document.getElementById('originalPanel');
            const correctedPanel = document.getElementById('correctedPanel');

           
            document.querySelectorAll('.code-tab').forEach(t =>
                t.classList.remove('active')
            );

            tab.classList.add('active');

           
            if (target === 'original') {
                originalPanel.style.display = 'block';
                correctedPanel.style.display = 'none';
            } else {
                originalPanel.style.display = 'none';
                correctedPanel.style.display = 'block';
            }
        });
    });
    
    const resultsSection = document.getElementById('resultsSection');
    if (resultsSection) {
        resultsSection.style.display = 'block';
        resultsSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}
const demoBtn = document.getElementById('demoBtn');
if (demoBtn) {
    demoBtn.addEventListener('click', async function() {
        try {
            console.log('Loading demo data from backend...');

            const demoData = await apiRequest('/Analysis/demo');

            console.log('Demo data loaded:', demoData);

            const languageEl = document.getElementById('language');
            const errorLogsEl = document.getElementById('errorLogs');
            const sourceCodeEl = document.getElementById('sourceCode');

            if (languageEl) languageEl.value = demoData.language;
            if (errorLogsEl) errorLogsEl.value = demoData.errorLogs;
            if (sourceCodeEl) sourceCodeEl.value = demoData.sourceCode;

            selectedLanguage = demoData.language;

            showNotification('Demo data loaded successfully!', 'success');

        } catch (error) {
            console.error('Error loading demo:', error);
            showNotification('Failed to load demo data: ' + error.message, 'danger');
        }
    });
}

function showNotification(message, type = 'info') {
    const existingNotifications = document.querySelectorAll('.notification-toast');
    existingNotifications.forEach(n => n.remove());

    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed notification-toast`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alertDiv);

    setTimeout(() => alertDiv.remove(), 5000);
}



document.querySelectorAll('.code-tab').forEach(tab => {
    tab.addEventListener('click', function() {
        const targetTab = this.dataset.tab;

        document.querySelectorAll('.code-tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.code-panel').forEach(p => p.classList.remove('active'));

        this.classList.add('active');
        const panel = document.getElementById(targetTab + 'Panel');
        if (panel) panel.classList.add('active');
    });
});


const clearBtn = document.getElementById('clearBtn');
if (clearBtn) {
    clearBtn.addEventListener('click', function() {
        if (confirm('Are you sure you want to clear all inputs?')) {
            const debugForm = document.getElementById('debugForm');
            if (debugForm) debugForm.reset();
        }
    });
}


const newAnalysisBtn = document.getElementById('newAnalysisBtn');
if (newAnalysisBtn) {
    newAnalysisBtn.addEventListener('click', function() {
        const resultsSection = document.getElementById('resultsSection');
        if (resultsSection) resultsSection.style.display = 'none';

        const debugForm = document.getElementById('debugForm');
        if (debugForm) debugForm.reset();

        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
}

const downloadBtn = document.getElementById('downloadBtn');
if (downloadBtn) {
    downloadBtn.addEventListener('click', function() {
        const rootCause = document.getElementById('rootCause')?.textContent || '';
        const explanation = document.getElementById('explanation')?.textContent || '';
        const fix = document.getElementById('fix')?.textContent || '';
        const correctedCode = document.getElementById('correctedCode')?.textContent || '';

        const content = `BUGLENS ANALYSIS REPORT
========================

ROOT CAUSE:
${rootCause}

EXPLANATION:
${explanation}

SUGGESTED FIX:
${fix}

CORRECTED CODE:
${correctedCode}

Generated by BugLens - ${new Date().toLocaleString()}
`;

        const blob = new Blob([content], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `buglens-report-${Date.now()}.txt`;
        a.click();
        window.URL.revokeObjectURL(url);
    });
}


const copyResultBtn = document.getElementById('copyResultBtn');
if (copyResultBtn) {
    copyResultBtn.addEventListener('click', function() {
        const rootCause = document.getElementById('rootCause')?.textContent || '';
        const explanation = document.getElementById('explanation')?.textContent || '';
        const fix = document.getElementById('fix')?.textContent || '';

        const text = `Root Cause: ${rootCause}\n\nExplanation: ${explanation}\n\nSuggested Fix: ${fix}`;

        navigator.clipboard.writeText(text).then(() => {
            showNotification('Results copied to clipboard!', 'success');
        }).catch(err => {
            console.error('Copy failed:', err);
            showNotification('Failed to copy results', 'danger');
        });
    });
}

document.addEventListener('keydown', function(e) {
   
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        const errorLogsEl = document.getElementById('errorLogs');
        if (errorLogsEl) errorLogsEl.focus();
    }

    // Ctrl/Cmd + Enter to submit form
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        e.preventDefault();
        const analyzeBtn = document.getElementById('analyzeBtn');
        if (analyzeBtn && !analyzeBtn.disabled) analyzeBtn.click();
    }
});


function loadRerunData() {
    const rerunData = sessionStorage.getItem('rerunAnalysis');

    if (rerunData) {
        try {
            const data = JSON.parse(rerunData);

            console.log('✅ Re-run data found! Auto-populating form...');
            console.log('Language:', data.language);
            console.log('Error logs length:', data.errorLogs?.length);
            console.log('Source code length:', data.sourceCode?.length);

           
            const languageEl = document.getElementById('language');
            const errorLogsEl = document.getElementById('errorLogs');
            const sourceCodeEl = document.getElementById('sourceCode');

            if (languageEl && data.language) {
                languageEl.value = data.language;
                console.log('✅ Language set to:', data.language);
            }

            if (errorLogsEl && data.errorLogs) {
                errorLogsEl.value = data.errorLogs;
                console.log('✅ Error logs populated');
            }

            if (sourceCodeEl && data.sourceCode) {
                sourceCodeEl.value = data.sourceCode;
                console.log('✅ Source code populated');
            }

           
            sessionStorage.removeItem('rerunAnalysis');
            console.log('✅ Re-run data cleared from sessionStorage');

          
            showNotification('Previous analysis data loaded! Ready to re-run.', 'info');

           
            setTimeout(() => {
                const debugForm = document.getElementById('debugForm');
                if (debugForm) {
                    debugForm.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }, 500);

        } catch (error) {
            console.error('Error loading re-run data:', error);
            sessionStorage.removeItem('rerunAnalysis');
        }
    } else {
        console.log('No re-run data found in sessionStorage');
    }
}


document.addEventListener('DOMContentLoaded', async function() {
    console.log('Dashboard initializing...');

   
    debugToken();

  
    if (!checkAuth()) return;

    
    initLanguageDropdown();

  
    loadRerunData();

   
    await loadDashboardStats();

  
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function() {
            if (confirm('Are you sure you want to logout?')) {
              
                localStorage.clear();
                sessionStorage.clear();

              
                window.location.href = '/welcome.html';
            }
        });
    }

    console.log('Dashboard initialized successfully');
});