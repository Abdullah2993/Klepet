String.format = function() {
    var theString = arguments[0];
    for (var i = 1; i < arguments.length; i++) {
        var regEx = new RegExp("\\{" + (i - 1) + "\\}", "gm");
        theString = theString.replace(regEx, arguments[i]);
    }
    return theString;
};
var hub = $.connection.chatHub;

$.extend(hub.client, {
    receiveMessage: function (connId, message) {
        var dt = new Date();
        var time = dt.getHours() + ":" + dt.getMinutes();
        $("#msgs").append(String.format('<div class="media msg"><div class="media-body"><small class="pull-right time"><i class="fa fa-clock-o"></i>{0}</small><h5 class="media-heading">{1}</h5><small class="col-lg-10">{2}</small></div></div>',time, connId, message));
    },

    clientConnected: function(id) {
        $("#users").append(String.format('<div id="{0}" class="media conversation"><div class="media-body"><h5 class="media-heading">{0}</h5><small>Hello</small></div></div>', id));
    },
    clientDisconnected: function(connId) {
        if ($("#" + connId).length) {
            $("#" + connId).remove();
        }
    }

});

$("#msgIn").keypress(function (e) {
    if ($("#msgIn").val().length>0 && e.which == 13) {
        hub.invoke("sendMessage", $("#msgIn").val());
        hub.client.receiveMessage($.connection.hub.id, $("#msgIn").val());
        $("#msgIn").val("");

        return false;
    }
});

window.onbeforeunload = function (e) {
    $.connection.hub.stop();
};

$.connection.hub.start();