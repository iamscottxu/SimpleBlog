$(function () {
    let addUE = UE.getEditor('add_item_container', { initialFrameWidth: null, topOffset: 43, zIndex: 1120 });
    let editUE = UE.getEditor('edit_item_container', { initialFrameWidth: null, topOffset: 43, zIndex: 1120 });
    UE.Editor.prototype._bkGetActionUrl = UE.Editor.prototype.getActionUrl;
    UE.Editor.prototype.getActionUrl = function (action) {
        switch (action){
            case 'config':
                return '../Editor/Config';
            case 'uploadimage':
                return '../Editor/UploadImage';
            case 'uploadscrawl':
                return '../Editor/UploadScrawl';
            case 'uploadvideo':
                return '../Editor/UploadVideo';
            case 'uploadfile':
                return '../Editor/UploadFile';
            case 'catchimage':
                return '../Editor/CatchImage';
            case 'listimage':
                return '../Editor/ListImage';
            case 'listfile':
                return '../Editor/ListFile';
        }
    }

    $('.delete-item').click(function () {
        $('#delete_item_confirm').modal({
            relatedTarget: this,
            onConfirm: function (options) {
                var relatedTarget = $(this.relatedTarget);
                var form = relatedTarget.parents('form');
                $.ajax({
                    url: form.attr('action'),
                    data: {
                        __RequestVerificationToken: form.find('[name="__RequestVerificationToken"]').val(),
                        deleteGuid: [relatedTarget.parents('li.table-item').data('guid')]
                    },
                    type: form.attr('method'),
                    beforeSend: openModalLoading,
                    complete: closeModalLoading,
                    dataType: 'json',
                    error: showNetError,
                    success: data => jsonAjaxSuccess(data, function () { location.reload() })
                });
            }
        });
    });

    $('.edit-item').click(function () {
        let guid = $(this).parents('li.table-item').data('guid');
        $('#edit_item_guid').val(guid);
        $.ajax({
            url: 'ArticleManager_GetItem',
            data: {
                guid: guid
            },
            type: 'post',
            beforeSend: openModalLoading,
            complete: closeModalLoading,
            dataType: 'json',
            error: showNetError,
            success: data => jsonAjaxSuccess(data, function (data) {
                $('#edit_item_name').val(data.name);
                $('#edit_item_type > option').removeAttr('selected');
                $('#edit_item_type > option[value="' + data.articleTypeGuid + '"]').attr('selected', true);
                $('#edit_item_type').trigger('changed.selected.amui');
                $('#edit_item_label > option').removeAttr('selected');
                for (labelGuid of data.articleLabelGuids) {
                    $('#edit_item_label > option[value="' + labelGuid + '"]').attr('selected', true);
                }
                $('#edit_item_label').trigger('chosen:updated');
                editUE.ready(function () {
                    editUE.setContent(data.content);
                    $('#edit_item').modal();
                });
            })
        });
    });

    $('#delete_select').click(function () {
        $('#delete_select_confirm').modal({
            relatedTarget: this,
            onConfirm: function (options) {
                var table_form = $('#table_form');
                if (table_form.find('[name="deleteGuid"]:checked').length > 0)
                    table_form.submit();
            }
        });
    });

    $("#articleType_select").on("change", function () {
        $(this).parents('form').submit();
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

    setInterval(function () {
        let edui_editor_toolbarbox = $('#add_item_form .edui-editor-toolbarbox.edui-default');
        let first_am_form_group = $('#add_item_form > .am-form-group:first');
        if (edui_editor_toolbarbox.css('position') == 'fixed')
            first_am_form_group.css('margin-top', edui_editor_toolbarbox.height() + 'px');
        else
            first_am_form_group.css('margin-top', '0');
    }, 200);

});