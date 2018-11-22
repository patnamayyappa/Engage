/// <reference path="../../../node_modules/@types/xrm/index.d.ts" />
var CMC;
(function (CMC) {
    var Engage;
    (function (Engage) {
        var cmc_positiveacknowledgement;
        (function (cmc_positiveacknowledgement) {
            var _optionSetCache = {};
            var cmc_acknowledgementreason;
            (function (cmc_acknowledgementreason) {
                cmc_acknowledgementreason[cmc_acknowledgementreason["Academic"] = 175490000] = "Academic";
                cmc_acknowledgementreason[cmc_acknowledgementreason["Financial"] = 175490001] = "Financial";
                cmc_acknowledgementreason[cmc_acknowledgementreason["Social"] = 175490002] = "Social";
            })(cmc_acknowledgementreason = cmc_positiveacknowledgement.cmc_acknowledgementreason || (cmc_positiveacknowledgement.cmc_acknowledgementreason = {}));
            ;
            var cmc_acknowledgementreasondetail;
            (function (cmc_acknowledgementreasondetail) {
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["ImprovedAttendance"] = 175490000] = "ImprovedAttendance";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["GradeImprovements"] = 175490001] = "GradeImprovements";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["ImprovementInAssignedWork"] = 175490002] = "ImprovementInAssignedWork";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["FAFSATurnedIn"] = 175490003] = "FAFSATurnedIn";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["DepositPaidOnTime"] = 175490004] = "DepositPaidOnTime";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["FoundPartTimeJob"] = 175490005] = "FoundPartTimeJob";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["AttendedFirstEvent"] = 175490006] = "AttendedFirstEvent";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["JoinedAClub"] = 175490007] = "JoinedAClub";
                cmc_acknowledgementreasondetail[cmc_acknowledgementreasondetail["RanForLeadershipPosition"] = 175490008] = "RanForLeadershipPosition";
            })(cmc_acknowledgementreasondetail = cmc_positiveacknowledgement.cmc_acknowledgementreasondetail || (cmc_positiveacknowledgement.cmc_acknowledgementreasondetail = {}));
            ;
            function onLoad(executionContext) {
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
            cmc_positiveacknowledgement.onLoad = onLoad;
            function buildOptionSetCache(optionSets) {
                optionSets.forEach(function (option) { return _optionSetCache[option.value] = option; });
            }
            function clearAcknowledgementReasonDetailOptions(acknowledgementReasonDetail) {
                Object.keys(cmc_acknowledgementreasondetail)
                    .filter(function (key) { return typeof cmc_acknowledgementreasondetail[key] === 'number'; })
                    .map(function (key) { return acknowledgementReasonDetail.removeOption(cmc_acknowledgementreasondetail[key]); });
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
                }
                ;
                addAcknowledgementReasonDetailLookupFilters(executionContext, values);
            }
            function addAcknowledgementReasonDetailLookupFilters(executionContext, values) {
                var formContext = executionContext.getFormContext();
                var acknowledgementReasonDetail = formContext.getControl('cmc_acknowledgementreasondetail');
                var acknowledgmentReasonDetailAttribute = acknowledgementReasonDetail.getAttribute();
                var currentValue = acknowledgmentReasonDetailAttribute.getValue();
                if (values) {
                    values.map(function (value) { return acknowledgementReasonDetail.addOption(_optionSetCache[value]); });
                    if (values.indexOf(currentValue) !== -1) {
                        acknowledgmentReasonDetailAttribute.setValue(currentValue);
                    }
                    else {
                        acknowledgmentReasonDetailAttribute.setValue(null);
                        acknowledgmentReasonDetailAttribute.fireOnChange();
                    }
                }
            }
        })(cmc_positiveacknowledgement = Engage.cmc_positiveacknowledgement || (Engage.cmc_positiveacknowledgement = {}));
    })(Engage = CMC.Engage || (CMC.Engage = {}));
})(CMC || (CMC = {}));
