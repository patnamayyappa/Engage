import { Component, Injectable, Input, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Rx';
import { UtilityService } from 'WebComponents/common/services/utility.service';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';
@Component({
  selector: 'app-contact-survey',
  templateUrl: './contact-survey.component.html',
  styleUrls: ['./contact-survey.component.less']
})
export class ContactSurveyComponent implements OnInit {
  @Input() survey: any;
  @Input() questionTypes: any;
  @Input() defaultTheme: any;
  constructor(private http: HttpClient,
    private _sharedService: UtilityService,
  private appConstants:AppConstantsModule) { }

  ngOnInit() {
    this.survey = {};
    this.survey.questions = [];
    this._sharedService.getDefaultTheme().then(this.prepareThemeObject.bind(this));
    this.getSurveyResponseForContact().subscribe(this.mapQuestionsWithAnswers.bind(this));
  }
  prepareThemeObject(results: any) {
    let that = this;
    if (results.entities.length > 0) {
      that.defaultTheme = {
        backgroundcolor: results.entities[0].backgroundcolor,
        controlborder: results.entities[0].controlborder,
        controlshade: results.entities[0].controlshade,
        defaultcustomentitycolor: results.entities[0].defaultcustomentitycolor,
        defaultentitycolor: results.entities[0].defaultentitycolor,
        globallinkcolor: results.entities[0].globallinkcolor,
        headercolor: results.entities[0].headercolor,
        navbarbackgroundcolor: results.entities[0].navbarbackgroundcolor,
        navbarshelfcolor: results.entities[0].navbarshelfcolor,
        processcontrolcolor: results.entities[0].processcontrolcolor,
        selectedlinkeffect: results.entities[0].selectedlinkeffect,
        panelheaderbackgroundcolor: results.entities[0].panelheaderbackgroundcolor,
        hoverlinkeffect: results.entities[0].hoverlinkeffect,
        accentcolor: results.entities[0].accentcolor,
        pageheaderbackgroundcolor: results.entities[0].pageheaderbackgroundcolor,
        maincolor: results.entities[0].maincolor,
      }

    }
  }
  mapQuestionsWithAnswers(results: any) {
    var questionResponses = results[0].cmc_staffsurveyresponse_cmc_staffsurveyqu;
    var surveyId = results[0]._cmc_staffsurveyid_value;
    this.getSurveyQuestionsFromSurvey(surveyId).subscribe((results: any) => {
      let surveyQuestions = results[0].cmc_staffsurvey_cmc_staffsurveyquestion;
      surveyQuestions.forEach((data: any) => {
        let questionResponse = questionResponses.filter(response =>
          response._cmc_staffsurveyquestionid_value === data.cmc_staffsurveyquestionid
        );
        this.survey.questions.push({
          id: data.cmc_staffsurveyquestionid,
          title: data.cmc_staffsurveyquestionname,
          type: data.cmc_questiontype,
          answers: data.cmc_questiontype == 2 ? JSON.parse(questionResponse[0].cmc_response) : JSON.parse(data.cmc_choice),
          selectedAnswer: data.cmc_questiontype == 4 ? parseInt(questionResponse[0].cmc_response) : questionResponse[0].cmc_response
        })
      })

    })
  }

  getSurveyResponseForContact() {
    const rwindow: any = window;
    let surveyResponseId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() +
      '/api/data/'+ this.appConstants.oDataApiVersion + '/cmc_staffsurveyresponses(' + surveyResponseId + ')?' +
      '$select=_cmc_staffsurveyid_value&$expand=cmc_staffsurveyresponse_cmc_staffsurveyqu';
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'
        }
      }));
  }
  getSurveyQuestionsFromSurvey(surveyId) {
    const rwindow: any = window;
    const url = rwindow.parent.Xrm.Page.context.getClientUrl() + '/api/data/'+ this.appConstants.oDataApiVersion + '/cmc_staffsurveies(' + surveyId + ')?$expand=cmc_staffsurvey_cmc_staffsurveyquestion($select=cmc_staffsurveyquestionname,cmc_choice,cmc_questiontype)';
    return Observable.forkJoin(
      this.http.get(url, {
        headers: {
          'Cache-control': 'no-cache'
        }
      }));
  }

}
