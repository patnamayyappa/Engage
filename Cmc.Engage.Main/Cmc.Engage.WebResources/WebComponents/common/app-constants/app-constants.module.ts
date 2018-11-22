import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: []
})
export class AppConstantsModule { 

  public readonly oDataApiVersion: string = "v9.0";
}


export enum QuestionTypes {
  none=0,
  text = 1,
  singleSelect = 2,
  multiSelect = 3,
  boolean = 4
}

export enum RelationshipType {
  OneToManyRelationship = 1,
  ManyToOneRelationship = 2,
  ManyToManyRelationship = 3

}
