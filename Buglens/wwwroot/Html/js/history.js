

let allHistory = [];
let filteredHistory = [];
let currentPage = 1;
const itemsPerPage = 10;

const sidebarToggle = document.getElementById('sidebarToggle');

sidebarToggle.addEventListener('click', function () {
    const sidebar = document.querySelector('.sidebar');
    sidebar.classList.toggle('hide');
});


function authFetch(url, options = {}) {
    const token = localStorage.getItem('authToken');

    return fetch(url, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...(options.headers || {})
        }
    });
}

function loadUserNameFromStorage() {
    const userName = localStorage.getItem('userName') || 'Developer';
    const userNameEl = document.querySelector('.user-name');
    if (userNameEl) {
        userNameEl.textContent = userName;
    }
}

async function loadHistory() {
    showLoading();
    hideEmpty();

    try {
        const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.HISTORY}?limit=100`;
        const response = await authFetch(url);

        if (response.status === 401) {
            alert("Session expired. Please login again.");
            window.location.href = "login.html";
            return;
        }

        if (!response.ok) {
            throw new Error('Failed to load history');
        }

        const data = await response.json();
        allHistory = data;
        filteredHistory = data;

        if (data.length === 0) {
            showEmpty();
        } else {
            displayHistory();
        }
    } catch (error) {
        console.error('Error loading history:', error);
        showEmpty();
    } finally {
        hideLoading();
    }
}

function displayHistory() {
    const container = document.getElementById('historyContainer');
    const start = (currentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const pageItems = filteredHistory.slice(start, end);

    if (pageItems.length === 0) {
        showEmpty();
        return;
    }

    container.innerHTML = pageItems.map((item, index) => {
        const serial = start + index + 1; // 1, 2, 3...

        return `
        <div class="history-card" data-id="${item.id}">
            <div class="history-card-header">
                <div class="history-card-title">
                    <span class="history-language-badge">
                        <i class="bi bi-code-square"></i> ${item.language}
                    </span>
                    <span class="history-id">#${serial}</span>
                </div>
                <div class="history-date">
                    <i class="bi bi-clock"></i>
                    ${formatDate(item.createdAt || item.timestamp)}
                </div>
            </div>
            <div class="history-card-body">
                <div class="history-root-cause">
                    <i class="bi bi-exclamation-circle"></i>
                    <span>${item.rootCause}</span>
                </div>
                <div class="history-preview">
                    ${item.explanation.substring(0, 150)}...
                </div>
            </div>
            <div class="history-card-footer">
                <div class="history-tags">
                    <span class="history-tag">
                        <i class="bi bi-check-circle"></i> Resolved
                    </span>
                </div>
                <div class="history-actions">
                    <button class="history-action-btn primary" onclick="viewDetails(${item.id})">
                        <i class="bi bi-eye"></i> View
                    </button>
                    <button class="history-action-btn" onclick="rerunAnalysis(${item.id})">
                        <i class="bi bi-arrow-clockwise"></i> Re-run
                    </button>
                    <button class="history-action-btn" onclick="deleteAnalysis(${item.id})">
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </div>
            </div>
        </div>
        `;
    }).join('');

    updatePagination();
}


async function viewDetails(id) {
    try {
        const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.GET_ANALYSIS}/${id}`;
        const response = await authFetch(url);

        if (response.status === 401) {
            alert("Session expired. Please login again.");
            window.location.href = "login.html";
            return;
        }

        if (!response.ok) {
            throw new Error('Failed to load analysis details');
        }

        const data = await response.json();

    
        document.getElementById('detailLanguage').textContent = data.language;
        document.getElementById('detailDate').textContent = formatDate(data.createdAt || data.timestamp);
        document.getElementById('detailRootCause').textContent = data.rootCause;
        document.getElementById('detailExplanation').textContent = data.explanation;
        document.getElementById('detailFix').textContent = data.fix;
        document.getElementById('detailErrorLogs').textContent = data.errorLogs;
        document.getElementById('detailOriginalCode').textContent = data.sourceCode;
        document.getElementById('detailCorrectedCode').textContent = data.correctedCode;

     
        document.getElementById('rerunAnalysisBtn').setAttribute('data-id', id);

        
        document.getElementById('detailsModal').classList.add('show');
    } catch (error) {
        console.error('Error loading details:', error);
        alert('Failed to load analysis details.');
    }
}


async function rerunAnalysis(id) {
    try {
        const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.GET_ANALYSIS}/${id}`;
        const response = await authFetch(url);

        if (response.status === 401) {
            alert("Session expired. Please login again.");
            window.location.href = "login.html";
            return;
        }

        if (!response.ok) {
            throw new Error('Failed to load analysis');
        }

        const data = await response.json();

    
        sessionStorage.setItem('rerunAnalysis', JSON.stringify({
            language: data.language,
            errorLogs: data.errorLogs,
            sourceCode: data.sourceCode
        }));

        console.log('✅ Re-run data saved to sessionStorage:', {
            language: data.language,
            errorLogsLength: data.errorLogs?.length,
            sourceCodeLength: data.sourceCode?.length
        });

      
        window.location.href = 'index.html';
    } catch (error) {
        console.error('Error re-running analysis:', error);
        alert('Failed to re-run analysis.');
    }
}


async function deleteAnalysis(id) {
    if (!confirm('Are you sure you want to delete this analysis? This action cannot be undone.')) {
        return;
    }

    try {
       
        allHistory = allHistory.filter(item => item.id !== id);
        filteredHistory = filteredHistory.filter(item => item.id !== id);

        if (filteredHistory.length === 0) {
            showEmpty();
        } else {
            displayHistory();
        }

        alert('Analysis deleted successfully.');
    } catch (error) {
        console.error('Error deleting analysis:', error);
        alert('Failed to delete analysis.');
    }
}


document.getElementById('applyFilters').addEventListener('click', function () {
    const language = document.getElementById('filterLanguage').value;
    const dateRange = document.getElementById('filterDate').value;
    const sortBy = document.getElementById('sortBy').value;


    filteredHistory = language
        ? allHistory.filter(item => item.language === language)
        : [...allHistory];

  
    if (dateRange !== 'all') {
        const now = new Date();
        filteredHistory = filteredHistory.filter(item => {
            const itemDate = new Date(item.createdAt || item.timestamp);
            switch (dateRange) {
                case 'today':
                    return itemDate.toDateString() === now.toDateString();
                case 'week':
                    const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                    return itemDate >= weekAgo;
                case 'month':
                    const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
                    return itemDate >= monthAgo;
                case 'year':
                    const yearAgo = new Date(now.getTime() - 365 * 24 * 60 * 60 * 1000);
                    return itemDate >= yearAgo;
                default:
                    return true;
            }
        });
    }

   
    switch (sortBy) {
        case 'date-desc':
            filteredHistory.sort((a, b) => new Date(b.createdAt || b.timestamp) - new Date(a.createdAt || a.timestamp));
            break;
        case 'date-asc':
            filteredHistory.sort((a, b) => new Date(a.createdAt || a.timestamp) - new Date(b.createdAt || a.timestamp));
            break;
        case 'language':
            filteredHistory.sort((a, b) => a.language.localeCompare(b.language));
            break;
    }

    currentPage = 1;
    displayHistory();
});


document.getElementById('searchHistory').addEventListener('input', function (e) {
    const searchTerm = e.target.value.toLowerCase();

    if (!searchTerm) {
        filteredHistory = [...allHistory];
    } else {
        filteredHistory = allHistory.filter(item =>
            item.rootCause.toLowerCase().includes(searchTerm) ||
            item.explanation.toLowerCase().includes(searchTerm) ||
            item.language.toLowerCase().includes(searchTerm)
        );
    }

    currentPage = 1;
    displayHistory();
});


document.getElementById('prevPage').addEventListener('click', function () {
    if (currentPage > 1) {
        currentPage--;
        displayHistory();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
});

document.getElementById('nextPage').addEventListener('click', function () {
    const totalPages = Math.ceil(filteredHistory.length / itemsPerPage);
    if (currentPage < totalPages) {
        currentPage++;
        displayHistory();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
});

function updatePagination() {
    const totalPages = Math.ceil(filteredHistory.length / itemsPerPage);
    document.getElementById('currentPage').textContent = currentPage;
    document.getElementById('totalPages').textContent = totalPages;

    document.getElementById('prevPage').disabled = currentPage === 1;
    document.getElementById('nextPage').disabled = currentPage === totalPages;

    const paginationContainer = document.getElementById('paginationContainer');
    paginationContainer.style.display = totalPages > 1 ? 'flex' : 'none';
}


document.getElementById('rerunAnalysisBtn').addEventListener('click', function () {
    const id = this.getAttribute('data-id');
    rerunAnalysis(id);
});


function showLoading() {
    document.getElementById('historyLoading').style.display = 'block';
    document.getElementById('historyContainer').style.display = 'none';
}

function hideLoading() {
    document.getElementById('historyLoading').style.display = 'none';
    document.getElementById('historyContainer').style.display = 'block';
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

function debugToken() {
    const token = localStorage.getItem('authToken');
    console.log('=== TOKEN DEBUG ===');
    console.log('Raw token:', token);
    if (token) {
        const payload = parseJwt(token);
        console.log('Decoded payload:', payload);
    } else {
        console.log('No token found!');
    }
    console.log('===================');
}

function checkAuth() {
    const token = localStorage.getItem('authToken');
    if (!token) {
        window.location.href = 'login.html';
        return false;
    }

    const payload = parseJwt(token);
    if (!payload || (payload.exp && payload.exp < Math.floor(Date.now() / 1000))) {
        localStorage.removeItem('authToken');
        window.location.href = 'login.html';
        return false;
    }
    return true;
}

function showEmpty() {
    document.getElementById('emptyState').style.display = 'block';
    document.getElementById('historyContainer').style.display = 'none';
    document.getElementById('paginationContainer').style.display = 'none';
}

function hideEmpty() {
    document.getElementById('emptyState').style.display = 'none';
}

function formatDate(timestamp) {
    const date = new Date(timestamp);

    return date.toLocaleString("en-US", {
        year: "numeric",
        month: "short",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
    });
}


window.addEventListener('load', () => {
    loadHistory();
});

document.addEventListener('DOMContentLoaded', async function() {
    console.log('History page initializing...');

    debugToken();

    if (!checkAuth()) return;

    loadUserNameFromStorage();

 
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function() {
            if (confirm('Are you sure you want to logout?')) {
              
                localStorage.clear();
                sessionStorage.clear();

               
                window.location.href = 'welcome.html';
            }
        });
    }

    console.log('History page initialized successfully');
});