// CSRF token
const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val() ?? '';

// TOAST
function showToast(message, type = 'success') {
    $('.flora-toast').remove();

    //success ise tik ikonu, error ise uyarı ikonu 
    const icon = type === 'success'
        ? '<polyline points="20 6 9 17 4 12"/>'
        : '<circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>';

    const toast = $(`
        <div class="flora-toast flora-toast--${type}">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                ${icon}
            </svg>
            <span>${message}</span>
        </div>
    `);

    $('body').append(toast);
    setTimeout(() => toast.addClass('show'), 10);
    setTimeout(() => {
        toast.removeClass('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// SEPETİ GÜNCELLE
function refreshCartCount() {
    $.get('/Cart/GetCount', function (data) {
        const badge = $('#cartBadge');
        badge.text(data.count);
        badge.css('display', data.count > 0 ? 'flex' : 'none');
    });
}

// FAVORİ SAYACINI GÜNCELLE
function refreshFavoriteCount() {
    $.get('/Favorite/GetCount', function (data) {
        const badge = $('#favBadge');
        badge.text(data.count);
        badge.css('display', data.count > 0 ? 'flex' : 'none');
    });
}

// SEPETE EKLE
function addToCart(productId) {
    $.post('/Cart/Add', { productId: productId }, function (data) {
        if (data.requireLogin) {
            showToast(data.message, 'error');
            setTimeout(() => window.location.href = '/Account/Login', 1500);
            return;
        }
        if (data.success) {
            showToast(data.message, 'success');
            refreshCartCount();
            if ($('#cartDrawer').hasClass('open')) {
                loadCartDrawer();
            }
        } else {
            showToast(data.message || 'Bir hata oluştu.', 'error');
        }
    }).fail(function () {
        showToast('Bağlantı hatası.', 'error');
    });
}

// FAVORİ
function toggleFavorite(productId, btn) {
    $.post('/Favorite/Toggle', { productId: productId }, function (data) {
        if (data.requireLogin) {
            showToast(data.message, 'error');
            setTimeout(() => window.location.href = '/Account/Login', 1500);
            return;
        }
        if (data.success) {
            showToast(data.message, data.isFavorited ? 'success' : 'error');
            refreshFavoriteCount();
            if (btn) {
                $(btn).toggleClass('active', data.isFavorited);
            }
            $(`[id="pw${productId}"]`).toggleClass('active', data.isFavorited);
        }
    }).fail(function () {
        showToast('Bağlantı hatası.', 'error');
    });
}

// SAYFA YÜKLENİNCE FAVORİ BUTONLARINI RENKLENDIR
function markFavoriteButtons() {
    $.get('/Favorite/GetFavoriteIds', function (data) {
        if (data.ids && data.ids.length > 0) {
            $.each(data.ids, function (_, id) {
                $(`#pw${id}`).addClass('active');
            });
        }
    });
}

// SEPET DRAWER
function loadCartDrawer() {
    const drawer = $('#cartDrawer');
    if (!drawer.length) return;

    const body = drawer.find('.cart-drawer-body');
    body.html('<div class="cart-loading">Yükleniyor...</div>');

    $.get('/Cart/GetItems', function (data) {
        if (!data.items || data.items.length === 0) {
            body.html(`
                <div class="cart-empty">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#ccc" stroke-width="1.5">
                        <path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/>
                        <line x1="3" y1="6" x2="21" y2="6"/>
                        <path d="M16 10a4 4 0 0 1-8 0"/>
                    </svg>
                    <p>Sepetiniz boş</p>
                    <a href="/Products" class="cart-empty-btn">Alışverişe Başla</a>
                </div>`);
            drawer.find('.cart-drawer-footer').hide();
            return;
        }

        const html = $.map(data.items, function (item) {
            return `
                <div class="cart-item" id="ci${item.id}">
                    <img class="cart-item-img" src="${item.image}" alt="${item.name}" />
                    <div class="cart-item-info">
                        <p class="cart-item-name">${item.name}</p>
                        <p class="cart-item-price">₺${item.price.toLocaleString('tr-TR')}</p>
                        <div class="cart-item-qty">
                            <button onclick="updateCartQty(${item.id}, ${item.quantity - 1})">−</button>
                            <span>${item.quantity}</span>
                            <button onclick="updateCartQty(${item.id}, ${item.quantity + 1})">+</button>
                        </div>
                    </div>
                    <button class="cart-item-remove" onclick="removeCartItem(${item.id})">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>
                </div>`;
        }).join('');

        body.html(html);
        drawer.find('.cart-total-price').text(`₺${data.total.toLocaleString('tr-TR')}`);
        drawer.find('.cart-drawer-footer').show();

    }).fail(function () {
        body.html('<div class="cart-error">Hata oluştu.</div>');
    });
}

function updateCartQty(cartItemId, newQty) {
    $.post('/Cart/UpdateQuantity', { cartItemId: cartItemId, quantity: newQty }, function (data) {
        if (data.success) {
            refreshCartCount();
            loadCartDrawer();
        }
    });
}

function removeCartItem(cartItemId) {
    $.post('/Cart/Remove', { cartItemId: cartItemId }, function (data) {
        if (data.success) {
            refreshCartCount();
            loadCartDrawer();
        }
    });
}

function openCartDrawer() {
    $('#cartDrawer').addClass('open');
    $('#drawerOverlay').addClass('show');
    loadCartDrawer();
}

function closeCartDrawer() {
    $('#cartDrawer').removeClass('open');
    $('#drawerOverlay').removeClass('show');
}

// INIT
$(document).ready(function () {
    refreshCartCount();
    refreshFavoriteCount();
    markFavoriteButtons();

    $('#drawerOverlay').on('click', closeCartDrawer);
});

// CURSOR
const cur = document.getElementById('cur');
const curRing = document.getElementById('curRing');

if (cur && curRing) {
    $(document).on('mousemove', function (e) {
        $(cur).css({ left: e.clientX, top: e.clientY });
        $(curRing).css({ left: e.clientX, top: e.clientY });
    });

    $(document).on('mousedown', function () {
        $(cur).css({ width: '6px', height: '6px' });
        $(curRing).css({ width: '28px', height: '28px' });
    });

    $(document).on('mouseup', function () {
        $(cur).css({ width: '10px', height: '10px' });
        $(curRing).css({ width: '36px', height: '36px' });
    });

    $('a, button').on('mouseenter', function () {
        $(cur).css({ width: '16px', height: '16px' });
        $(curRing).css({ width: '48px', height: '48px' });
    }).on('mouseleave', function () {
        $(cur).css({ width: '10px', height: '10px' });
        $(curRing).css({ width: '36px', height: '36px' });
    });
}