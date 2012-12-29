var _this = this;
var SelectedWord = (function () {
    function SelectedWord(settings, element) {
        this.settings = settings;
        this.element = element;
        this.length = 1;
        this.init();
    }
    SelectedWord.prototype.init = function () {
        var _this = this;
        this.selectedWord = $(this.element).html();
        this.selectedSentence = this.getCurrentSentence();
        $('#currentBox').removeClass().addClass('badge');
        $.post(this.settings.ajaxUrl + '/find-term', {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            console.log(data);
            _this.createTemplate(data);
            _this.updateModalDisplay();
        });
    };
    SelectedWord.prototype.createTemplate = function (data) {
        var ulSource = $("#term-ul-template").html();
        var ulTemplate = Handlebars.compile(ulSource);
        var ulHtml = ulTemplate(data);
        var divSource = $("#term-div-template").html();
        var divTemplate = Handlebars.compile(divSource);
        var divHtml = divTemplate(data);
        var messageSource = $("#term-message-template").html();
        var messageTemplate = Handlebars.compile(messageSource);
        var messageHtml = messageTemplate(data);
        $('#tabTermDefintions').html(ulHtml);
        $('#tabContent').html(divHtml);
        $('#termMessages').html(messageHtml);
        $('#currentBox').html(data.box);
        $('#termMessage').html(data.message);
        if(data.id == '00000000-0000-0000-0000-000000000000') {
            $('#termMessage').removeClass('label-info').addClass('label-warning');
        }
        for(var i = 0; i < data.individualTerms.length; i++) {
            var it = data.individualTerms[i];
        }
        ($('#tabTermDefintions a:first')).tab('show');
        $('#itermMessage0').show();
    };
    SelectedWord.prototype.updateModalDisplay = function () {
        $('#selectedWord').text(this.selectedWord);
        this.refreshDictionaryLinks();
    };
    SelectedWord.prototype.saveChanges = function () {
        $.post(this.settings.ajaxUrl + '/save-term', {
        }, function (data) {
            if(data.result == "OK") {
                $('#modalMessage').removeClass().addClass('label label-success').html(data.Message);
                $('#currentBox').removeClass().addClass('badge badge-success').html(data.Data.Box);
            } else {
                $('#modalMessage').removeClass().addClass('label label-error').html(data.Message);
            }
        });
    };
    SelectedWord.prototype.resetWord = function () {
        console.log('reset word');
    };
    SelectedWord.prototype.refreshSentence = function () {
        console.log('refresh sentence');
        this.sentence = this.getCurrentSentence();
        if(!$('#sentence').parent('div').parent('div').hasClass('warning')) {
            $('#sentence').parent('div').parent('div').addClass('warning');
        }
        this.updateModalDisplay();
    };
    SelectedWord.prototype.increaseWord = function () {
        console.log('increase word');
        if(this.length >= 7) {
            return;
        }
        this.length++;
        this.changePhrase();
    };
    SelectedWord.prototype.decreaseWord = function () {
        console.log('decrease word');
        if(this.length <= 1) {
            return;
        }
        this.length--;
        this.changePhrase();
    };
    SelectedWord.prototype.refreshDictionaryLinks = function () {
        var _this = this;
        $('.dictionary').each(function (index, a) {
            var anchor = $(a);
            var id = anchor.data('id');
            var parameter = anchor.data('parameter');
            var urlEncode = anchor.data('urlencode');
            var url = anchor.data('url');
            var auto = anchor.data('autoopen');
            var input = parameter == 'sentence' ? _this.selectedSentence : _this.selectedWord;
            if(auto == undefined) {
                auto = false;
            }
            if(urlEncode) {
                $.post(_this.settings.ajaxUrl + '/encode-term', {
                    languageId: _this.settings.languageId,
                    dictionaryId: id,
                    input: input
                }, function (data) {
                    if(data.Result == "OK") {
                        anchor.attr('href', data.Message);
                        if(auto) {
                            anchor[0].click();
                            console.log(auto);
                        }
                    }
                });
            } else {
                anchor.attr('href', url.replace('###', input));
                if(auto) {
                    anchor[0].click();
                }
            }
        });
    };
    SelectedWord.prototype.changePhrase = function () {
        var currentWord = '';
        var i = 0;
        var next = $(this.element);
        do {
            if(next == null) {
                break;
            }
            if(next.attr('class') == this.settings.classes.spaceClass || next.attr('class') == this.settings.classes.punctuationClass || next.attr('class') == this.settings.classes.multiClass || next.is("sup")) {
                next = next.next();
                continue;
            }
            currentWord += next.text() + ' ';
            next = next.next();
            i++;
        }while(i < this.length)
        this.selectedWord = currentWord;
        this.updateModalDisplay();
    };
    SelectedWord.prototype.getCurrentSentence = function () {
        console.log('get current sentence');
        var sentenceNode = $(this.element).parent();
        var children = sentenceNode.children();
        var sentence = this.buildSentence(children);
        if(sentence.length < 25) {
            var prev = sentenceNode.prev();
            if(prev != null) {
                sentence = this.buildSentence(prev.children()) + ' ' + sentence;
            }
        }
        if(sentence.length < 25) {
            var next = sentenceNode.next();
            if(prev != null) {
                sentence = sentence + ' ' + this.buildSentence(next.children());
            }
        }
        return sentence;
    };
    SelectedWord.prototype.buildSentence = function (elements) {
        var sentence = '';
        elements.each(function (index, node) {
            if(node.nodeName == 'SUP') {
                return;
            }
            var nodeContent = node.textContent;
            if(nodeContent == '') {
                nodeContent = node.innerText;
            }
            sentence += nodeContent;
        });
        return $.trim(sentence);
    };
    return SelectedWord;
})();
