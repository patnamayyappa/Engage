/// <reference path="../Libraries/sonomaCmc.debug.js" />

(function () {
    "use strict";

    window.CampusManagement = window.CampusManagement || {};
    window.CampusManagement.retrieveMultiLingualValues = window.CampusManagement.retrieveMultiLingualValues || {};
    window.CampusManagement.retrieveMultiLingualValues = (function () {

        function get(keys) {
            //todo: could add caching (local/session storage) and check there first, still returning promise.
            if (!(keys instanceof Array)) {
                keys = [keys];
            }

            var input = JSON.stringify(keys);

            return SonomaCmc.WebAPI.post("cmc_RetrieveMultiLingualValues", { Keys: input })
                .then(function (result) {
                    return JSON.parse(result.Output);
                });
        }

        return {
            get: get
        };
    }());
}());