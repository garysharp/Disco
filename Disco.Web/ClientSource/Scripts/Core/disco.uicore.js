(function ($, window, document, Modernizr) {

    // Document Ready
    $(function () {

        // Search Functionality
        $('#term').watermark('Search').keypress(function (e) {
            if (e.keyCode == 13) {
                $(this).closest('form').submit();
            }
        }).focus(function () {
            $(this).select();
        });

        // Menu Functionality
        var $menu = $('#menu');
        var $menuSubVisible = [];
        $menu.find('li').each(function () {
            var $menuItem = $(this);
            var $subMenu = $menuItem.children('ul').first();
            if ($subMenu.length > 0) {
                $menuItem.mouseover(function () {
                    menuShow($menuItem, $subMenu);
                }).mouseout(function () {
                    menuHide($menuItem, $subMenu);
                }).addClass('hasSubmenu');

                // Touch-enabled browser
                if (Modernizr.touch) {
                    $menuItem.on('touchstart', function (e) {
                        // Already Open - allow 'click'
                        for (var i = 0; i < $menuSubVisible.length; i++)
                            if ($menuSubVisible[0] === $subMenu)
                                return;

                        // Show
                        menuShow($menuItem, $subMenu);
                        $menuSubVisible.push($subMenu);
                        $(document).on('click', menuTouchHide)
                        e.preventDefault();
                    });
                }
            };
        });
        function menuTouchHide() {
            while ($menuSubVisible.length > 0) {
                var $subMenu = $menuSubVisible.pop();
                $subMenu.hide();
            }
        }
        function menuShow($menuItem, $subMenu) {
            var timeoutToken = $menuItem.data('menuTimeoutToken');
            if (timeoutToken)
                window.clearTimeout(timeoutToken);
            if (!$subMenu.is(':visible'))
                $subMenu.show();
        }
        function menuHide($menuItem, $subMenu) {
            var timeoutToken = subMenuHideToken = window.setTimeout(function () {
                $subMenu.hide();
            }, 250);
            $menuItem.data('menuTimeoutToken', timeoutToken);
        }


    });
})(jQuery, window, document, Modernizr);