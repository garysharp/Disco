using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Disco.Services.Extensions
{
    public static class UIHelpers
    {
        /// <summary>
        /// FontAwesome Category Icons
        /// </summary>
        public static ReadOnlyCollection<KeyValuePair<string, string>> Icons { get; private set; }
        /// <summary>
        /// User-selectable Colour Themes
        /// </summary>
        public static ReadOnlyCollection<KeyValuePair<string, string>> ThemeColours { get; private set; }
        /// <summary>
        /// Noticeboard Colour Themes
        /// </summary>
        public static ReadOnlyCollection<KeyValuePair<string, string>> NoticeboardThemes { get; private set; }

        /// <summary>
        /// Returns a randomly selected Icon using <see cref="Random"/>.
        /// </summary>
        public static string RandomIcon()
        {
            return RandomIcon(null);
        }
        /// <summary>
        /// Returns a randomly selected Icon using <see cref="Random"/>.
        /// </summary>
        /// <param name="Except">A list of Icons which will be ignored (if all are excluded, a random one will be returned)</param>
        public static string RandomIcon(IEnumerable<string> Except)
        {
            var rnd = new Random();
            if (Except != null)
            {
                var availableIcons = Icons.Select(i => i.Key).Except(Except).ToList();
                if (availableIcons.Count > 0)
                    return availableIcons[rnd.Next(availableIcons.Count - 1)];
            }
            return Icons[rnd.Next(Icons.Count - 1)].Key;
        }

        /// <summary>
        /// Returns a randomly selected Theme Colour using <see cref="Random"/>.
        /// </summary>
        public static string RandomThemeColour()
        {
            return RandomThemeColour(null);
        }
        /// <summary>
        /// Returns a randomly selected Theme Colour using <see cref="Random"/>.
        /// </summary>
        /// <param name="Except">A list of Theme Colours which will be ignored (if all are excluded, a random one will be returned)</param>
        public static string RandomThemeColour(IEnumerable<string> Except)
        {
            var rnd = new Random();
            if (Except != null)
            {
                var availableColours = ThemeColours.Select(i => i.Key).Except(Except).ToList();
                if (availableColours.Count > 0)
                    return availableColours[rnd.Next(availableColours.Count - 1)];
            }
            return ThemeColours[rnd.Next(ThemeColours.Count - 1)].Key;
        }

        static UIHelpers()
        {
            // Icons
            Icons = new List<KeyValuePair<string, string>>(){
                new KeyValuePair<string, string>("address-book" , "Address Book"),
                new KeyValuePair<string, string>("address-card" , "Address Card"),
                new KeyValuePair<string, string>("adjust" , "Adjust"),
                new KeyValuePair<string, string>("amazon" , "Amazon"),
                new KeyValuePair<string, string>("ambulance" , "Ambulance"),
                new KeyValuePair<string, string>("anchor" , "Anchor"),
                new KeyValuePair<string, string>("android" , "Android"),
                new KeyValuePair<string, string>("apple" , "Apple"),
                new KeyValuePair<string, string>("archive" , "Archive"),
                new KeyValuePair<string, string>("area-chart" , "Area Chart"),
                new KeyValuePair<string, string>("arrow-circle-down" , "Arrow Circle Down"),
                new KeyValuePair<string, string>("arrow-circle-left" , "Arrow Circle Left"),
                new KeyValuePair<string, string>("arrow-circle-right" , "Arrow Circle Right"),
                new KeyValuePair<string, string>("arrow-circle-up" , "Arrow Circle Up"),
                new KeyValuePair<string, string>("arrow-down" , "Arrow Down"),
                new KeyValuePair<string, string>("arrow-left" , "Arrow Left"),
                new KeyValuePair<string, string>("arrow-right" , "Arrow Right"),
                new KeyValuePair<string, string>("arrow-up" , "Arrow Up"),
                new KeyValuePair<string, string>("arrows" , "Arrows"),
                new KeyValuePair<string, string>("asterisk" , "Asterisk"),
                new KeyValuePair<string, string>("at" , "At"),
                new KeyValuePair<string, string>("balance-scale" , "Balance Scale"),
                new KeyValuePair<string, string>("ban" , "Ban"),
                new KeyValuePair<string, string>("bank" , "Bank"),
                new KeyValuePair<string, string>("bar-chart" , "Bar Chart"),
                new KeyValuePair<string, string>("barcode" , "Barcode"),
                new KeyValuePair<string, string>("bars" , "Bars"),
                new KeyValuePair<string, string>("bath" , "Bath"),
                new KeyValuePair<string, string>("battery-0" , "Battery 0"),
                new KeyValuePair<string, string>("battery-1" , "Battery 1"),
                new KeyValuePair<string, string>("battery-2" , "Battery 2"),
                new KeyValuePair<string, string>("battery-3" , "Battery 3"),
                new KeyValuePair<string, string>("battery-4" , "Battery 4"),
                new KeyValuePair<string, string>("bed" , "Bed"),
                new KeyValuePair<string, string>("beer" , "Beer"),
                new KeyValuePair<string, string>("bell" , "Bell"),
                new KeyValuePair<string, string>("bell-slash" , "Bell Slash"),
                new KeyValuePair<string, string>("binoculars" , "Binoculars"),
                new KeyValuePair<string, string>("birthday-cake" , "Cake"),
                new KeyValuePair<string, string>("bitcoin" , "Bitcoin"),
                new KeyValuePair<string, string>("black-tie" , "Black Tie"),
                new KeyValuePair<string, string>("blind" , "Blind"),
                new KeyValuePair<string, string>("bluetooth" , "Bluetooth"),
                new KeyValuePair<string, string>("bolt" , "Bolt"),
                new KeyValuePair<string, string>("bomb" , "Bomb"),
                new KeyValuePair<string, string>("book" , "Book"),
                new KeyValuePair<string, string>("bookmark" , "Bookmark"),
                new KeyValuePair<string, string>("briefcase" , "Briefcase"),
                new KeyValuePair<string, string>("bug" , "Bug"),
                new KeyValuePair<string, string>("bicycle" , "Bicycle"),
                new KeyValuePair<string, string>("building-o" , "Building"),
                new KeyValuePair<string, string>("bullhorn" , "Bullhorn"),
                new KeyValuePair<string, string>("bullseye" , "Bullseye"),
                new KeyValuePair<string, string>("bus" , "Bus"),
                new KeyValuePair<string, string>("cab" , "Cab"),
                new KeyValuePair<string, string>("calculator" , "Calculator"),
                new KeyValuePair<string, string>("calendar" , "Calendar"),
                new KeyValuePair<string, string>("calendar-check-o" , "Calendar Check"),
                new KeyValuePair<string, string>("calendar-minus-o" , "Calendar Minus"),
                new KeyValuePair<string, string>("calendar-o" , "Calendar"),
                new KeyValuePair<string, string>("calendar-plus-o" , "Calendar Plus"),
                new KeyValuePair<string, string>("calendar-times-o" , "Calendar Times"),
                new KeyValuePair<string, string>("car" , "Car"),
                new KeyValuePair<string, string>("caret-square-o-down" , "Caret Square Down"),
                new KeyValuePair<string, string>("caret-square-o-left" , "Caret Square Left"),
                new KeyValuePair<string, string>("caret-square-o-right" , "Caret Square Right"),
                new KeyValuePair<string, string>("caret-square-o-up" , "Caret Square Up"),
                new KeyValuePair<string, string>("cart-arrow-down" , "Cart Arrow Down"),
                new KeyValuePair<string, string>("cart-plus" , "Cart Plus"),
                new KeyValuePair<string, string>("cc" , "CC"),
                new KeyValuePair<string, string>("cc-amex" , "CC American Express"),
                new KeyValuePair<string, string>("cc-mastercard" , "CC MasterCard"),
                new KeyValuePair<string, string>("cc-paypal" , "CC PayPal"),
                new KeyValuePair<string, string>("cc-stripe" , "CC Stripe"),
                new KeyValuePair<string, string>("cc-visa" , "CC VISA"),
                new KeyValuePair<string, string>("certificate" , "Certificate"),
                new KeyValuePair<string, string>("chain" , "Chain"),
                new KeyValuePair<string, string>("chain-broken" , "Chain Broken"),
                new KeyValuePair<string, string>("check" , "Check"),
                new KeyValuePair<string, string>("check-circle" , "Check Circle"),
                new KeyValuePair<string, string>("check-circle-o" , "Check Circle Outline"),
                new KeyValuePair<string, string>("check-square" , "Check Square"),
                new KeyValuePair<string, string>("check-square-o" , "Check Square Outline"),
                new KeyValuePair<string, string>("child" , "Child"),
                new KeyValuePair<string, string>("chrome" , "Chrome"),
                new KeyValuePair<string, string>("clipboard" , "Clipboard"),
                new KeyValuePair<string, string>("clock-o" , "Clock"),
                new KeyValuePair<string, string>("close" , "Close"),
                new KeyValuePair<string, string>("cloud" , "Cloud"),
                new KeyValuePair<string, string>("cloud-download" , "Cloud Download"),
                new KeyValuePair<string, string>("cloud-upload" , "Cloud Upload"),
                new KeyValuePair<string, string>("code" , "Code"),
                new KeyValuePair<string, string>("code-fork" , "Code Fork"),
                new KeyValuePair<string, string>("cog" , "Cog"),
                new KeyValuePair<string, string>("cogs" , "Cogs"),
                new KeyValuePair<string, string>("coffee" , "Coffee"),
                new KeyValuePair<string, string>("comment" , "Comment"),
                new KeyValuePair<string, string>("comment-o" , "Comment Outline"),
                new KeyValuePair<string, string>("commenting" , "Commenting"),
                new KeyValuePair<string, string>("commenting-o" , "Commenting Outline"),
                new KeyValuePair<string, string>("comments" , "Comments"),
                new KeyValuePair<string, string>("comments-o" , "Comments Outline"),
                new KeyValuePair<string, string>("compass" , "Compass"),
                new KeyValuePair<string, string>("compress" , "Compress"),
                new KeyValuePair<string, string>("copy" , "Copy"),
                new KeyValuePair<string, string>("copyright" , "Copyright"),
                new KeyValuePair<string, string>("creative-commons" , "Creative-Commons"),
                new KeyValuePair<string, string>("credit-card" , "Credit Card"),
                new KeyValuePair<string, string>("credit-card-alt" , "Credit Card ALternative"),
                new KeyValuePair<string, string>("crop" , "Crop"),
                new KeyValuePair<string, string>("crosshairs" , "Crosshairs"),
                new KeyValuePair<string, string>("cube" , "Cube"),
                new KeyValuePair<string, string>("cubes" , "Cubes"),
                new KeyValuePair<string, string>("cut" , "Cut"),
                new KeyValuePair<string, string>("cutlery" , "Cutlery"),
                new KeyValuePair<string, string>("database" , "Database"),
                new KeyValuePair<string, string>("deaf" , "Deaf"),
                new KeyValuePair<string, string>("desktop" , "Desktop"),
                new KeyValuePair<string, string>("diamond" , "Diamond"),
                new KeyValuePair<string, string>("dollar" , "Dollar"),
                new KeyValuePair<string, string>("dot-circle-o" , "Dot Circle"),
                new KeyValuePair<string, string>("drivers-license" , "Drivers License"),
                new KeyValuePair<string, string>("drivers-license-o" , "Drivers License Outline"),
                new KeyValuePair<string, string>("edge" , "Edge"),
                new KeyValuePair<string, string>("edit" , "Edit"),
                new KeyValuePair<string, string>("eject" , "Eject"),
                new KeyValuePair<string, string>("envelope" , "Envelope"),
                new KeyValuePair<string, string>("envelope-o" , "Envelope Outline"),
                new KeyValuePair<string, string>("envelope-open" , "Envelope Open"),
                new KeyValuePair<string, string>("envelope-open-o" , "Envelope Open Outline"),
                new KeyValuePair<string, string>("envelope-square" , "Envelope Square"),
                new KeyValuePair<string, string>("eraser" , "Eraser"),
                new KeyValuePair<string, string>("exchange" , "Exchange"),
                new KeyValuePair<string, string>("exclamation" , "Exclamation"),
                new KeyValuePair<string, string>("exclamation-circle" , "Exclamation Circle"),
                new KeyValuePair<string, string>("exclamation-triangle" , "Exclamation Triangle"),
                new KeyValuePair<string, string>("expand" , "Expand"),
                new KeyValuePair<string, string>("eye" , "Eye"),
                new KeyValuePair<string, string>("eye-slash" , "Eye Slash"),
                new KeyValuePair<string, string>("eyedropper" , "Eyedropper"),
                new KeyValuePair<string, string>("fax" , "Fax"),
                new KeyValuePair<string, string>("feed" , "Feed"),
                new KeyValuePair<string, string>("female" , "Female"),
                new KeyValuePair<string, string>("fighter-jet" , "Fighter Jet"),
                new KeyValuePair<string, string>("file" , "File"),
                new KeyValuePair<string, string>("film" , "Film"),
                new KeyValuePair<string, string>("filter" , "Filter"),
                new KeyValuePair<string, string>("fire" , "Fire"),
                new KeyValuePair<string, string>("fire-extinguisher" , "Fire Extinguisher"),
                new KeyValuePair<string, string>("firefox" , "FireFox"),
                new KeyValuePair<string, string>("flag" , "Flag"),
                new KeyValuePair<string, string>("flag-checkered" , "Flag Checkered"),
                new KeyValuePair<string, string>("flag-o" , "Flag Outline"),
                new KeyValuePair<string, string>("floppy-o" , "Floppy Outline"),
                new KeyValuePair<string, string>("folder" , "Folder"),
                new KeyValuePair<string, string>("folder-o" , "Folder Outline"),
                new KeyValuePair<string, string>("folder-open" , "Folder Open"),
                new KeyValuePair<string, string>("folder-open-o" , "Folder Open Outline"),
                new KeyValuePair<string, string>("flask" , "Flask"),
                new KeyValuePair<string, string>("frown-o" , "Frown"),
                new KeyValuePair<string, string>("gamepad" , "Gamepad"),
                new KeyValuePair<string, string>("gift" , "Gift"),
                new KeyValuePair<string, string>("glass" , "Glass"),
                new KeyValuePair<string, string>("globe" , "Globe"),
                new KeyValuePair<string, string>("graduation-cap" , "Graduation Cap"),
                new KeyValuePair<string, string>("group" , "Group"),
                new KeyValuePair<string, string>("hand-grab-o" , "Hand Grab"),
                new KeyValuePair<string, string>("hand-o-down" , "Hand Down"),
                new KeyValuePair<string, string>("hand-o-left" , "Hand Left"),
                new KeyValuePair<string, string>("hand-o-right" , "Hand Right"),
                new KeyValuePair<string, string>("hand-o-up" , "Hand Up"),
                new KeyValuePair<string, string>("hand-paper-o" , "Hand Paper"),
                new KeyValuePair<string, string>("hand-pointer-o" , "Hand Pointer"),
                new KeyValuePair<string, string>("hand-scissors-o" , "Hand Scissors"),
                new KeyValuePair<string, string>("hand-spock-o" , "Hand Spock"),
                new KeyValuePair<string, string>("handshake-o" , "Handshake"),
                new KeyValuePair<string, string>("hashtag" , "Hashtag"),
                new KeyValuePair<string, string>("hdd-o" , "Hdd"),
                new KeyValuePair<string, string>("heart" , "Heart"),
                new KeyValuePair<string, string>("heart-o" , "Heart Outline"),
                new KeyValuePair<string, string>("heartbeat" , "Heartbeat"),
                new KeyValuePair<string, string>("history" , "History"),
                new KeyValuePair<string, string>("home" , "Home"),
                new KeyValuePair<string, string>("hospital-o" , "Hospital"),
                new KeyValuePair<string, string>("hourglass" , "Hourglass"),
                new KeyValuePair<string, string>("hourglass-1" , "Hourglass 1"),
                new KeyValuePair<string, string>("hourglass-2" , "Hourglass 2"),
                new KeyValuePair<string, string>("hourglass-3" , "Hourglass 3"),
                new KeyValuePair<string, string>("hourglass-o" , "Hourglass Outline"),
                new KeyValuePair<string, string>("id-badge" , "Id Badge"),
                new KeyValuePair<string, string>("image" , "Image"),
                new KeyValuePair<string, string>("inbox" , "Inbox"),
                new KeyValuePair<string, string>("info" , "Info"),
                new KeyValuePair<string, string>("info-circle" , "Info Circle"),
                new KeyValuePair<string, string>("key" , "Key"),
                new KeyValuePair<string, string>("keyboard-o" , "Keyboard"),
                new KeyValuePair<string, string>("language" , "Language"),
                new KeyValuePair<string, string>("laptop" , "Laptop"),
                new KeyValuePair<string, string>("leaf" , "Leaf"),
                new KeyValuePair<string, string>("legal" , "Legal"),
                new KeyValuePair<string, string>("lemon-o" , "Lemon"),
                new KeyValuePair<string, string>("level-down" , "Level Down"),
                new KeyValuePair<string, string>("level-up" , "Level Up"),
                new KeyValuePair<string, string>("life-ring" , "Life Ring"),
                new KeyValuePair<string, string>("lightbulb-o" , "Lightbulb"),
                new KeyValuePair<string, string>("line-chart" , "Line Chart"),
                new KeyValuePair<string, string>("linux" , "Linux"),
                new KeyValuePair<string, string>("location-arrow" , "Location Arrow"),
                new KeyValuePair<string, string>("lock" , "Lock"),
                new KeyValuePair<string, string>("magnet" , "Magnet"),
                new KeyValuePair<string, string>("male" , "Male"),
                new KeyValuePair<string, string>("map-marker" , "Map Marker"),
                new KeyValuePair<string, string>("medkit" , "Medkit"),
                new KeyValuePair<string, string>("meh-o" , "Meh"),
                new KeyValuePair<string, string>("microchip" , "Microchip"),
                new KeyValuePair<string, string>("microphone" , "Microphone"),
                new KeyValuePair<string, string>("microphone-slash" , "Microphone Slash"),
                new KeyValuePair<string, string>("minus" , "Minus"),
                new KeyValuePair<string, string>("minus-circle" , "Minus Circle"),
                new KeyValuePair<string, string>("minus-square" , "Minus Square"),
                new KeyValuePair<string, string>("minus-square-o" , "Minus Square Outline"),
                new KeyValuePair<string, string>("mobile" , "Mobile"),
                new KeyValuePair<string, string>("money" , "Money"),
                new KeyValuePair<string, string>("moon-o" , "Moon"),
                new KeyValuePair<string, string>("motorcycle" , "Motorcycle"),
                new KeyValuePair<string, string>("music" , "Music"),
                new KeyValuePair<string, string>("newspaper-o" , "Newspaper"),
                new KeyValuePair<string, string>("paint-brush" , "Paint Brush"),
                new KeyValuePair<string, string>("paper-plane" , "Paper Plane"),
                new KeyValuePair<string, string>("paper-plane-o" , "Paper Plane Outline"),
                new KeyValuePair<string, string>("paperclip" , "Paperclip"),
                new KeyValuePair<string, string>("paw" , "Paw"),
                new KeyValuePair<string, string>("pencil" , "Pencil"),
                new KeyValuePair<string, string>("phone" , "Phone"),
                new KeyValuePair<string, string>("pie-chart" , "Pie Chart"),
                new KeyValuePair<string, string>("picture-o" , "Picture"),
                new KeyValuePair<string, string>("plane" , "Plane"),
                new KeyValuePair<string, string>("plug" , "Plug"),
                new KeyValuePair<string, string>("plus" , "Plus"),
                new KeyValuePair<string, string>("plus-circle" , "Plus Circle"),
                new KeyValuePair<string, string>("plus-square" , "Plus Square"),
                new KeyValuePair<string, string>("plus-square-o" , "Plus Square Outline"),
                new KeyValuePair<string, string>("power-off" , "Power Off"),
                new KeyValuePair<string, string>("print" , "Print"),
                new KeyValuePair<string, string>("puzzle-piece" , "Puzzle Piece"),
                new KeyValuePair<string, string>("qrcode" , "QR Code"),
                new KeyValuePair<string, string>("question" , "Question"),
                new KeyValuePair<string, string>("question-circle" , "Question Circle"),
                new KeyValuePair<string, string>("question-circle-o" , "Question Circle Outline"),
                new KeyValuePair<string, string>("random" , "Random"),
                new KeyValuePair<string, string>("recycle" , "Recycle"),
                new KeyValuePair<string, string>("refresh" , "Refresh"),
                new KeyValuePair<string, string>("registered" , "Registered"),
                new KeyValuePair<string, string>("repeat" , "Repeat"),
                new KeyValuePair<string, string>("retweet" , "Retweet"),
                new KeyValuePair<string, string>("road" , "Road"),
                new KeyValuePair<string, string>("rocket" , "Rocket"),
                new KeyValuePair<string, string>("safari" , "Safari"),
                new KeyValuePair<string, string>("search" , "Search"),
                new KeyValuePair<string, string>("search-minus" , "Search Minus"),
                new KeyValuePair<string, string>("search-plus" , "Search Plus"),
                new KeyValuePair<string, string>("server" , "Server"),
                new KeyValuePair<string, string>("shield" , "Shield"),
                new KeyValuePair<string, string>("ship" , "Ship"),
                new KeyValuePair<string, string>("shopping-bag" , "Shopping Bag"),
                new KeyValuePair<string, string>("shopping-basket" , "Shopping Basket"),
                new KeyValuePair<string, string>("shopping-cart" , "Shopping Cart"),
                new KeyValuePair<string, string>("signal" , "Signal"),
                new KeyValuePair<string, string>("smile-o" , "Smile"),
                new KeyValuePair<string, string>("space-shuttle" , "Space Shuttle"),
                new KeyValuePair<string, string>("star" , "Star"),
                new KeyValuePair<string, string>("star-half" , "Star Half"),
                new KeyValuePair<string, string>("star-half-empty" , "Star Half Empty"),
                new KeyValuePair<string, string>("star-o" , "Star Outline"),
                new KeyValuePair<string, string>("stethoscope" , "Stethoscope"),
                new KeyValuePair<string, string>("sticky-note" , "Sticky Note"),
                new KeyValuePair<string, string>("sticky-note-o" , "Sticky Note Outline"),
                new KeyValuePair<string, string>("street-view" , "Street View"),
                new KeyValuePair<string, string>("subway" , "Subway"),
                new KeyValuePair<string, string>("suitcase" , "Suitcase"),
                new KeyValuePair<string, string>("sun-o" , "Sun"),
                new KeyValuePair<string, string>("tablet" , "Tablet"),
                new KeyValuePair<string, string>("tachometer" , "Tachometer"),
                new KeyValuePair<string, string>("tag" , "Tag"),
                new KeyValuePair<string, string>("tags" , "Tags"),
                new KeyValuePair<string, string>("tasks" , "Tasks"),
                new KeyValuePair<string, string>("television" , "Television"),
                new KeyValuePair<string, string>("thermometer-0" , "Thermometer 0"),
                new KeyValuePair<string, string>("thermometer-1" , "Thermometer 1"),
                new KeyValuePair<string, string>("thermometer-2" , "Thermometer 2"),
                new KeyValuePair<string, string>("thermometer-3" , "Thermometer 3"),
                new KeyValuePair<string, string>("thermometer-4" , "Thermometer 4"),
                new KeyValuePair<string, string>("thumb-tack" , "Thumb Tack"),
                new KeyValuePair<string, string>("thumbs-down" , "Thumbs Down"),
                new KeyValuePair<string, string>("thumbs-o-down" , "Thumbs Down"),
                new KeyValuePair<string, string>("thumbs-o-up" , "Thumbs Up"),
                new KeyValuePair<string, string>("thumbs-up" , "Thumbs Up"),
                new KeyValuePair<string, string>("ticket" , "Ticket"),
                new KeyValuePair<string, string>("trash" , "Trash"),
                new KeyValuePair<string, string>("tree" , "Tree"),
                new KeyValuePair<string, string>("trophy" , "Trophy"),
                new KeyValuePair<string, string>("truck" , "Truck"),
                new KeyValuePair<string, string>("umbrella" , "Umbrella"),
                new KeyValuePair<string, string>("university" , "University"),
                new KeyValuePair<string, string>("unlock" , "Unlock"),
                new KeyValuePair<string, string>("unlock-alt" , "Unlock Alternative"),
                new KeyValuePair<string, string>("usb" , "USB"),
                new KeyValuePair<string, string>("user" , "User"),
                new KeyValuePair<string, string>("user-circle" , "User Circle"),
                new KeyValuePair<string, string>("user-circle-o" , "User Circle Outline"),
                new KeyValuePair<string, string>("user-md" , "User MD"),
                new KeyValuePair<string, string>("user-o" , "User Outline"),
                new KeyValuePair<string, string>("user-secret" , "User Secret"),
                new KeyValuePair<string, string>("wheelchair" , "Wheelchair"),
                new KeyValuePair<string, string>("windows" , "Windows"),
                new KeyValuePair<string, string>("wrench" , "Wrench"),
                new KeyValuePair<string, string>("wifi" , "WiFi")
            }.AsReadOnly();

            // Icon Colours
            ThemeColours = new List<KeyValuePair<string, string>>(){
                new KeyValuePair<string, string>("lime" , "Lime"),
                new KeyValuePair<string, string>("green" , "Green"),
                new KeyValuePair<string, string>("emerald" , "Emerald"),
                new KeyValuePair<string, string>("teal" , "Teal"),
                new KeyValuePair<string, string>("cyan" , "Cyan"),
                new KeyValuePair<string, string>("cobalt" , "Cobalt"),
                new KeyValuePair<string, string>("indigo" , "Indigo"),
                new KeyValuePair<string, string>("violet" , "Violet"),
                new KeyValuePair<string, string>("pink" , "Pink"),
                new KeyValuePair<string, string>("magenta" , "Magenta"),
                new KeyValuePair<string, string>("crimson" , "Crimson"),
                new KeyValuePair<string, string>("red" , "Red"),
                new KeyValuePair<string, string>("orange" , "Orange"),
                new KeyValuePair<string, string>("amber" , "Amber"),
                new KeyValuePair<string, string>("yellow" , "Yellow"),
                new KeyValuePair<string, string>("brown" , "Brown"),
                new KeyValuePair<string, string>("olive" , "Olive"),
                new KeyValuePair<string, string>("steel" , "Steel"),
                new KeyValuePair<string, string>("mauve" , "Mauve"),
                new KeyValuePair<string, string>("sienna" , "Sienna")
            }.AsReadOnly();

            // Noticeboard Themes
            NoticeboardThemes = new List<KeyValuePair<string, string>>(){
                 new KeyValuePair<string, string>("default" , "Default Blue"),
                new KeyValuePair<string, string>("default-soft" , "Default Blue Soft"),
                new KeyValuePair<string, string>("green" , "Green"),
                new KeyValuePair<string, string>("green-soft" , "Green Soft"),
                new KeyValuePair<string, string>("violet" , "Violet"),
                new KeyValuePair<string, string>("violet-soft" , "Violet Soft"),
                new KeyValuePair<string, string>("magenta" , "Magenta"),
                new KeyValuePair<string, string>("magenta-soft" , "Magenta Soft"),
                new KeyValuePair<string, string>("crimson" , "Crimson"),
                new KeyValuePair<string, string>("crimson-soft" , "Crimson Soft"),
                new KeyValuePair<string, string>("amber" , "Amber"),
                new KeyValuePair<string, string>("amber-soft" , "Amber Soft"),
                new KeyValuePair<string, string>("brown" , "Brown"),
                new KeyValuePair<string, string>("brown-soft" , "Brown Soft"),
                new KeyValuePair<string, string>("steel" , "Steel"),
                new KeyValuePair<string, string>("steel-soft" , "Steel Soft")
            }.AsReadOnly();
        }
    }
}
