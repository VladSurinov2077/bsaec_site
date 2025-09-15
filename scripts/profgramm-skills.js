function animateSkills() {
  const skills = document.querySelectorAll('.skill');
  skills.forEach(skill => {
    const rect = skill.getBoundingClientRect();
    const inView = rect.top < window.innerHeight && rect.bottom > 0;
    const fill = skill.querySelector('.bar-fill');
    const percent = skill.getAttribute('data-percent');
    if (inView) {
      fill.style.width = percent + '%';
    } else {
      fill.style.width = '0%';
    }
  });
}
window.addEventListener('scroll', animateSkills);
window.addEventListener('load', animateSkills);