const cache = {
  pages: new Map(),
  partials: {
    header: null,
    footer: null
  }
};

// Инициализация приложения
async function initApp() {
  await loadPartials();
  await handleNavigation(window.location.pathname);
}

// Загрузка повторяющихся элементов
async function loadPartials() {
  if (!cache.partials.header) {
    cache.partials.header = await fetch('./partials/header.html')
      .then(res => res.ok ? res.text() : Promise.reject('Header not found'))
      .catch(() => '<header>Ошибка загрузки шапки</header>');
  }
  
  if (!cache.partials.footer) {
    cache.partials.footer = await fetch('./partials/footer.html')
      .then(res => res.ok ? res.text() : Promise.reject('Footer not found'))
      .catch(() => '<footer>Ошибка загрузки подвала</footer>');
  }
  
  document.getElementById('headerImportBox').innerHTML = cache.partials.header;
  document.getElementById('footerImportBox').innerHTML = cache.partials.footer;
}

// Основная функция навигации
async function handleNavigation(path) {
  try {
    // Плавное исчезновение контента
    document.getElementById('contentImportBox').style.opacity = '0';
    
    // Для главной страницы используем встроенный контент
    if (path === '/' || path === '/index.html') {
      if (!cache.pages.has('/')) {
        cache.pages.set('/', document.getElementById('contentImportBox').innerHTML);
      }
    } else {
      // Загрузка контента страницы
      if (!cache.pages.has(path)) {
        const response = await fetch(path);
        if (!response.ok) throw new Error('Страница не найдена');
        
        const html = await response.text();
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        
        cache.pages.set(path, doc.body.innerHTML);
        document.title = doc.querySelector('title')?.textContent || document.title;
      }
    }
    
    // Вставляем контент
    document.getElementById('contentImportBox').innerHTML = cache.pages.get(path) || cache.pages.get('/');
    
    // Плавное появление
    setTimeout(() => {
      document.getElementById('contentImportBox').style.opacity = '1';
    }, 100);
    
  } catch (error) {
    console.error('Ошибка загрузки:', error);
    document.getElementById('contentImportBox').innerHTML = `
      <div class="error">
        <h1>404</h1>
        <p>Страница не найдена</p>
      </div>
    `;
    document.getElementById('contentImportBox').style.opacity = '1';
  }
}

// Обработка кликов по ссылкам
document.addEventListener('click', (e) => {
  const link = e.target.closest('a');
  if (!link || !link.href.startsWith(window.location.origin)) return;
  
  e.preventDefault();
  const url = new URL(link.href);
  window.history.pushState({}, '', url.pathname);
  handleNavigation(url.pathname);
});

// Обработка кнопок Назад/Вперед
window.addEventListener('popstate', () => {
  handleNavigation(window.location.pathname);
});

// Запуск приложения
window.addEventListener('DOMContentLoaded', initApp);