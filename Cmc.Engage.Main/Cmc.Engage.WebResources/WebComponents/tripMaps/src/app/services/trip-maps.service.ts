import { Injectable , Input } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';
import { Observable } from 'rxjs/Rx';

@Injectable()
export class TripMapsService {

  constructor(private http: HttpClient, private appConstants: AppConstantsModule) { }
  @Input() tripId: string;
  getData(){
    enum Status {
      Cancelled = 175490002     
    }
  const rwindow: any = window;
    this.tripId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);

    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/' + this.appConstants.oDataApiVersion + '/cmc_tripactivities?$select=cmc_activitytype,cmc_name,cmc_location,cmc_startdatetime,cmc_enddatetime,cmc_tripactivitylatitude,cmc_tripactivitylongitude&$filter=((_cmc_trip_value)eq(' + this.tripId + ') and (cmc_tripactivitystatus ne ' + Status.Cancelled + '))&$orderby=cmc_startdatetime';

  return Observable.forkJoin(
    this.http.get(url, {
      headers: {
        'Cache-control': 'no-cache'
      }
    })
  );
  }
  getBingMapKey() {
    const rwindow: any = window;
    var fetch =
      ['<fetch top="1">',
        '<entity name="cmc_configuration">',
        '<filter type="and">',
        '<condition attribute="cmc_configurationname" operator="eq" value="BingMapApiKey" />',
        '</filter>',
        '</entity>',
        '</fetch>'
      ].join('');

    return Observable.forkJoin(
      rwindow.parent.Xrm.WebApi.retrieveMultipleRecords('cmc_configuration', '?fetchXml=' + fetch)
        .then(function (result) {
          var configuration = result && result.entities && result.entities.length > 0 ? result.entities[0] : null;
          if (!configuration) {
            console.log("Bing Map Configuration Key Not Exist.");
            return;
          }
          return configuration["cmc_value"];
        },
        function (error) {
          console.log(error);
        }));

  }

}
