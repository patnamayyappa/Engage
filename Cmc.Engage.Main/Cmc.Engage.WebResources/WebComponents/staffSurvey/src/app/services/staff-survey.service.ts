
import { Injectable ,Input} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Rx';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';
declare let SonomaCmc: any;


@Injectable()
export class StaffSurveyService {
  @Input() surveyId: string;
  constructor(private http: HttpClient,private appConstants:AppConstantsModule) { }

  //public methods
  getQuestionTypes() {
    let rwindow: any = window;
    let url = rwindow.parent.Xrm.Page.context.getClientUrl() + "/api/data/"+ this.appConstants.oDataApiVersion+"/EntityDefinitions(LogicalName='cmc_staffsurveyquestion')/Attributes(LogicalName='cmc_questiontype')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=OptionSet($select=Options)";
    return Observable.forkJoin(
      this.http.get(url)

    );


  }

  getTemplateData() {
    let rwindow: any = window, templateID = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    let fetchQuestions = [
      '<fetch>',
      '<entity name="cmc_staffsurveyquestion">',
      '<attribute name="cmc_staffsurveyquestionname" />',
      '<attribute name="cmc_choice" />',
      '<attribute name="cmc_questiontype" />',
      '<attribute name="cmc_questionorder" />',
      '<filter type="and">',
      '<condition attribute="cmc_staffsurveytemplateid" operator="eq" value="', templateID, '"/>',
      '</filter>',
      '</entity>',
      '</fetch>'
    ].join('');
    return SonomaCmc.Promise.all([
      rwindow.parent.Xrm.WebApi.retrieveMultipleRecords('cmc_staffsurveyquestion', '?fetchXml=' + fetchQuestions),
    ]);
  }

  getTemplateDescription() {

    let rwindow: any = window, templateID = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);

    return SonomaCmc.Promise.all([

      rwindow.parent.Xrm.WebApi.retrieveRecord('cmc_staffsurveytemplate', templateID)
    ]);
  }

  saveTemplateQuestions(questions: any) {
    console.log('called save method');
    let rwindow: any = window, templateID = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    return SonomaCmc.WebAPI.post('cmc_staffsurveytemplates(' + templateID + ')/Microsoft.Dynamics.CRM.cmc_SurveyTemplateCreateQuestions',
      {
        Questions: questions
      });
      
  }
  getQuestions() {
    const rwindow: any = window;
    this.surveyId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    // tslint:disable-next-line:max-line-length
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/'+ this.appConstants.oDataApiVersion +'/cmc_staffsurveies(' + this.surveyId + ')?$select=cmc_staffsurveyname,cmc_description,statecode&$expand=cmc_staffsurvey_cmc_staffsurveyquestion($select=cmc_staffsurveyquestionname,cmc_choice,cmc_questiontype,cmc_questionorder)';
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'

        }
      })
    );

  }
  getStudents() {
    const rwindow: any = window;
    if (this.surveyId == null) {
      this.surveyId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    }

    // tslint:disable-next-line:max-line-length

    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/'+ this.appConstants.oDataApiVersion +'/cmc_staffsurveies(' + this.surveyId + ')?$select=cmc_staffsurveyname&$expand=cmc_staffsurvey_contact($select=fullname)';
    return Observable.forkJoin(
      this.http.get(url)
    );
  }
  getSurveyResponse() {
    const rwindow: any = window;
    if (this.surveyId == null) {
      this.surveyId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    }

    // tslint:disable-next-line:max-line-length
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/'+ this.appConstants.oDataApiVersion +'/cmc_staffsurveies(' + this.surveyId + ')?$select=cmc_staffsurveyname&$expand=cmc_staffsurvey_cmc_staffsurveyresponse($select=_cmc_staffsurveyquestionid_value,_cmc_contactid_value,cmc_staffsurveyresponsename)';
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'

        }
      })
    );
  }

  getSurveyStudents() {
    const rwindow: any = window;
    if (this.surveyId == null) {
      this.surveyId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    }
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/'+ this.appConstants.oDataApiVersion +'/cmc_staffsurveyresponses?$select=_cmc_contactid_value,cmc_staffsurveyId,cmc_staffsurveyresponseid&$filter=_cmc_staffsurveyid_value eq ' + this.surveyId + '&$expand=cmc_staffsurveyresponse_cmc_staffsurveyqu,cmc_contactid($select=fullname)';
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'

        }
      })
    );

  }
  geturlData(path) {
    const rwindow: any = window
    const url = path
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'

        }
      })
    );

  }
  async isBusinessAdmin() {
    const rwindow: any = window;
    let roles = rwindow.parent.Xrm.Page.context.getUserRoles();
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + "/XRMServices/2011/OrganizationData.svc/RoleSet?$filter=Name eq 'CMC - Business Unit Administrator'&$select=Name,RoleId";

    let roleId: any = await this.http.get(url).toPromise();

    if (roles.indexOf(roleId.d.results[0].RoleId) > -1)
      return true;
    else
      return false;
  }
}
