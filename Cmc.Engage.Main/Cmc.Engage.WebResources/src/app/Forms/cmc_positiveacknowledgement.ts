/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
module CMC.Engage.cmc_positiveacknowledgement {

    var _optionSetCache = {}

    export enum cmc_acknowledgementreason {
        Academic = 175490000,
        Financial = 175490001,
        Social = 175490002
    };

    export enum cmc_acknowledgementreasondetail {
        ImprovedAttendance = 175490000,
        GradeImprovements = 175490001,
        ImprovementInAssignedWork = 175490002,
        FAFSATurnedIn = 175490003,
        DepositPaidOnTime = 175490004,
        FoundPartTimeJob = 175490005,
        AttendedFirstEvent = 175490006,
        JoinedAClub = 175490007,
        RanForLeadershipPosition = 175490008,
    };

    export function onLoad(executionContext) {
        var formContext = executionContext.getFormContext();
        var acknowledgementReason = formContext.getAttribute('cmc_acknowledgmentreason');
        var acknowledgementReasonDetail = formContext.getControl('cmc_acknowledgementreasondetail');

        if (!acknowledgementReason || !acknowledgementReasonDetail) {
            return;
        }

        buildOptionSetCache(acknowledgementReasonDetail.getOptions());
        clearAcknowledgementReasonDetailOptions(acknowledgementReasonDetail);

        acknowledgementReason.addOnChange(setAcknowledgementReasonDetailFilter);
        setAcknowledgementReasonDetailFilter(executionContext);
    }

    function buildOptionSetCache(optionSets) {
        optionSets.forEach(option => _optionSetCache[option.value] = option);
    }

    function clearAcknowledgementReasonDetailOptions(acknowledgementReasonDetail) {
        Object.keys(cmc_acknowledgementreasondetail)
            .filter(key => typeof cmc_acknowledgementreasondetail[key] === 'number')
            .map(key => acknowledgementReasonDetail.removeOption(cmc_acknowledgementreasondetail[key]))
    }

    function setAcknowledgementReasonDetailFilter(executionContext) {
        var formContext = executionContext.getFormContext();
        var acknowledgementReason = formContext.getAttribute('cmc_acknowledgmentreason');
        var acknowledgementReasonDetail = formContext.getControl('cmc_acknowledgementreasondetail');

        clearAcknowledgementReasonDetailOptions(acknowledgementReasonDetail);

        var values = [];

        if (!acknowledgementReason) {
            values = null;
        }
        else if (acknowledgementReason.getValue() == cmc_acknowledgementreason.Academic) {
            values = [
                cmc_acknowledgementreasondetail.ImprovedAttendance,
                cmc_acknowledgementreasondetail.GradeImprovements,
                cmc_acknowledgementreasondetail.ImprovementInAssignedWork
            ];
        }
        else if (acknowledgementReason.getValue() == cmc_acknowledgementreason.Financial) {
            values = [
                cmc_acknowledgementreasondetail.FAFSATurnedIn,
                cmc_acknowledgementreasondetail.DepositPaidOnTime,
                cmc_acknowledgementreasondetail.FoundPartTimeJob
            ];

        }
        else if (acknowledgementReason.getValue() == cmc_acknowledgementreason.Social) {
            values = [
                cmc_acknowledgementreasondetail.AttendedFirstEvent,
                cmc_acknowledgementreasondetail.JoinedAClub,
                cmc_acknowledgementreasondetail.RanForLeadershipPosition
            ];
        };
        
        addAcknowledgementReasonDetailLookupFilters(executionContext, values);
    }

    function addAcknowledgementReasonDetailLookupFilters(executionContext, values) {
        var formContext = executionContext.getFormContext();
        var acknowledgementReasonDetail = formContext.getControl('cmc_acknowledgementreasondetail');
        var acknowledgmentReasonDetailAttribute = acknowledgementReasonDetail.getAttribute();
        var currentValue = acknowledgmentReasonDetailAttribute.getValue();

        if (values) {
            values.map(value => acknowledgementReasonDetail.addOption(_optionSetCache[value]));

            if (values.indexOf(currentValue) !== -1) {
                acknowledgmentReasonDetailAttribute.setValue(currentValue);
            }
            else {
                acknowledgmentReasonDetailAttribute.setValue(null);
                acknowledgmentReasonDetailAttribute.fireOnChange();
            }
        }
    }
}