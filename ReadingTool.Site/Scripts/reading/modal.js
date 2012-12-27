var SelectedWord = (function () {
    function SelectedWord(settings, element) {
        this.settings = settings;
        this.element = element;
        this.length = length;
        this.init();
    }
    SelectedWord.prototype.init = function () {
        var _this = this;
        this.selectedWord = $(this.element).html();
        $.post(this.settings.ajaxUrl + '/find-term', {
            languageId: this.settings.languageId,
            termPhrase: this.selectedWord
        }, function (data) {
            console.log(data);
            if(data == null) {
                $('#unknownState').attr('checked', true);
                $('#baseTerm').val('');
                $('#romanisation').val('');
                $('#definition').val('');
                $('#tags').val('');
                $('#sentence').val('');
                $('#modalMessage').removeClass().addClass('label label-warning').html('new word, defaulted to unknown');
            } else {
            }
            _this.updateModalDisplay();
        });
    };
    SelectedWord.prototype.updateModalDisplay = function () {
        $('#selectedWord').html(this.selectedWord);
    };
    SelectedWord.prototype.saveChanges = function () {
        console.log('save changes');
    };
    SelectedWord.prototype.resetWord = function () {
        console.log('reset word');
    };
    SelectedWord.prototype.refreshSentence = function () {
        console.log('refresh sentence');
    };
    SelectedWord.prototype.increaseWord = function () {
        console.log('increase word');
    };
    SelectedWord.prototype.decreaseWord = function () {
        console.log('decrease word');
    };
    return SelectedWord;
})();
