var CampusManagement;
(function (CampusManagement) {
    var ribbon;
    (function (ribbon) {
        var checkin;
        (function (checkin) {
            function quickCreateCheckin() {
                Xrm.Navigation.openForm({
                    entityName: 'msevtmgt_checkin',
                    useQuickCreateForm: true
                }, null);
            }
            checkin.quickCreateCheckin = quickCreateCheckin;
        })(checkin = ribbon.checkin || (ribbon.checkin = {}));
    })(ribbon = CampusManagement.ribbon || (CampusManagement.ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
