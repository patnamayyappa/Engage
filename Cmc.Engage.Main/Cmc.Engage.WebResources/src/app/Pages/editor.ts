interface KendoEditorOptions extends kendo.ui.EditorOptions {
    value: any;
}

module CampusManagement.editor {
    declare let SonomaCmc;
    var fieldName;
    export function load() {
        fieldName = getFieldName();
        // parent.Xrm.Page is still used throughout this file as there isn't a good replacement
        // yet for Dynamics CRM 9.0 
        var description;
        if (SonomaCmc.LocalStorage.get('customEditorValue') != null) {
            description = SonomaCmc.LocalStorage.get('customEditorValue');
            parent.Xrm.Page.getAttribute(fieldName).setValue(description);
            SonomaCmc.LocalStorage.remove('customEditorValue')
        }
        else
            description = parent.Xrm.Page.getAttribute(fieldName).getValue();

        $("#editor").kendoEditor(<KendoEditorOptions>{
            value: description,
            change: onChange,
            tools: ["bold", "italic", "underline", "viewHtml", "createLink", "fontName", "fontSize", "foreColor", "insertImage", "justifyLeft", "justifyCenter", "justifyRight", "justifyFull"] 
        });
    }

    function onChange() {
        parent.Xrm.Page.getAttribute(fieldName).setValue(this.value());
    }

    function getFieldName() {
        var data = CampusManagement.utilities.getDataParameter();
        return data.fieldName;
    }
}