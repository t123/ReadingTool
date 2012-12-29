var SelectedWord = (function () {
    function SelectedWord(settings, element) {
        this.settings = settings;
        this.element = element;
        this.length = 1;
        this.selectedWord = $(this.element).html();
        this.selectedSentence = this.getCurrentSentence();
        this.findTerm();
    }
    SelectedWord.prototype.findTerm = function () {
        var _this = this;
        $('#currentBox').removeClass().addClass('badge');
        $.post(this.settings.ajaxUrl + '/find-term', {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            console.log(data);
            _this.updateModalDisplay();
            _this.createTemplate(data);
        });
    };
    SelectedWord.prototype.createTemplate = function (data) {
        var ulHtml = termUlTemplate(data);
        var divHtml = termDivTemplate(data);
        var messageHtml = termMessageTemplate(data);
        $('#tabTermDefintions').html(ulHtml);
        $('#tabContent').html(divHtml);
        $('#termMessages').html(messageHtml);
        $('#currentBox').html(data.box);
        $('#termMessage').html(data.message);
        $('#termId').val(data.id);
        if(data.id == '00000000-0000-0000-0000-000000000000') {
            $('#termMessage').removeClass('label-info').addClass('label-warning');
        }
        if(data.stateClass == this.settings.classes.knownClass) {
            $('#knownState').attr('checked', true);
        } else {
            if(data.stateClass == this.settings.classes.unknownClass) {
                $('#unknownState').attr('checked', true);
            } else {
                if(data.stateClass == this.settings.classes.ignoredClass) {
                    $('#ignoredState').attr('checked', true);
                } else {
                    $('#notseenState').attr('checked', true);
                }
            }
        }
        for(var i = 0; i < data.individualTerms.length; i++) {
            var it = data.individualTerms[i];
            $('#sentence' + it.id).val(it.sentence);
            $('#baseTerm' + it.id).val(it.baseTerm);
            $('#romanisation' + it.id).val(it.romanisation);
            $('#definition' + it.id).val(it.definition);
            $('#tags' + it.id).val(it.tags);
            if(it.id == '00000000-0000-0000-0000-000000000000') {
                $('#itermMessage' + i).removeClass('label-info').addClass('label-important');
                $('#sentence' + it.id).val(this.selectedSentence);
            }
        }
        ($('#tabTermDefintions a:first')).tab('show');
        $('#itermMessage0').show();
    };
    SelectedWord.prototype.updateModalDisplay = function () {
        $('#selectedWord').text(this.selectedWord);
        $('#termPhrase').val(this.selectedWord);
        this.refreshDictionaryLinks();
    };
    SelectedWord.prototype.saveChanges = function () {
        var _this = this;
        var currentIndex = ($('#tabTermDefintions li.active')).index();
        if(currentIndex < 0) {
            currentIndex = 1;
        }
        $.post(this.settings.ajaxUrl + '/save-term', $('#formTerms').serialize(), function (data) {
            if(data.result == "OK") {
                $('#termMessage').removeClass('label-info').addClass('label-success').html(data.message);
                _this.createTemplate(data.data);
                ($('#tabTermDefintions a')).eq(currentIndex).tab('show');
                _this.updateModalDisplay();
            } else {
                $('#termMessage').removeClass('label-info').addClass('label-error').html(data.message);
            }
        });
    };
    SelectedWord.prototype.resetWord = function () {
        console.log('reset word');
    };
    SelectedWord.prototype.refreshSentence = function (element) {
        console.log('refresh sentence');
        this.sentence = this.getCurrentSentence();
        if(!$(element).hasClass('refresh')) {
            $(element).addClass('refresh');
        }
        $(element).val(this.sentence);
        this.updateModalDisplay();
    };
    SelectedWord.prototype.increaseWord = function () {
        console.log('increase word');
        if(this.length >= 7) {
            return;
        }
        this.length++;
        this.changePhrase();
        this.findTerm();
    };
    SelectedWord.prototype.decreaseWord = function () {
        console.log('decrease word');
        if(this.length <= 1) {
            return;
        }
        this.length--;
        this.changePhrase();
        this.findTerm();
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
