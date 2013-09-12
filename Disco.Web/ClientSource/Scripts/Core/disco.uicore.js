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
        var $menuItems = $menu.find('li');
        var $menuItemParents = $menuItems.filter('.hasSubMenu');
        var $menuSubMenus = $menuItems.filter('.subMenu');
        var menuAllowTouchNavigation = null;

        $menuItemParents.each(function () {
            var $parent = $(this);
            var $subMenu = $parent.children('ul.subMenu');
            $parent.data('menuSubMenu', $subMenu);
        }).mouseover(function () {
            var $parent = $(this);
            var $subMenu = $parent.data('menuSubMenu');
            var hideToken = $parent.data('menuHideToken');
            if (hideToken)
                window.clearTimeout(hideToken);
            if (!$subMenu.is(':visible')) {
                $subMenu.show();
                if (menuAllowTouchNavigation !== null)
                    menuTouchPreventNavigation();
            }
        }).mouseout(function () {
            var $parent = $(this);
            var $subMenu = $parent.data('menuSubMenu');
            var hideToken = window.setTimeout(function () {
                $subMenu.hide();
            }, 250);
            $parent.data('menuHideToken', hideToken);
        });

        if (Modernizr.touch) {
            menuAllowTouchNavigation = true;
            $menuItemParents.children('a').on('touchstart', menuTouchStarted);
        } else if (window.navigator.msPointerEnabled) {
            menuAllowTouchNavigation = true;
            $menuItemParents.children('a').on('MSPointerUp', menuTouchMSPointerUp);
        }
        function menuTouchPreventNavigation() {
            // Block Touch Navigation for 350ms
            allowTouchNavigation = false;
            window.setTimeout(function () {
                allowTouchNavigation = true;
            }, 350);
        }
        function menuTouchNavigationBlockClick(e) {
            $(this).off('click', menuTouchNavigationBlockClick);
            e.preventDefault();
        }
        //#region TouchEvents Implementation
        function menuSubMenuVisible($element) {
            return $element.closest('li').data('menuSubMenu').is(':visible');
        }
        function menuTouchStarted(e) {
            var $this = $(this);
            if (!menuSubMenuVisible($this))
                $this.click(menuTouchNavigationBlockClick);
        }
        //#endregion

        //#region MS Pointer Implementation
        function menuTouchMSPointerUp(e) {
            if (!allowTouchNavigation && e.originalEvent.pointerType == e.originalEvent.MSPOINTER_TYPE_TOUCH)
                $(this).click(menuTouchNavigationBlockClick);
        }
        //#endregion

    });
})(jQuery, window, document, Modernizr);