
const API_BASE_URL = 'http://localhost:5112/api';

document.getElementById('forgotPasswordForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const submitBtn = document.getElementById('submitBtn');
    const successMsg = document.getElementById('successMessage');
    const errorMsg = document.getElementById('errorMessage');

   
    successMsg.style.display = 'none';
    errorMsg.style.display = 'none';

    
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Sending...';

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/forgot-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email })
        });

        const data = await response.json();

        if (response.ok) {
            document.getElementById('successText').textContent = data.message;
            successMsg.style.display = 'block';
            document.getElementById('forgotPasswordForm').reset();
        } else {
            throw new Error(data.details || data.error || 'Failed to send reset link');
        }
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('errorText').textContent = error.message;
        errorMsg.style.display = 'block';
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="bi bi-send"></i> Send Reset Link';
    }
});