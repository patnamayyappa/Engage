var CampusManagement;
(function (CampusManagement) {
    var localization;
    (function (localization) {
        function getResourceString(key) {
            var rwindow = window;
            var resourceName = "cmc_/Resources/ResourceStrings";
            if (window.opener != undefined)
                return window.opener.Xrm.Utility.getResourceString(resourceName, key);
            else if (window.parent != undefined)
                return window.parent.Xrm.Utility.getResourceString(resourceName, key);
            else
                return window.Xrm.Utility.getResourceString(resourceName, key);
        }
        localization.getResourceString = getResourceString;
    })(localization = CampusManagement.localization || (CampusManagement.localization = {}));
})(CampusManagement || (CampusManagement = {}));
