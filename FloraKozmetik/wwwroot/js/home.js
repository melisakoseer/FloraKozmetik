// Hero slider
let currentSlide = 0;
const slides = $('.hero-slide');

function changeSlide(dir) {
    slides.eq(currentSlide).removeClass('active');
    currentSlide = (currentSlide + dir + slides.length) % slides.length;
    slides.eq(currentSlide).addClass('active');
    updateDots();
}

function updateDots() {
    $('.hero-dot').each(function (i) {
        $(this).toggleClass('active', i === currentSlide);
    });
}

// Dots oluştur
const dotsContainer = $('#heroDots');
slides.each(function (i) {
    const dot = $('<button>')
        .addClass('hero-dot' + (i === 0 ? ' active' : ''))
        .on('click', function () {
            slides.eq(currentSlide).removeClass('active');
            currentSlide = i;
            slides.eq(currentSlide).addClass('active');
            updateDots();
        });
    dotsContainer.append(dot);
});

// Otomatik geçiş
setInterval(() => changeSlide(1), 5000);

// Marquee
const marqItems = ['Yüz Bakımı', 'Parfümler', 'Makyaj', '%100 Doğal', 'Cruelty-Free', 'Ücretsiz Kargo', 'Yeni Koleksiyon'];
const track = $('#marqTrack');
if (track.length) {
    const html = marqItems.map(i => `<span class="marq-item">${i}</span><span class="marq-sep">✦</span>`).join('');
    track.html(html + html);
}

// Reveal animasyonu
$(document).ready(function () {
    const observer = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (e.isIntersecting) $(e.target).addClass('visible');
        });
    }, { threshold: 0.1 });

    $('.reveal').each(function () {
        observer.observe(this);
    });
});