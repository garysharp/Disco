///#source 1 1 /ClientSource/Scripts/Modules/Disco-jQueryExtensions/disco.jQueryExtensions.js
/// <reference path="../../Core/jquery-1.7.1.js" />
(function ($) {

    var checkboxBulkSelectMethods = {
        init: function (options) {

            options = $.extend({ parentSelector: 'tr' }, options);

            return this.each(function () {
                var $this = $(this);
                $this.data('checkboxBulkSelect_parentSelector', options.parentSelector);
                var $checkboxes = $this.closest(options.parentSelector).find('input[type="checkbox"]');

                if ($checkboxes.length > 0) {
                    var $selectAll, $selectNone;

                    $selectAll = $('<a>').addClass('selectAll').attr('href', '#').text('ALL').click(selectAll);
                    $selectNone = $('<a>').addClass('selectNone').attr('href', '#').text('NONE').click(selectNone);

                    $this.append($('<span>').text('Select: '), $selectAll, $('<span>').text(' | '), $selectNone);
                    $checkboxes.click(update);

                    update();

                    function selectAll() {
                        $checkboxes.attr('checked', 'checked');
                        update();
                        return false;
                    }
                    function selectNone() {
                        $checkboxes.removeAttr('checked');
                        update();
                        return false;
                    }
                    function update() {
                        checkboxBulkSelectMethods.update.apply($this, [$checkboxes, options.parentSelector, $selectAll, $selectNone]);
                    }
                }
            });
        },
        update: function ($checkboxes, parentSelector, $selectAll, $selectNone) {
            return this.each(function () {
                $this = $(this);
                if (!parentSelector)
                    parentSelector = $this.data('checkboxBulkSelect_parentSelector');
                if (!$checkboxes)
                    $checkboxes = $this.closest(parentSelector).find('input[type="checkbox"]');
                if (!$selectAll)
                    $selectAll = $this.find('a.selectAll').first();
                if (!$selectNone)
                    $selectNone = $this.find('a.selectNone').first();
                var $selectedCheckboxes = $checkboxes.filter(':checked');

                if ($checkboxes.length == $selectedCheckboxes.length) {
                    // All Selected
                    $selectAll.attr('disabled', 'disabled');
                    $selectNone.removeAttr('disabled');
                } else {
                    if ($selectedCheckboxes.length == 0) {
                        // None Selected
                        $selectNone.attr('disabled', 'disabled');
                        $selectAll.removeAttr('disabled');
                    } else {
                        // Some Selected
                        $selectAll.removeAttr('disabled');
                        $selectNone.removeAttr('disabled');
                    }
                }
            });
        }
    }

    $.fn.checkboxBulkSelect = function (method) {
        if (checkboxBulkSelectMethods[method]) {
            return checkboxBulkSelectMethods[method].apply(this, Array.prototype.slice(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return checkboxBulkSelectMethods.init.apply(this, arguments);
        } else {
            $.error('Method ' + method + ' does not exist on jQuery.checkboxBulkSelect');
        }
    }
})(jQuery);
