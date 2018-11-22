module CampusManagement.Ribbon.cmc_tripactivity {
  declare let SonomaCmc: any;  
  declare let Xrm: any;

  // send email 
  export function SendEmail() {
    console.log("Trip activity id is " + Xrm.Page.data.entity.getId());
    var data = {
      '@odata.type': 'Microsoft.Dynamics.CRM.cmc_tripactivity',
      cmc_tripactivityid: Xrm.Page.data.entity.getId(),   
    };
    SonomaCmc.WebAPI.post('cmc_tripactivitysendemailaction',
      {
        TripActivityId: data
      }).then(result => {
      CommonUiControls.openAlertDialog(localization.getResourceString("Email_Success"), localization.getResourceString("OkButton")); 
    });

  }
 

  // Send Email button display rule
  export function showSendEmailButton(): boolean {
    let returnValue = false;   
    let trip = Xrm.Page.getAttribute("cmc_trip").getValue();
    if (trip != null) {
      let tripId = SonomaCmc.Guid.decapsulate(trip[0].id);
      let url = Xrm.Page.context.getClientUrl() + "/api/data/v" + getVersion() + "/cmc_trips?$select=cmc_status&$filter=(cmc_tripid eq " + tripId + ")";
      $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        url: url,
        beforeSend: xmlHttpRequest => {
          xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
          xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
          xmlHttpRequest.setRequestHeader("Accept", "application/json");
        },
        async: false,
        success(result) {         
          if (result.value.length > 0) {
            let trip = result.value[0];
            if (trip != null) { // to be safe
              console.log("Result of the Trip status   is " + trip.cmc_status);
              if (trip.cmc_status === Constants.TripStatus.Canceled || trip.cmc_status === Constants.TripStatus.Completed)
                returnValue=  false;//button will hide
              else
                returnValue= true;//button will not hide
            }
          }        
        },
        error() {
          returnValue= false;
        }
      });
    }
    return returnValue;
  }

  function getVersion() {
    return Xrm.Page.context.getVersion().split('.').slice(0, 2).join('.');
  }


}
