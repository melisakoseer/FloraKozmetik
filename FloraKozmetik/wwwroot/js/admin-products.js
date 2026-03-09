let activeDeleteId = null;

function openDeleteModal(id) {
    activeDeleteId = id;
    $('#deleteModal').css('display', 'flex');
}

function closeDeleteModal() {
    $('#deleteModal').hide();
    activeDeleteId = null;
}

function confirmDelete() {
    if (activeDeleteId) {
        $('#deleteForm-' + activeDeleteId).submit();
    }
}

$('#deleteModal').on('click', function (e) {
    if (e.target === this) closeDeleteModal();
});

let activeHardDeleteId = null;

function openHardDeleteModal(id) {
    activeHardDeleteId = id;
    $('#hardDeleteModal').css('display', 'flex');
}

function closeHardDeleteModal() {
    $('#hardDeleteModal').hide();
    activeHardDeleteId = null;
}

function confirmHardDelete() {
    if (activeHardDeleteId) {
        $('#hardDeleteForm-' + activeHardDeleteId).submit();
    }
}

$('#hardDeleteModal').on('click', function (e) {
    if (e.target === this) closeHardDeleteModal();
});