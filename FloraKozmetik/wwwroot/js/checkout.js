'use strict';

let discountAmount = 0;
let appliedCoupon = '';
const subtotal = window.CHECKOUT_SUBTOTAL || 0;
const shippingFee = subtotal >= 500 ? 0 : 49;

// ADRES TOGGLE
function initAddressToggle() {
    $('input[name="addressId"]').on('change', function () {
        $('#newAddressForm').css('display', this.value === '0' ? 'block' : 'none');
    });
}

// KART FORMATLAMA
function initCardFormatting() {
    $('#cardNumber').on('input', function () {
        let val = $(this).val().replace(/\D/g, '').substring(0, 16);
        $(this).val(val.replace(/(.{4})/g, '$1 ').trim());
        const icon = $('.card-icon');
        if (icon.length) {
            if (val.startsWith('4')) icon.text('VISA');
            else if (val.startsWith('5')) icon.text('MC');
            else icon.text('KART');
        }
    });

    $('#cardExpiry').on('input', function () {
        let val = $(this).val().replace(/\D/g, '').substring(0, 4);
        if (val.length >= 2) val = val.substring(0, 2) + '/' + val.substring(2);
        $(this).val(val);
    });
}

// KUPON
function applyCoupon() {
    const code = ($('#couponInput').val() || '').trim().toUpperCase();
    const resultEl = $('#couponResult');
    if (!resultEl.length) return;

    resultEl.show();

    if (!code) {
        resultEl.html('<span class="coupon-error">✗ Kupon kodu girin.</span>');
        return;
    }

    $.post('/Order/CheckCoupon', { couponCode: code }, function (data) {
        if (data.valid) {
            appliedCoupon = code;
            discountAmount = Math.round(subtotal * data.discountRate);
            resultEl.html(`<span class="coupon-success">✓ ${code} uygulandı — %${Math.round(data.discountRate * 100)} indirim!</span>`);
            $('#discountRow').show();
            $('#discountAmount').text('-₺' + discountAmount);
        } else {
            appliedCoupon = '';
            discountAmount = 0;
            resultEl.html(`<span class="coupon-error">✗ ${data.message}</span>`);
            $('#discountRow').hide();
        }
        const total = subtotal + shippingFee - discountAmount;
        $('#summaryTotal').text('₺' + total.toLocaleString('tr-TR'));
    });
}

// SİPARİŞ VER
function placeOrder() {
    const addressRadio = $('input[name="addressId"]:checked');
    const addressId = addressRadio.length ? parseInt(addressRadio.val()) : -1;

    if (addressId === -1) {
        showToast('Lütfen bir teslimat adresi seçin.', 'error');
        return;
    }

    if (addressId === 0) {
        const fullName = $('#newAddressFullName').val() || '';
        const phone = $('#newAddressPhone').val() || '';
        const city = $('#newAddressCity').val() || '';
        const fullAddress = $('#newAddressFullAddress').val() || '';
        if (!fullName || !phone || !city || !fullAddress) {
            showToast('Lütfen tüm adres alanlarını doldurun.', 'error');
            return;
        }
    }

    const cardNumber = ($('#cardNumber').val() || '').replace(/\s/g, '');
    const cardExpiry = $('#cardExpiry').val() || '';
    const cardCvv = $('#cardCvv').val() || '';
    const cardName = $('#cardName').val() || '';

    if (!cardName) { showToast('Kart üzerindeki ismi girin.', 'error'); return; }
    if (cardNumber.length < 16) { showToast('Geçerli bir kart numarası girin.', 'error'); return; }
    if (cardExpiry.length < 5) { showToast('Son kullanma tarihini girin.', 'error'); return; }
    if (cardCvv.length < 3) { showToast('CVV kodunu girin.', 'error'); return; }

    const btn = $('.checkout-submit-btn');
    btn.prop('disabled', true).text('İşleniyor...');

    const formData = new FormData();
    formData.append('addressId', addressId);

    if (addressId === 0) {
        formData.append('newAddressTitle', $('#newAddressTitle').val() || '');
        formData.append('newAddressFullName', $('#newAddressFullName').val() || '');
        formData.append('newAddressPhone', $('#newAddressPhone').val() || '');
        formData.append('newAddressCity', $('#newAddressCity').val() || '');
        formData.append('newAddressDistrict', $('#newAddressDistrict').val() || '');
        formData.append('newAddressFullAddress', $('#newAddressFullAddress').val() || '');
    }

    formData.append('couponCode', appliedCoupon);
    formData.append('paymentMethod', 'Kredi Kartı');

    $.ajax({
        url: '/Order/PlaceOrder',
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data.success) {
                window.location.href = '/Order/Success/' + data.orderId;
            } else {
                showToast(data.message || 'Bir hata oluştu.', 'error');
                btn.prop('disabled', false).text('Siparişi Tamamla');
            }
        },
        error: function () {
            showToast('Bir hata oluştu, tekrar deneyin.', 'error');
            btn.prop('disabled', false).text('Siparişi Tamamla');
        }
    });
}

// BAŞLAT
$(document).ready(function () {
    initAddressToggle();
    initCardFormatting();

    const radios = $('input[name="addressId"]');
    if (radios.length === 1 && radios.first().attr('id') === 'newAddressRadio') {
        radios.first().prop('checked', true).trigger('change');
    }
});

window.applyCoupon = applyCoupon;
window.placeOrder = placeOrder;