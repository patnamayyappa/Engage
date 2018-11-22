import { Injectable } from '@angular/core';

@Injectable()
export class UtilityService {

  constructor() { }

  getDefaultTheme() {
    let rwindow: any = window, that = this;
    return rwindow.CampusManagement.themes.getDefaultTheme();
  }
}
