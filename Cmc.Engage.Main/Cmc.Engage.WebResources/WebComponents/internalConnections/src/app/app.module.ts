import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { InternalConnectionsComponent } from './components/internalConnections/internal-connections.component';
import { NetworkVisDirective } from './directives/network-vis.directive';
import { Routes, RouterModule } from "@angular/router";
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { APP_BASE_HREF } from '@angular/common';
import {InternalConnectionsService} from './services/internal-connections.service';
const routes: Routes = [
  { path: '', redirectTo: 'AppComponent', pathMatch: 'full' },
  { path: 'internalConnections', component: InternalConnectionsComponent },
  { path: '**', component: AppComponent }
];
@NgModule({
  declarations: [
    AppComponent,
    InternalConnectionsComponent,
    NetworkVisDirective
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forRoot(routes)
  ],
  providers: [InternalConnectionsService, {
    provide: APP_BASE_HREF, useValue: '/' + window.location.pathname.split('/')[1] + 'WebResources/cmc_/dist/InternalConnections'}],
  bootstrap: [AppComponent]
})
export class AppModule { }
