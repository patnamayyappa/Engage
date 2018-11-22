(function(global) {
      'use strict';

      global.StudentPortal = global.StudentPortal || {};

      global.StudentPortal.Profile = (function(result) {
        function onLoad() {
            var $profileImage = $('img[src="/xrm-adx/images/contact_photo.png"]');
            $profileImage.attr('src', '/defaultprofileimage.png').width(62).height(62)

            $.ajax('ProfileRetrieveContactImage?stopCache=' + (+new Date()))
                .done(function(result) {
                    if (result && result.ContactImage) {
                        $profileImage.attr('src',
                            'data:image/png;base64,' + result.ContactImage
                        );
                    }
                });
        }

        $(document).ready(onLoad);

        return {};
      })();
    }(this));