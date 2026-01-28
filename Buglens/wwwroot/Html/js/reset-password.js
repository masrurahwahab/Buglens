
const API_BASE_URL = 'http://localhost:5112/api';

const urlParams = new URLSearchParams(window.location.search);
const resetToken = urlParams.get('token');

if (!resetToken) {
    document.getElementById('errorText').textContent = 'Invalid or missing reset token';
    document.getElementById('errorMessage').style.display = 'block';
    document.getElementById('resetPasswordForm').style.display = 'none';
}

document.getElementById('resetPasswordForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const submitBtn = document.getElementById('submitBtn');
    const successMsg = document.getElementById('successMessage');
    const errorMsg = document.getElementById('errorMessage');

  
    successMsg.style.display = 'none';
    errorMsg.style.display = 'none';


    if (newPassword !== confirmPassword) {
        document.getElementById('errorText').textContent = 'Passwords do not match';
        errorMsg.style.display = 'block';
        return;
    }

    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Resetting...';

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/reset-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                token: resetToken,
                newPassword: newPassword
            })
        });

        const data = await response.json();

        if (response.ok) {
            successMsg.style.display = 'block';
            document.getElementById('resetPasswordForm').reset();

            
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 2000);
        } else {
            throw new Error(data.details || data.error || 'Failed to reset password');
        }
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('errorText').textContent = error.message;
        errorMsg.style.display = 'block';
    } finally {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="bi bi-check-circle"></i> Reset Password';
    }
});