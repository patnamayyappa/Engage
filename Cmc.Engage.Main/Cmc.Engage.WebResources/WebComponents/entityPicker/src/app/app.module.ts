import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { Routes, RouterModule } from "@angular/router";
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { APP_BASE_HREF } from '@angular/common';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { EntityPickerComponent } from './components/entity-picker/entity-picker.component';
import { EntityPickerService } from './services/entity-picker.service'
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';
const routes: Routes = [
  { path: '', redirectTo: 'AppComponent', pathMatch: 'full' },
  { path: 'entityPicker', component: EntityPickerComponent },
  { path: '**', component: AppComponent }
];
@NgModule({
  declarations: [
    AppComponent,
    EntityPickerComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    DropDownsModule,
    FormsModule,
    HttpClientModule,
    AppConstantsModule,
    RouterModule.forRoot(routes)
  ],
  providers: [EntityPickerService, {
    provide: APP_BASE_HREF, useValue: '/' + window.location.pathname.split('/')[1] + 'WebResources/cmc_/dist/AttributePicker'
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
