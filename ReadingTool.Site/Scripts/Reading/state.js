﻿var ModalHandler = function (routes, settings) {
    var self = this;
    self.settings = settings;
    self.routes = routes;
    var textModal = $('#textModal');
    var sentence = $('#currentSentence');
    var word = $('#currentWord');
    var baseWord = $('#baseWord');
    var message = $('#saveMessage');
    var termId = $('#termId');
    var definition = $('#definition');
    var tags = $('#tags');
    var state = $('#state');
    var markRemainingMessage = $('#markRemainingAsKnownProgress');
    var currentLength;

    var hasChanged = false;
    var currentElement;

    self.init = function () {
        $('#textModalCloseLink').hover(function () {
            if (!hasChanged) {
                self.close();
            }
        });

        $('#textModalCloseLink').click(function () {
            self.close();
        });

        $('#readingareainner p span span').live('mousedown', function (e) {
            if (textModal.is(":visible")) {
                if (!hasChanged) {
                    self.close();
                }
            } else {
                self.open(e.srcElement);
            }
        });

        $('#increaseWord').click(function () {
            self.increaseWord();
        });

        $('#decreaseWord').click(function () {
            self.decreaseWord();
        });

        $('#refreshSentence').click(function () {
            self.refreshSentence();
        });

        $('#btnReset').click(function () {
            self.reset();
        });

        $('#btnSave').click(function () {
            self.save();
        });

        $('input[type="text"], input[type="radio"]').change(function () {
            hasChanged = true;
        });

        $('textarea').change(function () {
            hasChanged = true;
        });

        $('#btnMarkRemainingAsKnown').click(function () {
            self.markRemainingAsKnown();
        });

        $(document).keyup(function (event) {
            var code = (event.keyCode ? event.keyCode : event.which);

            if (textModal.is(":visible")) {
                if (code == 27) {
                    self.close();
                } else if (event.ctrlKey && code == 13) {
                    self.save();
                    self.close();
                }
            }
        });
    };

    self.save = function () {
        $.ajax({
            url: self.routes.ajax.saveTerm,
            type: 'POST',
            data: {
                termId: termId.val(),
                phrase: word.text(),
                basePhrase: baseWord.val(),
                sentence: sentence.val(),
                definition: definition.val(),
                tags: tags.val(),
                languageId: settings.languageId,
                textId: settings.textId,
                state: $('input[name="state"]:checked').val(),
            }
        }).done(function (data) {
            hasChanged = false;
            self._populateModal(data);
            self._updateTips(data);
            message.html(data.message);
        });
    };

    self.reset = function () {
        $.ajax({
            url: self.routes.ajax.resetTerm,
            type: 'POST',
            data: {
                termId: termId.val(),
            }
        }).done(function (data) {
            hasChanged = false;
            self._populateModal(data);
            self._updateTips(data);
            message.html(data.message);
        });
    };

    self.close = function () {
        termId.val('');
        textModal.hide();
    };

    self.open = function (element) {
        currentElement = $(element).closest('span');
        self._buildCurrentPopup();
        self._updateModalLocaion();
        self._load();
    };

    self.refreshSentence = function () {
        hasChanged = true;
        sentence.val(this._getCurrentSentence());
        sentence.addClass('changed');
    };

    self.increaseWord = function () {
        if (currentLength >= 7) {
            return;
        }

        currentLength++;
        self._updateWord();
        termId.val('');
        self._load();
        self.refreshSentence();
    };

    self.decreaseWord = function () {
        if (this.length <= 1) {
            return;
        }
        currentLength--;
        self._updateWord();
        termId.val('');
        self._load();
        self.refreshSentence();
    };

    self._updateWord = function () {
        var currentWord = '';
        var i = 0;
        var next = currentElement;

        if (next.is('a')) {
            next = next.closest('span');
        }

        if (next.hasClass('mxx')) {
            next = next.closest('span').next();
        }

        var safety = 0;
        do {
            safety++;
            if (next == null) {
                break;
            }
            if (next.attr('class') == 'wsx' || next.attr('class') == 'pcx' || next.attr('class') == 'mxx' || !next.is("span")) {
                next = next.next();
                continue;
            }
            currentWord += next.text() + ' ';
            next = next.next();
            i++;
        } while (i < currentLength && safety < 200)

        word.text(currentWord);
    };

    self.markRemainingAsKnown = function () {
        var toWorkOn = $('#readingareainner ._n');

        var words = [];
        toWorkOn.each(function (i) {
            if ($(this).is('sup')) return;
            words.push($(this).text());
        });

        markRemainingMessage.removeClass('alert-info alert-success alert-error');
        if (words.length > 0) {
            markRemainingMessage.addClass('alert-info').html('Saving words....').show();
        } else {
            markRemainingMessage.addClass('alert-error').html('No words to save.').show();
            return;
        }

        $.ajax({
            traditional: true,
            url: self.routes.ajax.markRemaingAsKnown,
            type: 'POST',
            data: {
                languageId: settings.languageId,
                textId: settings.textId,
                terms: words
            }
        }).done(function (data) {
            markRemainingMessage.removeClass('alert-info alert-success alert-error');

            if (data == "OK") {
                markRemainingMessage.addClass('alert-success').html('All words saved.');
            } else {
                markRemainingMessage.addClass('alert-error').html('There was an error saving your words. Please refresh and try again.');
            }

            markRemainingMessage.show();
        });

        toWorkOn.removeClass('_n').addClass('_k');
    };

    self._updateDictionaries = function () {
        $('.dictionary').each(function (index, a) {
            var anchor = $(a);
            var id = anchor.data('id');
            var parameter = anchor.data('parameter');
            var urlEncode = anchor.data('urlencode');
            var url = anchor.data('url');
            var auto = anchor.data('autoopen');
            var input = parameter == true ? sentence.val() : currentElement.text();
            if (auto == undefined) {
                auto = false;
            }

            if (urlEncode) {
                $.ajax({
                    type: 'POST',
                    url: self.routes.ajax.encodeTerm,
                    data: {
                        languageId: settings.languageId,
                        dictionaryId: id,
                        input: input
                    }
                }).done(function (data) {
                    if (data != '') {
                        anchor.attr('href', data);
                        if (auto) {
                            anchor[0].click();
                        }
                    }
                });
            } else {
                anchor.attr('href', url.replace('###', input));
                if (auto) {
                    anchor[0].click();
                }
            }
        });
    };

    self._updateTips = function (data) {
        $('#readingareainner .' + data.phrase)
                        .removeClass('_k _u _n _i box1 box2 box3 box4 box5 box6 box7 box8 box9')
                        .addClass(data.state == 'NotKnown' ? 'box' + data.box : data.stateClass);

        var tempDef = data.basePhrase.length > 0 ? data.basePhrase : '';
        if (data.definition.length > 0) tempDef += "\n" + data.definition;
        if (data.tags.length > 0) tempDef += "\n" + data.tags;

        $('#readingareainner .' + data.phrase).each(function (index) {
            var phrase = $(this).text();
            $(this).html(
                (tempDef.length > 0 ? '<a title="' + tempDef + '">' : '') + phrase + (tempDef.length > 0 ? '</a>' : '')
            );
        });
    };

    self._load = function () {
        var text = word.text();
        if (currentElement.hasClass('mxx')) {
            text = currentElement.data('phrase');
            word.text(text);
            $('#increaseWord').hide();
            $('#decreaseWord').hide();
        } else {
            $('#increaseWord').show();
            $('#decreaseWord').show();
        }

        $.ajax({
            url: self.routes.ajax.findTerm,
            type: 'POST',
            data: {
                termId: termId.val(),
                languageId: settings.languageId,
                spanTerm: text
            }
        }).done(function (data) {
            self._populateModal(data);
            self._updateDictionaries();
            message.html(data.message);
        });
    };

    self._populateModal = function (data) {
        if (data.exists) {
            sentence.val(data.sentence);
            baseWord.val(data.basePhrase);
            definition.val(data.definition);
            tags.val(data.tags);
        } else {
            baseWord.val('');
            definition.val('');
            tags.val('');
        }

        currentLength = data.length;
        termId.val(data.termId);
        $('[name=state][value=' + data.state + ']').prop('checked', 'true');
    };

    self._updateModalLocaion = function () {
        var offset = currentElement.offset();

        var newLeft = offset.left;

        if (newLeft + 700 > $(document).width()) {
            newLeft -= 660;
        }

        var newTop = offset.top;
        var windowHeight = $(window).height() - 80;
        var modalHeight = textModal.height();

        if (newTop + modalHeight > windowHeight) {
            newTop -= (modalHeight + 20);
        } else {
            newTop = offset.top + 20;
        }

        textModal.show();
        textModal.offset({ top: newTop, left: newLeft });
    };

    self._buildCurrentPopup = function () {
        word.text(currentElement.text());
        sentence.val(this._getCurrentSentence());
    };

    self._buildSentence = function (elements) {
        sentence.removeClass('changed');
        var currentSentence = '';
        elements.each(function (index, node) {
            if (node.nodeName == 'SUP') return;
            var nodeContent = node.textContent;
            if (nodeContent == '') nodeContent = node.innerText;
            currentSentence += nodeContent;
        });

        return $.trim(currentSentence);
    };

    self._getCurrentSentence = function () {
        var sentenceNode = currentElement.closest('.sentence');
        var children = sentenceNode.children();
        var currentSentence = self._buildSentence(children);

        if (currentSentence.length < 25) {
            var prev = sentenceNode.prev();

            if (prev != null) {
                currentSentence = self._buildSentence(prev.children()) + ' ' + currentSentence;
            }
        }

        if (currentSentence.length < 25) {
            var next = sentenceNode.next();

            if (prev != null) {
                currentSentence = currentSentence + ' ' + self._buildSentence(next.children());
            }
        }

        return currentSentence;
    };

    self.init();
};