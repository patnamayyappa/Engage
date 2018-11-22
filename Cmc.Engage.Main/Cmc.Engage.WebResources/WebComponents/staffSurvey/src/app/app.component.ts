import { Component, Input } from '@angular/core';
import { Router, ActivatedRoute, Params, RoutesRecognized } from '@angular/router';
declare var SonomaCmc;
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  inputParameter: string;
  constructor(private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.prepareRouteNavigation();
  }
  ngAfterViewInit() {
    document.getElementById('loadingOverlay').style.display = 'none';
  }
  prepareRouteNavigation() {
    let that = this, rwindow: any = window;;
    let queryparameters = rwindow.GetGlobalContext().getQueryStringParameters();
    let customParameters: any = queryparameters.data ? this.parseData(queryparameters.data) : SonomaCmc.getQueryStringParams().data;
    this.router.navigate(['/' + customParameters.component],{ queryParams: customParameters });
  }

  parseData(query) {
    var result = {};
    if (typeof query == "undefined" || query == null) {
      return result;
    }
    var queryparts = query.split("&");
    for (var i = 0; i < queryparts.length; i++) {
      var params = queryparts[i].split("=");
      result[params[0]] = params.length > 1 ? params[1] : null;
    }
    return result;
  }

}


