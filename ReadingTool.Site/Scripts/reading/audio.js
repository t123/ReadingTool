var _this = this;
var AudioPlayer = (function () {
    function AudioPlayer(settings) {
        var _this = this;
        this.settings = settings;
        this.audioPlayer = document.getElementById('audioPlayer');
        if(this.audioPlayer == null) {
            throw "Audio player could not be initialised";
        }
        this.audioPlayer.addEventListener("loadedmetadata", function (e) {
            var duration = _this.audioPlayer.duration;
            $.post(routes.reading.saveAudioLength, {
                textId: _this.settings.textId,
                length: duration
            }, function () {
            });
        });
        if(this.settings.keyBindings.autoPause) {
            this.audioPlayer.addEventListener("play", function (e) {
                ui.setIsPlaying(true);
                ui.setWasPlaying(false);
            });
            this.audioPlayer.addEventListener("pause", function (e) {
                if(ui.isPlaying) {
                    ui.setWasPlaying(true);
                }
                ui.setIsPlaying(false);
            });
            this.audioPlayer.addEventListener("ended", function (e) {
                ui.setIsPlaying(false);
                ui.setWasPlaying(false);
            });
        }
    }
    AudioPlayer.prototype.pauseAudio = function () {
        this.audioPlayer.pause();
    };
    AudioPlayer.prototype.resumeAudio = function () {
        if(this.settings.keyBindings.autoPause && ui.getWasPlaying()) {
            this.audioPlayer.currentTime = this.audioPlayer.currentTime - 0.5;
            this.audioPlayer.play();
        }
    };
    AudioPlayer.prototype.increaseVolume = function () {
        if(this.audioPlayer.volume < 1) {
            this.audioPlayer.volume = this.audioPlayer.volume + 0.1;
        }
    };
    AudioPlayer.prototype.decreaseVolume = function () {
        if(this.audioPlayer.volume > 0) {
            this.audioPlayer.volume = this.audioPlayer.volume - 0.1;
        }
    };
    AudioPlayer.prototype.speedUpAudio = function () {
        if(this.audioPlayer.playbackRate < 2) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate + 0.1;
        }
    };
    AudioPlayer.prototype.slowDownAudio = function () {
        if(this.audioPlayer.playbackRate > 0) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate - 0.1;
        }
    };
    AudioPlayer.prototype.restartAudio = function () {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.play();
    };
    AudioPlayer.prototype.rewindAudio = function () {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime - this.settings.keyBindings.secondsToRewind;
    };
    AudioPlayer.prototype.stopAudio = function () {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.pause();
        ui.setIsPlaying(false);
        ui.setWasPlaying(false);
    };
    AudioPlayer.prototype.fastForwardAudio = function () {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime + this.settings.keyBindings.secondsToRewind;
    };
    AudioPlayer.prototype.playAudio = function () {
        if(ui.getIsPlaying()) {
            this.audioPlayer.pause();
            return false;
        } else {
            this.audioPlayer.play();
            return true;
        }
    };
    return AudioPlayer;
})();
var NullAudioPlayer = (function () {
    function NullAudioPlayer() { }
    NullAudioPlayer.prototype.pauseAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.resumeAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.increaseVolume = function () {
        return;
    };
    NullAudioPlayer.prototype.decreaseVolume = function () {
        return;
    };
    NullAudioPlayer.prototype.speedUpAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.slowDownAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.restartAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.rewindAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.stopAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.fastForwardAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.playAudio = function () {
        return true;
    };
    return NullAudioPlayer;
})();
