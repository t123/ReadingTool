mReader = function () {
    var words, languageId, itemId;
    var element;
    var baseUrl;
    var phraseLength;
};

mReader.prototype = {
    init: function (itemId, languageId, baseUrl) {
        this.itemId = itemId;
        this.languageId = languageId;
        this.baseUrl = baseUrl;
        this.phraseLength = 1;
    },

    createPopup: function (element) {
        this.phraseLength = 1;
        this.element = element;
        var offset = $(element).offset();
        this._findWord(element);

        var newLeft = offset.left;

        if (newLeft + 500 > $(document).width()) {
            newLeft -= 500;
        }

        $('#textModal').show();

        var newTop = offset.top;
        var windowHeight = $(window).height() - 80;
        var modalHeight = $('#textModal').height();

        if (newTop + modalHeight > windowHeight) {
            newTop -= (modalHeight + 20);
        } else {
            newTop = offset.top + 20;
        }
        $('#textModal').offset({ top: newTop, left: newLeft });
    },

    quicksave: function (element, state) {
        if (element.parent().is("sup")) return;
        var thisObject = this;
        var word = element.text();
        var sentence = this._getCurrentSentence(element);

        $.post(
            this.baseUrl + '/quicksaveword',
            {
                languageId: thisObject.languageId,
                word: word,
                state: state,
                itemId: thisObject.ItemId,
                sentence: sentence,
                videoId: thisObject.videoId
            },
            function (data) {
                if (data != null && data.result == "OK" && data.word != null) {
                    console.log(data);

                    $('#textContent .' + data.word.wordLower)
                        .removeClass(settings.knownClass + ' ' + settings.unknownClass + ' ' + settings.ignoredClass + ' ' + settings.notseenClass)
                        .addClass(data.word.state.toLowerCase());
                }
            }
        );
    },

    save: function () {
        var currentWord = $('#currentWord').text();
        var state = $('input[name=state]:checked').val()
        var baseWord = $('#baseWord').val();
        var romanisation = $('#romanisation').val();
        var definition = $('#definition').val();
        var tags = $('#tags').val();
        var sentence = $('#currentSentence').val();
        var thisObject = this;
        $('#saveMessage').html();

        $.post(
            this.baseUrl + '/saveword',
            {
                languageId: thisObject.languageId,
                word: currentWord,
                state: state,
                baseWord: baseWord,
                romanisation: romanisation,
                definition: definition,
                tags: tags,
                sentence: sentence,
                itemId: thisObject.itemId
            },
            function (data) {
                if (data != null && data.result == "OK" && data.word != null) {
                    if (data.word.multiword) {
                        thisObject._updateMultiSpan(data.word);
                    } else {
                        thisObject._updateSingleSpan(data.word);
                    }

                    $('#saveMessage').html('...saved, state is ' + data.word.stateHuman);
                } else {
                    $('#saveMessage').html('...not saved');
                }
            }
        );
    },

    _updateSingleSpan: function (word) {
        $('#textContent .' + word.wordLower)
                        .removeClass(settings.knownClass + ' ' + settings.unknownClass + ' ' + settings.ignoredClass + ' ' + settings.notseenClass)
                        .addClass(word.state);

        $('#textContent .' + word.wordLower).each(function (index) {
            var currentWord = $(this).text();
            $(this).html(
                (word.definition.length > 0 ? '<a title="' + word.definition + '">' : '') + currentWord + (word.definition.length > 0 ? '</a>' : '')
            );
        });
    },

    _updateMultiSpan: function (word) {
        var id = this.element.data('id');
        $('#textContent .' + id)
                        .removeClass(settings.knownClass + ' ' + settings.unknownClass + ' ' + settings.ignoredClass + ' ' + settings.notseenClass)
                        .addClass(word.state);

        $('#textContent .' + id).each(function (index) {
            var currentWord = $(this).text();
            $(this).html(
                (word.definition.length > 0 ? '<a title="' + word.definition + '">' : '') + word.length + (word.definition.length > 0 ? '</a>' : '')
            );
        });
    },

    _changePhrase: function () {
        var currentWord = '';
        var i = 0;
        var next = this.element;

        do {
            if (next == null) break;
            if (
                next.attr('class') == settings.spaceClass ||
                next.attr('class') == settings.punctuationClass ||
                next.attr('class') == settings.multiClass ||
                next.is("sup")
                ) { next = next.next(); continue; }

            currentWord += next.text() + ' ';
            next = next.next();
            i++;
        } while (i < this.phraseLength);

        $('#currentWord').text(currentWord);
    },

    increaseWord: function () {
        if (this.phraseLength >= 7) return;
        this.phraseLength++;
        this._changePhrase();
    },

    decreaseWord: function () {
        if (this.phraseLength <= 1) return;
        this.phraseLength--;
        this._changePhrase();
    },

    refreshSentence: function () {
        $('#currentSentence').css({ 'background-color': '#F5B8A9' });
        $('#currentSentence').val(this._getCurrentSentence(this.element));
    },

    markRemainingAsKnown: function () {
        var startIndex = 0;
        var max = 100000;
        //var toWorkOn = $('#textContent .notseen').slice(startIndex,max);
        var toWorkOn = $('#textContent .' + settings.notseenClass);

        //while(toWorkOn.length>0) {
        var words = [];
        toWorkOn.each(function (i) {
            if ($(this).is('sup')) return;
            words.push($(this).text());
        });

        if (words.length > 0) {
            $('#markRemainingAsKnownProgress').html('saving words');
            $('#markRemainingAsKnownProgress').show();
        } else {
            $('#markRemainingAsKnownProgress').html('no words to save').show();
        }

        var thisObject = this;

        $.post(
                this.baseUrl + '/markremainingasknown',
                {
                    languageId: thisObject.languageId,
                    words: words,
                    itemId: thisObject.itemId
                }, function (data) {
                    if (data != null) {
                        $('#markRemainingAsKnownProgress').show();

                        if (data.result == "OK") {
                            $('#markRemainingAsKnownProgress').html('all words saved');
                        } else {
                            $('#markRemainingAsKnownProgress').html('some words not saved, please refresh and retry.');
                        }
                    }
                }
            );

        $('#markRemainingAsKnownProgress').html('saving words, awaiting confirmation');
        toWorkOn.removeClass(settings.notseenClass).addClass(settings.knownClass);
        //startIndex += max;
        //toWorkOn = $('#textContent .notseen').slice(startIndex,startIndex+max);
        //}
    },

    _findWord: function (element) {
        $('#saveMessage').html('');
        $('#currentWord').html('');

        var thisObject = this;
        var text = $(element).hasClass(settings.multiClass) ? $(element).data('phrase') : element.text();
        $.post(
            this.baseUrl + '/getword',
            {
                languageId: thisObject.languageId,
                word: text
            },
            function (data) {
                thisObject._createPopupContent(element, data);
            }
        );
    },

    _createPopupContent: function (element, data) {
        $('#currentSentence').css({ 'background-color': 'white' });

        if (element.hasClass(settings.multiClass)) {
            this._createPopupMulti(element, data);
        } else {
            this._createPopupSingle(element, data);
        }
    },

    _createPopupMulti: function (element, data) {
        $('#currentWord').text(element.data('phrase'));
        if (data == null || data.word == null) {
            $('#unknownState').attr('checked', true);
            $('#baseWord').val('');
            $('#romanisation').val('');
            $('#definition').val('');
            $('#tags').val('');
            $('#currentSentence').val('');
            $('#wordInfo').html('');
            $('#saveMessage').html('New word, defaulted to unknown');
        } else {
            if (data.word.state == settings.knownClass) {
                $('#knownState').attr('checked', true);
            } else if (data.word.state == settings.unknownClass) {
                $('#unknownState').attr('checked', true);
            } else if (data.word.state == settings.ignoredClass) {
                $('#ignoredState').attr('checked', true);
            } else {
                $('#notseenState').attr('checked', true);
            }

            $('#baseWord').val(data.word.baseWord);
            $('#romanisation').val(data.word.romanisation);
            $('#definition').val(data.word.definition);
            $('#tags').val(data.word.tags);
            $('#currentSentence').val(data.word.sentence);
            $('#wordInfo').html('Current box: ' + data.word.box);
        }

        if ($('#currentSentence').val() == '') {
            $('#currentSentence').val(this._getCurrentSentence($(element)));
            $('#currentSentence').css({ 'background-color': '#F5B8A9' });
        }
    },

    _createPopupSingle: function (element, data) {
        $('#currentWord').text(element.text());
        if (data == null || data.word == null) {
            $('#unknownState').attr('checked', true);
            $('#baseWord').val('');
            $('#romanisation').val('');
            $('#definition').val('');
            $('#tags').val('');
            $('#currentSentence').val('');
            $('#wordInfo').html('');
            $('#saveMessage').html('New word, defaulted to unknown');
        } else {
            if (data.word.state == settings.knownClass) {
                $('#knownState').attr('checked', true);
            } else if (data.word.state == settings.unknownClass) {
                $('#unknownState').attr('checked', true);
            } else if (data.word.state == settings.ignoredClass) {
                $('#ignoredState').attr('checked', true);
            } else {
                $('#notseenState').attr('checked', true);
            }

            $('#baseWord').val(data.word.baseWord);
            $('#romanisation').val(data.word.romanisation);
            $('#definition').val(data.word.definition);
            $('#tags').val(data.word.tags);
            $('#currentSentence').val(data.word.sentence);
            $('#wordInfo').html('Current box: ' + data.word.box);
        }

        if ($('#currentSentence').val() == '') {
            $('#currentSentence').val(this._getCurrentSentence($(element)));
            $('#currentSentence').css({ 'background-color': '#F5B8A9' });
        }
    },

    _buildSentence: function (elements) {
        var sentence = '';
        elements.each(function (index, node) {
            if (node.nodeName == 'SUP') return;

            var nodeContent = node.textContent;
            if (nodeContent == '') nodeContent = node.innerText;
            sentence += nodeContent;
        });

        return sentence;
    },

    _getCurrentSentence: function (element) {
        var sentenceNode = $(element).parent();
        var children = sentenceNode.children();
        var sentence = this._buildSentence(children);

        if (sentence.length < 25) {
            var prev = sentenceNode.prev();

            if (prev != null) {
                sentence = this._buildSentence(prev.children()) + sentence;
            }
        }

        if (sentence.length < 25) {
            var next = sentenceNode.next();

            if (prev != null) {
                sentence = sentence + this._buildSentence(next.children());
            }
        }

        return sentence;
    }
}