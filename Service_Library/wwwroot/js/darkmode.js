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
        'table': 'table-dark-mode',
        'cart-summary': 'cart-summary-dark-mode',
        'countdown-timer': 'countdown-timer-dark-mode'
    };
    function applyDarkMode() {
        body.classList.add('dark-mode');
        navbar.classList.add('navbar-dark-mode');

        if (filterPanel) {
            filterPanel.classList.remove('bg-light');
            filterPanel.classList.add('bg-dark');
            filterPanel.classList.add('dark-mode');
        }

        for (const [originalClass, darkModeClass] of Object.entries(classMappings)) {
            document.querySelectorAll(`.${originalClass}`).forEach(element => {
                element.classList.add(darkModeClass);
            });
        }
    }
    function removeDarkMode() {
        body.classList.remove('dark-mode');
        navbar.classList.remove('navbar-dark-mode');

        if (filterPanel) {
            filterPanel.classList.remove('bg-dark');
            filterPanel.classList.add('bg-light');
            filterPanel.classList.remove('dark-mode');
        }

        for (const [originalClass, darkModeClass] of Object.entries(classMappings)) {
            document.querySelectorAll(`.${originalClass}`).forEach(element => {
                element.classList.remove(darkModeClass);
            });
        }
    }

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
