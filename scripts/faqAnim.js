function toggleFaq(el) {
  const item = el.closest('.faq-item');
  const isOpen = item.classList.contains('open');

  // Закрываем все
  document.querySelectorAll('.faq-item').forEach(faq => {
    faq.classList.remove('open');
  });

  // Открываем текущий, если он был закрыт
  if (!isOpen) {
    item.classList.add('open');
  }
}
