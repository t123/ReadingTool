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
    asParallel: bool;

    dictionaries: Dictionary[];

    constructor() {
        this.classes = new Class();
        this.keyBindings = new KeyBindings();
    }
}