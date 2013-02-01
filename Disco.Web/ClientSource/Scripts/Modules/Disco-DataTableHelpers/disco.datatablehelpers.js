(function (window, document, $) {
    var dataTables = [];

    $(function () {
        $('table.jobTable').each(function () {
            var $table = $(this);
            var tableDrawn = false;

            var dataTableOptionsPagination = ($table.find('tr').length > 20);
            var dataTableOptions = {
                "bPaginate": dataTableOptionsPagination,
                "sPaginationType": "full_numbers",
                "bLengthChange": dataTableOptionsPagination,
                "iDisplayLength": 20,
                "bFilter": true,
                "bSort": true,
                "bInfo": false,
                "bAutoWidth": false,
                "aoColumnDefs": [
                    { 'aTargets': ['dates'], 'sSortDataType': 'disco_datetime', 'sType': 'disco_datetime' }
                ],
                "aaSorting": [],
                "oLanguage": {
                    "sSearch": "Filter:"
                },
                "fnDrawCallback": function () {
                    if (tableDrawn)
                        scrollCheck.apply($table);
                    else
                        tableDrawn = true;
                }
            };

            var $dataTable = $table.dataTable(dataTableOptions);

            // hideStatusClosed Extension
            if ($table.hasClass('hideStatusClosed')) {
                var wrapper = $(this).closest('.dataTables_wrapper');

                var wrapperContext = wrapper;
                if (wrapper.parent('.jobTable').length > 0)
                    wrapperContext = wrapper.parent();
                var wrapperPrev = wrapperContext.prev();
                if (wrapperPrev.length > 0 && (wrapperPrev.is('h1') || wrapperPrev.is('h2') || wrapperPrev.is('h3'))) {
                    wrapperPrev.data('dataTable_originalContent', wrapperPrev.html()).text('Active ' + wrapperPrev.text());
                } else {
                    wrapperPrev = null;
                }

                var showClosedAnchor = $('<a class="dataTables_showStatusClosed" href="#">').text('Show Closed');
                wrapper.prepend(showClosedAnchor);
                showClosedAnchor.click(function () {

                    $table.removeClass('hideStatusClosed');
                    showClosedAnchor.remove();
                    if (wrapperPrev)
                        wrapperPrev.html(wrapperPrev.data('dataTable_originalContent'));

                    scrollCheck.apply($table[0]);
                    return false;
                });
            }

            dataTables.push(this);
        });

        $('table.deviceTable').each(function () {
            var $table = $(this);

            var dataTableOptionsPagination = ($table.find('tr').length > 20);
            var dataTableOptions = {
                "bPaginate": dataTableOptionsPagination,
                "sPaginationType": "full_numbers",
                "bLengthChange": dataTableOptionsPagination,
                "iDisplayLength": 20,
                "bFilter": true,
                "bSort": true,
                "bInfo": false,
                "bAutoWidth": false,
                "aaSorting": [],
                "oLanguage": {
                    "sSearch": "Filter:"
                }
            };

            $table.dataTable(dataTableOptions);
            dataTables.push(this);
        });

        $('table.userTable').each(function () {
            var $table = $(this);

            var dataTableOptionsPagination = ($table.find('tr').length > 20);
            var dataTableOptions = {
                "bPaginate": dataTableOptionsPagination,
                "sPaginationType": "full_numbers",
                "bLengthChange": dataTableOptionsPagination,
                "iDisplayLength": 20,
                "bFilter": true,
                "bSort": true,
                "bInfo": false,
                "bAutoWidth": false,
                "aaSorting": [],
                "oLanguage": {
                    "sSearch": "Filter:"
                }
            };

            $table.dataTable(dataTableOptions);
            dataTables.push(this);
        });

        function scrollCheck() {
            var wrapper = $(this).closest('.dataTables_wrapper');
            if (wrapper.length > 0) {
                window.setTimeout(function () {
                    var $window = $(window);
                    var wrapperHeight = wrapper.height();
                    var wrapperOffset = wrapper.offset();
                    var windowScrollTop = $window.scrollTop();
                    var windowHeight = $window.height();

                    var wrapperTopNotShown = windowScrollTop - wrapperOffset.top;
                    if (wrapperTopNotShown > 0) {
                        $('html').animate({ scrollTop: wrapperOffset.top }, 125);
                    } else {
                        var wrapperBottomNotShown = ((windowScrollTop + windowHeight) - (wrapperHeight + wrapperOffset.top)) * -1;
                        if (wrapperBottomNotShown > 0) {
                            if (wrapperHeight > windowHeight)
                                $('html').animate({ scrollTop: wrapperOffset.top }, 125);
                            else
                                $('html').animate({ scrollTop: windowScrollTop + wrapperBottomNotShown }, 125);
                        }
                    }
                }, 1);
            }
        }

        //$(dataTables).bind('page', scrollCheck);
        //$(dataTables).bind('filter', scrollCheck);
        //$(dataTables).bind('sort', scrollCheck);
    });
})(window, document, $);