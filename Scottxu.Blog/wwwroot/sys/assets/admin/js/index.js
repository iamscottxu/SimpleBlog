$(function() {
    getWebServerType();
});
function getWebServerType() {
    let webserver_type = $('#webserver_type');
    $.ajax({
        type: 'HEAD', // 获取头信息，type=HEAD即可
        url : window.location.href,
        complete: function(xhr,data){
            let serverType = xhr.getResponseHeader('Server');
            if (serverType == null || serverType == '') serverType = '未知';
            webserver_type.text(serverType);
        }
    });
}