if (!document.DiscoFunctions) {
    document.DiscoFunctions = {};
}
if (!document.DiscoFunctions.PropertyChangeHelper) {
    document.DiscoFunctions.PropertyValue = function (propertyField) {
        if (propertyField[0].nodeName.toLowerCase() == 'input' && propertyField.attr('type') == 'checkbox') {
            return propertyField.is(':checked');
        }
        return propertyField.val();
    };
    document.DiscoFunctions.PropertyChangeHelper = function (propertyField, fieldWatermark, updateUrl, updatePropertyName, data, validator) {
        var fieldValue = document.DiscoFunctions.PropertyValue(propertyField);
        var fieldChangeToken = null;
        var $ajaxSave = propertyField.nextAll('.ajaxSave').first();
        var $ajaxLoading = propertyField.nextAll('.ajaxLoading').first();
        var fieldChangeFunction = function () {
            $ajaxSave.hide();
            var changedValue = document.DiscoFunctions.PropertyValue(propertyField);
            if (fieldValue != changedValue) {
                if (validator && !validator(changedValue)) {
                    return;
                }

                fieldValue = changedValue;
                if (fieldChangeToken)
                    window.clearTimeout(fieldChangeToken);
                fieldChangeToken = window.setTimeout(async function () {
                    $ajaxLoading.show();
                    const body = new FormData();
                    body.append('__RequestVerificationToken', document.body.dataset.antiforgery);

                    if (!data) {
                        for (const prop in data) {
                            body.append(prop, data[prop]);
                        }
                    }
                    body.append(updatePropertyName, fieldValue);

                    try {
                        const response = await fetch(updateUrl, {
                            method: 'POST',
                            body: body
                        });

                        if (response.ok) {
                            $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                        } else {
                            alert('Unable to change property "' + updatePropertyName + '":\n' + response.statusText);
                            $ajaxLoading.hide();
                        }
                    } catch (e) {
                        alert('Unable to change property "' + updatePropertyName + '":\n' + e);
                        $ajaxLoading.hide();
                    }
                    fieldChangeToken = null;
                }, 500);
            };
        }
        if (propertyField[0].nodeName.toLowerCase() == 'input' && propertyField.attr('type') == 'checkbox') {
            propertyField.click(fieldChangeFunction);
        } else {
            propertyField.change(fieldChangeFunction);
        }
        // For Input Text Boxes
        if (propertyField[0].nodeName.toLowerCase() == 'input' && propertyField.attr('type') == 'text') {
            propertyField.keydown(function (e) {
                $ajaxSave.show();
                if (e.which == 13) {
                    $(this).blur();
                }
            })
                .blur(function () {
                    $ajaxSave.hide();
                }).focus(function () {
                    $(this).select();
                });
            if (fieldWatermark) {
                propertyField.watermark(fieldWatermark);
            }
        }
        // For TextAreas
        if (propertyField[0].nodeName.toLowerCase() == 'textarea') {
            propertyField.keydown(function () {
                $ajaxSave.show();
            }).blur(function () {
                $ajaxSave.hide();
            });
        }
    }
};
if (!document.DiscoFunctions.DateChangeUserHelper) {
    document.DiscoFunctions.DateChangeUserHelper = function (DateField, UserField, DateFieldWatermark, UpdateUrl, UpdatePropertyName, minDate, dateOnly) {
        var dateFieldValue = DateField.val();
        var dateFieldChangeToken = null;
        var $ajaxLoading = UserField.next('.ajaxLoading');
        DateField
            .watermark(DateFieldWatermark)
            .change(function () {
                var dateText = DateField.val();
                if (dateFieldValue.toLowerCase() != dateText.toLowerCase()) {
                    dateFieldValue = dateText;
                    if (dateFieldChangeToken)
                        window.clearTimeout(dateFieldChangeToken);
                    dateFieldChangeToken = window.setTimeout(async function () {
                        $ajaxLoading.show();
                        const body = new FormData();
                        body.append('__RequestVerificationToken', document.body.dataset.antiforgery);
                        body.append(UpdatePropertyName, dateFieldValue);

                        try {
                            const response = await fetch(UpdateUrl, {
                                method: 'POST',
                                body: body
                            });

                            if (response.ok) {
                                const result = await response.json();

                                UserField.text('by ' + result.UserDescription);
                                $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                            } else {
                                alert('Unable to change Date:\n' + response.statusText);
                                $ajaxLoading.hide();
                            }
                        } catch (e) {
                            alert('Unable to change Date:\n' + response.statusText);
                            $ajaxLoading.hide();
                        }
                        dateFieldChangeToken = null;
                    }, 500);
                }
            }).focus(function () {
                $(this).select();
            });

        if (dateOnly) {
            DateField.datepicker({
                defaultDate: new Date(),
                minDate: moment(minDate).toDate(),
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd',
                beforeShow: function (input, inst) {
                    $input = $(input);
                    if (!$input.val()) {
                        $input.datepicker('setDate', new Date());
                    }
                }
            });
        } else {
            DateField.datetimepicker({
                defaultDate: new Date(),
                ampm: true,
                minDate: moment(minDate).toDate(),
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd',
                beforeShow: function (input, inst) {
                    $input = $(input);
                    if (!$input.val()) {
                        $input.datetimepicker('setDate', new Date());
                    }
                }
            });
        }

    }
};
if (!document.DiscoFunctions.DateChangeHelper) {
    document.DiscoFunctions.DateChangeHelper = function (DateField, DateFieldWatermark, UpdateUrl, UpdatePropertyName, minDate, dateOnly) {
        var dateFieldValue = DateField.val();
        var dateFieldChangeToken = null;
        var $ajaxLoading = DateField.next('.ajaxLoading');
        DateField
            .watermark(DateFieldWatermark)
            .change(function () {
                var dateText = DateField.val();
                if (dateFieldValue.toLowerCase() != dateText.toLowerCase()) {
                    dateFieldValue = dateText;
                    if (dateFieldChangeToken)
                        window.clearTimeout(dateFieldChangeToken);
                    dateFieldChangeToken = window.setTimeout(async function () {
                        $ajaxLoading.show();

                        const body = new FormData();
                        body.append('__RequestVerificationToken', document.body.dataset.antiforgery);
                        body.append(UpdatePropertyName, dateFieldValue);

                        try {
                            const response = await fetch(UpdateUrl, {
                                method: 'POST',
                                body: body
                            });

                            if (response.ok) {
                                $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                            } else {
                                alert('Unable to change Date:\n' + response.statusText);
                                $ajaxLoading.hide();
                            }
                        } catch (e) {
                            alert('Unable to change Date:\n' + response.statusText);
                            $ajaxLoading.hide();
                        }

                        dateFieldChangeToken = null;
                    }, 500);
                }
            }).focus(function () {
                $(this).select();
            });

        if (dateOnly) {
            DateField.datepicker({
                defaultDate: new Date(),
                minDate: moment(minDate).toDate(),
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd',
                beforeShow: function (input, inst) {
                    $input = $(input);
                    if (!$input.val()) {
                        $input.datepicker('setDate', new Date());
                    }
                }
            });
        } else {
            DateField.datetimepicker({
                defaultDate: new Date(),
                ampm: true,
                minDate: moment(minDate).toDate(),
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd',
                beforeShow: function (input, inst) {
                    $input = $(input);
                    if (!$input.val()) {
                        $input.datetimepicker('setDate', new Date());
                    }
                }
            });
        }

    };
}
if (!document.DiscoFunctions.DateDialogCreateUpdater) {
    var dialog, dialogForm, dialogHeader, dialogDateBox, dialogDatePropertyNameBox;
    var updateUrl, friendlyName, dateField, userField, updatePropertyName, notSetDisplay, minDate, useAjax;

    function dateDialogGet() {
        if (!dialog) {
            dialog = $('<div>').attr({ 'class': 'dialog' })
            dialogForm = $('<form>').attr({ 'action': '/', 'method': 'post' }).appendTo(dialog);
            var dialogBody = $('<p>').appendTo(dialogForm);
            dialogHeader = $('<h3>').attr('autofocus', 'autofocus').appendTo(dialogBody);
            dialogDatePropertyNameBox = $('<input>').attr({ 'type': 'hidden', 'name': 'key' }).appendTo(dialogBody);
            dialogDateBox = $('<input>').attr({ 'type': 'datetime', 'name': 'value' }).css({ 'display': 'block', 'margin-top': 15, 'margin-left': 'auto', 'margin-right': 'auto' }).appendTo(dialogBody);
            $('<input>').attr({ 'type': 'hidden', 'name': 'redirect' }).val('true').appendTo(dialogBody);

            dialog.dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                buttons: {
                    "Update": function () {
                        dateDialogUpdate();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                open: function () {
                    dialog.dialog('widget').find('.ui-dialog-buttonpane :tabbable:first').focus();
                }
            });
            dialogDateBox.datetimepicker({
                defaultDate: new Date(),
                ampm: true,
                changeYear: true,
                changeMonth: true,
                dateFormat: 'yy/mm/dd',
            });
        }
        return dialog;
    }

    async function dateDialogUpdate() {
        var dateValue = dialogDateBox.val();

        if (useAjax) {
            // Use Ajax
            var $dateField, $userField;
            $dateField = $('#' + dateField);
            if (userField)
                $userField = $('#' + userField);

            dialog.dialog("close");

            var $ajaxLoading = ($userField ? $userField.next('.ajaxLoading') : $dateField.next('.ajaxLoading')).show();

            const body = new FormData();
            body.append('__RequestVerificationToken', document.body.dataset.antiforgery);
            body.append('key', updatePropertyName);
            body.append('value', dateValue);

            try {
                const response = await fetch(updateUrl, {
                    method: 'POST',
                    body: body
                });

                if (response.ok) {
                    const result = await response.json();

                    if (result.DateTimeFull) {
                        $dateField.attr('data-isodate', result.DateTimeISO8601)
                            .attr('data-livestamp', result.DateTimeUnixEpoc)
                            .attr('title', result.DateTimeFull)
                            .text(result.DateTimeFriendly);
                    } else {
                        $dateField.attr('data-isodate', '')
                            .attr('data-livestamp', '-1')
                            .attr('title', notSetDisplay)
                            .text(notSetDisplay);
                    }
                    if ($userField)
                        $userField.text('by ' + result.UserDescription);
                    $ajaxLoading.hide().next('.ajaxOk').show().delay('fast').fadeOut('slow');
                } else {
                    alert('Unable to change ' + friendlyName + ' Date:\n' + response.statusText);
                    $ajaxLoading.hide();
                }
            } catch (e) {
                alert('Unable to change ' + friendlyName + ' Date:\n' + response.statusText);
                $ajaxLoading.hide();
            }
        } else {
            // Post Form & Redirect
            dialog.dialog("disable");
            dialog.dialog("option", "buttons", null);

            dialogDatePropertyNameBox.val(updatePropertyName);
            dialogForm.attr('action', updateUrl);
            dialogForm.submit();
        }
    }

    function dateDialogOpen(UpdateUrl, FriendlyName, DateField, UserField, UpdatePropertyName, NotSetDisplay, MinDate, UseAjax) {
        updateUrl = UpdateUrl;
        friendlyName = FriendlyName;
        dateField = DateField;
        userField = UserField;
        updatePropertyName = UpdatePropertyName;
        notSetDisplay = NotSetDisplay;
        minDate = MinDate;
        useAjax = UseAjax;

        var d = dateDialogGet();

        d.dialog('option', 'title', friendlyName);
        dialogHeader.text(friendlyName + ' Date');

        var dfVal = $('#' + DateField).attr('data-isodate');

        if (dfVal)
            dialogDateBox.datetimepicker('setDate', new Date(dfVal));
        else
            dialogDateBox.datetimepicker('setDate', new Date());

        if (MinDate)
            dialogDateBox.datetimepicker('option', 'minDate', moment(minDate).toDate());
        else
            dialogDateBox.datetimepicker('option', 'minDate', null);

        d.dialog('open');
    }

    function dateDialogCreateUpdater(UpdateUrl, FriendlyName, DateField, UserField, UpdatePropertyName, NotSetDisplay, MinDate, UseAjax) {
        $('<a>').attr({ href: '#', 'class': 'button small', style: 'margin-right: 5px;' }).text('Update').click(function (event) {
            event.preventDefault();
            dateDialogOpen(UpdateUrl, FriendlyName, DateField, UserField, UpdatePropertyName, NotSetDisplay, MinDate, UseAjax);
        }).insertBefore('#' + DateField);
    }

    document.DiscoFunctions.DateDialogCreateUpdater = dateDialogCreateUpdater;
}