import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppComponent } from './app.component';
import { AttributePickerComponent } from './components/attribute-picker/attribute-picker.component';
import { Routes, RouterModule } from "@angular/router";
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { APP_BASE_HREF } from '@angular/common';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
const routes: Routes = [
  { path: '', redirectTo: 'AppComponent', pathMatch: 'full' },
  { path: 'attributePicker', component: AttributePickerComponent },
  { path: '**', component: AppComponent }
];
@NgModule({
  declarations: [
    AppComponent,
    AttributePickerComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    DropDownsModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot(routes)
  ],
  providers: [{
    provide: APP_BASE_HREF, useValue: '/' + window.location.pathname.split('/')[1] + 'WebResources/cmc_/dist/AttributePicker'}],
  bootstrap: [AppComponent]
})
export class AppModule { }
