/// <reference path="settings.ts"/>
/// <reference path="jquery.d.ts"/>

class SelectedWord {
    settings: Settings;
    element: any;
    selectedWord: string;
    sentence: string;
    baseTerm: string;
    romanisation: string;
    definition: string;
    tags: string;
    length: number;

    constructor(settings: Settings, element) {
        this.settings = settings;
        this.element = element;
        this.length = length;

        this.init();
    }

    private init() {
        this.selectedWord = $(this.element).html();

        $.post(
            this.settings.ajaxUrl + '/find-term',
            {
                languageId: this.settings.languageId,
                termPhrase: this.selectedWord,
            }, function (data) => {
                console.log(data);
                if (data == null) {
                    //new word
                    $('#unknownState').attr('checked', true);
                    $('#baseTerm').val('');
                    $('#romanisation').val('');
                    $('#definition').val('');
                    $('#tags').val('');
                    $('#sentence').val('');

                    $('#modalMessage').removeClass().addClass('label label-warning').html('new word, defaulted to unknown');
                } else {
                }

                this.updateModalDisplay();
            });
    }

    private updateModalDisplay() {
        $('#selectedWord').html(this.selectedWord);
    }

    public saveChanges() {
        console.log('save changes');
    }

    public resetWord() {
        console.log('reset word');
    }

    public refreshSentence() {
        console.log('refresh sentence');
    }

    public increaseWord() {
        console.log('increase word');
    }

    public decreaseWord() {
        console.log('decrease word');
    }
}