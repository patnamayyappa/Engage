import { Component, OnInit, Input } from '@angular/core';
import { EntityPickerService } from './../../services/entity-picker.service'
declare var SonomaCmc;

@Component({
  selector: 'app-entity-picker',
  templateUrl: './entity-picker.component.html',
  styleUrls: ['./entity-picker.component.less']
})
export class EntityPickerComponent implements OnInit {

  entityList: Array<{ ObjectTypeCode: number, entity: string, entityLabel: string }> = [];
  selectedItem: any;
  disabled: boolean = false;
  controlHeight: string;
  resizeControl: boolean=false;
  controlName: string="";
  controlId:string="";
  showLabel: string = "true";
  @Input() entityPicker: any;
  private nameField: string = "";
  private existingSchemaValue: string = "";
  
  constructor(private entityPickerService: EntityPickerService) { }

  ngOnInit() {
    console.log("In EntityPicker");
    this.entityPickerService.getEntityList().subscribe(this.setEntityList.bind(this));
    this.nameField = SonomaCmc.getQueryStringParams().namefield;
    this.controlName = SonomaCmc.getQueryStringParams().controlname || "";
    this.controlId = SonomaCmc.getQueryStringParams().controlid || "";
    this.resizeControl = SonomaCmc.getQueryStringParams().resizecontrol || false;
    var schemaField = (<any>window).parent.Xrm.Page.getAttribute(this.nameField);
    this.disabled = SonomaCmc.getQueryStringParams().disabled || false;
    if (schemaField) {
      var Value = schemaField.getValue();
      if (Value) {
        this.existingSchemaValue = Value;
      }
    }
    this.initializeDisplayStrings();
  }

  setEntityList(result) {
    if (result && result[0].value) {
      var control = this;
      result[0].value.forEach(function (entity) {
        control.entityList.push({
          ObjectTypeCode: entity.ObjectTypeCode,
          entity: entity.LogicalName,
          entityLabel: entity.DisplayName.UserLocalizedLabel.Label
        });
      });
      if (control.existingSchemaValue)
        control.selectedItem = control.entityList.find(x => x.entity === this.existingSchemaValue);
    }
  }

  setValues(item) {
    var schemaField = (<any>window).parent.Xrm.Page.getAttribute(this.nameField);
    if (item === undefined) {
      schemaField.setValue(null);
      schemaField.fireOnChange();
    }
    else if (item) {
      schemaField.setValue(item.entity);
      schemaField.fireOnChange();
    }
  }
  initializeDisplayStrings() {
    let rwindow: any = window;
    this.entityPicker = {
      errorPrefix: rwindow.CampusManagement.localization.getResourceString("ErrorPrefix"),
      okButton: rwindow.CampusManagement.localization.getResourceString("OkButton"),
      entityPicker_Label: rwindow.CampusManagement.localization.getResourceString("EntityPicker_Label")
    }
    console.log(this.entityPicker.entityPicker_Label);
  }
  onPopUpOpen() {
    if (this.resizeControl ) {
      var containerId = (<any>window).parent.document.getElementById(this.controlId), control = (<any>window).parent.Xrm.Page.getControl(this.controlName);
      
      if (containerId && this.controlId !== "")
        containerId.style.height = "auto";
      if (control && this.controlName !== "") {
        this.controlHeight = control.getObject().style.height;
        control.getObject().style.height = "auto";
      }
    }
  }

  onPopUpClose() {
    if (this.resizeControl && this.controlName !== "") {
      var control = (<any>window).parent.Xrm.Page.getControl(this.controlName);
      if (control)
        control.getObject().style.height = this.controlHeight;
    }
  }
}
