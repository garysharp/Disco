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
        var d = $(this).children('span.date[data-discodatetime]');
        if (d.length > 0)
            aData.push((d.attr('data-discodatetime')) * 1);
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

jQuery.fn.DataTable.defaults.aLengthMenu = [[10, 20, 50, -1], [10, 20, 50, "All"]];