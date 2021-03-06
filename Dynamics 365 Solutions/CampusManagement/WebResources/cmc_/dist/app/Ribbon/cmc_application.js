var CampusManagement;
(function (CampusManagement) {
    var ribbon;
    (function (ribbon) {
        var application;
        (function (application) {
            // Form Ids are deployed across solutions
            var _applicationFeeInvoiceFormId = 'd59b845b-835c-43eb-8c05-8439d22c3a80';
            function createInvoice(executionContext) {
                // Unified Interface and Web Interface pass in different arguments. This will handle both
                var formContext = (executionContext.getFormContext && executionContext.getFormContext()) ||
                    executionContext;
                // Contact will pull in through the relationship. Price List will be pulled down on the
                // Invoice form.
                Xrm.Navigation.openForm({
                    createFromEntity: {
                        entityType: formContext.data.entity.getEntityName(),
                        id: formContext.data.entity.getId()
                    },
                    entityName: 'invoice',
                    formId: _applicationFeeInvoiceFormId
                }, null);
            }
            application.createInvoice = createInvoice;
        })(application = ribbon.application || (ribbon.application = {}));
    })(ribbon = CampusManagement.ribbon || (CampusManagement.ribbon = {}));
})(CampusManagement || (CampusManagement = {}));
