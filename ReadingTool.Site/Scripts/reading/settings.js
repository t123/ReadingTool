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
