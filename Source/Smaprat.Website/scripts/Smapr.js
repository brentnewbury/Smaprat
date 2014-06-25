/// <reference path="knockout-3.1.0.js" />
/// <reference path="jquery-2.1.1.js" />
/// <reference path="Autolinker.min.js" />
/// <reference path="jquery.signalR-2.0.3.js" />

var autolinker = new Autolinker({ stripPrefix: false });

var viewModel = null;

function scrollToMessage() {
    $("#messages").prop({ scrollTop: $("#messages").prop("scrollHeight") });
}

ko.bindingHandlers.executeOnEnter = {
    init: function (element, valueAccessor, allBindings, viewModel) {
        var value = valueAccessor();
        $(element).keypress(function (event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            if (keyCode === 13) {
                value.call(viewModel);
                return false;
            }
            return true;
        });
    }
};

$('#name').focus();

$(function () {

    var chat = $.connection.chatHub;

    // Receive message from server
    chat.client.message = function (user, message) {
        viewModel.addMessage(user, message, 'message');
        scrollToMessage();
    }

    // Recieve notification from server
    chat.client.notification = function (message) {
        viewModel.addMessage('Server', message, 'notification');
        scrollToMessage();
    };

    viewModel = {
        name: ko.observable(''),
        nameError: ko.observable(null),
        message: ko.observable(''), 
        messageError: ko.observable(null),
        messages: ko.observableArray(),
        joined: ko.observable(false),
        sendingMessage: ko.observable(false),
        settingName: ko.observable(false),

        // Add message to chat
        addMessage: function (from, message, type) {
            var parsedFrom = parse(from, false, true);
            var parsedMessage = parse(message, true, true);
            var messageDate = new Date();
            this.messages.push({
                from: parsedFrom,
                rawFrom: from,
                message: parsedMessage,
                type: type
            });
            if (this.messages().length > 100)
                this.messages.remove(this.messages()[0]);
            scrollToMessage();
        },

        // Send message to server
        sendMessage: function () {
            var newMessage = this.message().trim();
            if (newMessage == '')
                return;
            if (newMessage.length > 500)
                return;
            viewModel.sendingMessage(true);
            chat.server.sendToAll(newMessage).done(function () {
                viewModel.sendingMessage(false);
                viewModel.addMessage('Me', newMessage, 'self');
                viewModel.message('');
                $('#message').select();
            }).fail(function (e) {
                viewModel.sendingMessage(false);
                $('#message').select();
                viewModel.messageError(e);
            })
        },

        // Set name on the server
        setName: function () {
            var newName = this.name().trim();
            if (newName.length > 50)
                return;
            viewModel.settingName(true);
            chat.server.initializeUser(newName).done(function () {
                viewModel.joined(true);
                viewModel.settingName(false);
                viewModel.hideUserDetails();
                $('#message').focus();
            }).fail(function (e) {
                viewModel.settingName(false);
                $('#name').select();
                viewModel.nameError(e);
            });
        },

        showUserDetails: function () {
            $('.mask').show();
            $('.mask').animate({ 'opacity': '.5' }, 50);
            $('#greeting').animate({ 'opacity': '0' }, 50, function () {
                $(this).hide();
            });
            $('#user-details').slideDown('fast', function () {
                $('#name').select();
            });
        },

        hideUserDetails: function () {
            var newName = this.name();
            if (newName == '')
                return;
            if (!viewModel.joined())
                return;
            if (viewModel.nameError() != null)
                return;
            $('#user-details').slideUp('fast');
            $('#greeting').show();
            $('#greeting').animate({ 'opacity': '1' }, 50);
            $('.mask').animate({ 'pacity': '0' }, function () {
                $('.mask').hide();
                $('#message').focus();
            });
        }
    };

    // Clear errors on text change
    viewModel.message.subscribe(function () {
        viewModel.messageError(null);
    });
    viewModel.name.subscribe(function () {
        viewModel.nameError(null);
    });

    ko.applyBindings(viewModel);

    $.connection.hub.start();

    $.connection.hub.disconnected(function () {
        viewModel.joined(false);
    });

    $.connection.hub.reconnected(function () {
        var existingName = viewModel.name();
        if (existingName == '')
            return;
        if (existingName.length > 50)
            return;
        viewModel.settingName(true);
        chat.server.join(existingName).done(function () {
            viewModel.settingName(false);
            viewModel.joined(true);
            $('#message').focus();
        }).fail(function (e) {
            viewModel.settingName(false);
            viewModel.joined(false);
            viewModel.showUserDetails();
            $('#name').select();
            viewModel.nameError(e);
        });
    })
})

var smilies = [
    { text: ':\\)', icon: 'smile' },
    { text: ':-\\)', icon: 'smile' },
    { text: ':D', icon: 'happy' },
    { text: ':-D', icon: 'happy' },
    { text: ':P', icon: 'tongue' },
    { text: ':-P', icon: 'tongue' },
    { text: ':\\(', icon: 'sad' },
    { text: ':-\\(', icon: 'sad' },
    { text: ':o', icon: 'shocked' },
    { text: ':\\\\', icon: 'wondering' },
    { text: ':-\\\\', icon: 'wondering' },
    //{ text: ':/', icon: 'wondering' }, // Remove temporarily due to problems with matching {urlshceme}://
    { text: ':-/', icon: 'wondering' },
    { text: ':s', icon: 'confused' },
    { text: ':-s', icon: 'confused' },
    { text: ':S', icon: 'confused' },
    { text: ':-S', icon: 'confused' },
    { text: ';\\)', icon: 'wink' },
    { text: ';-\\)', icon: 'wink' },
    { text: '&lt;3', icon: 'heart' },
    { text: ':\\|', icon: 'neutral' },
    { text: ':-\\|', icon: 'neutral' },
    { text: '\\^\\^', icon: 'grin' },
    { text: '\\(happy\\)', icon: 'grin' },
    { text: '\\(cool\\)', icon: 'cool' },
    { text: '\\(devil\\)', icon: 'evil' },
    { text: '\\(evil\\)', icon: 'evil' },
    { text: '\\(angry\\)', icon: 'angry' },
    { text: ':@', icon: 'angry' }
];


function parse(message, link, smilify) {
    if (link == null)
        link = true;
    if (smilify == null)
        smilify = true;

    message = $('<span />').text(message).html();

    if (link === true)
        message = autolinker.link(message);

    if (smilify === true) {
        for (i = 0; i < smilies.length; i++) {
            var regex = new RegExp(smilies[i].text, 'gi');
            message = message.replace(regex, '<span class=\'icon smiley ' + smilies[i].icon + '\'></span>');
        }
    }
    return message;
}