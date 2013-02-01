///#source 1 1 /ClientSource/Scripts/Modules/Disco-CreateJob/disco.createjob.js
/// <reference path="../../Core/jquery-1.8.1.js" />
/// <reference path="../../Core/jquery-ui-1.8.23.js" />

(function ($, window, document) {
    $(function () {
        var createJobDialog = null;
        var dialogMethods = {
            close: function () {
                createJobDialog.dialog('close');
            },
            setButtons: function (buttons) {
                if (createJobDialog)
                    createJobDialog.dialog('option', 'buttons', buttons);
            }
        }

        // Create Job Button
        $('#buttonCreateJob').click(function () {
            var $this = $(this);
            var href = $this.attr('href');
            var iframe = $('<iframe>').attr({ 'src': href }).width('100%').height('100%').css('border', 'none');
            createJobDialog = $('<div>').attr('id', 'createJobDialog').width('100%').height('100%')
                .appendTo(document)
                .append(iframe)
                .dialog({
                    resizable: false,
                    draggable: false,
                    modal: true,
                    autoOpen: true,
                    title: 'Create Job',
                    width: 850,
                    height: $(window).height() - 50,
                    close: function () {
                        createJobDialog.find('iframe').attr('src', 'about:blank');
                        createJobDialog.dialog('destroy').remove();
                        createJobDialog = null;
                    },
                    buttons: {}
                });
            createJobDialog[0].discoDialogMethods = dialogMethods;

            return false;
        });
    })
})($, window, document);
