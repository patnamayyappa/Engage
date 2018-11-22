import { Component, Input, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute, Params, RoutesRecognized } from '@angular/router';
import { Observable } from 'rxjs/Rx';
import { StaffSurveyService } from './../../services/staff-survey.service'
import { UtilityService } from 'WebComponents/common/services/utility.service';
import { QuestionTypes } from 'WebComponents/common/app-constants/app-constants.module';

declare let SonomaCmc: any;


export interface QuestionType {
  id: number;
  name: string;
}

@Component({
  selector: 'app-staff-survey',

  templateUrl: './staff-survey.component.html',

  styleUrls: ['./staff-survey.component.less']
})

export class StaffSurveyComponent implements OnInit {
  @Input() defaultTheme: any;
  @Input() survey: any;
  @Input() question: any;
  @Input() questionTypes: Array<QuestionType>;
  @Input() showAccordion: boolean;
  @Input() showDiv: boolean;
  @Input() selectedName: any;
  @Input() questions: Array<any>;
  @Input() accordion: number;
  @Input() numberOfTicks: number;
  @Input() surveyStrings: any;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private _facultySurveyService: StaffSurveyService,
    private _sharedService: UtilityService,
    private _zone: NgZone,
    private ref: ChangeDetectorRef
  ) {
    let rwindow: any = window;
    rwindow.angularComponentRef = {
      zone: this._zone,
      componentFn: (value) => this.saveTemplateQuestion(),
      component: this
    };

    setInterval(() => {
      this.numberOfTicks++;
      this.ref.markForCheck();
    }, 1000);
  }

  ngOnInit() {
    this.survey = {};
    this.survey.description = "";
    this.surveyStrings = {};
    this.defaultTheme = {};
    this._sharedService.getDefaultTheme().then(this.prepareThemeObject.bind(this));
    this.survey.questions = [];
    this.initializeDisplayStrings();
    this.getQuestionTypes();
    this.getTemplateData().then(this.prepareSurveyObject.bind(this)); 
    this.setPreviewStyle();

  }
  setPreviewStyle() {
    this.survey.previewClass = "col-md-5 preview prvwDiv";
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






  initializeDisplayStrings() {
    let rwindow: any = window;
    this.surveyStrings = {
      showQuestions: rwindow.CampusManagement.localization.getResourceString("ShowQuestions"),
      hideQuestions: rwindow.CampusManagement.localization.getResourceString("HideQuestions"),
      addQuestion: rwindow.CampusManagement.localization.getResourceString("AddQuestion"),
      deleteQuestion: rwindow.CampusManagement.localization.getResourceString("DeleteQuestion"),
      addOption: rwindow.CampusManagement.localization.getResourceString("AddOption"),
      deleteOption: rwindow.CampusManagement.localization.getResourceString("DeleteOption"),
      expandQuestion: rwindow.CampusManagement.localization.getResourceString("ExpandQuestion"),
      collapseQuestion: rwindow.CampusManagement.localization.getResourceString("CollapseQuestion"),
      questionsContainerText: rwindow.CampusManagement.localization.getResourceString("QuestionsContainerText"),
      previewContainerText: rwindow.CampusManagement.localization.getResourceString("PreviewContainerText"),
      question: rwindow.CampusManagement.localization.getResourceString("Question"),
      options: rwindow.CampusManagement.localization.getResourceString("Options"),
      studentQuestionHeader: rwindow.CampusManagement.localization.getResourceString("StudentQuestionHeader"),
      enterQuestion: rwindow.CampusManagement.localization.getResourceString("EnterQuestion"),
      selectQuestionType: rwindow.CampusManagement.localization.getResourceString("SelectQuestionType"),
      student1: rwindow.CampusManagement.localization.getResourceString("Student1"),
      student2: rwindow.CampusManagement.localization.getResourceString("Student2"),
      okButton: rwindow.CampusManagement.localization.getResourceString("OkButton"),
      cannotDeleteQuestionText: rwindow.CampusManagement.localization.getResourceString("CannotDeleteQuestionText"),
      deleteQuestionConfirmation: rwindow.CampusManagement.localization.getResourceString("DeleteQuestionConfirmation"),
      enterOption: rwindow.CampusManagement.localization.getResourceString("EnterOption"),
      emptyQuestionText: rwindow.CampusManagement.localization.getResourceString("EmptyQuestionText"),
      questionTitle: rwindow.CampusManagement.localization.getResourceString("QuestionTitle")
    }


  }
  ngAfterViewInit() {
    document.getElementById('loadingOverlay').style.display = 'none';
  }  
  valueChanged(question) {
    let rwindow: any = window;
    question.title = "";
    switch (parseInt(question.type)) {
      case QuestionTypes.text:
        question.type = QuestionTypes.text;
        break;
      case QuestionTypes.boolean:
        question.type = QuestionTypes.boolean;
        break;
      case QuestionTypes.multiSelect:
        question.type = QuestionTypes.multiSelect;
        break;
      case QuestionTypes.singleSelect:
        question.type = QuestionTypes.singleSelect;
        break;
    }
    question.answers = [];
    var newAnswerNo = question.answers.length + 1;
    question.answers.push({ 'id': newAnswerNo, 'title': '' });
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(true);
  }
  onHideQuestions(event) {
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    this.survey.showQuestions = false;
    this.survey.previewClass = "col-md-11 preview";
  }
  onShowQuestions(event) {
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    this.survey.showQuestions = true;
    this.survey.previewClass = "col-md-5 preview prvwDiv";
  }
  onAccordionClick(question, index, event) {
    let that = this;
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    this.accordion = index + 1;
    this.survey.questions.forEach(element => {
      if (question.title != element.title) {
        element.showAccordion = true;
        element.showDiv = false;
        element.activeClass = 'fa fa-angle-down';
        element.expandCollapseTitle = that.surveyStrings.expandQuestion;
      }
    });
    question.showDiv = !question.showDiv;
    if (question.activeClass == 'fa fa-angle-down') {
      question.activeClass = 'fa fa-angle-up';
      question.expandCollapseTitle = that.surveyStrings.collapseQuestion;
    }
    else {
      question.activeClass = 'fa fa-angle-down';
      question.expandCollapseTitle = that.surveyStrings.expandQuestion;
    }
  }
  prepareSurveyObject(results: any) {
    let that = this;
    if (results && results.length === 1 && results[0].entities.length > 0) {
      for (let i = 0; i < results[0].entities.length; i++) {
        this.survey.questions.push(
          {
            id: results[0].entities[i].cmc_staffsurveyquestionid,
            title: results[0].entities[i].cmc_staffsurveyquestionname,
            type: results[0].entities[i].cmc_questiontype,
            questionOrder: results[0].entities[i].cmc_questionorder,
            answers: results[0].entities[i].cmc_choice ? JSON.parse(results[0].entities[i].cmc_choice) : [],
            showAccordion: true,
            showDiv: false,
            activeClass: 'fa fa-angle-down',
            expandCollapseTitle: this.surveyStrings.expandQuestion
          });
        this.accordion += 1;
      }
      this.survey.questions = this.survey.questions.sort((question1: any, question2: any) => {
        if (question1.questionOrder < question2.questionOrder) {
          return -1;
        } else if (question1.questionOrder > question2.questionOrder) {
          return 1;
        } else {
          return 0;
        }
      });
    }
    else {

      this.survey = { 'title': '', 'questions': [{ id: '00000000-0000-0000-0000-000000000000', 'title': '', 'activeClass': 'fa fa-angle-down', 'expandCollapseTitle': this.surveyStrings.collapseQuestion, 'answers': [], showAccordion: false, showDiv: true, type: 0 }] };
      this.survey.description = "";
    }
    this.survey.activeClass = "fa fa-angle-left";
    this.survey.hideClass = "fa fa-angle-right";
    this.survey.showQuestions = true;
    this.survey.students = new Array<object>();
    this.survey.students.push({ 'studentName': this.surveyStrings.student1 }, { 'studentName': this.surveyStrings.student2 });
  }
  getQuestionTypes() {
    this.questionTypes = [];
    this.questionTypes.push({
      id: 0,
      name: this.surveyStrings.selectQuestionType
    });
    this._facultySurveyService.getQuestionTypes().subscribe(
      (data: any) => {
        let options = data[0].OptionSet.Options;
        for (let i = 0; i < options.length; i++) {
          this.questionTypes.push({
            id: options[i].Value,
            name: options[i].Label.LocalizedLabels[0].Label
          });
        }
      }
    );

  }
  getTemplateData() {
    return this._facultySurveyService.getTemplateData();
  };

  addNewQuestion(event) {
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    let rwindow: any = window;
    this.survey.questions.forEach(element => {
      element.showAccordion = true;
      element.showDiv = false;
      element.activeClass = 'fa fa-angle-down'
    });
    var newQuestionNo = '00000000-0000-0000-0000-000000000000';
    this.survey.questions.push({ 'id': newQuestionNo, 'title': '', type: 0, 'activeClass': 'fa fa-angle-up', 'expandCollapseTitle': this.surveyStrings.collapseQuestion, 'answers': [{ id: 1, 'title': '', 'correct': 0 }], showAccordion: false, showDiv: true });
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(true);
  };

  addNewAnswer(question, event) {
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    let rwindow: any = window;
    let newAnswerNo = question.answers.length + 1;
    question.answers.push({ 'id': newAnswerNo, 'title': '' });
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(true);
  };

  handleModelChange() {
    let rwindow: any = window;
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(true);
  }

  deleteQuestion(item, items, event) {
    let rwindow: any = window,that=this;
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    if (items.length == 1) {
      rwindow.parent.Xrm.Navigation.openAlertDialog({
        text: this.surveyStrings.cannotDeleteQuestionText,
        confirmButtonLabel: this.surveyStrings.okButton
      }, null);
    }
    else {

      rwindow.parent.Xrm.Utility.confirmDialog(this.surveyStrings.deleteQuestionConfirmation, function () {
        var newQuestionNo = '00000000-0000-0000-0000-000000000000';
        if (item.id.toString() == newQuestionNo) {
          items.splice(items.indexOf(item), 1);
        }
        else {
          that.showProgress(true);
          rwindow.parent.Xrm.WebApi.deleteRecord("cmc_staffsurveyquestion", item.id.toString()).then(
            function success(result) {
              items.splice(items.indexOf(item), 1);
              that.showProgress(false);
            },
            function (error) {
              items.splice(items.indexOf(item), 1);
              that.showProgress(false);
            });
        }
      });

    }
 
  };

  deleteOption(item, items, event) {
    if (event.type == "keyup" && event.keyCode != 13) return true;
    event.stopPropagation();
    let rwindow: any = window;
    items.splice(items.indexOf(item), 1);
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(true);
  }


  saveTemplateQuestion() {
    let rwindow: any = window,that=this;
    rwindow.parent.Xrm.Page.getAttribute("cmc_isdirty").setValue(false);
    let templateID = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    let questions = [];
    let questionOrder = 0;
    for (let i = 0; i < this.survey.questions.length; i++) {
      if (this.survey.questions[i].type == QuestionTypes.none ||
        this.survey.questions[i].title == '') {      
        rwindow.parent.Xrm.Navigation.openAlertDialog({
          text: this.surveyStrings.emptyQuestionText,
          confirmButtonLabel: this.surveyStrings.okButton
        });
        return false;
      }
      else {     
        questionOrder++;
        questions.push({
          '@odata.type': 'Microsoft.Dynamics.CRM.cmc_staffsurveyquestion',
          "cmc_staffsurveyquestionname": this.survey.questions[i].title,
          "cmc_choice": JSON.stringify(this.survey.questions[i].answers),
          "cmc_questiontype": this.survey.questions[i].type,
          "cmc_staffsurveyquestionid": this.survey.questions[i].id.toString(),
          "cmc_questionorder": questionOrder
        });
      }
    }
    this.showProgress(true);
    this._facultySurveyService.saveTemplateQuestions(questions)
      .then(this.refreshForm.bind(this))
      .catch(function (error) {
        that.showProgress(false);
      });
  }
  refreshForm() {
    let rwindow: any = window;
    rwindow.parent.Xrm.Page.data.setFormDirty(false);
    this.showProgress(false);    
  }
  showProgress(isShow: boolean) {
    document.getElementById('loadingOverlay').style.display = isShow ? 'block' : 'none';
  }
}
