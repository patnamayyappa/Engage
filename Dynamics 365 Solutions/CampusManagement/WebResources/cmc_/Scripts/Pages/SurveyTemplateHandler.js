function Form_OnSave(executionContext)

{
// Xrm.Page.getAttribute("cmc_isdirty").setValue("false");
executionContext.getEventArgs().preventDefault();
Xrm.Page.data.entity.save();
var windowObject=Xrm.Page.getControl("WebResource_SurveyTemplate").getObject().contentWindow.window;
windowObject.angularComponentRef.zone.run(function () { windowObject.angularComponentRef.componentFn()});
}

 function  onDesignerLoad() {
 //Xrm.Page.getAttribute("cmc_isdirty").setValue("true");
      var  _loadFunction = Xrm.Page.ui.getFormType() == 1? 
                   navigateToForm("Staff Survey Template") :
                 onLoadContinue();
       
    }
  function  onLoadContinue() {
        if (Xrm.Page.ui.getFormType() != 1) {
            var n = Xrm.Page.ui.controls.get("WebResource_SurveyTemplate");
            n.setVisible(!0)
        }
       
    }

  function navigateToForm(n) {
        var t = Xrm.Page.ui.formSelector.items.get();
        for (var r in t) {
            var i = t[r],
                u = i.getId(),
                f = i.getLabel();
            f == n && Xrm.Page.ui.formSelector.items.get(u).navigate()
        }
    }