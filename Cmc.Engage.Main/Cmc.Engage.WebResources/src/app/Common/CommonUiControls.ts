module CampusManagement.CommonUiControls {
    export function openAlertDialog(message, buttontext) {
        return Xrm.Navigation.openAlertDialog({
            text: message,
            confirmButtonLabel: buttontext
        }, null);
    }
}