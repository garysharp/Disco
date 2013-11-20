(function ($, window, document, Modernizr) {

    // Document Ready
    $(function () {

        // Search Functionality
        $('#term').watermark('Search').keypress(function (e) {
            if (e.keyCode == 13) {
                $(this).closest('form').submit();
                return false;
            }
        }).focus(function () {
            $(this).select();
        });

        // Menu Functionality
        var $menu = $('#menu');

        if ($menu.length > 0) {

            function subMenuShow() {
                var $this = $(this);
                var $subMenu = $this.children('ul.subMenu');
                var hideToken = $this.data('menuHideToken');

                if (hideToken)
                    window.clearTimeout(hideToken);

                if (!$subMenu.is(':visible'))
                    $subMenu.show();
            }
            function subMenuHide() {
                var $this = $(this);
                var $subMenu = $this.children('ul.subMenu');

                var hideToken = window.setTimeout(function () {
                    $subMenu.hide();
                }, 250);

                $this.data('menuHideToken', hideToken);
            }
            function subMenuTouchDown(e, preventClick) {
                var $this = $(this);
                var $link = $this.children('a');
                var $subMenu = $this.children('ul.subMenu');

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
                    .on('pointerover', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType !== 'touch') {
                            subMenuShow.call(this);
                        }
                    })
                    .on('pointerout', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType !== 'touch') {
                            subMenuHide.call(this);
                        }
                    })
                    .on('pointerdown', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType === 'touch') {
                            return subMenuTouchDown.call(this, e, true);
                        }
                    });
                $(document).on('pointerdown', function (e) {
                    if (e.originalEvent.pointerType === 'touch') {
                        if ($(e.target).closest('#menu').length == 0)
                            $menu.find('li.hasSubMenu>ul.subMenu:visible').hide();
                    }
                });
            } else if (Modernizr.hasEvent('mspointerdown')) {
                // MS Pointer Events
                $menu
                    .on('MSPointerOver', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType !== e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            subMenuShow.call(this);
                        }
                    })
                    .on('MSPointerOut', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType !== e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            subMenuHide.call(this);
                        }
                    })
                    .on('MSPointerDown', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType === e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                            return subMenuTouchDown.call(this, e, true);
                        }
                    });
                $(document).on('MSPointerDown', function (e) {
                    if (e.originalEvent.pointerType === e.originalEvent.MSPOINTER_TYPE_TOUCH) {
                        if ($(e.target).closest('#menu').length == 0)
                            $menu.find('li.hasSubMenu>ul.subMenu:visible').hide();
                    }
                });
            } else if (Modernizr.touch) {
                // Touch Events
                $menu
                    .on('mouseover', 'li.hasSubMenu', subMenuShow)
                    .on('mouseout', 'li.hasSubMenu', subMenuHide)
                    .on('touchstart', 'li.hasSubMenu', function (e) {
                        return subMenuTouchDown.call(this, e, false);
                    });
            } else {
                // Mouse Events
                $menu
                    .on('mouseover', 'li.hasSubMenu', subMenuShow)
                    .on('mouseout', 'li.hasSubMenu', subMenuHide);
            }
        }

    });
})(jQuery, window, document, Modernizr);