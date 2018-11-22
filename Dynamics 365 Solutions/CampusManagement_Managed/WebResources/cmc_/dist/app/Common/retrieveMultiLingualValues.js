var CampusManagement;
(function (CampusManagement) {
    var retrieveMultiLingualValues;
    (function (retrieveMultiLingualValues) {
        function get(keys) {
            var webApi = SonomaCmc.WebAPI;
            //todo: could add caching (local/session storage) and check there first, still returning promise.
            if (!(keys instanceof Array)) {
                keys = [keys];
            }
            var input = JSON.stringify(keys);
            return webApi.post("cmc_RetrieveMultiLingualValues", { Keys: input })
                .then(function (result) { return JSON.parse(result.Output); });
        }
        retrieveMultiLingualValues.get = get;
    })(retrieveMultiLingualValues = CampusManagement.retrieveMultiLingualValues || (CampusManagement.retrieveMultiLingualValues = {}));
})(CampusManagement || (CampusManagement = {}));
