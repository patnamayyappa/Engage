module CampusManagement.retrieveMultiLingualValues {
    declare var SonomaCmc: any;

    export function get(keys) {
        var webApi = SonomaCmc.WebAPI;

        //todo: could add caching (local/session storage) and check there first, still returning promise.
        if (!(keys instanceof Array)) {
            keys = [keys];
        }

        const input = JSON.stringify(keys); 

        return webApi.post("cmc_RetrieveMultiLingualValues", { Keys: input })
            .then(result => JSON.parse(result.Output));
    }
}