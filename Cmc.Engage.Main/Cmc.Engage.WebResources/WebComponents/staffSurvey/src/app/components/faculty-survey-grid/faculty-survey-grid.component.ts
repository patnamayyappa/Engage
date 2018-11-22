import { Component, Input, OnInit, AfterViewInit, NgZone, ChangeDetectorRef, Pipe, PipeTransform  } from '@angular/core';
// tslint:disable-next-line:import-blacklist
import { Observable } from 'rxjs/Rx';
import { UtilityService } from 'WebComponents/common/services/utility.service';
import { StaffSurveyService } from './../../services/staff-survey.service'
import { DomSanitizer } from '@angular/platform-browser';
import { QuestionTypes } from 'WebComponents/common/app-constants/app-constants.module';
import * as $ from 'jquery';
declare let SonomaCmc: any, Xrm: any, CampusManagement:any;
// tslint:disable-next-line:class-name
interface surveyResponse {
    surveyId: string;
    StudentId: string;
    questionId: string;
    answer: any;
}


@Component({
    selector: 'app-grid',
    templateUrl: './faculty-survey-grid.component.html',
    styleUrls: ['./faculty-survey-grid.component.less',]
    
})


export class FacultySurveyGridComponent implements OnInit {
    @Input() survey: any;
  @Input() surveyResponse: any;
  @Input() question: any;
    @Input() student: any;
    @Input() entity: any;
    @Input() description: any;
    @Input() isInactive: any;
    @Input() numberOfTicks: number;
    @Input() defaultTheme: any;
    @Input() surveyStrings: any;
    constructor(private _facultySurveygridService: StaffSurveyService,
      private _zone: NgZone,
      private ref: ChangeDetectorRef,
      private _sharedService: UtilityService) {
        const rwindow: any = window;
        rwindow.angularComponentRef = {
            zone: this._zone,
            componentfn: () => this.saveSurveyResponse(0),
            submitfn: () => this.saveSurveyResponse(1),
            component: this
        };

        setInterval(() => {
            this.numberOfTicks++;
            // the following is required, otherwise the view will not be updated
            this.ref.markForCheck();
        }, 1000);
    }
  ngOnInit() {
    this.description = "";
    this.defaultTheme = {};
    this.initializeDisplayStrings();
	  this._sharedService.getDefaultTheme().then(this.prepareThemeObject.bind(this));
    this.getSurveyData();

    }
  initializeDisplayStrings() {
    let rwindow: any = window;
    this.surveyStrings = {
      submitConfirmation: rwindow.CampusManagement.localization.getResourceString("SubmitConfirmation"),
      studentQuestionHeader: rwindow.CampusManagement.localization.getResourceString("StudentQuestionHeader")

    }
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
  ngAfterViewInit() {
    
      $('.fixedheader').on('scroll', function () {
        var topValue = $('.fixedheader').scrollTop();
        var transformTopValue = (topValue <= 0 ? 'none' : 'translateY(' + topValue + 'px)')
        $('.fixedheader thead th').css('transform', transformTopValue);
        var leftValue = $('.fixedheader').scrollLeft();
        var transformLeftValue = (leftValue <= 0 ? 'none' : 'translateX(' + leftValue + 'px)')
        $('.fixedheader tr td:first-child').css('transform', transformLeftValue);
      });
  }
    getSurveyData = () => {
        // tslint:disable-next-line:prefer-const
        let rwindow: any = window, that = this;
        that.surveyResponse = []
        this._facultySurveygridService.getSurveyStudents().subscribe(
            (data: any) => {
                that.student = data[0].value;
                let surveyStu = [];
                that._facultySurveygridService.getQuestions().subscribe(
                    (result: any) => {
                        that.description = result[0].cmc_description||"";
                      that.isInactive = (result[0].statecode == 1 ? Promise.resolve(true) : that._facultySurveygridService.isBusinessAdmin())
                        that.question = result[0].cmc_staffsurvey_cmc_staffsurveyquestion;

                        that.question = that.question.sort((question1:  any,  question2:  any)  =>  {
                          if (question1.cmc_questionorder < question2.cmc_questionorder) {
                            return  -1;
                          } else if (question1.cmc_questionorder > question2.cmc_questionorder  ) {
                            return  1;
                          }  else  {
                            return  0;
                          }
                        });

                        const questionString = JSON.stringify(that.question);
                        that.student.forEach(function(stu) {
                            
                            that._facultySurveygridService.geturlData(stu["cmc_staffsurveyresponse_cmc_staffsurveyqu@odata.nextLink"]).subscribe(
                                (result: any) => {
                                    stu.cmc_staffsurveyresponse_cmc_staffsurveyqu = result[0].value;
                                    stu.question = JSON.parse(questionString);
                                    stu.question.forEach(function(que) {
                                      que.cmc_choice = JSON.parse(que.cmc_choice);

                                      const Locdata = (stu.cmc_staffsurveyresponse_cmc_staffsurveyqu.length > 0
                                        ? stu.cmc_staffsurveyresponse_cmc_staffsurveyqu.filter(response => response._cmc_staffsurveyquestionid_value === que.cmc_staffsurveyquestionid)//that.getProvidedanswer(que.cmc_staffsurveyquestionid, stu.cmc_staffsurveyresponse_cmc_staffsurveyqu)
                                        : []);
                                        que.options = [];
                                        if (Locdata.length == 0) {
                                          if (que.cmc_questiontype == QuestionTypes.singleSelect) {
                                                que.cmc_choice.forEach((data: any) => {
                                                    que.options.push({ 'id': data.id, 'title': data.title, 'checked': false })
                                                });
                                          }
                                          else if (que.cmc_questiontype == QuestionTypes.boolean)
                                                que.options = 0;
                                            else
                                                que.options = "";
                                        }
                                        else if (Locdata.length > 0) {
                                          if (que.cmc_questiontype == QuestionTypes.singleSelect) 
                                            que.options = JSON.parse(Locdata[0].cmc_response)
                                          else if (que.cmc_questiontype == QuestionTypes.boolean)
                                              que.options = parseInt(Locdata[0].cmc_response);
                                            else
                                              que.options = Locdata[0].cmc_response;
                                        }
                                        que.name = stu._cmc_contactid_value + que.cmc_staffsurveyquestionid;
                                        que.cmc_staffsurveyQueresponseid = (Locdata.length > 0 ? Locdata[0].cmc_staffsurveyquestionresponseid : "00000000-0000-0000-0000-000000000000")
                                    });
                                })
                           });
                    });
            },
            (error: any) => { console.log(error) });
    }
    saveSurveyResponse(issubmitted: number) {
      if (issubmitted == 1) {
        if (!confirm(this.surveyStrings.submitConfirmation))
          return;
          }
        const rwindow: any = window;
        const surveyId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
        const surveyResponseData = [];
        const currentContext = this;

        currentContext.toggleLoading(true);

        this.student.forEach(surveyItem => {
            surveyItem.question.forEach(questionItem => {
                surveyResponseData.push({                   
                    '@odata.type': 'Microsoft.Dynamics.CRM.cmc_staffsurveyquestionresponse',
                    'cmc_staffsurveyquestionid@odata.bind': "/cmc_staffsurveyquestions(" + questionItem.cmc_staffsurveyquestionid + ")",
                    'cmc_response': (questionItem.cmc_questiontype == 2 ? JSON.stringify(questionItem.options) : (questionItem.options != null ? questionItem.options.toString():"")),
                    'cmc_staffsurveyquestionresponseid': questionItem.cmc_staffsurveyQueresponseid,
                    'cmc_staffsurveyresponseid@odata.bind': "/cmc_staffsurveyresponses(" + surveyItem.cmc_staffsurveyresponseid +")"
                });
            });
        });
        SonomaCmc.WebAPI.post('cmc_staffsurveies(' + surveyId + ')/Microsoft.Dynamics.CRM.cmc_CreateUpdateStaffSurveyQuestionResponses',
            {
                surveyData: surveyResponseData,
                isSubmitted: issubmitted
              
             
            }).then(function(result) {

                var response = JSON.parse(result.staffSurveyResponse);
                if (issubmitted == 1) {
                  rwindow.parent.Xrm.Page.ui.refreshRibbon();
                  rwindow.parent.Xrm.Utility.openEntityForm(rwindow.parent.Xrm.Page.data.entity.getEntityName(), rwindow.parent.Xrm.Page.data.entity.getId());
                }
                currentContext.toggleLoading(false);
                currentContext.getSurveyData();
            }).catch(function(error) { alert(error); console.log(error) });
    }
    toggleLoading(isLoading) {
        document.getElementById('loadingOverlay').style.display = isLoading
            ? ''
            : 'none';
    }

}
