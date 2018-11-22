import { Component, OnInit,Input } from '@angular/core';
import { StaffSurveyService } from './../../services/staff-survey.service';
@Component({
  selector: 'app-preview',
  templateUrl: './preview.component.html',
  styleUrls: ['./preview.component.less']
})
export class PreviewComponent implements OnInit {
  @Input() survey: any;
  @Input() defaultTheme: any;
  @Input() surveyStrings: any;
  constructor(private _facultySurveyService: StaffSurveyService) { }

  ngOnInit() {
    this.getTemplateDescription().then(this.setSurveyDescription.bind(this));
  }
  getTemplateDescription() {
    return this._facultySurveyService.getTemplateDescription();
  };
  setSurveyDescription(results: any) {
    this.survey.description = results[0].cmc_description == undefined ? "" : results[0].cmc_description;
  }  
}
