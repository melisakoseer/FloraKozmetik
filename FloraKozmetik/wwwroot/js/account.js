function switchTab(tab, btn) {
    $('.profile-tab').removeClass('active');
    $('.profile-nav-item').removeClass('active');
    $('#tab-' + tab).addClass('active');
    $(btn).addClass('active');
}

function toggleEdit() {
    $('#infoView').toggle();
    $('#infoEdit').toggle();
}

function saveProfile() {
    $.post('/Account/UpdateProfile', {
        firstName: $('#editFirstName').val(),
        lastName: $('#editLastName').val(),
        phoneNumber: $('#editPhone').val(),
        gender: $('#editGender').val()
    }, function (data) {
        if (data.success) {
            showToast(data.message);
            setTimeout(() => location.reload(), 1000);
        }
    });
}

function toggleAddressForm() {
    $('#addressForm').toggle();
}

function saveAddress() {
    const title = $('#addrTitle').val();
    const fullName = $('#addrFullName').val();
    const phone = $('#addrPhone').val();
    const city = $('#addrCity').val();
    const district = $('#addrDistrict').val();
    const fullAddress = $('#addrFull').val();

    if (!title || !fullName || !phone || !city || !district || !fullAddress) {
        showToast('Lütfen tüm alanlarý doldurun.', 'error');
        return;
    }

    $.post('/Account/AddAddress', {
        title, fullName, phone, city, district, fullAddress,
        isDefault: $('#addrDefault').prop('checked')
    }, function (data) {
        if (data.success) {
            showToast(data.message);
            setTimeout(() => location.reload(), 1000);
        }
    });
}

var activeAddressId = null;
var activeAddressBtn = null;

function deleteAddress(id, btn) {
    activeAddressId = id;
    activeAddressBtn = btn;
    $('#deleteAddressModal').css('display', 'flex');
}

function closeDeleteAddressModal() {
    $('#deleteAddressModal').hide();
    activeAddressId = null;
    activeAddressBtn = null;
}

function confirmDeleteAddress() {
    $.post('/Account/DeleteAddress', { id: activeAddressId }, function (data) {
        if (data.success) {
            $(activeAddressBtn).closest('.address-card').remove();
            showToast('Adres silindi.');
            closeDeleteAddressModal();
        }
    });
}

function changePassword() {
    const oldPw = $('#oldPassword').val();
    const newPw = $('#newPassword').val();
    const newPw2 = $('#newPassword2').val();

    if (!oldPw || !newPw || !newPw2) {
        showToast('Tüm alanlarý doldurun.', 'error');
        return;
    }
    if (newPw !== newPw2) {
        showToast('Yeni þifreler eþleþmiyor.', 'error');
        return;
    }

    $.post('/Account/ChangePassword', {
        oldPassword: oldPw,
        newPassword: newPw,
        newPassword2: newPw2
    }, function (data) {
        showToast(data.message, data.success ? 'success' : 'error');
        if (data.success) {
            $('#oldPassword, #newPassword, #newPassword2').val('');
        }
    });
}

function deleteAccount() {
    $('#deleteAccountModal').css('display', 'flex');
}

function closeDeleteAccountModal() {
    $('#deleteAccountModal').hide();
}

function confirmDeleteAccount() {
    $.post('/Account/DeleteAccount', function (data) {
        if (data.success) {
            window.location.href = '/';
        }
    });
}

$(document).ready(function () {
    $('#deleteAddressModal').on('click', function (e) {
        if (e.target === this) closeDeleteAddressModal();
    });
    $('#deleteAccountModal').on('click', function (e) {
        if (e.target === this) closeDeleteAccountModal();
    });
});