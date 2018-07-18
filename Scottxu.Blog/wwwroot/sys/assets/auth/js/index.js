
$(function () {
    $('#btn_Login').click(function () {
        let btn_Login = $('#btn_Login');
        let email = $('#email');
        let password = $('#password');
        if (email.val() == '') {
            showMessage('请输入邮箱。');
            return false;
        } else if (!email[0].checkValidity()) {
            showMessage('邮箱格式错误。');
            return false;
        } else if (password.val() == '') {
            showMessage('请输入密码。');
            return false;
        } else if (!password[0].checkValidity()) {
            showMessage('密码格式错误。');
            return false;
        } else {
            hideMessage();
            //禁用按钮
            btn_Login.button('loading');
            $.ajax({
                type: 'POST',
                url: window.location.pathname + '/Login',
                data: {
                    'email': email.val(),
                    'password': hex_md5(hex_md5(password.val()))
                },
                success: function (data) {
                    if (!data.success) {
                        showMessage(data.message);
                        //启用按钮
                        btn_Login.button('reset');
                        $('#password').val("");
                    } else {
                        let returnUrl = getQueryString("ReturnUrl");
                        if (returnUrl == null || returnUrl == '') returnUrl = '/';
                        window.location.replace(returnUrl);
                    }
                },
                error: function () {
                    messagebox.css('display', 'block');
                    messagebox.html('未知错误。');
                    //启用按钮
                    btn_Login.button('reset');
                },
                dataType: 'json'
            });
            return false;
        }
    });
    //获取参数值
    function getQueryString(name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return decodeURI(unescape(r[2])); return null;
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