/// <reference path="jquery.d.ts"/>
/// <reference path="readingui.ts"/>

class Reading {
    element: any;
    settings: Settings;
    modal: any;

    constructor(element :any, settings: Settings) {
        this.element = element;
        this.settings = settings;
        this.modal = $('#textModal');
    }
}
