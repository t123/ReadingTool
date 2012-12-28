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
        this.reading = new Reading();
        if(settings.hasAudio) {
        }
    }
    ReadingToolUi.prototype.toggleL1 = function () {
        var li = $('#toggleL1').parent('li');
        if(li.hasClass('active')) {
            li.removeClass('active');
            this.closeTextModal();
            $('td.f p span').hide();
            $('#textContent p span').hide();
        } else {
            li.addClass('active');
            $('td.f p span').show();
            $('#textContent p span').show();
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
    ReadingToolUi.prototype.openTextModal = function (isClick) {
        if(!isClick && this.settings.modalBehaviour != 'Rollover') {
            return;
        }
        if(this.settings.quickmode) {
            return;
        }
        if($('#textModal').is(":visible")) {
            this.closeTextModal();
            return;
        }
        if($(this).hasClass(this.settings.classes.punctuationClass) || $(this).hasClass(this.settings.classes.spaceClass)) {
            return;
        }
        if(this.isPlaying) {
            this.wasPlaying = true;
        }
        if(this.isPlaying && this.settings.keyBindings.autoPause) {
            this.audioPlayer.pause();
        }
    };
    ReadingToolUi.prototype.closeTextModal = function () {
    };
    ReadingToolUi.prototype.pauseAudio = function () {
        if(this.audioPlayer == null) {
            return;
        }
        this.audioPlayer.pause();
    };
    ReadingToolUi.prototype.resumeAudio = function () {
        if(this.audioPlayer == null) {
            return;
        }
        if(this.settings.keyBindings.autoPause && this.wasPlaying) {
            this.audioPlayer.currentTime = this.audioPlayer.currentTime - 0.5;
            this.audioPlayer.play();
        }
    };
    ReadingToolUi.prototype.increaseVolume = function () {
        if(this.audioPlayer == null) {
            return;
        }
        if(this.audioPlayer.volume < 1) {
            this.audioPlayer.volume = this.audioPlayer.volume + 0.1;
        }
    };
    ReadingToolUi.prototype.decreaseVolume = function () {
        if(this.audioPlayer == null) {
            return;
        }
        if(this.audioPlayer.volume > 0) {
            this.audioPlayer.volume = this.audioPlayer.volume - 0.1;
        }
    };
    ReadingToolUi.prototype.speedUpAudio = function () {
        if(this.audioPlayer == null) {
            return;
        }
        if(this.audioPlayer.playbackRate < 2) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate + 0.1;
        }
    };
    ReadingToolUi.prototype.slowDownAudio = function () {
        if(this.audioPlayer == null) {
            return;
        }
        if(this.audioPlayer.playbackRate > 0) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate - 0.1;
        }
    };
    ReadingToolUi.prototype.restartAudio = function () {
        if(this.audioPlayer == null) {
            return;
        }
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.play();
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
$('#textContent p span span').bind("contextmenu", function (event) {
    return ui.settings.modalBehaviour != 'RightClick';
});
$('#textContent p span span').on('mousedown', function (event) {
    ui.openTextModal(true);
});
$('#textContent p span span').on('mouseenter', function (event) {
    ui.openTextModal(false);
});
