function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const main = document.querySelector('.main-content');
    if (sidebar.style.width === '60px') {
        sidebar.style.width = '250px';
        main.style.marginLeft = '250px';
    } else {
        sidebar.style.width = '60px';
        main.style.marginLeft = '60px';
    }
}

// Auto-dismiss alerts
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 500);
        }, 4000);
    });
});
