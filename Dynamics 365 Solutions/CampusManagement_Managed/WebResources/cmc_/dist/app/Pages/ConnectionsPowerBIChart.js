/// <reference path="../../../node_modules/@types/jquery/index.d.ts" />
var PowerBIReport;
(function (PowerBIReport) {
    (function () {
        reportDetails = {};
        parseQueryString();
        getReport();
    }());
    function parseQueryString() {
        var params = [];
        if (window.location.search !== "") {
            params = window.location.search.substr(1).split("&");
            for (var i in params) {
                params[i] = params[i].replace(/\+/g, " ").split("=");
                if (params[i][0].toLowerCase() === "data") {
                    parseDataValue(params[i][1]);
                }
                else if (params[i][0].toLowerCase() === "id") {
                    reportDetails["id"] = params[i][1];
                }
            }
        }
    }
    function parseDataValue(dataValue) {
        if (dataValue !== "") {
            var vals = decodeURIComponent(decodeURIComponent(dataValue)).split("&");
            for (var i in vals) {
                var val = vals[i], params = val.replace(/\+/g, " ").split("=");
                var param = params[0].toLowerCase();
                var value = params[1];
                reportDetails[param] = value;
            }
        }
    }
    function getReport() {
        // commented this as we are not using powerbi solution anymore.
        //$.ajax({
        //    url: Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v8.2/" + "sonoma_RetrieveAccessToken",
        //    headers: {
        //        'Accept': 'Application/json',
        //        'Content-Type': 'application/json; charset=utf-8',
        //        'OData-MaxVersion': '4.0',
        //        'OData-Version': '4.0'
        //    },
        //    type: "POST"
        //}).then(
        //    function (result) {
        //        var accessToken = JSON.parse(result.AccessToken);
        //        if (accessToken.Error) {
        //            Xrm.Navigation.openAlertDialog({
        //                text: accessToken.Error
        //            }, null);
        //            return;
        //        } else {
        //            loadReport(accessToken.AccessToken, reportDetails.reportid,
        //                reportDetails.id);
        //        }
        //    },
        //    function (error) {
        //        // A Status of 0 means that the request was canceled
        //        if (error && error.status === 0) {
        //            return;
        //        }
        //        Xrm.Navigation.openAlertDialog({
        //            text: JSON.stringify(error)
        //        }, null);
        //    });
    }
    function loadReport(accessToken, reportId, id) {
        var component = $('#report')[0], models = window['powerbi-client'].models, config = {
            type: 'report',
            tokenType: models.TokenType.Aad,
            accessToken: accessToken,
            embedUrl: 'https://app.powerbi.com/reportEmbed',
            id: reportId,
            settings: buildSettings(models)
        }, report = powerbi.embed(component, config);
        report.on('loaded', function (event) {
            report.getFilters()
                .then(function (filters) {
                filters.push({
                    $schema: "http://powerbi.com/product/schema#basic",
                    target: {
                        table: 'Connections',
                        column: 'Connected FromId'
                    },
                    operator: 'In',
                    values: [id && id.replace(/(%7b)|(%7d)/gi, '')]
                });
                return report.setFilters(filters);
            });
            $(window).resize(function () {
                report.updateSettings(buildSettings(models));
            });
        });
    }
    function buildSettings(models) {
        var $report = $('#report'), width = $report.width() - 1, height = $report.height() - 20, settings = {
            filterPaneEnabled: false,
            navContentPaneEnabled: false,
            layoutType: models.LayoutType.Custom,
            customLayout: {
                displayOptions: models.DisplayOption.FitToWidth,
                pageSize: {
                    type: models.PageSizeType.Custom,
                    width: width,
                    height: height
                },
                pagesLayout: {
                    'ReportSection': {
                        defaultLayout: {
                            displayState: {
                                mode: models.VisualContainerDisplayMode.Visible
                            },
                            width: width,
                            height: height
                        },
                        visualsLayout: {}
                    }
                }
            }
        };
        return settings;
    }
})(PowerBIReport || (PowerBIReport = {}));
