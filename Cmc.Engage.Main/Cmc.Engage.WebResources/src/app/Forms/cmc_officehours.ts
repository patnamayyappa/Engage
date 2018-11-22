/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CampusManagement.cmc_officehours {

    var _invalidDurationErrorId = "invalid_duration";
    var _invalidDateErrorId = "invalid_date";
    var _invalidTimeErrorId = "invalid_time";


    export function onLoad(executionContext) {
        var formContext = executionContext.getFormContext(),
            durationAttr = formContext.getAttribute("cmc_duration"),
            enddateAttr = formContext.getAttribute("cmc_enddate"),
            startdateAttr = formContext.getAttribute("cmc_startdate"),
            endtimeAttr = formContext.getAttribute("cmc_endtime"),
            starttimeAttr = formContext.getAttribute("cmc_starttime");

        if (durationAttr) {
            durationAttr.addOnChange(checkForValidDuration);
        }

        if (enddateAttr && startdateAttr) {
            enddateAttr.addOnChange(dateIntervalIsValid);
            startdateAttr.addOnChange(dateIntervalIsValid);
        }

        if (endtimeAttr && starttimeAttr) {
            if (formContext.ui.getFormType() === XrmEnum.FormType.Create) {
                starttimeAttr.setValue(new Date(9999, 11, 30, 9, 0, 0));
                endtimeAttr.setValue(new Date(9999, 11, 30, 9, 0, 0));

                formContext.data.entity.addOnSave(timeIntervalIsValid);
            }

            endtimeAttr.addOnChange(timeIntervalIsValid);
            starttimeAttr.addOnChange(timeIntervalIsValid);
        }
    }

    function checkForValidDuration(executionContext) {
        var formContext = executionContext.getFormContext(),
            duration = formContext.getAttribute("cmc_duration").getValue(),
            isDurationValid;

        if (!duration) {
            return;
        }

        isDurationValid = duration % 0.25 !== 0;
        toggleDurationNotification(isDurationValid, formContext);
    }

    function toggleDurationNotification(enable, formContext) {
        var durationControls = formContext.getAttribute("cmc_duration").controls;

        durationControls.forEach(function (control) {
            if (enable) {
                control.setNotification(CampusManagement.localization.getResourceString("QuaterHoursError"), "ERROR", _invalidDurationErrorId);
            }
            else {
                control.clearNotification();
            }
        });
    }

    function dateIntervalIsValid(executionContext) {
        var formContext = executionContext.getFormContext(),
            enddate = formContext.getAttribute("cmc_enddate").getValue(),
            startdate = formContext.getAttribute("cmc_startdate").getValue(),
            dateDifference;

        if (!enddate || !startdate) {
            return;
        }

        dateDifference = enddate.valueOf() - startdate.valueOf();

        var isDateIntervalValid = dateDifference > 0;
        toggleDateNotification(isDateIntervalValid, formContext);
    }

    function toggleDateNotification(enable, formContext) {
        var enddateControls = formContext.getAttribute("cmc_enddate").controls;

        enddateControls.forEach(function (control) {
            if (!enable) {
                control.setNotification(CampusManagement.localization.getResourceString("StartDateGreaterthanEndDateError"), "ERROR", _invalidDateErrorId);
            }
            else {
                control.clearNotification();
            }
        });
    }

    function timeIntervalIsValid(executionContext) {
        var formContext = executionContext.getFormContext(),
            endtime = formContext.getAttribute("cmc_endtime").getValue(),
            starttime = formContext.getAttribute("cmc_starttime").getValue(),
            dateDifference;

        if (!endtime || !starttime) {
            return;
        }

        dateDifference = endtime.valueOf() - starttime.valueOf();

        var isTimeIntervalValid = dateDifference > 0;
        toggleTimeNotification(isTimeIntervalValid, formContext);
    }

    function toggleTimeNotification(enable, formContext) {
        var endtimeControls = formContext.getAttribute("cmc_endtime").controls;

        endtimeControls.forEach(function (control) {
            if (!enable) {
                control.setNotification(CampusManagement.localization.getResourceString("StartTimeGreaterthanEndTimeError"), "ERROR", _invalidTimeErrorId);
            }
            else {
                control.clearNotification();
            }
        });
    }
}