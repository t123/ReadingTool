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
            });
            this.audioPlayer.addEventListener("pause", function (e) {
                ui.setIsPlaying(false);
            });
            this.audioPlayer.addEventListener("ended", function (e) {
                ui.setIsPlaying(false);
            });
        }
    }
    AudioPlayer.prototype.pauseAudio = function () {
        this.audioPlayer.pause();
    };
    AudioPlayer.prototype.resumeAudio = function (autoPause, wasPlaying) {
        if(autoPause && wasPlaying) {
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
    AudioPlayer.prototype.rewindAudio = function (seconds) {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime - seconds;
    };
    AudioPlayer.prototype.stopAudio = function () {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.pause();
    };
    AudioPlayer.prototype.fastForwardAudio = function (seconds) {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime + seconds;
    };
    AudioPlayer.prototype.playAudio = function (isPlaying) {
        if(isPlaying) {
            this.audioPlayer.pause();
            return false;
        } else {
            isPlaying = true;
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
    NullAudioPlayer.prototype.resumeAudio = function (autoPause, wasPlaying) {
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
    NullAudioPlayer.prototype.rewindAudio = function (seconds) {
        return;
    };
    NullAudioPlayer.prototype.stopAudio = function () {
        return;
    };
    NullAudioPlayer.prototype.fastForwardAudio = function (seconds) {
        return;
    };
    NullAudioPlayer.prototype.playAudio = function (isPlaying) {
        return true;
    };
    return NullAudioPlayer;
})();
