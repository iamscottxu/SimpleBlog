
$(function () {
    $('#btn_Login').click(function () {
        let btn_Login = $('#btn_Login');
        let userName = $('#userName');
        let blogName = $('#blogName');
        let email = $('#email');
        let password = $('#password');
        let rePassword = $('#rePassword');
        if (!userName[0].checkValidity()) {
            showMessage('请输入昵称。');
            return false;
        } else if (!userName[0].checkValidity()) {
            showMessage('昵称格式错误。');
            return false;
        } else if (blogName.val() == '') {
            showMessage('请输入博客名称。');
            return false;
        } else if (!blogName[0].checkValidity()) {
            showMessage('博客名称格式错误。');
            return false;
        } else if (email.val() == '') {
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
        } else if (rePassword.val() == '') {
            showMessage('请重复输入密码。');
            return false;
        } else if (!rePassword[0].checkValidity()) {
            showMessage('两次输入的密码不一致。');
            return false;
        } else {
            hideMessage();
            //禁用按钮
            btn_Login.button('loading');
            $.ajax({
                type: 'POST',
                url: window.location.pathname + '/Install',
                data: {
                    'userName': userName.val(),
                    'blogName': blogName.val(),
                    'email': email.val(),
                    'password': hex_md5(hex_md5(password.val()))
                },
                success: function (data) {
                    if (!data.success) {
                        showMessage(data.message);
                        //启用按钮
                        btn_Login.button('reset');
                    } else {
                        window.location.replace(window.location.pathname + '/../Admin');
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