/// <reference path="reading.ts"/>
/// <reference path="jquery.d.ts"/>
/// <reference path="audio.ts"/>

/// see http://typescript.codeplex.com/SourceControl/changeset/view/92d9e637f6e1#typings/jquery.d.ts

declare var ui: ReadingToolUi;

class Class {
    knownClass: string;
    unknownClass: string;
    ignoredClass: string;
    notseenClass: string;
    spaceClass: string;
    punctuationClass: string;
    multiClass: string;
}

class Dictionary {
    id: number;
    url: string;
    name: string;
    windowName: string;
    urlEncoding: string;
    parameter: string;
    autoOpen: bool;
    displayOrder: number;

    constructor() {
    }
}


class KeyBindings {
    autoPause: bool;
    controlsEnabled: bool;

    constructor() {
        this.autoPause = true;
        this.controlsEnabled = true;
    }
}

class Settings {
    classes: Class;
    keyBindings: KeyBindings;

    hasAudio: bool;
    audioUrl: string;
    textId: number;
    languageId: number;
    modalBehaviour: string;
    ajaxUrl: string;
    quickmode: bool;

    dictionaries: Dictionary[];

    constructor() {
        this.classes = new Class();
        this.keyBindings = new KeyBindings();
    }
}

class ReadingToolUi {
    isPlaying: bool;
    wasPlaying: bool;
    hasChanged: bool;
    modalVisible: bool;

    reading: Reading;
    settings: Settings;

    audio: IAudioPlayer;

    constructor(settings: Settings) {
        this.isPlaying = false;
        this.wasPlaying = false;
        this.hasChanged = false;

        this.settings = settings;

        if (settings.hasAudio) {
            this.audio = new AudioPlayer();
        } else {
            this.audio = new NullAudioPlayer();
        }
    }

    toggleL1() {
        var li = $('#toggleL1').parent('li');

        if (li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            if ($('#textContent td.s').length > 0) {
                $('#textContent table td.f span').hide();
            } else {
                $('#textContent p span').hide();
            }
        } else {
            li.addClass('active');
            $('td.f p span').show();
            //$('#textContent p span').show();
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

    openTextModal(isClick: bool, event: any) {
        if (!isClick && this.settings.modalBehaviour != 'Rollover') return;
        if (this.settings.quickmode) return;

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

            if (this.settings.quickmode) {
                this.reading = new Reading($(this), this.settings);

                if ($(this).hasClass(this.settings.classes.notseenClass)) {
                    //reader.quicksave($(this), settings.unknownClass);
                } else {
                    //reader.quicksave($(this), settings.notseenClass);
                };

                return;
            }
        }

        if (this.isPlaying) {
            this.wasPlaying = true;
        }

        if (this.isPlaying && this.settings.keyBindings.autoPause) {
            this.audio.pauseAudio();
        }

        this.reading = new Reading($(this), this.settings);
        selectedWord = new SelectedWord(event.srcElement);
        modal.modal('show');
    }

    closeTextModal() {
        modal.modal('hide');
    }
}

class SelectedWord {
    element: any;
    selectedWord: string;
    sentence: string;
    baseTerm: string;
    romanisation: string;
    definition: string;
    tags: string;
    length: number;

    constructor(element) {
        this.element = element;
        this.length = length;

        this.init();
        this.updateModalDisplay();
    }

    private init() {
        this.selectedWord = $(this.element).html();
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

$('#toggleL1').click(function () { ui.toggleL1(); return false; });
$('#toggleL2').click(function () { ui.toggleL2(); return false; });
$('#quickmode').click(function () { ui.toggleQuickmode(); return false; });
$('#btnSaveAndClose').click(function () { selectedWord.saveChanges(); ui.closeTextModal(); });
$('#btnReset').click(function () { selectedWord.resetWord(); });
$('#btnRefresh').click(function () { selectedWord.refreshSentence(); });
$('#increaseWord').click(function () { selectedWord.increaseWord(); });
$('#decreaseWord').click(function () { selectedWord.decreaseWord(); });

var modal;
var selectedWord: SelectedWord;

$(function () {
    //Bind the mouse rollover to the spans
    if (ui.settings.modalBehaviour == 'Rollover') {
        $('#textContent p span span').on('mouseenter', function (event) { ui.openTextModal(false, event); });
    } else {
        //Stop the context menu on rightclick
        $('#textContent p span span').bind("contextmenu", function (event) { return ui.settings.modalBehaviour != 'RightClick'; });

        //Bind the clicks
        $('#textContent p span span').on('mousedown', function (event) { ui.openTextModal(true, event); });
    }

    modal = $("#myModal");

    modal.on("show", function () {
        ui.modalVisible = true;
        modal.show();
    });

    modal.on("hide", function () {
        console.log('hide');
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