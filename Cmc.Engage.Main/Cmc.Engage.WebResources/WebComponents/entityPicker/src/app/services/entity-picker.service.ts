import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Rx';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';

@Injectable()
export class EntityPickerService {

  constructor(private http: HttpClient, private appConstants: AppConstantsModule) { }

  getEntityList() {

    const rwindow: any = window;
    // tslint:disable-next-line:max-line-length
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + "/api/data/v9.0/EntityDefinitions?$select=LogicalName,ObjectTypeCode,DisplayName&$filter=LogicalName eq 'account' or LogicalName eq 'lead' or LogicalName eq 'contact' or LogicalName eq 'opportunity'";
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'
        }
      })
    );
  }
}
