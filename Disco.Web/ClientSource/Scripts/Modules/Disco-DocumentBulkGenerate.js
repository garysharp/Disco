$(() => {
    const users = [];
    const $table = $('#DocumentTemplate_BulkGenerate table');

    function redrawTable() {
        if (users.length > 0) {
            $table.find('tbody tr:first-child').hide();
        }
        const $tbody = $table.find('tbody');
        let checkedCount = 0;
        for (var i = 0; i < users.length; i++) {
            var user = users[i];
            if (user.checkbox === undefined) {
                const tr = $('<tr><td><input id="BulkGenerate_User_' + i.toString() + '" type="checkbox" /></td><td><label for="BulkGenerate_User_' + i.toString() + '"></label></td><td><span class="name"></span></td><td><span class="scope"></span></td></tr>');
                const checkbox = tr.find('input')[0];
                const label = tr.find('label');
                const name = tr.find('span.name');
                const scope = tr.find('span.scope');
                label.text(user.Id);
                scope.text(user.Scope);
                if (!user.IsError) {
                    checkbox.checked = true;
                    name.text(user.DisplayName);
                    checkedCount++;
                } else {
                    tr.addClass('error');
                    checkbox.checked = false;
                    checkbox.disabled = true;
                }
                user.checkbox = checkbox;
                $tbody.append(tr);
            } else {
                if (!user.IsError && user.checkbox.checked) {
                    checkedCount++;
                }
            }
        }
        if (checkedCount > 0) {
            $('#BulkGenerate').attr('disabled', null);
        } else {
            $('#BulkGenerate').attr('disabled', 'disabled');
        }
    }

    function addUsers(r) {
        let changeCount = 0;
        for (var i = 0; i < r.length; i++) {
            const user = r[i];
            const record = users.find(u => u.Id === user.Id);
            if (record === undefined || user.IsError) {
                users.push(user);
                changeCount++;
            } else if (record.checkbox !== undefined && !record.checkbox.checked && !record.IsError) {
                record.checkbox.checked = true;
                changeCount++;
            };
        }
        if (changeCount) {
            redrawTable();
        }
    }

    function excludeUsers(r) {
        let changeCount = 0;
        for (var i = 0; i < r.length; i++) {
            const user = r[i];
            const record = users.find(u => u.Id === user.Id);
            if (record !== undefined && record.checkbox !== undefined) {
                record.checkbox.checked = false;
                changeCount++;
            }
        }
        if (changeCount) {
            redrawTable();
        }
    }

    function excludeOtherUsers(r) {
        let changeCount = 0;
        for (var i = 0; i < users.length; i++) {
            const user = users[i];
            if (!r.find(u => u.Id === user.Id)) {
                user.checkbox.checked = false;
                changeCount++;
            }
        }
        if (changeCount) {
            redrawTable();
        }
    }

    $table.on('change', 'input[type="checkbox"]', e => {
        redrawTable();
    });

    $('#BulkGenerate').click(e => {
        let userIds = [];
        for (var i = 0; i < users.length; i++) {
            var user = users[i];
            if (!user.IsError && user.checkbox !== undefined && user.checkbox.checked) {
                userIds.push(user.Id);
            }
        }
        if (userIds.length > 0) {
            $('#DocumentTemplate_BulkGenerate_DataIds').val(userIds.join('\r\n'));
            $('#BulkGenerate').closest('form').submit();
        }
    });

    let dialogAddUsers = null;
    $('#AddUsers').click(e => {
        e.preventDefault();

        let dialog = dialogAddUsers;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                if (form.reportValidity()) {
                    const body = new FormData(form);
                    fetch(form.action, {
                        method: 'POST',
                        body: body
                    })
                        .then(r => r.json())
                        .then(r => {
                            applier(r);
                            dialog.find('textarea').html('').val('');
                            dialog.dialog("close");
                            dialog.dialog("enable");
                        })
                        .catch(reason => {
                            alert('Failed to validate users: ' + reason);
                        });
                }
                dialog.dialog("disable");
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddUsers').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Other Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Users": function () {
                        action(excludeUsers);
                    },
                    "Add Users": function () {
                        action(addUsers);
                    }
                }
            });
            dialogAddUsers = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddGroupMembers = null;
    $('#AddGroupMembers').click(e => {
        e.preventDefault();

        let dialog = dialogAddGroupMembers;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                if (form.reportValidity()) {
                    const body = new FormData(form);
                    fetch(form.action, {
                        method: 'POST',
                        body: body
                    })
                        .then(r => r.json())
                        .then(r => {
                            applier(r);
                            dialog.find('input[type="text"]').val('');
                            dialog.dialog("close");
                            dialog.dialog("enable");
                        })
                        .catch(reason => {
                            alert('Failed to validate group: ' + reason);
                        });
                }
                dialog.dialog("disable");
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddGroupMembers').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Non-Group Members": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Group Members": function () {
                        action(excludeUsers);
                    },
                    "Add Group Members": function () {
                        action(addUsers);
                    }
                }
            });
            const $input = dialog.find('input[type="text"]');
            $input.autocomplete({
                source: $input.attr('data-autocomplete-src'),
                minLength: 2,
                select: function (e, ui) {
                    $input.val(ui.item.Id);
                    return false;
                }
            }).data('ui-autocomplete')._renderItem = function (ul, item) {
                return $("<li>")
                    .data("item.autocomplete", item)
                    .append("<a><strong>" + item.Name + "</strong><br>" + item.Id + " (" + item.Type + ")</a>")
                    .appendTo(ul);
            };
            dialogAddGroupMembers = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddUserFlag = null;
    $('#AddUserFlag').click(e => {
        e.preventDefault();

        let dialog = dialogAddUserFlag;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                if (form.reportValidity()) {
                    const body = new FormData(form);
                    fetch(form.action, {
                        method: 'POST',
                        body: body
                    })
                        .then(r => r.json())
                        .then(r => {
                            applier(r);
                            dialog.find('input[name="flagId"]').val('');
                            dialog.find('div.item').removeClass('selected');
                            dialog.dialog("close");
                            dialog.dialog("enable");
                        })
                        .catch(reason => {
                            alert('Failed to validate user flag: ' + reason);
                        });
                }
                dialog.dialog("disable");
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddUserFlag').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Unassigned Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Assigned Users": function () {
                        action(excludeUsers);
                    },
                    "Add Assigned Users": function () {
                        action(addUsers);
                    }
                }
            });
            const $input = dialog.find('input[name="flagId"]');
            dialog.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                $input.val($target.attr('data-userflagid'));
                dialog.find('div.item').removeClass('selected');
                $target.addClass('selected');
                return false;
            });
            dialogAddUserFlag = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddDeviceProfile = null;
    $('#AddDeviceProfile').click(e => {
        e.preventDefault();
        let dialog = dialogAddDeviceProfile;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                const input = dialog.find('input[name="deviceProfileId"]');
                if (input.val()) {
                    if (form.reportValidity()) {
                        const body = new FormData(form);
                        fetch(form.action, {
                            method: 'POST',
                            body: body
                        })
                            .then(r => r.json())
                            .then(r => {
                                applier(r);
                                input.val('');
                                dialog.find('div.item').removeClass('selected');
                                dialog.dialog("close");
                                dialog.dialog("enable");
                            })
                            .catch(reason => {
                                alert('Failed to validate device profile: ' + reason);
                            });
                        dialog.dialog("disable");
                    }
                }
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddDeviceProfile').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Unassigned Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Assigned Users": function () {
                        action(excludeUsers);
                    },
                    "Add Assigned Users": function () {
                        action(addUsers);
                    }
                }
            });
            const $input = dialog.find('input[name="deviceProfileId"]');
            dialog.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                $input.val($target.attr('data-id'));
                dialog.find('div.item').removeClass('selected');
                $target.addClass('selected');
                return false;
            });
            dialogAddDeviceProfile = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddDeviceBatch = null;
    $('#AddDeviceBatch').click(e => {
        e.preventDefault();
        let dialog = dialogAddDeviceBatch;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                const input = dialog.find('input[name="deviceBatchId"]');
                if (input.val()) {
                    if (form.reportValidity()) {
                        const body = new FormData(form);
                        fetch(form.action, {
                            method: 'POST',
                            body: body
                        })
                            .then(r => r.json())
                            .then(r => {
                                applier(r);
                                input.val('');
                                dialog.find('div.item').removeClass('selected');
                                dialog.dialog("close");
                                dialog.dialog("enable");
                            })
                            .catch(reason => {
                                alert('Failed to validate device batch: ' + reason);
                            });
                        dialog.dialog("disable");
                    }
                }
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddDeviceBatch').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Unassigned Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Assigned Users": function () {
                        action(excludeUsers);
                    },
                    "Add Assigned Users": function () {
                        action(addUsers);
                    }
                }
            });
            const $input = dialog.find('input[name="deviceBatchId"]');
            dialog.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                $input.val($target.attr('data-id'));
                dialog.find('div.item').removeClass('selected');
                $target.addClass('selected');
                return false;
            });
            dialogAddDeviceBatch = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddDocumentAttachment = null;
    $('#AddDocumentAttachment').click(e => {
        e.preventDefault();
        let dialog = dialogAddDocumentAttachment;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                const input = dialog.find('input[name="documentTemplateId"]');
                if (input.val()) {
                    if (form.reportValidity()) {
                        const body = new FormData(form);
                        fetch(form.action, {
                            method: 'POST',
                            body: body
                        })
                            .then(r => r.json())
                            .then(r => {
                                applier(r);
                                input.val('');
                                dialog.find('div.item').removeClass('selected');
                                dialog.dialog("close");
                                dialog.dialog("enable");
                            })
                            .catch(reason => {
                                alert('Failed to validate device batch: ' + reason);
                            });
                        dialog.dialog("disable");
                    }
                }
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddDocumentAttachment').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 460,
                buttons: {
                    "Exclude Unassigned Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Assigned Users": function () {
                        action(excludeUsers);
                    },
                    "Add Assigned Users": function () {
                        action(addUsers);
                    }
                }
            });
            const $input = dialog.find('input[name="documentTemplateId"]');
            dialog.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                $input.val($target.attr('data-id'));
                dialog.find('div.item').removeClass('selected');
                $target.addClass('selected');
                return false;
            });
            dialogAddDocumentAttachment = dialog;
        }
        dialog.dialog('open');
        return false;
    });

    let dialogAddUserDetail = null;
    $('#AddUserDetail').click(e => {
        e.preventDefault();
        let dialog = dialogAddUserDetail;
        if (!dialog) {
            const action = function (applier) {
                const form = dialog.find('form')[0];
                const key = $(form).find('input[name="key"]');
                if (key.val()) {
                    if (form.reportValidity()) {
                        const body = new FormData(form);
                        fetch(form.action, {
                            method: 'POST',
                            body: body
                        })
                            .then(r => r.json())
                            .then(r => {
                                applier(r);
                                key.val('');
                                $(form).find('input[name="value"]').val('');
                                $('#DocumentTemplate_BulkGenerate_Dialog_AddUserDetail_Value').empty();
                                dialog.find('div.item').removeClass('selected');
                                dialog.dialog("close");
                                dialog.dialog("enable");
                            })
                            .catch(reason => {
                                alert('Failed to validate user detail: ' + reason);
                            });
                        dialog.dialog("disable");
                    }
                }
            }
            dialog = $('#DocumentTemplate_BulkGenerate_Dialog_AddUserDetail').dialog({
                resizable: false,
                modal: true,
                autoOpen: false,
                width: 690,
                buttons: {
                    "Exclude Unmatched Users": function () {
                        action(excludeOtherUsers);
                    },
                    "Exclude Matched Users": function () {
                        action(excludeUsers);
                    },
                    "Add Matched Users": function () {
                        action(addUsers);
                    }
                }
            });
            const $key = dialog.find('input[name="key"]');
            const $value = dialog.find('input[name="value"]');
            const $keys = dialog.find('#DocumentTemplate_BulkGenerate_Dialog_AddUserDetail_Key');
            const $values = dialog.find('#DocumentTemplate_BulkGenerate_Dialog_AddUserDetail_Value');
            $keys.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                const keyValue = $target.attr('data-id');
                $key.val(keyValue);
                $keys.find('div.item').removeClass('selected');
                $target.addClass('selected');

                $values.empty();
                $values.append($('<i class="ajaxLoading" title="Loading"></i>'));

                const form = dialog.find('form')[1];
                const body = new FormData(form);
                fetch(form.action, {
                    method: 'POST',
                    body: body
                })
                    .then(r => r.json())
                    .then(r => {
                        $values.empty();

                        const allValues = $('<div class="item selected" data-id=""><i class="fa fa-info fa-fw fa-lg"></i><em>All Matched Users</em></div>');
                        allValues.appendTo($values);
                        $value.val('');

                        r.map(v => {
                            const container = $('<div class="item"><i class="fa fa-info fa-fw fa-lg"></i ></div>');
                            container.attr('data-id', v);
                            const span = $('<span>').text(v);
                            span.appendTo(container);
                            container.appendTo($values);
                        })
                    })
                    .catch(reason => {
                        alert('Failed to validate user detail: ' + reason);
                    });
                dialog.dialog("disable");

                return false;
            });
            $values.on('click', 'div.item:not(.disabled)', e => {
                e.preventDefault();
                const $target = $(e.currentTarget);
                $value.val($target.attr('data-id'));
                $values.find('div.item').removeClass('selected');
                $target.addClass('selected');
            });
            dialogAddUserDetail = dialog;
        }
        dialog.dialog('open');
        return false;
    });

});