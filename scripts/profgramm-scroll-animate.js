// Анимация секций при скролле
function revealOnScroll() {
  document.querySelectorAll('.reveal').forEach(section => {
    const rect = section.getBoundingClientRect();
    const inView = rect.top < window.innerHeight - 40 && rect.bottom > 60;
    if (inView) {
      section.classList.add('visible');
      // stagger-появление детей (если нужны задержки для .reveal-child)
      const children = section.querySelectorAll('.reveal-child');
      children.forEach((child, i) => {
        setTimeout(() => child.classList.add('visible'), i * 110);
      });
    } else {
      section.classList.remove('visible');
      section.querySelectorAll('.reveal-child').forEach(child => child.classList.remove('visible'));
    }
  });
}
window.addEventListener('scroll', revealOnScroll);
window.addEventListener('load', revealOnScroll);