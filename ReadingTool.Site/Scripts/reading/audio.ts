/// <reference path="jquery.d.ts"/>
/// <reference path="settings.ts"/>

declare var routes: any;

interface IAudioPlayer {
    pauseAudio();
    resumeAudio(autoPause: bool, wasPlaying: bool);
    increaseVolume();
    decreaseVolume();
    speedUpAudio();
    slowDownAudio();
    restartAudio();
    rewindAudio(seconds: number);
    stopAudio();
    fastForwardAudio(seconds: number);
    playAudio(isPlaying: bool): bool;
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
    }

    public pauseAudio() {
        this.audioPlayer.pause();
    }

    public resumeAudio(autoPause: bool, wasPlaying: bool) {
        if (autoPause && wasPlaying) {
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

    public rewindAudio(seconds: number) {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime - seconds;
    }

    public stopAudio() {
        this.audioPlayer.currentTime = 0;
        this.audioPlayer.pause();
    }

    public fastForwardAudio(seconds: number) {
        this.audioPlayer.currentTime = this.audioPlayer.currentTime + seconds;
    }

    public playAudio(isPlaying: bool): bool {
        if (isPlaying) {
            this.audioPlayer.pause();
            return false;
        } else {
            isPlaying = true;
            this.audioPlayer.play();
            return true;
        }
    }
}

class NullAudioPlayer implements IAudioPlayer {
    public pauseAudio() { return; }
    public resumeAudio(autoPause: bool, wasPlaying: bool) { return; }
    public increaseVolume() { return; }
    public decreaseVolume() { return; }
    public speedUpAudio() { return; }
    public slowDownAudio() { return; }
    public restartAudio() { return; }
    public rewindAudio(seconds: number) { return; }
    public stopAudio() { return; }
    public fastForwardAudio(seconds: number) { return; }
    public playAudio(isPlaying: bool) { return true; }
}