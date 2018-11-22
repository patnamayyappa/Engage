var CampusManagement;
(function (CampusManagement) {
    var CommonUiControls;
    (function (CommonUiControls) {
        function openAlertDialog(message, buttontext) {
            return Xrm.Navigation.openAlertDialog({
                text: message,
                confirmButtonLabel: buttontext
            }, null);
        }
        CommonUiControls.openAlertDialog = openAlertDialog;
    })(CommonUiControls = CampusManagement.CommonUiControls || (CampusManagement.CommonUiControls = {}));
})(CampusManagement || (CampusManagement = {}));
