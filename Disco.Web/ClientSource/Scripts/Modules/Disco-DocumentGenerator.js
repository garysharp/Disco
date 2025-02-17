(function (window, document, $) {
    $(function () {
        let $generationHost = null;
        const $container = $('#Document_Generation_Container');
        const $control = $container.find('#Document_Generate');
        const targetId = $container.attr('data-targetid');
        const targetType = $container.attr('data-targettype');
        const generatePdfUrl = $container.attr('data-generatepdfurl');
        const generatePackageUrl = $container.attr('data-generatepackageurl');
        const handlersPresent = $container.attr('data-handlerspresent') === 'true';
        const handlersUrl = $container.attr('data-handlersurl');
        let $handlersDialog = null;
        let lastTemplateId = null;

        const downloadPdf = function (templateId) {

            let url;
            if (templateId.lastIndexOf('Package:', 0) === 0)
                url = generatePackageUrl + templateId.substring(8);
            else
                url = generatePdfUrl + templateId;
            url = url + '?TargetId=' + targetId;

            if ($.connection && $.connection.hub && $.connection.hub.transport &&
                $.connection.hub.transport.name == 'foreverFrame') {
                // SignalR active with foreverFrame transport - use popup window
                window.open(url, '_blank', 'height=150,width=250,location=no,menubar=no,resizable=no,scrollbars=no,status=no,toolbar=no');
            } else {
                // use iFrame
                if (!$generationHost) {
                    $generationHost = $('<iframe>')
                        .attr({ 'src': url, 'title': 'Document Generation Host' })
                        .addClass('hidden')
                        .appendTo('body')
                        .contents();
                } else {
                    $generationHost[0].location.href = url;
                }
            }
        }

        const updateHandlers = function (templateId) {
            const $handlerPicker = $handlersDialog.find('.handlerPicker');
            const $loadingUi = $handlersDialog.find('#Document_Generation_Dialog_Handlers_Loading');

            $handlerPicker.find('div.handler').remove();
            $loadingUi.show();

            var formData = new FormData();
            formData.append('templateId', decodeURI(templateId));
            formData.append('targetId', decodeURI(targetId));
            fetch(handlersUrl, {
                method: 'POST',
                body: formData
            }).then(r => r.json())
                .then(data => {
                    $loadingUi.hide();
                    $.each(data.Handlers, (i, h) => {
                        $('<div class="handler">').text(h.Title).attr({
                            'data-id': h.Id,
                            'data-uiurl': h.UiUrl
                        }).prepend($('<i class="fa fa-fw fa-lg">').addClass('fa-'+h.Icon)).appendTo($handlerPicker);
                    });
                });
        }

        $control.change(function () {
            var templateId = $control.val();
            if (templateId) {
                lastTemplateId = templateId;
                if (handlersPresent) {
                    if (!$handlersDialog) {
                        $handlersDialog = $container.find('#Document_Generation_Dialog');
                        $handlersDialog.dialog({
                            width: 750,
                            height: 500,
                            resizable: false,
                            modal: true,
                            autoOpen: false,
                            buttons: {
                                Cancel: function () {
                                    $(this).dialog("close");
                                }
                            }
                        });
                        $handlersDialog.find('#Document_Generation_Dialog_Download').click(e => {
                            e.preventDefault();
                            downloadPdf(lastTemplateId);
                            $handlersDialog.dialog('close');
                            return false;
                        })
                        const $handlerPicker = $handlersDialog.find('.handlerPicker');
                        const $Document_Generation_Dialog_Download_Container = $handlersDialog.find('#Document_Generation_Dialog_Download_Container');
                        const $Document_Generation_Dialog_HandlerUI = $handlersDialog.find('#Document_Generation_Dialog_HandlerUI');
                        $handlerPicker.on('click', 'div[data-id]', e => {
                            $handlerPicker.find('div').removeClass('selected');
                            const $this = $(e.currentTarget);
                            $this.addClass('selected');
                            const handlerId = $this.attr('data-id');
                            if (handlerId === 'download') {
                                $Document_Generation_Dialog_Download_Container.show();
                                $Document_Generation_Dialog_HandlerUI.hide();
                                $Document_Generation_Dialog_HandlerUI.empty();
                            } else {
                                $Document_Generation_Dialog_Download_Container.hide();
                                $Document_Generation_Dialog_HandlerUI.empty();
                                $Document_Generation_Dialog_HandlerUI.show();
                                const uiurl = $this.attr('data-uiurl');
                                fetch(uiurl, { method: 'POST' })
                                    .then(r => r.text())
                                    .then(html => {
                                        $Document_Generation_Dialog_HandlerUI.html(html);
                                    })
                            }
                        });
                    }

                    const $handlerPicker = $handlersDialog.find('.handlerPicker');
                    const $Document_Generation_Dialog_Download_Container = $handlersDialog.find('#Document_Generation_Dialog_Download_Container');
                    const $Document_Generation_Dialog_HandlerUI = $handlersDialog.find('#Document_Generation_Dialog_HandlerUI');
                    $handlerPicker.find('div').removeClass('selected');
                    $handlerPicker.find('div[data-id=download]').addClass('selected');
                    $Document_Generation_Dialog_Download_Container.show();
                    $Document_Generation_Dialog_HandlerUI.hide();
                    $Document_Generation_Dialog_HandlerUI.empty();

                    $handlersDialog.dialog('option', 'title', 'Generate Document: ' + $control[0].selectedOptions[0].label);
                    $handlersDialog.dialog('open');
                    updateHandlers(templateId);
                } else {
                    downloadPdf(templateId);
                }
                $control.val('').blur();
            }
        });

    })
})(window, document, $);