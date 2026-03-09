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
    $.post('/Account/AddAddress', {
        title: $('#addrTitle').val(),
        fullName: $('#addrFullName').val(),
        phone: $('#addrPhone').val(),
        city: $('#addrCity').val(),
        district: $('#addrDistrict').val(),
        fullAddress: $('#addrFull').val(),
        isDefault: $('#addrDefault').prop('checked')
    }, function (data) {
        if (data.success) {
            showToast(data.message);
            setTimeout(() => location.reload(), 1000);
        }
    });
}

function deleteAddress(id, btn) {
    if (!confirm('Bu adresi silmek istediðinize emin misiniz?')) return;
    $.post('/Account/DeleteAddress', { id: id }, function (data) {
        if (data.success) {
            $(btn).closest('.address-card').remove();
            showToast('Adres silindi.');
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