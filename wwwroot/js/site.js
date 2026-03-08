// WeatherApp - site.js
// Minimal JavaScript for enhancements

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        // Auto-dismiss alerts after 5 seconds
        document.querySelectorAll('.alert.alert-dismissible').forEach(function (alert) {
            setTimeout(function () {
                var bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
                if (bsAlert) bsAlert.close();
            }, 5000);
        });

        // Add fade-in class to main content
        var main = document.querySelector('.main-content .container');
        if (main) main.classList.add('fade-in');

        // Confirm delete dialogs (for forms with data-confirm attribute)
        document.querySelectorAll('[data-confirm]').forEach(function (el) {
            el.addEventListener('submit', function (e) {
                if (!confirm(el.dataset.confirm || 'Вы уверены?')) {
                    e.preventDefault();
                }
            });
        });
    });
})();
