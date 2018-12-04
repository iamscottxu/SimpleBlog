function sendFormDataByAjax(form, beforeSend, complete, success, error) {
    $(form).validator({
        submit: function() {
            if(this.isFormValid()) {
                $.ajax({
                    url: $(form).attr('action'),
                    data: new FormData(form),
                    processData: false,
                    contentType: false,
                    beforeSend: beforeSend,
                    complete: complete,
                    dataType: 'json',
                    type: $(form).attr('method'),
                    success: success,
                    error: error
                });
            }
            return false;
         }
    });
}

function reLoadPage() {
    let currentUrl = window.location.href;
    let targetUrl = currentUrl.replace(/\?\S*$/, "");
    window.location.href = targetUrl;
}

function openModalLoading() {
    $('#modal-loading').modal();
}

function closeModalLoading() {
    $('#modal-loading').modal('close');
}

function showError(message) {
    $('#error-alert').find('.am-modal-bd').text(message);
    $('#error-alert').modal();
}

function showNetError() {
    showError("网络错误。");
}

function jsonAjaxSuccess(data, success) {
    if(data.success) success(data.data);
    else showError(data.message);
}

function replacePagination() {
    let setLiHref = function(li, href) {
        li.children('a').attr('href', href);
    };
    let disabledLi = function(li) {
        li.addClass('am-disabled');
    };
    let archiveLi = function(li) {
        li.addClass('am-active');
    };
    $('pagination').each(function(index, value) {
        let pageIndex = $(value).attr('pageIndex');
        let pageCount = $(value).attr('pageCount');
        let parameters = $(value).attr('parameters');
        if (typeof(parameters) === 'undefined' || parameters == null) parameters = '';
        else parameters = "&" + parameters;
        let ul = $('<ul class="am-pagination tpl-pagination"></ul>');
        let liFirstPage = $('<li><a>«</a></li>');
        if (pageIndex == 0) disabledLi(liFirstPage);
            else setLiHref(liFirstPage, '?page=0' + parameters);
        ul.append(liFirstPage);
        let pageStart = pageIndex - 2;
        let pageEnd = pageIndex + 2;
        if (pageStart < 0) {
            pageStart = 0;
            pageEnd = pageCount < 5 ? pageCount - 1 : 4;
        } else if (pageEnd > pageCount - 1) {
            pageStart = pageCount < 5 ? 0 : pageCount - 6;
            pageEnd = pageCount - 1;
        }
        for (let i = 0; i < pageCount; i++) {
            let liPageIndex = $('<li><a>' + (i + 1) + '</a></li>');
            if (pageIndex == i) archiveLi(liPageIndex);
                else setLiHref(liPageIndex, '?page=' + i + parameters);
            ul.append(liPageIndex);
        }
        let liLastPage = $('<li><a>»</a></li>');
        if (pageIndex == pageCount - 1) disabledLi(liLastPage);
            else setLiHref(liLastPage, '?page=' + (pageCount - 1) + parameters);
        ul.append(liLastPage);
        $(value).replaceWith(ul);
    });
}

function loadChosenSelect(){
    $('.chosen-select').chosen({
        no_results_text: '未找到匹配的项！'
    });
}

$(function(){
    replacePagination();
    loadChosenSelect();
});