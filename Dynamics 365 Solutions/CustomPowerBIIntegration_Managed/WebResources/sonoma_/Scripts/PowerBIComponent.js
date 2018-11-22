var Xrm = window.Xrm || parent.window.Xrm;
var PowerBIReport = (function (global) {
    "use strict";

    var _powerBiApiUrl = "https://api.powerbi.com/v1.0/myorg/";
    
    (function() {
        var componentDetails = _parseQueryString();

        if (!_isValidComponentType(componentDetails["type"])) {
            throw "Invalid component type.";
        }

        if (!_validParameters(componentDetails)) {
            throw "Invalid parameters.";
        }

        _getAccessToken(_renderComponent, _normalizeComponentDetails(componentDetails));
    }());

    function _parseQueryString() {
        var params = {}
        if (global.location.search !== "") {
            params = global.location.search.substr(1).split("&");
            for (var i in params) {
                params[i] = params[i].replace(/\+/g, " ").split("=");
            }

            for (var i in params) {
                if (params[i][0].toLowerCase() === "data") {
                    return _parseDataValue(params[i][1]);
                }
            }
        }
    }

    function _parseDataValue(dataValue) {
        var dataObj = {};

        if (dataValue !== "") {
            var vals = decodeURIComponent(decodeURIComponent(dataValue));
            vals = vals.split("&");

            for (var i in vals) {
                vals[i] = vals[i].replace(/\+/g, " ").split("=");

                var param = vals[i][0].toLowerCase();
                var value = vals[i][1];
                dataObj[param] = value;
            }
        }

        return dataObj;
    }

    function _getAccessToken(callback, componentDetails) {
        $.ajax({
            url:
                Xrm.Page.context.getClientUrl() +
                "/api/data/v8.2/" +
                "sonoma_RetrieveAccessToken",
            headers: {
                Accept: "Application/json",
                "Content-Type": "application/json; charset=utf-8",
                "OData-MaxVersion": "4.0",
                "OData-Version": "4.0"
            },
            type: "POST"
      }).done(function(result) {
            var accessToken = JSON.parse(result.AccessToken);
            if (accessToken.Error) {
                alert(accessToken.Error);
                return;
            }

            callback(accessToken.AccessToken, componentDetails);
        }).catch(function(error) {
            alert(JSON.stringify(error));
        });
    }

    function _normalizeComponentDetails(componentDetails) {
        var type = componentDetails["type"];

        var isDashboardReport = false;
        if (type === "dashboardreport") {
            isDashboardReport = true;
            type = "report";
        }

        var componentId = componentDetails[type + 'id']; 
        var groupId = componentDetails['groupid'];

        var normalized = {
            Action: _getActionByComponentType(type)
        };

        if (type === 'report') {
            normalized.Url = "https://app.powerbi.com/reportEmbed?reportId=" + componentId;
        } else if (type === 'dashboard') {
            normalized.Url = "https://app.powerbi.com/dashboardEmbed?dashboardId=" + componentId;
        } else if (type === 'tile') {
            normalized.Url = "https://app.powerbi.com/embed"
                + "?dashboardId=" + componentDetails["dashboardid"]
                + "&tileId=" + componentId;     
        } else if (isDashboardReport) {
            normalized.Url = "https://app.powerbi.com/embed"
                + "?dashboardId=" + componentDetails["dashboardid"]
                + "&tileId=" + componentId;
        }

        if (groupId) {
            normalized.Url += ("&groupId=" + groupId);
        }

        return normalized;
    }

    function _validParameters(params) {
        if (params['type'] === 'report' && !params['reportid']) { return false; }
        if (params['type'] === 'dashboard' && !params['dashboardid']) { return false; }
        if (params['type'] === 'tile' && !params['dashboardid'] && !parmas['tileid']) { return false; }
        return true;
    }

    function _isValidComponentType(type) {
        return [
            'dashboard', 
            'report', 
            'tile'
        ].indexOf(type) >= 0;
    }

    function _getActionByComponentType(type) {
        if (!type || !_isValidComponentType(type)) {
            return '';
        }

        return {
            'dashboard': 'loadDashboard',
            'report': 'loadReport',
            'tile': 'loadTile'
        }[type.toLowerCase()];
    }

    function _renderComponent(accessToken, normalizedComponentDetails) { 
        var iframe = document.getElementById("powerbicomponent");
        iframe.src = normalizedComponentDetails.Url;
        iframe.height = iframe.parentElement.scrollHeight;
        iframe.width = iframe.parentElement.scrollWidth;

        iframe.onload = function() {
            var message = JSON.stringify({
                action: normalizedComponentDetails.Action,
                accessToken: accessToken,
                width: this.scrollWidth,
                height: this.scrollHeight
            });

            this.contentWindow.postMessage(message, "*");
        };
    }
}(this));