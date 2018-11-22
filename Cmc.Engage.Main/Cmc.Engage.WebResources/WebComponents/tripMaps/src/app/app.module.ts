import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { TripMapsComponent } from './components/trip-maps/trip-maps.component';
import { Routes, RouterModule } from "@angular/router";
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { APP_BASE_HREF } from '@angular/common';
import { TripMapsService } from './services/trip-maps.service';
import { AppConstantsModule } from 'WebComponents/common/app-constants/app-constants.module';
const routes: Routes = [
  { path: '', redirectTo: 'AppComponent', pathMatch: 'full' },
  { path: 'tripMaps', component: TripMapsComponent },
  { path: '**', component: AppComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    TripMapsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppConstantsModule,
    RouterModule.forRoot(routes)
  ],
  providers: [TripMapsService,{
    provide: APP_BASE_HREF, useValue: '/' + window.location.pathname.split('/')[1] + 'WebResources/cmc_/dist/TripMaps'}],
  bootstrap: [AppComponent]
})
export class AppModule { }
