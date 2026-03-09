function resetPassword() {
    const email = $('#email').val();
    const newPassword = $('#newPassword').val();
    const newPassword2 = $('#newPassword2').val();

    if (!email || !newPassword || !newPassword2) {
        showMsg('Tüm alanlarý doldurun.', false);
        return;
    }

    $.post('/Account/ForgotPassword', {
        email: email,
        newPassword: newPassword,
        newPassword2: newPassword2
    }, function (data) {
        showMsg(data.message, data.success);
        if (data.success) {
            setTimeout(() => window.location.href = '/Account/Login', 2000);
        }
    });
}

function showMsg(msg, success) {
    const el = $('#resultMsg');
    el.show()
        .css({
            background: success ? '#e8f5e9' : '#fce4ec',
            color: success ? '#2e7d32' : '#c62828'
        })
        .text(msg);
}