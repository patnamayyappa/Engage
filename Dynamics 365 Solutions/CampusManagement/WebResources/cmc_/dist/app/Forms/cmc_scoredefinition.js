/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CampusManagement;
(function (CampusManagement) {
    var cmc_scoredefinition;
    (function (cmc_scoredefinition) {
        var isSaveConfirmed = false;
        var _fieldNames = {
            baseEntityField: "cmc_baseentity",
            attribute: "cmc_targetattributename",
        };
        function onLoad(executionContext) {
            var formContext = executionContext.getFormContext();
            var baseEntiy = formContext.getAttribute(_fieldNames.baseEntityField);
            baseEntiy.addOnChange(LoadAttributePicker); //used to call attribute picker component on selecting the entity on Create Form Mode.
            if (baseEntiy && baseEntiy.getValue() !== "") {
                LoadAttributePicker(executionContext); //used to call attribute picker component on selecting the entity on Update Form Mode.
            }
            if (formContext.ui.getFormType() !== 1 /* Create */) {
                var entityPicker = formContext.getControl("WebResource_EntityPicker");
                entityPicker.setSrc(entityPicker.getSrc() + encodeURIComponent("&disabled=true")); //adding diabled key to disable the combobox on demand.
            }
        }
        cmc_scoredefinition.onLoad = onLoad;
        function onSave(executionContext) {
            var formContext = executionContext.getFormContext();
            if (formContext.ui.getFormType() === 1 /* Create */) {
                if (isSaveConfirmed) {
                    isSaveConfirmed = false;
                    return;
                }
                ;
                executionContext.getEventArgs().preventDefault(); //prevents the save by default and for dialoge function return if the result is confirm it recall the save event and exit the loop (Above return is used for that)
                showConfirmationDialog().then(function (r) {
                    if (r.confirmed) {
                        isSaveConfirmed = true;
                        Xrm.Page.data.save();
                    }
                });
                ;
            }
        }
        cmc_scoredefinition.onSave = onSave;
        function LoadAttributePicker(executionContext) {
            var formContext = executionContext.getFormContext();
            var baseEntiy = formContext.getAttribute(_fieldNames.baseEntityField).getValue();
            var attributePicker = formContext.getControl("WebResource_attributePicker");
            attributePicker.setVisible(false);
            if (!baseEntiy) {
                formContext.getAttribute(_fieldNames.attribute).setValue(null);
                return;
            }
            attributePicker.setVisible(true);
            var input = '{"EntityLogicalNames": "' + baseEntiy + '"}';
            attributePicker.setSrc("$webresources/cmc_/dist/attributePicker/index.html?data=" + encodeURIComponent("component=attributePicker&includerelationshiptypes=2&includeAttributeTypes=5&excludeRelationShips=true&entity=" + baseEntiy + "&schemafield=" + _fieldNames.attribute + "&showLabel=true&retrieveMetadataJsonAction=cmc_retrieveattributesandrelationshipsforentities&retrieveMetadataJsonInput=" + input));
        }
        function showConfirmationDialog() {
            var confirmStrings = { text: CampusManagement.localization.getResourceString("EntityPicker_DisableAlert"), title: CampusManagement.localization.getResourceString("ConfirmationDialog") };
            var confirmOptions = { height: 200, width: 450 };
            return Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions);
        }
    })(cmc_scoredefinition = CampusManagement.cmc_scoredefinition || (CampusManagement.cmc_scoredefinition = {}));
})(CampusManagement || (CampusManagement = {}));
