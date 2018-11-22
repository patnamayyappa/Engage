var CampusManagement;
(function (CampusManagement) {
    var ribbon;
    (function (ribbon) {
        var opportunity;
        (function (opportunity) {
            function HideRecalculateLifecycleRibbon() {
                return false; //Used to hide "RecalculateLifecycle" Ribbon in the Opportunity form.
            }
            opportunity.HideRecalculateLifecycleRibbon = HideRecalculateLifecycleRibbon;
        })(opportunity = ribbon.opportunity || (ribbon.opportunity = {}));
    })(ribbon = CampusManagement.ribbon || (CampusManagement.ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
