document.addEventListener('DOMContentLoaded', function() {
    const observer = new MutationObserver(function(mutations, obs) {
        const menuToggle = document.getElementById('headerMenuToggle');
        const headerMenu = document.getElementById('headerMenu');
        const menuToggleImage = document.getElementById('headerMenuToggleImage');
        
        if (menuToggle && headerMenu) {
            // Отключаем observer, так как элементы найдены
            obs.disconnect();
            
            // Инициализация меню
            menuToggle.addEventListener('click', function() {
                // Переключаем видимость
                headerMenu.classList.toggle('header-menu--visible');
                
                // Обновляем атрибут доступности
                const isExpanded = headerMenu.classList.contains('header-menu--visible');
                this.setAttribute('aria-expanded', isExpanded);
                
                // Анимация иконки (опционально)
                this.classList.toggle('menu-open');
            });
            
            // Закрытие при клике вне меню (опционально)
            document.addEventListener('click', function(e) {
                if (!headerMenu.contains(e.target) && !menuToggle.contains(e.target)) {
                    headerMenu.classList.remove('header-menu--visible');
                    menuToggle.setAttribute('aria-expanded', 'false');
                    menuToggle.classList.remove('menu-open');
                }
            });
        }
    });
    // Начинаем наблюдение
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});