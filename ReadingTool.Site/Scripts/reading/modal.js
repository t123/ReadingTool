var _this = this;
var SelectedWord = (function () {
    function SelectedWord(settings, element, quicksave) {
        this.settings = settings;
        this.element = element;
        this.length = 1;
        $('.modal-footer').removeClass('failed');
        if(element.nodeName == 'A') {
            this.element = $(element).parent();
        }
        if($(this.element).hasClass(this.settings.classes.multiClass)) {
            this.selectedWord = $(this.element).data('phrase');
        } else {
            this.selectedWord = $(this.element).text();
        }
        this.selectedSentence = this.getCurrentSentence();
        if(quicksave) {
            this.quicksave();
        } else {
            this.findTerm();
        }
    }
    SelectedWord.prototype.findTerm = function () {
        var _this = this;
        $('#currentBox').removeClass().addClass('badge');
        $.post(routes.reading.findTerm, {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            _this.updateModalDisplay();
            _this.createTemplate(data.data);
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
    SelectedWord.prototype.quicksave = function () {
        var _this = this;
        $.post(routes.reading.quickSave, {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            console.log('quicksaving');
            if(data.result == "OK") {
                _this.updateTermClasses(data.data.termPhrase, data.data.term);
            }
        });
    };
    SelectedWord.prototype.saveChanges = function () {
        var _this = this;
        var currentIndex = ($('#tabTermDefintions li.active')).index();
        if(currentIndex < 0) {
            currentIndex = 1;
        }
        $('#iconResult').hide();
        $.post(routes.reading.saveTerm, $('#formTerms').serialize(), function (data) {
            console.log('saving term changes');
            console.log(data);
            if(data.result == "OK") {
                $('#termMessage').removeClass('label-info').addClass('label-success').html(data.message);
                _this.createTemplate(data.data.term);
                ($('#tabTermDefintions a')).eq(currentIndex).tab('show');
                _this.updateModalDisplay();
                $('#iconResult').removeClass().addClass('icon-ok-circle').show().fadeOut(2000);
                _this.updateTermClasses(data.data.termPhrase, data.data.term);
            } else {
                $('#termMessage').removeClass().addClass('label label-important').html(data.message);
                $('.modal-footer').addClass('failed');
                $('#iconResult').removeClass().addClass('icon-exclamation-sign icon-white').show().fadeOut(2000);
            }
        });
    };
    SelectedWord.prototype.updateTermClasses = function (termPhrase, term) {
        if(term.length == 1) {
            $('#textContent .' + termPhrase).removeClass(this.settings.classes.knownClass + ' ' + this.settings.classes.unknownClass + ' ' + this.settings.classes.ignoredClass + ' ' + this.settings.classes.notseenClass + ' box1 box2 box3 box4 box5 box6 box7 box8 box9').addClass(term.stateClass == this.settings.classes.unknownClass ? 'box' + term.box + ' ' + term.stateClass : term.stateClass);
            $('#textContent .' + termPhrase).each(function (index, elem) {
                var currentWord = $(elem).text();
                $(elem).html((term.definition.length > 0 ? '<a title="' + term.definition + '">' : '') + currentWord + (term.definition.length > 0 ? '</a>' : ''));
            });
        } else {
            var elem = $(this.element);
            var prependString = '<span';
            prependString += ' class="' + term.id + ' ' + this.settings.classes.multiClass + ' ' + term.stateClass + '"';
            prependString += ' data-id="' + term.id + '"';
            prependString += ' data-phrase="' + termPhrase + '"';
            prependString += '>' + term.length + '</span>';
            if(elem.prev().length == 0) {
                console.log('beginning of sentence or sup');
                if(elem.text() == term.length) {
                    console.log('replacing span');
                    $(elem).parent().html(prependString);
                } else {
                    console.log('adding sup');
                    $(elem).parent().prepend('<sup>' + prependString + '</sup>');
                }
            } else {
                if(elem.prev().length > 0) {
                    var found = false;
                    var temp = elem.prev();
                    while(temp.length > 0 && !found) {
                        if(temp.text() == term.length) {
                            found = true;
                            break;
                        }
                        temp = temp.prev();
                    }
                    console.log('has previous sup');
                    if(found) {
                        console.log("update existing sup ", temp.text());
                        $(temp).html(prependString);
                    } else {
                        console.log('prepend new');
                        if(elem.prev()[0].nodeName == 'SUP') {
                            console.log('prepend new sup');
                            $(elem).prev().before('<sup>' + prependString + '</sup>');
                        } else {
                            console.log('prepend new no sup');
                            $(elem).before('<sup>' + prependString + '</sup>');
                        }
                    }
                }
            }
            $('#textContent sup .' + term.id).removeClass(this.settings.classes.knownClass + ' ' + this.settings.classes.unknownClass + ' ' + this.settings.classes.ignoredClass + ' ' + this.settings.classes.notseenClass + ' box1 box2 box3 box4 box5 box6 box7 box8 box9').addClass(term.stateClass == this.settings.classes.unknownClass ? 'box' + term.box : term.stateClass);
            $('#textContent sup .' + term.id).each(function (index, elem) {
                var currentWord = $(elem).text();
                $(elem).html((term.definition.length > 0 ? '<a title="' + term.definition + '">' : '') + currentWord + (term.definition.length > 0 ? '</a>' : ''));
            });
        }
    };
    SelectedWord.prototype.resetTerm = function () {
        var _this = this;
        var currentIndex = ($('#tabTermDefintions li.active')).index();
        if(currentIndex < 0) {
            currentIndex = 1;
        }
        $('#iconResult').hide();
        $.post(routes.reading.resetTerm, {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            console.log('reset term');
            console.log(data);
            if(data.result == "OK") {
                $('#termMessage').removeClass('label-info').addClass('label-success').html(data.message);
                _this.createTemplate(data.data.term);
                ($('#tabTermDefintions a')).eq(currentIndex).tab('show');
                _this.updateModalDisplay();
                $('#iconResult').removeClass().addClass('icon-ok-circle').show().fadeOut(2000);
                _this.updateTermClasses(data.data.termPhrase, data.data.term);
            } else {
                $('#termMessage').removeClass().addClass('label label-important').html(data.message);
                $('.modal-footer').addClass('failed');
                $('#iconResult').removeClass().addClass('icon-exclamation-sign icon-white').show().fadeOut(2000);
            }
        });
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
                $.post(routes.reading.encodeTerm, {
                    languageId: _this.settings.languageId,
                    dictionaryId: id,
                    input: input
                }, function (data) {
                    if(data.result == "OK") {
                        anchor.attr('href', data.message);
                        if(auto) {
                            anchor[0].click();
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
        var sentenceNode = $(this.element).closest('.sentence');
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
        console.log('get current sentence: ', sentence);
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
    SelectedWord.prototype.blankModal = function () {
        $('#termId').val('');
        $('#termPhrase').val('');
        $('#tabTermDefintions').html('');
        $('#tabContent').html('');
        $('#termMessages').html('');
        $('#selectedWord').html('');
    };
    return SelectedWord;
})();
