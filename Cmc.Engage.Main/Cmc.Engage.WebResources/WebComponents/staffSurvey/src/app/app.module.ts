import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppComponent } from './app.component';
import { StaffSurveyComponent } from './components/staff-survey/staff-survey.component';
import { CheckboxComponent } from './components/checkbox/checkbox.component';
import { ContactSurveyComponent } from './components/contact-survey/contact-survey.component';
import { DropdownComponent } from './components/dropdown/dropdown.component';
import { FacultySurveyGridComponent } from './components/faculty-survey-grid/faculty-survey-grid.component';
import { PreviewComponent } from './components/preview/preview.component';
import { RadiobuttonComponent } from './components/radiobutton/radiobutton.component';
import { TextComponent } from './components/text/text.component';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule,Pipe, PipeTransform } from '@angular/core';
import { Routes, RouterModule } from "@angular/router";
import { DomSanitizer } from '@angular/platform-browser'
import { StaffSurveyService } from './services/staff-survey.service'
import { UtilityService } from 'WebComponents/common/services/utility.service';
import { APP_BASE_HREF } from '@angular/common';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module'

const routes: Routes = [
  { path: '', redirectTo: 'AppComponent', pathMatch: 'full' },
  { path: 'staffSurveyTemplate', component: StaffSurveyComponent },
  { path: 'staffSurvey', component: FacultySurveyGridComponent },
  { path: 'staffSurveyResponse', component: ContactSurveyComponent },
  { path: '**', component: AppComponent }
];
@Pipe({ name: 'safeHtml'})
export class SafeHtmlPipe implements PipeTransform  {
  constructor(private sanitized: DomSanitizer) {}
  transform(value) {
    return this.sanitized.bypassSecurityTrustHtml(value);
  }
}
@NgModule({
  declarations: [
    AppComponent,
    StaffSurveyComponent,
    CheckboxComponent,
    ContactSurveyComponent,
    DropdownComponent,
    FacultySurveyGridComponent,
    PreviewComponent,
    RadiobuttonComponent,
    TextComponent,
    SafeHtmlPipe


  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot(routes),
    AppConstantsModule
  ],
  providers: [StaffSurveyService,UtilityService,{
    provide: APP_BASE_HREF, useValue: '/' + window.location.pathname.split('/')[1] + 'WebResources/cmc_/dist/StaffSurvey'}],
  bootstrap: [AppComponent]
})
export class AppModule { }
