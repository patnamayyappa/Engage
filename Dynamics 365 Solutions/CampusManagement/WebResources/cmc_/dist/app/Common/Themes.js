var CampusManagement;
(function (CampusManagement) {
    var themes;
    (function (themes) {
        function getDefaultTheme() {
            var fetchXml = [
                '<fetch>',
                '<entity name="theme">',
                '<attribute name="backgroundcolor" />',
                '<attribute name="controlborder" />',
                '<attribute name="controlshade" />',
                '<attribute name="defaultcustomentitycolor" />',
                '<attribute name="defaultentitycolor" />',
                '<attribute name="globallinkcolor" />',
                '<attribute name="headercolor" />',
                '<attribute name="navbarbackgroundcolor" />',
                '<attribute name="navbarshelfcolor" />',
                '<attribute name="processcontrolcolor" />',
                '<attribute name="selectedlinkeffect" />',
                '<attribute name="panelheaderbackgroundcolor" />',
                '<attribute name="hoverlinkeffect" />',
                '<attribute name="accentcolor" />',
                '<attribute name="pageheaderbackgroundcolor" />',
                '<attribute name="maincolor" />',
                '<filter>',
                '<condition operator="eq" attribute="isdefaulttheme" value="true" />',
                '</filter>',
                '</entity>',
                '</fetch>'
            ].join('');
            return Xrm.WebApi.retrieveMultipleRecords('theme', '?fetchXml=' + fetchXml);
        }
        themes.getDefaultTheme = getDefaultTheme;
    })(themes = CampusManagement.themes || (CampusManagement.themes = {}));
})(CampusManagement || (CampusManagement = {}));
