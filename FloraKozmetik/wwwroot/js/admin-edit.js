$(document).ready(function () {
    const priceInput = $('#priceInput');
    const discountInput = $('#discountRate');
    const origInput = $('#originalPriceHidden');

    if (!priceInput.length || !discountInput.length || !origInput.length) return;

    discountInput.on('input', function () {
        let original = parseFloat(origInput.val().replace(',', '.')) || parseFloat(priceInput.val().replace(',', '.'));
        const rate = parseFloat(discountInput.val()) || 0;
        const discounted = original - (original * rate / 100);
        priceInput.val(discounted.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        origInput.val(original.toString());
    });

    priceInput.on('input', function () {
        let val = priceInput.val().replace(',', '.');
        if (!isNaN(val)) {
            priceInput.val(parseFloat(val).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        }
    });
});