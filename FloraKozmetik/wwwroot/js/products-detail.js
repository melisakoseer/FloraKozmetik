function selectSize(btn, size) {
    $('.size-btn').removeClass('active');
    $(btn).addClass('active');
    $('#selectedSize').text(size);
}

let qty = 1;

function changeQty(delta) {
    qty = Math.max(1, qty + delta);
    $('#detailQty').val(qty);
}

function switchThumb(el, src) {
    $('.gallery-thumb').removeClass('active');
    $(el).addClass('active');
    const main = $('#galleryMain');
    main.css('opacity', '0');
    setTimeout(() => {
        main.attr('src', src).css('opacity', '1');
    }, 200);
}

function toggleAccordion(btn) {
    $(btn).closest('.accordion-item').toggleClass('open');
}