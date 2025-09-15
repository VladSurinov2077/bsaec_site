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

        document.addEventListener('DOMContentLoaded', () => {
            const slides = document.querySelector('.slides');
            const slideElements = document.querySelectorAll('.slide');
            const dots = document.querySelectorAll('.dot');
            const nextButton = document.querySelector('.next');
            const prevButton = document.querySelector('.prev');

            if (!slides || !slideElements.length || !dots.length || !nextButton || !prevButton) {
                console.error('Slider elements not found:', {
                    slides: !!slides,
                    slideElements: slideElements.length,
                    dots: dots.length,
                    nextButton: !!nextButton,
                    prevButton: !!prevButton
                });
                return;
            }

            let currentSlide = 0;
            const totalSlides = slideElements.length;

            function showSlide(index) {
                if (index >= totalSlides) currentSlide = 0;
                else if (index < 0) currentSlide = totalSlides - 1;
                else currentSlide = index;

                slides.style.transform = `translateX(-${currentSlide * 100}%)`;
                dots.forEach((dot, i) => {
                    dot.classList.toggle('active', i === currentSlide);
                });
            }

            nextButton.addEventListener('click', () => {
                showSlide(currentSlide + 1);
            });

            prevButton.addEventListener('click', () => {
                showSlide(currentSlide - 1);
            });

            dots.forEach((dot, index) => {
                dot.addEventListener('click', () => {
                    showSlide(index);
                });
            });

            setInterval(() => {
                showSlide(currentSlide + 1);
            }, 5000);
        });