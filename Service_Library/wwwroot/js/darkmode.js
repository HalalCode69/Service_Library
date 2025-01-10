document.addEventListener('DOMContentLoaded', function () {
    const darkModeSwitch = document.getElementById('darkModeSwitch');
    const body = document.body;
    const navbar = document.querySelector('.navbar');
    const filterPanel = document.getElementById('filterPanel');

    const classMappings = {
        'book-card': 'card-dark-mode',
        'modal-content': 'modal-content-dark-mode',
        'modal-header': 'modal-header-dark-mode',
        'modal-body': 'modal-body-dark-mode',
        'modal-footer': 'modal-footer-dark-mode',
        'book-price': 'book-price-dark-mode',
        'display-rating': 'display-rating-dark-mode',
        'form-control': 'form-control-dark-mode',
        'text-muted': 'text-muted-dark-mode',
        'text-warning': 'text-warning-dark-mode',
        'text-info': 'text-info-dark-mode',
        'navbar-nav': 'navbar-nav-dark-mode',
        'btn': 'btn-dark-mode',
        'table': 'table-dark-mode', // Added for shopping cart table
        'cart-summary': 'cart-summary-dark-mode', // Added for cart summary
        'countdown-timer': 'countdown-timer-dark-mode' // Added for countdown timer
    };

    // Apply Dark Mode
    function applyDarkMode() {
        body.classList.add('dark-mode');
        navbar.classList.add('navbar-dark-mode');

        if (filterPanel) {
            filterPanel.classList.remove('bg-light'); // Remove light mode background
            filterPanel.classList.add('bg-dark'); // Add dark mode background
            filterPanel.classList.add('dark-mode'); // Additional dark mode class for styling
        }

        for (const [originalClass, darkModeClass] of Object.entries(classMappings)) {
            document.querySelectorAll(`.${originalClass}`).forEach(element => {
                element.classList.add(darkModeClass);
            });
        }
    }

    // Remove Dark Mode
    function removeDarkMode() {
        body.classList.remove('dark-mode');
        navbar.classList.remove('navbar-dark-mode');

        if (filterPanel) {
            filterPanel.classList.remove('bg-dark'); // Remove dark mode background
            filterPanel.classList.add('bg-light'); // Restore light mode background
            filterPanel.classList.remove('dark-mode'); // Remove dark mode class
        }

        for (const [originalClass, darkModeClass] of Object.entries(classMappings)) {
            document.querySelectorAll(`.${originalClass}`).forEach(element => {
                element.classList.remove(darkModeClass);
            });
        }
    }

    // Initialize Dark Mode Based on Local Storage
    if (localStorage.getItem('darkMode') === 'enabled') {
        applyDarkMode();
        if (darkModeSwitch) {
            darkModeSwitch.checked = true;
        }
    } else {
        removeDarkMode();
    }

    if (darkModeSwitch) {
        darkModeSwitch.addEventListener('change', function () {
            if (darkModeSwitch.checked) {
                applyDarkMode();
                localStorage.setItem('darkMode', 'enabled');
            } else {
                removeDarkMode();
                localStorage.setItem('darkMode', 'disabled');
            }
        });
    }
});
