(function () {
    "use strict";

    window.CampusManagement = window.CampusManagement || {};
    window.CampusManagement.global = (function () {

        document.addEventListener('DOMContentLoaded', function() {
            setUnreadToDos();
        });
        
        function setUnreadToDos() {
            var $toDos = $('.to-dos'),
                pathName = window.location.pathname || '';

            if (!$toDos.length || pathName.toLowerCase().replace(/\//gi, '') === 'to-dos') {
                return;
            }
            
            $.ajax('/retrievenumberofunreadtodos/?stopCache=' + (+new Date()))
                .then(function(result) {
                    if (!result || isNaN(result) || !parseInt(result)) {
                        return;
                    }

                    $toDos.closest('a').append('<span class="badge">' + result.trim() + '</span>');
                });
        }

    }());
}());