/// <reference path="settings.ts"/>
/// <reference path="jquery.d.ts"/>

declare var Handlebars;
declare var termUlTemplate;
declare var termDivTemplate;
declare var termMessageTemplate;

class SelectedWord {
    settings: Settings;
    element: any;

    /// The word the user has clicked on
    selectedWord: string;
    selectedSentence: string;
    selectedTermId: string;

    sentence: string;
    baseTerm: string;
    romanisation: string;
    definition: string;
    tags: string;
    length: number;

    constructor(settings: Settings, element, quicksave: bool) {
        this.settings = settings;
        this.element = element;
        this.length = 1;
        $('.modal-footer').removeClass('failed');

        if (element.nodeName == 'A') {
            this.element = $(element).parent();
        }

        if ($(this.element).hasClass(this.settings.classes.multiClass)) {
            this.selectedWord = $(this.element).data('phrase');
        } else {
            this.selectedWord = $(this.element).text();
        }

        this.selectedSentence = this.getCurrentSentence();

        if (quicksave) {
            this.quicksave();
        } else {
            this.findTerm();
        }
    }

    private findTerm() {
        $('#currentBox').removeClass().addClass('badge');

        $.post(
            this.settings.ajaxUrl + '/find-term',
            {
                languageId: this.settings.languageId,
                termPhrase: this.selectedWord,
            }, function (data) => {
                this.updateModalDisplay();
                this.createTemplate(data.data);
            });
    }

    private createTemplate(data) {
        var ulHtml = termUlTemplate(data);
        var divHtml = termDivTemplate(data);
        var messageHtml = termMessageTemplate(data);

        $('#tabTermDefintions').html(ulHtml);
        $('#tabContent').html(divHtml);
        $('#termMessages').html(messageHtml);
        $('#currentBox').html(data.box);
        $('#termMessage').html(data.message);
        $('#termId').val(data.id);

        if (data.id == '00000000-0000-0000-0000-000000000000') {
            $('#termMessage').removeClass('label-info').addClass('label-warning');
        }

        if (data.stateClass == this.settings.classes.knownClass) {
            $('#knownState').attr('checked', true);
        } else if (data.stateClass == this.settings.classes.unknownClass) {
            $('#unknownState').attr('checked', true);
        } else if (data.stateClass == this.settings.classes.ignoredClass) {
            $('#ignoredState').attr('checked', true);
        } else {
            $('#notseenState').attr('checked', true);
        }

        for (var i = 0; i < data.individualTerms.length; i++) {
            var it = data.individualTerms[i];
            $('#sentence' + it.id).val(it.sentence);
            $('#baseTerm' + it.id).val(it.baseTerm);
            $('#romanisation' + it.id).val(it.romanisation);
            $('#definition' + it.id).val(it.definition);
            $('#tags' + it.id).val(it.tags);

            if (it.id == '00000000-0000-0000-0000-000000000000') {
                $('#itermMessage' + i).removeClass('label-info').addClass('label-important');
                $('#sentence' + it.id).val(this.selectedSentence);
            }
        }

        (<any>$('#tabTermDefintions a:first')).tab('show');
        $('#itermMessage0').show();
    }

    private updateModalDisplay() {
        $('#selectedWord').text(this.selectedWord);
        $('#termPhrase').val(this.selectedWord);
        this.refreshDictionaryLinks();
    }

    private quicksave() {
        $.post(this.settings.ajaxUrl + '/quicksave',
            {
                languageId: this.settings.languageId,
                termPhrase: this.selectedWord
            },
            function (data) => {
                console.log('quicksaving');
                if (data.result == "OK") {
                    this.updateTermClasses(data.data.termPhrase, data.data.term);
                }
            });
    }

    public saveChanges() {
        var currentIndex = (<any>$('#tabTermDefintions li.active')).index();
        if (currentIndex < 0) currentIndex = 1;
        $('#iconResult').hide();

        $.post(this.settings.ajaxUrl + '/save-term',
            $('#formTerms').serialize(),
            function (data) => {
                console.log('saving term changes');
                console.log(data);
                if (data.result == "OK") {
                    $('#termMessage').removeClass('label-info').addClass('label-success').html(data.message);
                    this.createTemplate(data.data.term);
                    (<any>$('#tabTermDefintions a')).eq(currentIndex).tab('show');
                    this.updateModalDisplay();
                    $('#iconResult').removeClass().addClass('icon-ok-circle').show().fadeOut(2000);
                    this.updateTermClasses(data.data.termPhrase, data.data.term);
                } else {
                    $('#termMessage').removeClass().addClass('label label-important').html(data.message);
                    $('.modal-footer').addClass('failed');
                    $('#iconResult').removeClass().addClass('icon-exclamation-sign icon-white').show().fadeOut(2000);
                }
            });
    }

    private updateTermClasses(termPhrase: string, term) {
        if (term.length == 1) {
            $('#textContent .' + termPhrase)
                .removeClass(
                    this.settings.classes.knownClass + ' ' +
                    this.settings.classes.unknownClass + ' ' +
                    this.settings.classes.ignoredClass + ' ' +
                    this.settings.classes.notseenClass +
                    ' box1 box2 box3 box4 box5 box6 box7 box8 box9'
                )
                .addClass(term.stateClass == this.settings.classes.unknownClass ? 'box' + term.box + ' ' + term.stateClass : term.stateClass);

            $('#textContent .' + termPhrase).each(function (index, elem) => {
                var currentWord = $(elem).text();
                $(elem).html(
                (term.definition.length > 0 ? '<a title="' + term.definition + '">' : '') + currentWord + (term.definition.length > 0 ? '</a>' : '')
            );
            });
        } else {
            var elem = $(this.element);
            var prependString = '<span';
                    prependString += ' class="' + term.id + ' ' + this.settings.classes.multiClass + ' ' + term.stateClass + '"';
                    prependString += ' data-id="' + term.id + '"';
                    prependString += ' data-phrase="' + termPhrase + '"';
                    prependString += '>' + term.length + '</span>';

            if (elem.prev().length == 0) {
                console.log('beginning of sentence or sup');

                if (elem.text() == term.length) {
                    console.log('replacing span');
                    $(elem).parent().html(prependString);
                } else {
                    console.log('adding sup');
                    $(elem).parent().prepend('<sup>' + prependString + '</sup>');
                }
            } else if (elem.prev().length > 0) {
                var found = false;
                var temp = elem.prev();

                while (temp.length > 0 && !found) {
                    if (temp.text() == term.length) {
                        found = true;
                        break;
                    }

                    temp = temp.prev();
                }

                console.log('has previous sup');

                if (found) {
                    console.log("update existing sup ", temp.text());
                    $(temp).html(prependString);
                } else {
                    console.log('prepend new');

                    if (elem.prev()[0].nodeName == 'SUP') {
                        console.log('prepend new sup');
                        $(elem).prev().before('<sup>' + prependString + '</sup>');
                    } else {
                        console.log('prepend new no sup');
                        $(elem).before('<sup>' + prependString + '</sup>');
                    }
                }
            }

            $('#textContent sup .' + term.id).removeClass(
                    this.settings.classes.knownClass + ' ' +
                    this.settings.classes.unknownClass + ' ' +
                    this.settings.classes.ignoredClass + ' ' +
                    this.settings.classes.notseenClass +
                    ' box1 box2 box3 box4 box5 box6 box7 box8 box9'
                )
                .addClass(term.stateClass == this.settings.classes.unknownClass ? 'box' + term.box : term.stateClass);

            $('#textContent sup .' + term.id).each(function (index, elem) => {
                var currentWord = $(elem).text();
                $(elem).html(
                (term.definition.length > 0 ? '<a title="' + term.definition + '">' : '') + currentWord + (term.definition.length > 0 ? '</a>' : '')
            );
            });
        }
    }

    public resetTerm() {
        var currentIndex = (<any>$('#tabTermDefintions li.active')).index();
        if (currentIndex < 0) currentIndex = 1;
        $('#iconResult').hide();

        $.post(this.settings.ajaxUrl + '/reset-term',
            {
                languageId: this.settings.languageId,
                termPhrase: this.selectedWord
            },
            function (data) => {
                console.log('reset term');
                console.log(data);
                if (data.result == "OK") {
                    $('#termMessage').removeClass('label-info').addClass('label-success').html(data.message);
                    this.createTemplate(data.data.term);
                    (<any>$('#tabTermDefintions a')).eq(currentIndex).tab('show');
                    this.updateModalDisplay();
                    $('#iconResult').removeClass().addClass('icon-ok-circle').show().fadeOut(2000);
                    this.updateTermClasses(data.data.termPhrase, data.data.term);
                } else {
                    $('#termMessage').removeClass().addClass('label label-important').html(data.message);
                    $('.modal-footer').addClass('failed');
                    $('#iconResult').removeClass().addClass('icon-exclamation-sign icon-white').show().fadeOut(2000);
                }
            });
    }

    public refreshSentence(element) {
        console.log('refresh sentence');
        this.sentence = this.getCurrentSentence();

        if (!$(element).hasClass('refresh')) {
            $(element).addClass('refresh');
        }

        $(element).val(this.sentence);

        this.updateModalDisplay();
    }

    public increaseWord() {
        console.log('increase word');
        if (this.length >= 7) return;
        this.length++;
        this.changePhrase();
        this.findTerm();
    }

    public decreaseWord() {
        console.log('decrease word');
        if (this.length <= 1) return;
        this.length--;
        this.changePhrase();
        this.findTerm();
    }

    private refreshDictionaryLinks() {
        $('.dictionary').each(function (index, a) => {
            var anchor = $(a);
            var id = anchor.data('id');
            var parameter = anchor.data('parameter');
            var urlEncode = anchor.data('urlencode');
            var url = anchor.data('url');
            var auto = anchor.data('autoopen');
            var input = parameter == 'sentence' ? this.selectedSentence : this.selectedWord;

            if (auto == undefined) auto = false;

            if (urlEncode) {
                $.post(
                    this.settings.ajaxUrl + '/encode-term',
                    {
                        languageId: this.settings.languageId,
                        dictionaryId: id,
                        input: input
                    },
                    function (data) {
                        if (data.result == "OK") {
                            anchor.attr('href', data.message);

                            if (auto) {
                                anchor[0].click();
                            }
                        }
                    }
                );
            } else {
                anchor.attr('href', url.replace('###', input));
                if (auto) {
                    anchor[0].click();
                }
            }
        });
    }

    private changePhrase() {
        var currentWord = '';
        var i = 0;
        var next = $(this.element);

        do {
            if (next == null) break;
            if (
                next.attr('class') == this.settings.classes.spaceClass ||
                next.attr('class') == this.settings.classes.punctuationClass ||
                next.attr('class') == this.settings.classes.multiClass ||
                next.is("sup")
                ) { next = next.next(); continue; }

            currentWord += next.text() + ' ';
            next = next.next();
            i++;
        } while (i < this.length);

        this.selectedWord = currentWord;
        this.updateModalDisplay();
    }

    private getCurrentSentence(): string {
        console.log('get current sentence');

        var sentenceNode = $(this.element).closest('.sentence');

        var children = sentenceNode.children();
        var sentence = this.buildSentence(children);

        if (sentence.length < 25) {
            var prev = sentenceNode.prev();

            if (prev != null) {
                sentence = this.buildSentence(prev.children()) + ' ' + sentence;
            }
        }

        if (sentence.length < 25) {
            var next = sentenceNode.next();

            if (prev != null) {
                sentence = sentence + ' ' + this.buildSentence(next.children());
            }
        }

        console.log('get current sentence: ', sentence);
        return sentence;
    }

    private buildSentence(elements: any): string {
        var sentence = '';
        elements.each(function (index, node) {
            if (node.nodeName == 'SUP') return;

            var nodeContent = node.textContent;
            if (nodeContent == '') nodeContent = node.innerText;
            sentence += nodeContent;
        });

        return $.trim(sentence);
    }

    public blankModal() {
        $('#termId').val('');
        $('#termPhrase').val('');
        $('#tabTermDefintions').html('');
        $('#tabContent').html('');
        $('#termMessages').html('');
        $('#selectedWord').html('');
    }
}