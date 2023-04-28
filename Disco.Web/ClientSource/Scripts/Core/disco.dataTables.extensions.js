jQuery.fn.dataTableExt.afnSortData['text'] = function (oSettings, iColumn) {
    var aData = [];
    $('td:eq(' + iColumn + ')', oSettings.oApi._fnGetTrNodes(oSettings)).each(function () {
        aData.push(jQuery.trim($(this).text()));
    });
    return aData;
}
jQuery.fn.dataTableExt.afnSortData['disco_datetime'] = function (oSettings, iColumn) {
    var aData = [];
    $('td:eq(' + iColumn + ')', oSettings.oApi._fnGetTrNodes(oSettings)).each(function () {
        var d = $(this).children('span.date');
        if (d.length > 0)
            if (d.is('[data-livestamp]')) {
                aData.push((d.attr('data-livestamp')) * 1);
            } else if (d.data('livestampdata') !== undefined) {
                aData.push(d.data('livestampdata').moment.valueOf());
            } else {
                aData.push(-1);
            }
        else
            aData.push(-1);
    });
    return aData;
}

jQuery.fn.dataTableExt.oSort['au_date-pre'] = function (a) {
    var ukDatea = a.split('/');
    return (ukDatea[2] + ukDatea[1] + ukDatea[0]) * 1;
};
jQuery.fn.dataTableExt.oSort['au_date-asc'] = function (a, b) {
    return ((a < b) ? -1 : ((a > b) ? 1 : 0));
};
jQuery.fn.dataTableExt.oSort['au_date-desc'] = function (a, b) {
    return ((a < b) ? 1 : ((a > b) ? -1 : 0));
};

jQuery.fn.dataTableExt.oSort['disco_datetime-asc'] = function (a, b) {
    return ((a < b) ? -1 : ((a > b) ? 1 : 0));
};
jQuery.fn.dataTableExt.oSort['disco_datetime-desc'] = function (a, b) {
    return ((a < b) ? 1 : ((a > b) ? -1 : 0));
};

// Change Default Menu Lengths

jQuery.fn.DataTable.defaults.iDisplayLength = 20;
if (window.localStorage) {
    var length = 20;
    var lengthString = window.localStorage.getItem('datatable_default_length');
    if (!!lengthString) {
        length = parseInt(lengthString);
    }
    jQuery.fn.DataTable.defaults.iDisplayLength = parseInt(length);
    jQuery.fn.DataTable.defaults.fnPreDrawCallback = function (oSettings) {
        var newLength = oSettings._iDisplayLength;
        if (length !== newLength) {
            window.localStorage.setItem('datatable_default_length', newLength)
        }
    }
}

jQuery.fn.DataTable.defaults.aLengthMenu = [[10, 20, 50, 100, 200, -1], [10, 20, 50, 100, 200, "All"]];
