var $ = require("gulp-load-plugins")({ lazy: true });

/*
* @desc Log the messages
 */
module.exports = {
    log: function(msg) {
        if (typeof (msg) === "object") {
            for (var item in msg) {
                if (msg.hasOwnProperty(item)) {
                    $.util.log($.util.colors.blue(msg[item]));
                }
            }
        } else {
            $.util.log($.util.colors.blue(msg));
        }
    },

    info: function(msg, taskName) {
        var message = "TaskName:" + taskName + ", Information: " + msg.toString();
        $.util.log($.util.colors.blue(message));

    },

    error: function(msg, taskName) {
        var message = "TaskName:" + taskName + ", Error: " + msg.toString();
        $.util.log($.util.colors.red(message));
    },
    warning: function(msg, taskName) {
        var message = "TaskName:" + taskName + ", Warning: " + msg.toString();
        $.util.log($.util.colors.yellow(message));

    }


};