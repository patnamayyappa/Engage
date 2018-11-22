(function () {
    "use strict";

    window.CampusManagement = window.CampusManagement || {};
    window.CampusManagement.retrieveMultiLingualValues = window.CampusManagement.retrieveMultiLingualValues || {};
    window.CampusManagement.retrieveMultiLingualValues = (function () {

        function get(keys) {
            if (!(keys instanceof Array)) {
                keys = [keys];
            }

            var serializedKeys = null;
            keys.forEach(function (key) {
                if(serializedKeys){
                  serializedKeys += "~|~" + key;
                }
                else{
                  serializedKeys = key;
                }
            });

            return $.ajax({
                url: "/RetrieveMultiLingualValues?keys=" + encodeURI(serializedKeys) + "&stopCache=" + new Date(),
                dataType: "json",
                cache: false
            }).then(function (res) {
                return res;
            });
        }

        return {
            get: get
        };
    }());
}());