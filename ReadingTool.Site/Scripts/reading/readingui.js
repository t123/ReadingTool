var _this = this;
var ReadingToolUi = (function () {
    function ReadingToolUi(settings) {
        this.isPlaying = false;
        this.wasPlaying = false;
        this.hasChanged = false;
        this.modalVisible = false;
        this.settings = settings;
        if(settings.hasAudio) {
            this.audio = new AudioPlayer(this.settings);
        } else {
            this.audio = new NullAudioPlayer();
        }
    }
    ReadingToolUi.prototype.toggleL1 = function () {
        var li = $('#toggleL1').parent('li');
        if(li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            if(this.settings.asParallel) {
                if($('#textContent td.s').length > 0) {
                    $('#textContent table td.f span').hide();
                } else {
                    $('#textContent p span').hide();
                }
            } else {
                $('#textContent p span').hide();
            }
        } else {
            li.addClass('active');
            if(this.settings.asParallel) {
                $('td.f p span').show();
            } else {
                $('#textContent p span').show();
            }
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
    ReadingToolUi.prototype.changeRead = function (direction) {
        var words = $('#totalWords').data('value');
        $.post(this.settings.ajaxUrl + '/change-read', {
            textId: this.settings.textId,
            direction: direction,
            words: words
        }, function (data) {
            if(data.result == "OK") {
                $('#timesRead').html(data.message);
            }
        });
    };
    ReadingToolUi.prototype.changeListened = function (direction) {
        $.post(this.settings.ajaxUrl + '/change-listened', {
            textId: this.settings.textId,
            direction: direction
        }, function (data) {
            if(data.result == "OK") {
                $('#timesListened').html(data.message);
            }
        });
    };
    ReadingToolUi.prototype.openTextModal = function (isClick, event) {
        if(this.modalVisible && this.settings.modalBehaviour == 'Rollover') {
            return;
        }
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
        selectedWord = new SelectedWord(this.settings, event.srcElement);
        modal.modal('show');
    };
    ReadingToolUi.prototype.closeTextModal = function () {
        modal.modal('hide');
    };
    return ReadingToolUi;
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
$('#btnSave').click(function () {
    selectedWord.saveChanges();
});
$('#btnReset').click(function () {
    selectedWord.resetWord();
});
$('#increaseWord').click(function () {
    selectedWord.increaseWord();
});
$('#decreaseWord').click(function () {
    selectedWord.decreaseWord();
});
$('#minusRead').click(function () {
    ui.changeRead(-1);
    return false;
});
$('#plusRead').click(function () {
    ui.changeRead(1);
    return false;
});
$('#minusListened').click(function () {
    ui.changeListened(-1);
    return false;
});
$('#plusListened').click(function () {
    ui.changeListened(1);
    return false;
});
var modal;
var selectedWord;
$.fn.animateHighlight = function (highlightColor, duration) {
    var highlightBg = highlightColor || "#FFFF9C";
    var animateMs = duration || 1500;
    var originalBg = this.css("backgroundColor");
    this.stop().css("background-color", highlightBg).animate({
        backgroundColor: originalBg
    }, animateMs);
};
var termUlTemplate = Handlebars.compile($("#term-ul-template").html());
var termDivTemplate = Handlebars.compile($("#term-div-template").html());
var termMessageTemplate = Handlebars.compile($("#term-message-template").html());
$(function () {
    if(ui.settings.modalBehaviour == 'Rollover') {
        $(document).on('mouseenter', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) {
            ui.openTextModal(false, event);
        });
    } else {
        $(document).on('contextmenu', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) {
            return ui.settings.modalBehaviour != 'RightClick';
        });
        $(document).on('mousedown', '#textContent .knx, #textContent .nkx, #textContent .nsx, #textContent .igx, #textContent .mxx', function (event) {
            ui.openTextModal(true, event);
        });
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
$('#tabTermDefintions').on("click", "li", function (e) {
    e.preventDefault();
    ($(_this)).tab('show');
    var message = $(e.target).data('messageid');
    $('.itermMessage').hide();
    $('#' + message).show();
});
