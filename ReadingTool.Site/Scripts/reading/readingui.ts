/// <reference path="settings.ts"/>
/// <reference path="jquery.d.ts"/>
/// <reference path="audio.ts"/>
/// <reference path="modal.ts"/>

/// see http://typescript.codeplex.com/SourceControl/changeset/view/92d9e637f6e1#typings/jquery.d.ts

declare var ui: ReadingToolUi;

class ReadingToolUi {
    isPlaying: bool;
    wasPlaying: bool;
    hasChanged: bool;
    modalVisible: bool;

    settings: Settings;

    audio: IAudioPlayer;

    constructor(settings: Settings) {
        this.isPlaying = false;
        this.wasPlaying = false;
        this.hasChanged = false;
        this.modalVisible = false;

        this.settings = settings;

        if (settings.hasAudio) {
            this.audio = new AudioPlayer(this.settings);
        } else {
            this.audio = new NullAudioPlayer();
        }
    }

    toggleL1() {
        var li = $('#toggleL1').parent('li');

        if (li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();

            if (this.settings.asParallel) {
                if ($('#textContent td.s').length > 0) {
                    $('#textContent table td.f span').hide();
                } else {
                    $('#textContent p span').hide();
                }
            } else {
                $('#textContent p span').hide();
            }
        } else {
            li.addClass('active');
            if (this.settings.asParallel) {
                $('td.f p span').show();
            } else {
                $('#textContent p span').show();
            }
        }
    }

    toggleL2() {
        var li = $('#toggleL2').parent('li');

        if (li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            $('td.s p span').hide();
        } else {
            li.addClass('active');
            $('td.s p span').show();
        }
    }

    toggleQuickmode() {
        var li = $('#quickmode').parent('li');
        this.settings.quickmode = !this.settings.quickmode;
        if (this.settings.quickmode) {
            li.addClass('active');
        } else {
            li.removeClass('active');
        }
    }

    changeRead(direction: number) {
        var words = $('#totalWords').data('value');
        $.post(this.settings.ajaxUrl + '/change-read',
        {
            textId: this.settings.textId,
            direction: direction,
            words: words
        }, function (data) => {
            if (data.result == "OK") {
                $('#timesRead').html(data.message);
            }
        });
    }

    changeListened(direction: number) {
        $.post(this.settings.ajaxUrl + '/change-listened',
        {
            textId: this.settings.textId,
            direction: direction
        }, function (data) => {
            if (data.result == "OK") {
                $('#timesListened').html(data.message);
            }
        });
    }

    openTextModal(isClick: bool, event: any) {
        if (this.modalVisible && this.settings.modalBehaviour == 'Rollover') return;
        if (!isClick && this.settings.modalBehaviour != 'Rollover') return;

        if (this.settings.quickmode) {
            selectedWord = new SelectedWord(this.settings, event.srcElement, true);
            return;
        }

        if (this.modalVisible) {
            this.closeTextModal();
            return;
        }

        if (
            $(this).hasClass(this.settings.classes.punctuationClass) ||
            $(this).hasClass(this.settings.classes.spaceClass))
            return;

        if (isClick) {
            switch (this.settings.modalBehaviour) {
                case 'LeftClick':
                    if (event.which != 1) return;
                    if (event.altKey || event.shiftKey || event.ctrlKey) return;
                    break;

                case 'CtrlLeftClick':
                    if (event.which != 1 || !event.ctrlKey) return;
                    if (event.altKey || event.shiftKey) return;
                    break;

                case 'ShiftLeftClick':
                    if (event.which != 1 || !event.shiftKey) return;
                    if (event.altKey || event.ctrlKey) return;
                    break;

                case 'MiddleClick':
                    if (event.which != 2) return;
                    if (event.altKey || event.shiftKey || event.ctrlKey) return;
                    break;

                case 'RightClick':
                    if (event.which != 3) return;
                    if (event.altKey || event.shiftKey || event.ctrlKey) return;
                    break;

                default: return;
            }
        }

        if (this.isPlaying) {
            this.wasPlaying = true;
        }

        if (this.isPlaying && this.settings.keyBindings.autoPause) {
            this.audio.pauseAudio();
        }

        selectedWord = new SelectedWord(this.settings, event.srcElement, false);
        modal.modal('show');
    }

    closeTextModal() {
        modal.modal('hide');
    }

    public markRemainingAsKnown() {
        if (this.modalVisible) return;
        
        $('#altMessageArea').removeClass().addClass('alert alert-info').html('saving....').show();

        var terms = [];
        $('#textContent .' + this.settings.classes.notseenClass).each(function (i, elem) => {
            if ($(this).is('sup')) return;
            terms.push($(elem).text());
        });

        $.post(
            this.settings.ajaxUrl + '/mark-remaining-as-known',
            {
                languageId: this.settings.languageId,
                terms: terms,
                textId: this.settings.textId
            }, function (data) {
                if (data.result == 'OK') {
                    $('#altMessageArea').addClass('alert alert-success');
                    $('#textContent .nsx').removeClass('nsx').addClass('knx');
                } else {
                    $('#altMessageArea').addClass('alert alert-error');
                }

                $('#altMessageArea').html(data.message);
            }
        );
    }

    public reviewUnknown() {
        if (this.modalVisible) return;
        
        $('#altMessageArea').removeClass().addClass('alert alert-info').html('review unknown words....').show();

        var terms = [];
        $('#textContent .' + this.settings.classes.unknownClass).each(function (i, elem) => {
            if ($(this).is('sup')) return;
            terms.push($(elem).text());
        });

        $.post(
            this.settings.ajaxUrl + '/review-unknown',
            {
                languageId: this.settings.languageId,
                terms: terms,
                textId: this.settings.textId
            }, function (data) {
                if (data.result == 'OK') {
                    $('#altMessageArea').addClass('alert alert-success');
                    $('#textContent .nsx').removeClass('nsx').addClass('knx');
                } else {
                    $('#altMessageArea').addClass('alert alert-error');
                }

                $('#altMessageArea').html(data.message);
            }
        );
    }
}

$('#toggleL1').click(function () { ui.toggleL1(); return false; });
$('#toggleL2').click(function () { ui.toggleL2(); return false; });
$('#quickmode').click(function () { ui.toggleQuickmode(); return false; });
$('#reviewUnknownWords').click(function () { ui.reviewUnknown(); return false; });
$('#markRemainingAsKnown').click(function () { ui.markRemainingAsKnown(); return false; });
$('#btnSave').click(function () { selectedWord.saveChanges(); });
$('#btnSaveClose').click(function () { selectedWord.saveChanges(); ui.closeTextModal(); });
$('#btnReset').click(function () { selectedWord.resetTerm(); });
$('#increaseWord').click(function () { selectedWord.increaseWord(); });
$('#decreaseWord').click(function () { selectedWord.decreaseWord(); });
$('#minusRead').click(function () { ui.changeRead(-1); return false; });
$('#plusRead').click(function () { ui.changeRead(+1); return false; });
$('#minusListened').click(function () { ui.changeListened(-1); return false; });
$('#plusListened').click(function () { ui.changeListened(+1); return false; });

var modal;
var selectedWord: SelectedWord;

$.fn.animateHighlight = function (highlightColor, duration) {
    var highlightBg = highlightColor || "#FFFF9C";
    var animateMs = duration || 1500;
    var originalBg = this.css("backgroundColor");
    this.stop().css("background-color", highlightBg).animate({ backgroundColor: originalBg }, animateMs);
};

var termUlTemplate = Handlebars.compile($("#term-ul-template").html());
var termDivTemplate = Handlebars.compile($("#term-div-template").html());
var termMessageTemplate = Handlebars.compile($("#term-message-template").html());



$(function () {
    //Bind the mouse rollover to the spans
    if (ui.settings.modalBehaviour == 'Rollover') {
        $(document).on('mouseenter', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) { ui.openTextModal(false, event); });
    } else {
        //Stop the context menu on rightclick
        $(document).on('contextmenu', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) { return ui.settings.modalBehaviour != 'RightClick'; });

        //Bind the clicks
        $(document).on('mousedown', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) { ui.openTextModal(true, event); });
    }

    modal = $("#myModal");

    modal.on("show", function () {
        ui.modalVisible = true;
    });

    modal.on("hide", function () {
        selectedWord.blankModal();
        modal.hide();
        ui.modalVisible = false;
        ui.audio.resumeAudio(ui.settings.keyBindings.autoPause, ui.wasPlaying);
    });

    modal.modal({ show: false, keyboard: true, backdrop: false });
});

$(document).keyup(function (event) {
    var code = (event.keyCode ? event.keyCode : event.which);
    if (ui.modalVisible) {
        if (code == 27) {
            ui.closeTextModal();
        } else if ((<any>event).ctrlKey && code == 13) {
            selectedWord.saveChanges();
            ui.closeTextModal();
        }
    }
});

$('#tabTermDefintions').on("click", "li", function (e) => {
    e.preventDefault();
    (<any>$(this)).tab('show');
    var message = $(e.target).data('messageid');
    $('.itermMessage').hide();
    $('#' + message).show();
});