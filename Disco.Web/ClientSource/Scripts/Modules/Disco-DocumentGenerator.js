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
        const handlersPackageUrl = $container.attr('data-handlerspackageurl');
        let $handlersDialog = null;
        let lastTemplateId = null;
        let lastTemplateName = null;

        const downloadPdf = function (templateId) {
            let action = generatePdfUrl;
            if (templateId.lastIndexOf('Package:', 0) === 0) {
                templateId = templateId.substring(8);
                action = generatePackageUrl;
            }

            if (!$generationHost) {
                $generationHost = $('<iframe>')
                    .attr('title', 'Document Generation Host')
                    .addClass('hidden')
                    .appendTo('body')
                    .contents();
                $generationHost[0].body.innerHTML = '<form method="post"><input type="hidden" name="__RequestVerificationToken" value="' + document.body.dataset.antiforgery + '"><input type="hidden" name="id"><input type="hidden" name="targetId"></form>';
            }
            const form = $generationHost[0].forms[0];
            form.action = action;
            form.id.value = templateId;
            form.targetId.value = targetId;
            form.submit();
        }

        const viewPdf = function (templateId, templateName) {
            let action = generatePdfUrl;
            if (templateId.lastIndexOf('Package:', 0) === 0) {
                templateId = templateId.substring(8);
                action = generatePackageUrl;
            }

            const $dialog = $('<div id="Document_Generation_View_Dialog" class="dialog">')
                .appendTo(document.body)
                .dialog({
                    resizable: false,
                    modal: true,
                    autoOpen: true,
                    width: 850,
                    height: 700,
                    title: 'Document: ' + templateName,
                    close: function () {
                        $dialog.dialog('destroy').remove();
                    }
                });
            const $iframe = $('<iframe>').appendTo($dialog);
            const $iframeContents = $iframe.contents()[0];
            $iframeContents.body.innerHTML = '<form method="post"><input type="hidden" name="__RequestVerificationToken" value="' + document.body.dataset.antiforgery + '"><input type="hidden" name="id"><input type="hidden" name="targetId"><input type="hidden" name="inline" value="True"></form>';
            const form = $iframeContents.forms[0];
            form.action = action;
            form.id.value = templateId;
            form.targetId.value = targetId;
            form.submit();
        }

        const updateHandlers = function (templateId) {
            let action = handlersUrl;
            if (templateId.lastIndexOf('Package:', 0) === 0) {
                templateId = templateId.substring(8);
                action = handlersPackageUrl;
            }

            const $handlerPicker = $handlersDialog.find('.handlerPicker');
            const $loadingUi = $handlersDialog.find('#Document_Generation_Dialog_Handlers_Loading');

            $handlerPicker.find('div.handler').remove();
            $loadingUi.show();

            var formData = new FormData();
            formData.append('__RequestVerificationToken', document.body.dataset.antiforgery);
            formData.append('id', decodeURI(templateId));
            formData.append('targetId', decodeURI(targetId));
            fetch(action, {
                method: 'POST',
                body: formData
            }).then(r => r.json())
                .then(data => {
                    $loadingUi.hide();
                    $.each(data.Handlers, (i, h) => {
                        $('<div class="handler">').text(h.Title).attr({
                            'data-id': h.Id,
                            'data-uiurl': h.UiUrl
                        }).prepend($('<i class="fa fa-fw fa-lg">').addClass('fa-' + h.Icon)).appendTo($handlerPicker);
                    });
                });
        }

        $control.change(function () {
            var templateId = $control.val();
            if (templateId) {
                lastTemplateId = templateId;
                lastTemplateName = $control[0].selectedOptions[0].label;
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
                        });
                        if (navigator.pdfViewerEnabled) {
                            $handlersDialog.find('#Document_Generation_Dialog_View').css('display', 'block').on('click', e => {
                                e.preventDefault();
                                $handlersDialog.dialog('close');
                                viewPdf(lastTemplateId, lastTemplateName);
                                return false;
                            });
                        }
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
                                const formData = new FormData();
                                formData.append('__RequestVerificationToken', document.body.dataset.antiforgery);
                                fetch(uiurl, {
                                    method: 'POST',
                                    body: formData,
                                })
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