﻿$(function () {
    $('.delete-item').click(function () {
        $('#delete_item_confirm').modal({
            relatedTarget: this,
            onConfirm: function (options) {
                let relatedTarget = $(this.relatedTarget);
                let form = relatedTarget.parents('form');
                $.ajax({
                    url: form.attr('action'),
                    data: {
                        __RequestVerificationToken: form.find('[name="__RequestVerificationToken"]').val(),
                        deleteGuid: [relatedTarget.parents('tr').data('guid')]
                    },
                    type: form.attr('method'),
                    beforeSend: openModalLoading,
                    complete: closeModalLoading,
                    dataType: 'json',
                    error: showNetError,
                    success: data => jsonAjaxSuccess(data, function () {
                        location.reload()
                    })
                });
            }
        });
    });

    $('.edit-item').click(function () {
        $('#edit_item_guid').val($(this).parents('tr').data('guid'));
        $('#edit_item_name').val($(this).parents('tr').find('.table-articleLabel-name').text());
    });

    $('#delete_select').click(function () {
        $('#delete_select_confirm').modal({
            relatedTarget: this,
            onConfirm: function (options) {
                let table_form = $('#table_form');
                if (table_form.find('[name="deleteGuid"]:checked').length > 0)
                    table_form.submit();
            }
        });
    });

    sendFormDataByAjax(
        $('#add_item_form')[0],
        openModalLoading,
        closeModalLoading,
        data => jsonAjaxSuccess(data, reLoadPage),
        showNetError
    );

    sendFormDataByAjax(
        $('#edit_item_form')[0],
        openModalLoading,
        closeModalLoading,
        data => jsonAjaxSuccess(data, reLoadPage),
        showNetError
    );

    sendFormDataByAjax(
        $('#table_form')[0],
        openModalLoading,
        closeModalLoading,
        data => jsonAjaxSuccess(data, reLoadPage),
        showNetError
    );
});
