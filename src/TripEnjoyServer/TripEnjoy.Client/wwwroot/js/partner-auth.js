// This function will initialize the sign-in form validation and notifications.
function initializeSignInForm(messages) {
    const notyf = new Notyf({
        duration: 5000,
        position: { x: 'right', y: 'top' },
        types: [
            { type: 'error', background: '#FA5C7C', icon: { className: 'fa-regular fa-circle-xmark', tagName: 'i', color: 'white'} },
            { type: 'success', background: '#0ACF97', icon: { className: 'fa-regular fa-circle-check', tagName: 'i', color: 'white'} }
        ]
    });

    // Display initial messages passed from the server (TempData)
    if (messages.errorMessage) {
        notyf.error(messages.errorMessage);
    }
    if (messages.successMessage) {
        notyf.success(messages.successMessage);
    }

    $('#signInForm').submit(function (e) {
        let isValid = true;
        const email = $('#Email').val();
        const password = $('#Password').val();

        // Email validation
        if (!email) {
            notyf.error('Email is required.');
            isValid = false;
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            notyf.error('Please enter a valid email address.');
            isValid = false;
        }

        // Password validation
        if (!password) {
            notyf.error('Password is required.');
            isValid = false;
        }

        // If the form is not valid, prevent submission
        if (!isValid) {
            e.preventDefault();
        }
        
        // If the form is valid, the default submission will proceed
    });

    // Password toggle functionality
    $('#toggle-password').click(function () {
        togglePasswordVisibility($(this), $('#Password'));
    });

    function togglePasswordVisibility(toggleIcon, passwordInput) {
        toggleIcon.toggleClass('fa-eye fa-eye-slash');
        passwordInput.attr('type', passwordInput.attr('type') === 'password' ? 'text' : 'password');
    }
}
