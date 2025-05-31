document.addEventListener('DOMContentLoaded', function () {
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const submitButton = document.getElementById('submit');
    const requirements = document.querySelectorAll('#password-requirements li');

    function validatePasswordStrength() {
        const password = passwordInput.value;
        const hasMinLength = password.length >= 6;
        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasDigit = /[0-9]/.test(password);
        const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);

        requirements[0].style.color = hasMinLength ? '#4dabf7' : 'rgba(255, 255, 255, 0.7)';
        requirements[1].style.color = hasUpperCase ? '#4dabf7' : 'rgba(255, 255, 255, 0.7)';
        requirements[2].style.color = hasLowerCase ? '#4dabf7' : 'rgba(255, 255, 255, 0.7)';
        requirements[3].style.color = hasDigit ? '#4dabf7' : 'rgba(255, 255, 255, 0.7)';
        requirements[4].style.color = hasSpecialChar ? '#4dabf7' : 'rgba(255, 255, 255, 0.7)';

        return hasMinLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    function checkFormValidity() {
        const isPasswordValid = validatePasswordStrength();
        const doPasswordsMatch = passwordInput.value === confirmPasswordInput.value;
        submitButton.disabled = !(isPasswordValid && doPasswordsMatch);
    }

    // Add subtle animation to input fields when focused
    document.querySelectorAll('.e-input').forEach(input => {
        input.addEventListener('focus', () => {
            input.closest('.e-input-group').style.transform = 'scale(1.02)';
            input.closest('.e-input-group').style.transition = 'transform 0.3s ease';
        });

        input.addEventListener('blur', () => {
            input.closest('.e-input-group').style.transform = 'scale(1)';
        });
    });

    passwordInput.addEventListener('input', function () {
        validatePasswordStrength();
        checkFormValidity();
    });

    confirmPasswordInput.addEventListener('input', function () {
        checkFormValidity();
    });

    // Initial validation
    checkFormValidity();
}); 