// Данные для новостей (легко редактируемый массив)
const newsData = [
    {
        title: "О колледже",
        items: ["Структура колледжа", "Администрация", "Педагогический состав"]
    },
    {
        title: "Академическая жизнь",
        items: ["Расписание занятий", "Сессия и экзамены", "Научные мероприятия"]
    },
    {
        title: "Мероприятия",
        items: ["Культурные события", "Спорт и олимпиады", "Конференции"]
    }
];

// Генерация новостей
document.addEventListener('DOMContentLoaded', () => {
    const newsItemsContainer = document.getElementById('newsItems');
    newsData.forEach(news => {
        const newsItem = document.createElement('div');
        newsItem.className = 'news-item';
        newsItem.innerHTML = `
            <span class="news-dot"></span>
            <h3>${news.title}</h3>
            <ul>
                ${news.items.map(item => `<li>${item}</li>`).join('')}
            </ul>
        `;
        newsItemsContainer.appendChild(newsItem);
    });

    // Простая анимация при клике (опционально)
    const newsItems = document.querySelectorAll('.news-item');
    newsItems.forEach(item => {
        item.addEventListener('click', () => {
            item.classList.toggle('expanded');
        });
    });
});