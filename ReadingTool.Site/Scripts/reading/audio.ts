/// <reference path="jquery.d.ts"/>
/// <reference path="settings.ts"/>
/// <reference path="readingui.ts"/>

declare var routes: any;
declare var ui: ReadingToolUi;

interface IAudioPlayer {
    pauseAudio();
    resumeAudio();
    increaseVolume();
    decreaseVolume();
    speedUpAudio();
    slowDownAudio();
    restartAudio();
    rewindAudio();
    stopAudio();
    fastForwardAudio();
    playAudio(): bool;
}

class AudioPlayer implements IAudioPlayer {
    private audioPlayer: HTMLAudioElement;
    private settings: Settings;

    constructor(settings: Settings) {
        this.settings = settings;
        this.audioPlayer = <HTMLAudioElement>document.getElementById('audioPlayer');
        if (this.audioPlayer == null) {
            throw "Audio player could not be initialised";
        }

        this.audioPlayer.addEventListener("loadedmetadata", function (e) => {
            var duration = this.audioPlayer.duration;

            $.post(routes.reading.saveAudioLength,
                {
                    textId: this.settings.textId,
                    length: duration
                }, function () {
                }
            );
        });

        if (this.settings.keyBindings.autoPause) {
            this.audioPlayer.addEventListener("play", function (e) => {
                ui.setIsPlaying(true);
                ui.setWasPlaying(false);
            });

            this.audioPlayer.addEventListener("pause", function (e) => {
                if (ui.isPlaying) {
                    ui.setWasPlaying(true);
                }
                ui.setIsPlaying(false);
            });

            this.audioPlayer.addEventListener("ended", function (e) => {
                ui.setIsPlaying(false);
                ui.setWasPlaying(false);
            });
        }
    }

    public pauseAudio() {
        this.audioPlayer.pause();
    }

    public resumeAudio() {
        if (this.settings.keyBindings.autoPause && ui.getWasPlaying()) {
            this.audioPlayer.currentTime = this.audioPlayer.currentTime - 0.5;
            this.audioPlayer.play();
        }
    }

    public increaseVolume() {
        if (this.audioPlayer.volume < 1) {
            this.audioPlayer.volume = this.audioPlayer.volume + 0.1;
        }
    }

    public decreaseVolume() {
        if (this.audioPlayer.volume > 0) {
            this.audioPlayer.volume = this.audioPlayer.volume - 0.1;
        }
    }

    public speedUpAudio() {
        if (this.audioPlayer.playbackRate < 2) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate + 0.1;
        }
    }

    public slowDownAudio() {
        if (this.audioPlayer.playbackRate > 0) {
            this.audioPlayer.playbackRate = this.audioPlayer.playbackRate - 0.1;
        }
    }

    public restartAudio() {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.play();
    }

    public rewindAudio() {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime - this.settings.keyBindings.secondsToRewind;
    }

    public stopAudio() {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.pause();
        ui.setIsPlaying(false);
        ui.setWasPlaying(false);
    }

    public fastForwardAudio() {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime + this.settings.keyBindings.secondsToRewind;
    }

    public playAudio(): bool {
        if (ui.getIsPlaying()) {
            this.audioPlayer.pause();
            return false;
        } else {
            this.audioPlayer.play();
            return true;
        }
    }
}

class NullAudioPlayer implements IAudioPlayer {
    public pauseAudio() { return; }
    public resumeAudio() { return; }
    public increaseVolume() { return; }
    public decreaseVolume() { return; }
    public speedUpAudio() { return; }
    public slowDownAudio() { return; }
    public restartAudio() { return; }
    public rewindAudio() { return; }
    public stopAudio() { return; }
    public fastForwardAudio() { return; }
    public playAudio() { return true; }
}