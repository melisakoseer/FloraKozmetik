let cancelOrderId = null;

function openCancelModal(id) {
    cancelOrderId = id;
    $('#cancelModal').css('display', 'flex');
}

function closeCancelModal() {
    $('#cancelModal').hide();
    cancelOrderId = null;
}

function confirmCancel() {
    $.post('/Order/Cancel', { id: cancelOrderId }, function (data) {
        if (data.success) {
            closeCancelModal();
            showToast('Sipariþ iptal edildi.', 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            showToast(data.message, 'error');
        }
    });
}

$('#cancelModal').on('click', function (e) {
    if (e.target === this) closeCancelModal();
});