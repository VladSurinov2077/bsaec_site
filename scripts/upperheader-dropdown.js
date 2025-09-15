document.addEventListener('DOMContentLoaded', function() {
    document.addEventListener('click', function(e) {
        // Открытие/закрытие dropdown
        if (e.target.matches('#upperheader-dropdownToggle')) {
            const menu = document.querySelector('#upperheader-dropdownMenu');
            if (menu) menu.classList.toggle('show');
        }
        
        // Выбор пункта меню
        if (e.target.matches('#upperheader-dropdownMenu li a')) {
            e.preventDefault();
            const toggle = document.querySelector('#upperheader-dropdownToggle');
            if (toggle) toggle.textContent = e.target.textContent;
            const menu = document.querySelector('#upperheader-dropdownMenu');
            if (menu) menu.classList.remove('show');
            
            const selectedValue = e.target.getAttribute('data-value');
            console.log('Выбран вариант:', selectedValue);
        }
        
        // Закрытие dropdown при клике вне его
        if (!e.target.matches('#upperheader-dropdownToggle') && 
            !e.target.closest('#upperheader-dropdownMenu')) {
            const menu = document.querySelector('#upperheader-dropdownMenu');
            if (menu && menu.classList.contains('show')) {
                menu.classList.remove('show');
            }
        }
    });
});