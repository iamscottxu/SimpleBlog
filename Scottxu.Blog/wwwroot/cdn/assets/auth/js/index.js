$(function () {
    $('#btn_Login').click(function () {
        let btn_Login = $('#btn_Login');
        let email = $('#email');
        let password = $('#password');
        if (email.val() == '') {
            showMessage('请输入电子邮箱地址。');
            return true;
        } else if (!email[0].checkValidity()) {
            showMessage('邮箱格式错误。');
            return true;
        } else if (password.val() == '') {
            showMessage('请输入密码。');
            return true;
        } else if (!password[0].checkValidity()) {
            showMessage('密码格式错误。');
            return true;
        }
        hideMessage();
        //禁用按钮
        btn_Login.button('loading');
        getCaptchaText(login, {email: email.val(), password: password.val()});
        return false;
    });

    function login(captchaText, data) {
        let btn_Login = $('#btn_Login');
        if (captchaText == '') {
            showMessage('未完成人机验证。');
            //启用按钮
            btn_Login.button('reset');
        } else {
            $.ajax({
                type: 'POST',
                url: window.location.pathname + '/PostBack',
                data: {
                    'email': data.email,
                    'password': hex_md5(hex_md5(data.password)),
                    'captchaText': captchaText
                },
                success: function (data) {
                    if (!data.success) {
                        showMessage(data.message);
                        $('#password').val("");
                        resetCaptcha();
                        //启用按钮
                        btn_Login.button('reset');
                    } else {
                        let returnUrl = getQueryString("ReturnUrl");
                        if (returnUrl == null || returnUrl == '') returnUrl = '/';
                        window.location.replace(returnUrl);
                    }
                },
                error: function () {
                    messagebox.css('display', 'block');
                    messagebox.html('未知错误。');
                    resetCaptcha();
                    //启用按钮
                    btn_Login.button('reset');

                },
                dataType: 'json'
            });
        }
    }

    //获取参数值
    function getQueryString(name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return decodeURI(unescape(r[2]));
        return null;
    }

    function showMessage(message) {
        let messagebox = $('#message');
        messagebox.css('display', 'inherit');
        messagebox.text(message);
    }

    function hideMessage() {
        let messagebox = $('#message');
        messagebox.css('display', 'none');
    }
});