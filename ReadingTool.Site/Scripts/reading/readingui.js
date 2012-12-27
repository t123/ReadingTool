var Class = (function () {
    function Class() { }
    return Class;
})();
var Dictionary = (function () {
    function Dictionary() {
    }
    return Dictionary;
})();
var KeyBindings = (function () {
    function KeyBindings() {
        this.autoPause = true;
        this.controlsEnabled = true;
    }
    return KeyBindings;
})();
var Settings = (function () {
    function Settings() {
        this.classes = new Class();
        this.keyBindings = new KeyBindings();
    }
    return Settings;
})();
var ReadingToolUi = (function () {
    function ReadingToolUi(settings) {
        this.isPlaying = false;
        this.wasPlaying = false;
        this.hasChanged = false;
        this.settings = settings;
        if(settings.hasAudio) {
            this.audio = new AudioPlayer();
        } else {
            this.audio = new NullAudioPlayer();
        }
    }
    ReadingToolUi.prototype.toggleL1 = function () {
        var li = $('#toggleL1').parent('li');
        if(li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            if($('#textContent td.s').length > 0) {
                $('#textContent table td.f span').hide();
            } else {
                $('#textContent p span').hide();
            }
        } else {
            li.addClass('active');
            $('td.f p span').show();
        }
    };
    ReadingToolUi.prototype.toggleL2 = function () {
        var li = $('#toggleL2').parent('li');
        if(li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            $('td.s p span').hide();
        } else {
            li.addClass('active');
            $('td.s p span').show();
        }
    };
    ReadingToolUi.prototype.toggleQuickmode = function () {
        var li = $('#quickmode').parent('li');
        this.settings.quickmode = !this.settings.quickmode;
        if(this.settings.quickmode) {
            li.addClass('active');
        } else {
            li.removeClass('active');
        }
    };
    ReadingToolUi.prototype.openTextModal = function (isClick, event) {
        if(!isClick && this.settings.modalBehaviour != 'Rollover') {
            return;
        }
        if(this.settings.quickmode) {
            return;
        }
        if(this.modalVisible) {
            this.closeTextModal();
            return;
        }
        if($(this).hasClass(this.settings.classes.punctuationClass) || $(this).hasClass(this.settings.classes.spaceClass)) {
            return;
        }
        if(isClick) {
            switch(this.settings.modalBehaviour) {
                case 'LeftClick': {
                    if(event.which != 1) {
                        return;
                    }
                    if(event.altKey || event.shiftKey || event.ctrlKey) {
                        return;
                    }
                    break;

                }
                case 'CtrlLeftClick': {
                    if(event.which != 1 || !event.ctrlKey) {
                        return;
                    }
                    if(event.altKey || event.shiftKey) {
                        return;
                    }
                    break;

                }
                case 'ShiftLeftClick': {
                    if(event.which != 1 || !event.shiftKey) {
                        return;
                    }
                    if(event.altKey || event.ctrlKey) {
                        return;
                    }
                    break;

                }
                case 'MiddleClick': {
                    if(event.which != 2) {
                        return;
                    }
                    if(event.altKey || event.shiftKey || event.ctrlKey) {
                        return;
                    }
                    break;

                }
                case 'RightClick': {
                    if(event.which != 3) {
                        return;
                    }
                    if(event.altKey || event.shiftKey || event.ctrlKey) {
                        return;
                    }
                    break;

                }
                default: {
                    return;

                }
            }
            if(this.settings.quickmode) {
                this.reading = new Reading($(this), this.settings);
                if($(this).hasClass(this.settings.classes.notseenClass)) {
                } else {
                }
                ; ;
                return;
            }
        }
        if(this.isPlaying) {
            this.wasPlaying = true;
        }
        if(this.isPlaying && this.settings.keyBindings.autoPause) {
            this.audio.pauseAudio();
        }
        this.reading = new Reading($(this), this.settings);
        selectedWord = new SelectedWord(event.srcElement);
        modal.modal('show');
    };
    ReadingToolUi.prototype.closeTextModal = function () {
        modal.modal('hide');
    };
    return ReadingToolUi;
})();
var SelectedWord = (function () {
    function SelectedWord(element) {
        this.element = element;
        this.length = length;
        this.init();
        this.updateModalDisplay();
    }
    SelectedWord.prototype.init = function () {
        this.selectedWord = $(this.element).html();
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
$('#toggleL1').click(function () {
    ui.toggleL1();
    return false;
});
$('#toggleL2').click(function () {
    ui.toggleL2();
    return false;
});
$('#quickmode').click(function () {
    ui.toggleQuickmode();
    return false;
});
$('#btnSaveAndClose').click(function () {
    selectedWord.saveChanges();
    ui.closeTextModal();
});
$('#btnReset').click(function () {
    selectedWord.resetWord();
});
$('#btnRefresh').click(function () {
    selectedWord.refreshSentence();
});
$('#increaseWord').click(function () {
    selectedWord.increaseWord();
});
$('#decreaseWord').click(function () {
    selectedWord.decreaseWord();
});
var modal;
var selectedWord;
$(function () {
    if(ui.settings.modalBehaviour == 'Rollover') {
        $('#textContent p span span').on('mouseenter', function (event) {
            ui.openTextModal(false, event);
        });
    } else {
        $('#textContent p span span').bind("contextmenu", function (event) {
            return ui.settings.modalBehaviour != 'RightClick';
        });
        $('#textContent p span span').on('mousedown', function (event) {
            ui.openTextModal(true, event);
        });
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
    modal.modal({
        show: false,
        keyboard: true,
        backdrop: false
    });
});
$(document).keyup(function (event) {
    var code = (event.keyCode ? event.keyCode : event.which);
    if(ui.modalVisible) {
        if(code == 27) {
            ui.closeTextModal();
        } else {
            if((event).ctrlKey && code == 13) {
                selectedWord.saveChanges();
                ui.closeTextModal();
            }
        }
    }
});
