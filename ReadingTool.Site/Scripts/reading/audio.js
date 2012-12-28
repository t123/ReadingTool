var AudioPlayer = (function () {
    function AudioPlayer(element) {
        this.audioElement = element;
    }
    return AudioPlayer;
})();
var NullAudioPlayer = (function () {
    function NullAudioPlayer() { }
    return NullAudioPlayer;
})();
