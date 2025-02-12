(function ($, window, document, Modernizr) {

    // Document Ready
    $(function () {

        // Search Functionality
        var quickSearchInited = false;
        $('#SearchQuery').watermark('Search').keypress(function (e) {
            if (e.keyCode == 13) {
                $(this).closest('form').submit();
                return false;
            }
        }).focus(function () {
            $this = $(this);
            $this.select();

            if (!quickSearchInited) {
                var quickSearchUrl = $this.attr('data-quicksearchurl');
                if (quickSearchUrl) {
                    $this.autocomplete({
                        source: quickSearchUrl,
                        minLength: 2,
                        select: function (e, ui) {
                            $this.val(ui.item.tag);
                            $this.closest('form').submit();
                        },
                        response: function (e, ui) {
                            for (var i = 0; i < ui.content.length; i++) {
                                var item = ui.content[i];
                                switch (item.Type) {
                                    case 'Device':
                                        item.tag = '!' + item.Id;
                                        break;
                                    case 'Job':
                                        item.tag = '#' + item.Id;
                                        break;
                                    case 'User':
                                        item.tag = '@' + item.Id;
                                        break;
                                }
                            }
                        }
                    }).autocomplete("widget").attr('id', 'QuickSearchMenu');

                    $this.data('ui-autocomplete')._renderItem = function (ul, item) {
                        var template;

                        //"<a><strong>" + item.DisplayName + "</strong><br>" + item.Id + " (" + item.Type + ")</a>"

                        switch (item.Type) {
                            case 'Device':
                                template = $('<a>').append('<i class="fa fa-desktop fa-fw">').append($('<strong>').text('Device ' + item.Id)).append($('<div>').text(item.ComputerName + '; ' + item.DeviceModelDescription))
                                break;
                            case 'Job':
                                if (item.DeviceSerialNumber && item.UserId) {
                                    template = $('<a>').append('<i class="fa fa-question-circle fa-fw">').append($('<strong>').text('Job ' + item.Id)).append($('<div>').text(item.UserId + '; ' + item.DeviceSerialNumber))
                                } else if (item.DeviceSerialNumber) {
                                    template = $('<a>').append('<i class="fa fa-question-circle fa-fw">').append($('<strong>').text('Job ' + item.Id)).append($('<div>').text(item.DeviceSerialNumber))
                                } else if (item.UserId) {
                                    template = $('<a>').append('<i class="fa fa-question-circle fa-fw">').append($('<strong>').text('Job ' + item.Id)).append($('<div>').text(item.UserId))
                                }
                                break;
                            case 'User':
                                template = $('<a>').append('<i class="fa fa-user fa-fw">').append($('<strong>').text(item.DisplayName)).append($('<div>').text(item.Id))
                                break;
                        }

                        return $("<li>")
                            .data("item.autocomplete", item)
                            .append(template)
                            .appendTo(ul);
                    };

                }
                quickSearchInited = true;
            }
        });

        // Menu Functionality
        var $menu = $('#menu');

        if ($menu.length > 0) {

            function subMenuShow() {
                var $this = $(this);
                var $subMenu = $this.children('ul');
                var hideToken = $this.data('menuHideToken');

                if (hideToken)
                    window.clearTimeout(hideToken);

                if (!$subMenu.is(':visible'))
                    $subMenu.show();
            }
            function subMenuHide() {
                var $this = $(this);
                var $subMenu = $this.children('ul');

                var hideToken = window.setTimeout(function () {
                    $subMenu.hide();
                }, 250);

                $this.data('menuHideToken', hideToken);
            }
            function subMenuTouchDown(e, preventClick) {
                var $this = $(this);
                var $link = $this.children('a');
                var $subMenu = $this.children('ul');

                if (!$subMenu.is(':visible')) {

                    $subMenu.show();

                    e.preventDefault();
                    e.stopPropagation();

                    if (preventClick) {
                        // Stop Click Event
                        if ($link.length > 0) {
                            var preventClick = function () { $link.off('click', preventClick); return false; }
                            $link.on('click', preventClick);
                        }
                    }

                    return false;
                }
            }

            if (Modernizr.hasEvent('pointerdown')) {
                // Pointer Events
                $menu
                    .on('pointerover', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType !== 'touch') {
                            subMenuShow.call(this);
                        }
                    })
                    .on('pointerout', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType !== 'touch') {
                            subMenuHide.call(this);
                        }
                    })
                    .on('pointerdown', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType === 'touch') {
                            return subMenuTouchDown.call(this, e, true);
                        }
                    });
                $(document).on('pointerdown', function (e) {
                    if (e.originalEvent.pointerType === 'touch') {
                        if ($(e.target).closest('#menu').length == 0)
                            $menu.find('li.d-sm>ul.subMenu:visible').hide();
                    }
                });
            } else if (Modernizr.hasEvent('mspointerdown')) {
                // MS Pointer Events
                $menu
                    .on('MSPointerOver', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType !== e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            subMenuShow.call(this);
                        }
                    })
                    .on('MSPointerOut', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType !== e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            subMenuHide.call(this);
                        }
                    })
                    .on('MSPointerDown', 'li.d-sm', function (e) {
                        if (e.originalEvent.pointerType === e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            return subMenuTouchDown.call(this, e, true);
                        }
                    });
                $(document).on('MSPointerDown', function (e) {
                    if (e.originalEvent.pointerType === e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                        if ($(e.target).closest('#menu').length == 0)
                            $menu.find('li.d-sm>ul.subMenu:visible').hide();
                    }
                });
            } else if (Modernizr.touch) {
                // Touch Events
                $menu
                    .on('mouseover', 'li.d-sm', subMenuShow)
                    .on('mouseout', 'li.d-sm', subMenuHide)
                    .on('touchstart', 'li.d-sm', function (e) {
                        return subMenuTouchDown.call(this, e, false);
                    });
            } else {
                // Mouse Events
                $menu
                    .on('mouseover', 'li.d-sm', subMenuShow)
                    .on('mouseout', 'li.d-sm', subMenuHide);
            }
        }


        // Dialog Repositioning
        $(window).resize(function () {
            $('.ui-dialog-content').filter(':visible').dialog('option', 'position', 'center');
        });

        if (navigator.clipboard) {
            window.setTimeout(() => {
                $('[data-clipboard]:not(input)')
                    .on('mouseenter', e => {
                        const $this = $(e.currentTarget);
                        const previousPosition = $this.css('position');
                        $this.css('position', 'relative');
                        const link = $('<i class="clipboard-link fa fa-clipboard fa-fw">');
                        link.appendTo($this)
                        link.on('click', e => {
                            e.preventDefault();
                            let value = $this.attr('data-clipboard');
                            if (!value) {
                                value = $this.text().trim();
                            }
                            navigator.clipboard.writeText(value).then(() => {
                                link.removeClass('fa-clipboard').addClass('fa-check');
                            })
                            return false;
                        });
                        $this.data('clipboard', {
                            previousPosition: previousPosition,
                            link: link
                        })
                    }).on('mouseleave', e => {
                        const $this = $(e.currentTarget);
                        const data = $this.data('clipboard');
                        if (data) {
                            data.link.remove();
                            $this.css('position', data.previousPosition);
                            $this.removeData('clipboard');
                        }
                    });
                $('input[data-clipboard]')
                    .each((i, el) => {
                        const $this = $(el);
                        const link = $('<i class="clipboard-button fa fa-clipboard fa-fw">');
                        link.insertAfter($this);
                        link.on('click', e => {
                            e.preventDefault();
                            const value = $this.val();
                            navigator.clipboard.writeText(value).then(() => {
                                link.removeClass('fa-clipboard').addClass('fa-check');
                                window.setTimeout(() => {
                                    link.removeClass('fa-check').addClass('fa-clipboard');
                                }, 1000);
                            });
                        });
                    })
            }, 100);
        }
    });
})(jQuery, window, document, Modernizr);