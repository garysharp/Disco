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
            if (Modernizr.touch) {
                // Touch Events
                $menu
                    .on('mouseover', 'li.hasSubMenu', function (e) {
                        var $this = $(this);
                        var $subMenu = $this.children('ul.subMenu');
                        var hideToken = $this.data('menuHideToken');
                        if (hideToken)
                            window.clearTimeout(hideToken);
                        if (!$subMenu.is(':visible'))
                            $subMenu.show();
                    })
                    .on('mouseout', 'li.hasSubMenu', function (e) {
                        var $this = $(this);
                        var $subMenu = $this.children('ul.subMenu');
                        var hideToken = window.setTimeout(function () {
                            $subMenu.hide();
                        }, 250);
                        $this.data('menuHideToken', hideToken);
                    })
                    .on('touchstart', 'li.hasSubMenu', function (e) {
                        var $this = $(this);
                        var $link = $this.children('a');
                        var $subMenu = $this.children('ul.subMenu');
                        if (!$subMenu.is(':visible')) {
                            $subMenu.show();
                            e.preventDefault();
                            e.stopPropagation();
                            return false;
                        }
                    });
            } else if (Modernizr.testProp('pointerEvents')) {
                // Pointer Events
                $menu
                    .on('pointerover', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType != 'touch') {
                            var $this = $(this);
                            var $subMenu = $this.children('ul.subMenu');
                            var hideToken = $this.data('menuHideToken');
                            if (hideToken)
                                window.clearTimeout(hideToken);
                            if (!$subMenu.is(':visible'))
                                $subMenu.show();
                        }
                    })
                    .on('pointerout', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType != 'touch') {
                            var $this = $(this);
                            var $subMenu = $this.children('ul.subMenu');
                            var hideToken = window.setTimeout(function () {
                                $subMenu.hide();
                            }, 250);
                            $this.data('menuHideToken', hideToken);
                        }
                    })
                    .on('pointerdown', 'li.hasSubMenu', function (e) {
                        if (e.originalEvent.pointerType == 'touch') {
                            var $this = $(this);
                            var $link = $this.children('a');
                            var $subMenu = $this.children('ul.subMenu');
                            if (!$subMenu.is(':visible')) {
                                $subMenu.show();
                                e.preventDefault();
                                e.stopPropagation();
                                // Stop Click Event
                                if ($link.length > 0) {
                                    var preventClick = function () { $link.off('click', preventClick); return false; }
                                    $link.on('click', preventClick);
                                }
                                return false;
                            }
                        }
                    });
                $(document).on('pointerdown', function (e) {
                    if (e.originalEvent.pointerType == 'touch') {
                        if ($(e.target).closest('#menu').length == 0)
                            $menu.find('li.hasSubMenu>ul.subMenu:visible').hide();
                    }
                });
            } else {
                // Mouse Events
                $menu
                    .on('mouseover', 'li.hasSubMenu', function () {
                        var $this = $(this);
                        var $subMenu = $this.children('ul.subMenu');
                        var hideToken = $this.data('menuHideToken');
                        if (hideToken)
                            window.clearTimeout(hideToken);
                        if (!$subMenu.is(':visible'))
                            $subMenu.show();
                    })
                    .on('mouseout', 'li.hasSubMenu', function () {
                        var $this = $(this);
                        var $subMenu = $this.children('ul.subMenu');
                        var hideToken = window.setTimeout(function () {
                            $subMenu.hide();
                        }, 250);
                        $this.data('menuHideToken', hideToken);
                    });
            }
        }

    });
})(jQuery, window, document, Modernizr);