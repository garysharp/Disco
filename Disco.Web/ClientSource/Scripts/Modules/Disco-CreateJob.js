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

        if (!document.DiscoFunctions) {
            document.DiscoFunctions = {};
        }
        document.DiscoFunctions.CreateOpenJobDialog = function (url) {
            createJobDialog = $('<div>').attr('id', 'createJobDialog').css({ paddingTop: '0' }).appendTo(document.body);

            createJobDialog.dialog({
                resizable: false,
                draggable: false,
                modal: true,
                autoOpen: false,
                title: 'Create Job',
                width: 850,
                height: Math.min(670, $(window).height() - 50),
                close: function () {
                    createJobDialog.find('iframe').attr('src', 'about:blank');
                    createJobDialog.dialog('destroy').remove();
                    createJobDialog = null;
                },
                buttons: {}
            });

            var iframe = $('<iframe>')
                .attr({ 'src': url })
                .css({
                    'border': 'none',
                    'height': '100%',
                    'width': '100%'
                })
                .appendTo(createJobDialog);

            createJobDialog[0].discoDialogMethods = dialogMethods;

            window.setTimeout(function () {
                createJobDialog.dialog('open');
            }, 1);
        }

        // Create Job Button
        $('#buttonCreateJob').click(function () {
            var $this = $(this);
            var href = $this.attr('href');

            document.DiscoFunctions.CreateOpenJobDialog(href);

            return false;
        });
    })
})($, window, document);