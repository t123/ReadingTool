/// <reference path="settings.ts"/>
/// <reference path="jquery.d.ts"/>

declare var Handlebars;

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

    constructor(settings: Settings, element) {
        this.settings = settings;
        this.element = element;
        this.length = 1;

        this.init();
    }

    private init() {
        this.selectedWord = $(this.element).html();
        this.selectedSentence = this.getCurrentSentence();
        $('#currentBox').removeClass().addClass('badge');

        $.post(
            this.settings.ajaxUrl + '/find-term',
            {
                languageId: this.settings.languageId,
                termPhrase: this.selectedWord,
            }, function (data) => {
                console.log(data);
                //if (data == null) {
                //    //new word
                //    $('#unknownState').attr('checked', true);
                //    $('#baseTerm').val('');
                //    $('#romanisation').val('');
                //    $('#definition').val('');
                //    $('#tags').val('');
                //    $('#sentence').val(this.selectedSentence);

                //    $('#modalMessage').removeClass().addClass('label label-warning').html('new word, defaulted to unknown');
                //} else {
                //    this.selectedTermId = data.id;
                //}

                this.createTemplate(data);
                this.updateModalDisplay();
            });
    }

    private createTemplate(data) {
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
        
        if (data.id == '00000000-0000-0000-0000-000000000000') {
            $('#termMessage').removeClass('label-info').addClass('label-warning');
        }

        for (var i = 0; i < data.individualTerms.length; i++) {
            var it = data.individualTerms[i];
        }

        (<any>$('#tabTermDefintions a:first')).tab('show');
        $('#itermMessage0').show();
    }

    private updateModalDisplay() {
        $('#selectedWord').text(this.selectedWord);

        this.refreshDictionaryLinks();
    }

    public saveChanges() {
        $.post(this.settings.ajaxUrl + '/save-term',
            {
            }, function (data) {
                if (data.result == "OK") {
                    $('#modalMessage').removeClass().addClass('label label-success').html(data.Message);
                    $('#currentBox').removeClass().addClass('badge badge-success').html(data.Data.Box);
                } else {
                    $('#modalMessage').removeClass().addClass('label label-error').html(data.Message);
                }
            });
    }

    public resetWord() {
        console.log('reset word');
    }

    public refreshSentence() {
        console.log('refresh sentence');
        this.sentence = this.getCurrentSentence();

        if (!$('#sentence').parent('div').parent('div').hasClass('warning')) {
            $('#sentence').parent('div').parent('div').addClass('warning');
        }

        this.updateModalDisplay();
    }

    public increaseWord() {
        console.log('increase word');
        if (this.length >= 7) return;
        this.length++;
        this.changePhrase();
    }

    public decreaseWord() {
        console.log('decrease word');
        if (this.length <= 1) return;
        this.length--;
        this.changePhrase();
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
                        if (data.Result == "OK") {
                            anchor.attr('href', data.Message);

                            if (auto) {
                                anchor[0].click();
                                console.log(auto);
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

        var sentenceNode = $(this.element).parent();
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
}