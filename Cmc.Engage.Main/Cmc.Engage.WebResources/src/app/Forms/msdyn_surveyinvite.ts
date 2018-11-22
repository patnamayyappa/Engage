/// <reference path="../common/utilities.ts" />

module CampusManagement.msdyn_surveyinvite {
    export function onLoad(executionContext) {
        if (utilities && utilities.setActivityToFieldForContactOnLoad)
            utilities.setActivityToFieldForContactOnLoad(executionContext);
    }
}