
console.log('config.js loaded at:', new Date().toISOString());
const API_CONFIG = {
    BASE_URL: 'http://localhost:5112/api',

    ENDPOINTS: {
        ANALYZE: '/Analysis',
        HISTORY: '/Analysis',
        GET_ANALYSIS: '/Analysis',
        DEMO: '/Analysis/demo',
        LOGIN: '/Auth/login',
        REGISTER: '/Auth/register',
        CHECK_EMAIL: '/Auth/check-email',
        GOOGLE_AUTH_URL: '/OAuth/google/url',     
        GITHUB_AUTH_URL: '/OAuth/github/url',      
        GOOGLE_LOGIN: '/Auth/google-login',
        GITHUB_LOGIN: '/Auth/github-login'
    },

    TIMEOUT: 30000,
    MAX_RETRIES: 1
};


const DEMO_DATA = null;


const VALIDATION = {
    MAX_LOG_SIZE: 50000,
    MAX_CODE_SIZE: 100000,
    MIN_LOG_LENGTH: 10,
    MIN_CODE_LENGTH: 10
};


const MESSAGES = {
    ANALYZING: 'Analyzing with Gemini AI...',
    SUCCESS: 'Analysis complete!',
    ERROR: {
        EMPTY_INPUT: 'Please provide both error logs and source code.',
        TOO_LARGE: 'Input exceeds maximum size limit.',
        API_FAILED: 'Failed to analyze. Please try again.',
        NETWORK: 'Network error. Please check your connection.',
        TIMEOUT: 'Request timed out. Please try again.',
        INVALID_RESPONSE: 'Received invalid response from server.'
    }
};