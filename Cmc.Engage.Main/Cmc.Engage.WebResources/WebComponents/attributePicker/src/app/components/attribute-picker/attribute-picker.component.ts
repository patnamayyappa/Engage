import { Component, OnInit, NgZone, ChangeDetectorRef, Input } from '@angular/core';
import { Metadata } from './../../models/metadata'
import { RelationshipType } from 'WebComponents/common/app-constants/app-constants.module';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
declare var SonomaCmc;
declare var CampusManagement;

@Component({
  selector: 'app-attribute-picker',
  styleUrls: ['./attribute-picker.component.less'],
  templateUrl: './attribute-picker.component.html'
})
export class AttributePickerComponent implements OnInit {

  metadatas: Array<Metadata> = [];
  numberOfTicks: number = 0;
  private existingSchemas = [];
  private entity: string = "";
  private entityDisplayName: string = "";
  private attributeJson: string = "";
  private nameField: string = "";
  private schemaField: string = "";
  private isFirstLoad: boolean = false;
  private showLabel: string = "true";
  private retrieveMetadataJsonAction: string = "";
  private retrieveMetadataJsonInput: string = "";
  private includeAttributeTypes: string = "";
  private excludeRelationships: boolean = false;
  private includeRelationshipTypes: string = "";
  @Input() attributePicker: any;
  constructor(private _ngZone: NgZone, private ref: ChangeDetectorRef) {
    setInterval(() => {
      this.numberOfTicks++;
      this.ref.markForCheck();
    }, 1000);
  }

  public valueChange(value: any): void {
    var metadatasLength = this.metadatas.length;

    // Handle an existing value being cleared
    //   Remove any dropdown after the cleared dropdown
    if (value === undefined) {
      if (metadatasLength === 1) {
        this.setValues(null, null);
        return;
      }

      var firstBlankIndex = 0;
      for (var i = 0; i < metadatasLength; i++) {
        var metadata = this.metadatas[i];
        if (metadata.selectedItem === undefined) {
          firstBlankIndex = i;
          break;
        }
      }

      this.metadatas.splice(firstBlankIndex + 1, metadatasLength - (firstBlankIndex + 1));
      this.setValues(null, null);
      return;
    }

    // Handle an existing value being updated to a new value
    //   Remove any dropdown after the updated dropdown
    var existingIndex = -1;
    for (var i = 0; i < metadatasLength; i++) {
      var metadata = this.metadatas[i];
      if (metadata.selectedItem.attribute === value.attribute) {
        existingIndex = i;
        break;
      }
    }
    if (existingIndex > -1 && metadatasLength > 1) {
      var itemsToRemove = metadatasLength - (existingIndex + 1);
      this.metadatas.splice(existingIndex + 1, itemsToRemove);
      metadatasLength = metadatasLength - itemsToRemove;
    }

    if (value.type.toLowerCase() === "relationship") {
      var listItems = [];
      this.getMetadata(value.entity);
    }
    else {
      var name = this.entityDisplayName + " > ";
      var schema = this.entity + ".";

      for (var i = 0; i < metadatasLength; i++) {
        var field = this.metadatas[i];
        if (field.selectedItem.type.toLowerCase() === "attribute") {
          schema += field.selectedItem.attribute;
          name += field.selectedItem.display;
        }
        else {
          schema += field.selectedItem.attribute + ".";
          if (field.selectedItem.relationshipType === "ManyToOneRelationship") {
            name += field.selectedItem.relatedAttributeDisplay + " > ";
          }
          else {
            name += field.selectedItem.entityDisplayName + " > ";
          }
        }
      }

      if (this.isFirstLoad === false) {
        this.setValues(name, schema);
      }
    }
  }

  ngOnInit() {
    this.isFirstLoad = true;

    this.entity = SonomaCmc.getQueryStringParams().entity;
    this.nameField = SonomaCmc.getQueryStringParams().namefield;
    this.schemaField = SonomaCmc.getQueryStringParams().schemafield;
    this.showLabel = SonomaCmc.getQueryStringParams().showlabel;
    this.retrieveMetadataJsonAction = SonomaCmc.getQueryStringParams().retrievemetadatajsonaction;
    this.retrieveMetadataJsonInput = SonomaCmc.getQueryStringParams().retrievemetadatajsoninput;
    this.excludeRelationships = SonomaCmc.getQueryStringParams().excluderelationships || false;
    this.includeAttributeTypes = SonomaCmc.getQueryStringParams().includeattributetypes || "";
    this.includeRelationshipTypes = SonomaCmc.getQueryStringParams().includerelationshiptypes || "";
    var schemaField = (<any>window).parent.Xrm.Page.getAttribute(this.schemaField);

    if (schemaField) {
      var existingSchemaValue = schemaField.getValue();
      if (existingSchemaValue) {
        this.existingSchemas = existingSchemaValue.split(".");
      }
    }

    this.initializeDisplayStrings();
  }

  setValues(name, schema) {
    // getAttribute on undefined returns an array of attributes
    if (this.nameField) {
      var nameField = (<any>window).parent.Xrm.Page.getAttribute(this.nameField);
      if (nameField) {
        nameField.setValue(name);
        nameField.fireOnChange();
        nameField.setSubmitMode('always');
      }
    }

    if (this.schemaField) {
      var schemaField = (<any>window).parent.Xrm.Page.getAttribute(this.schemaField);
      if (schemaField) {
        schemaField.setValue(schema);
        schemaField.fireOnChange();
        schemaField.setSubmitMode('always');
      }
    }
  }

  setMetadata(result) {
    var metadata = new Metadata();
    var selectedItem;

    var metadataObject = JSON.parse(result.AttributeJson);
    metadata.entityDisplayName = Object.keys(metadataObject)[0];

    if (!this.entityDisplayName)
      this.entityDisplayName = metadata.entityDisplayName;

    var attributes = metadataObject[metadata.entityDisplayName].attributes;
    var includeAttributeDataTypes = this.includeAttributeTypes.split(',');
    for (var key in attributes) {
      if (attributes.hasOwnProperty(key)) {
        var attributeInfo = attributes[key];
        var attributeDisplay = attributeInfo.attributeDisplayName;
        key = key;
        var attributeListItem = { entity: this.entity, entityDisplayName: metadata.entityDisplayName, attribute: key, display: attributeDisplay, type: "Attribute", attributeType: attributeInfo.attributeType };
        var index = this.existingSchemas.indexOf(key);
        // An attribute needs to be in the second position of the array in order to be selected
        if (index === 1) {
          selectedItem = attributeListItem;
          this.existingSchemas.splice(index, 1);
        }
        if (this.includeAttributeTypes === "" || this.includeAttributeTypes.includes(attributeListItem.attributeType))
          metadata.listItems.push(attributeListItem);
      }
    }
    if (!this.excludeRelationships) {
      var relationships = metadataObject[metadata.entityDisplayName].relationships;
      this.includeRelationshipTypes.split(',');
      for (var key in relationships) {
        if (relationships.hasOwnProperty(key)) {
          var entity = "",
            entityDisplayName = "",
            display = "",
            relatedAttributeDisplay = "";

          if ((this.includeRelationshipTypes === "" || this.includeRelationshipTypes.includes(RelationshipType.OneToManyRelationship.toString())) && relationships[key].type === "OneToManyRelationship") {
            entity = relationships[key].relatedEntity;
            entityDisplayName = relationships[key].relatedEntityDisplayName;
            display = entityDisplayName + " (" + relationships[key].lookupFieldDisplayName + ")";
            relatedAttributeDisplay = relationships[key].lookupFieldDisplayName;
          }
          else if ((this.includeRelationshipTypes === "" || this.includeRelationshipTypes.includes(RelationshipType.ManyToOneRelationship.toString())) && relationships[key].type === "ManyToOneRelationship") {
            entity = relationships[key].primaryEntity;
            entityDisplayName = relationships[key].primaryEntityDisplayName;
            display = entityDisplayName + " (" + relationships[key].lookupFieldDisplayName + ")";
            relatedAttributeDisplay = relationships[key].lookupFieldDisplayName;
          }
          else if (this.includeRelationshipTypes === "" || this.includeRelationshipTypes.includes(RelationshipType.ManyToManyRelationship.toString())) {
            console.log(relationships[key].type);
            entity = relationships[key].relatedEntity;
            entityDisplayName = relationships[key].relatedEntityDisplayName;
            display = entityDisplayName;
          }

          if (entity === "")
              continue;
          var relationshipListItem = { entity: entity, entityDisplayName: entityDisplayName, display: display, attribute: key, relatedAttributeDisplay: relatedAttributeDisplay, type: "Relationship", relationshipType: relationships[key].type };
          key = key;
          var index = this.existingSchemas.indexOf(key);
          if (index === 1 && !selectedItem) // when multilevel relation ships are selected , there could be chances of this code breaking hence adding the check
          {
            selectedItem = relationshipListItem;
            this.existingSchemas.splice(index, 1);
          }

          metadata.listItems.push(relationshipListItem);
        }
      }
    }

    metadata.listItems.sort(
      function (a, b) {
        var nameA = a.display.toLowerCase(), nameB = b.display.toLowerCase()
        if (nameA < nameB)
          return -1;
        if (nameA > nameB)
          return 1;
        return 0;
      });

    this.metadatas.push(metadata);

    if (selectedItem) {
      metadata.selectedItem = selectedItem;
      if (selectedItem.type === "Relationship")
        this.valueChange.call(this, metadata.selectedItem);
      else {
        this.isFirstLoad = false;
      }
    }
    else {
      this.isFirstLoad = false;
    }

    (<any>window).parent.Xrm.Utility.closeProgressIndicator();
  }

  multiLingualAlert(message, error) {

    (<any>window).parent.Xrm.Navigation.openAlertDialog({
      text: message + error,
      confirmButtonLabel: this.attributePicker.okButton
    },
      null);
  }
  initializeDisplayStrings() {
    let rwindow: any = window;
    this.attributePicker = {
      errorPrefix: rwindow.CampusManagement.localization.getResourceString("ErrorPrefix"),
      okButton: rwindow.CampusManagement.localization.getResourceString("OkButton"),
      loadingText: rwindow.CampusManagement.localization.getResourceString("Ribbon_Loading"),
      attributePicker_Label: rwindow.CampusManagement.localization.getResourceString("AttributePicker_Label")
    }
    this.getMetadata(this.entity);
  }



  getMetadata(entity) {
    (<any>window).parent.Xrm.Utility.showProgressIndicator(this.attributePicker.loadingText);

    var control = this;
    this._ngZone.run(() => {
      var input = JSON.parse(this.retrieveMetadataJsonInput);
      if (input.EntityLogicalNames) {
        input.EntityLogicalNames = entity;
      }
      else if (input.EntityLogicalName) {
        input.EntityLogicalName = entity;
      }

      SonomaCmc.WebAPI.post(this.retrieveMetadataJsonAction, input)
        .then(this.setMetadata.bind(this),
          function (error) {
            control.multiLingualAlert(control.attributePicker.errorPrefix, error.message);
            console.log(error);
          });
    });
  }
}
