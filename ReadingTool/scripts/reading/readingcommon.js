function init() {
    $('#textModal').hide();

    reader.init(
            settings.itemId,
            settings.languageId,
            settings.baseUrl
    );

    if (settings.hasAudio) {
        $("#jquery_jplayer_1").jPlayer({
            ready: function (event) {
                $(this).jPlayer("setMedia", {
                    mp3: settings.url
                });
            },
            swfPath: settings.swfPath,
            solution: "html, flash",
            supplied: "mp3"
        });

        $("#jquery_jplayer_1").bind($.jPlayer.event.timeupdate, function (event) {
            $("#currentTime").text(Math.floor(event.jPlayer.status.currentTime));
        });
    }

    if (settings.hasVideo) {
        $("#jquery_jplayer_1").jPlayer({
            solution: "html, flash",
            supplied: "m4v",
            swfPath: settings.swfPath,
            //errorAlerts: true,
            //warningAlerts: true,
            ready: function () {
                $(this).jPlayer("setMedia", {
                    m4v: settings.url
                });
            }
        });

        $("#jquery_jplayer_1").bind($.jPlayer.event.timeupdate, function (event) {
            var time = event.jPlayer.status.currentTime;
            $("#currentTime").text(Math.floor(time));
            l1subs.some(function (item) {
                if (parseFloat(item.from) < time && parseFloat(item.to) > time) {
                    $('#first').html($('#' + item.id).html());
                    return true;
                }
            });

            l2subs.some(function (item) {
                if (parseFloat(item.from) < time && parseFloat(item.to) > time) {
                    $('#second').html($('#' + item.id).html());
                    return true;
                }
            });
        });
        //$("#jplayer_inspector").jPlayerInspector({ jPlayer: $("#jquery_jplayer_1") });
    }

    if (settings.removeSpaces) {
        $('#removespaces').attr('checked', true);
        $('.wsx').toggle();
    }

    if (settings.keepFocus) {
        $('#keepfocus').attr('checked', true);
    }

    $('#btnSave').click(function (event) { reader.save(); });
    $('#increaseWord').click(function (event) { reader.increaseWord(); });
    $('#decreaseWord').click(function (event) { reader.decreaseWord(); });

    $('#refreshSentence').click(function (event) {
        event.preventDefault();
        reader.refreshSentence();
    });

    $('#markRemainingAsKnown').click(function (event) {
        event.preventDefault();
        reader.markRemainingAsKnown();
    });

    $('#baseWord').keyup(function () { settings.changed = true; });
    $('#definition').keyup(function () { settings.changed = true; });
    $('#romanisation').keyup(function () { settings.changed = true; });
    $('#tags').keyup(function () { settings.changed = true; });
    $('#currentSentence').keyup(function () { settings.changed = true; });

    $('#textModalCloseLink').mouseover(function (event) {
        event.preventDefault();
        if (settings.changed) return;
        closeTextModal();
    });

    $('#textModalCloseLink').click(function (event) {
        event.preventDefault();
        closeTextModal();
    });

    $('#markRemainingAsKnownProgress').click(function (event) { $(this).hide(); });
    $("#jquery_jplayer_1").bind($.jPlayer.event.play, function (event) { settings.isPlaying = true; });
    //$("#jquery_jplayer_1").bind($.jPlayer.event.stop, function (event) { settings.isPlaying = false; }); //Errors in jQuery
    $("#jquery_jplayer_1").bind($.jPlayer.event.pause, function (event) { settings.isPlaying = false; });

    $('#btnRewind').click(function (event) {
        event.preventDefault();
        $("#jquery_jplayer_1").jPlayer("play", parseInt($("#currentTime").text(), 10) - settings.jPlayerTime);
    });

    $('#btnForward').click(function (event) {
        event.preventDefault();
        $("#jquery_jplayer_1").jPlayer("play", parseInt($("#currentTime").text(), 10) + settings.jPlayerTime);
    });

    function closeTextModal() {
        $('#textModal').hide();

        if (settings.autoPause && settings.wasPlaying) {
            $("#jquery_jplayer_1").jPlayer("play", parseInt($("#currentTime").text(), 10) - 0.5);
            $("#jquery_jplayer_1").jPlayer("play");
        }
    }

    $('#textContent p span span').live('mouseenter', function (event) {
        if (settings.modalBehaviour != 'rollover') return;
        var quickmode = $('#quickmode').is(":checked");
        if (quickmode) return;

        if ($('#textModal').is(":visible")) {
            return;
        } else {
            if ($(this).hasClass(settings.punctuationClass) || $(this).hasClass(settings.spaceClass)) return;

            if (settings.isPlaying)
                settings.wasPlaying = true;

            if (settings.isPlaying && settings.autoPause) {
                $("#jquery_jplayer_1").jPlayer("pause");
            }

            reader.createPopup($(this));

            if (settings.autoOpenDictionary) {
                //openDictionary(settings.autoDictionaryWindowName, settings.autoDictionaryUrl, $(this).text());
            }
        }
    });

    $('#textContent p span span').bind("contextmenu", function (event) { return settings.modalBehaviour != 'rightclick'; });

    $('#textContent p span span').live('mousedown', function (event) {
        if ($('#textModal').is(":visible")) {
            closeTextModal();
            return;
        }

        if ($(this).hasClass(settings.punctuationClass) || $(this).hasClass(settings.spaceClass)) return;
        
        switch (settings.modalBehaviour) {
            case 'leftclick':
                if (event.which != 1) return;
                if (event.altKey || event.shiftKey || event.ctrlKey) return;
                break;

            case 'ctrlleftclick':
                if (event.which != 1 || !event.ctrlKey) return;
                if (event.altKey || event.shiftKey) return;
                break;

            case 'shiftleftclick':
                if (event.which != 1 || !event.shiftKey) return;
                if (event.altKey || event.ctrlKey) return;
                break;

            case 'middleclick':
                if (event.which != 2) return;
                if (event.altKey || event.shiftKey || event.ctrlKey) return;
                break;

            case 'rightclick':
                if (event.which != 3) return;
                if (event.altKey || event.shiftKey || event.ctrlKey) return;
                break;

            default: return;
        }

        var quickmode = $('#quickmode').is(":checked");

        if (quickmode) {
            if ($(this).hasClass(settings.notseenClass)) {
                reader.quicksave($(this), settings.unknownClass);
            } else {
                reader.quicksave($(this), settings.notseenClass);
            };
        } else {
            if (settings.isPlaying)
                settings.wasPlaying = true;

            if (settings.autoPause) {
                $("#jquery_jplayer_1").jPlayer("pause");
            }

            reader.createPopup($(this));

            if (settings.autoOpenDictionary) {
//                openDictionary(settings.autoDictionaryWindowName, settings.autoDictionaryUrl, $(this).text());
            }
        }
    });

    if (navigator.userAgent.indexOf('iPad') != -1) {
        $('#textContent p span span').live('touchstart', function(event) {
            if ($('#textModal').is(":visible")) {
                closeTextModal();
                return;
            }

            if ($(this).hasClass(settings.punctuationClass) || $(this).hasClass(settings.spaceClass)) return;

            var quickmode = $('#quickmode').is(":checked");

            if (quickmode) {
                if ($(this).hasClass(settings.notseenClass)) {
                    reader.quicksave($(this), settings.unknownClass);
                } else {
                    reader.quicksave($(this), settings.notseenClass);
                }
                ;
            } else {
                if (settings.isPlaying)
                    settings.wasPlaying = true;

                if (settings.autoPause) {
                    $("#jquery_jplayer_1").jPlayer("pause");
                }

                reader.createPopup($(this));

                if (settings.autoOpenDictionary) {
//                    openDictionary(settings.autoDictionaryWindowName, settings.autoDictionaryUrl, $(this).text());
                }
            }
        });
    }
    
    $('#lnkTrans').click(function () {
        var sentence = $('#currentSentence').val();
        var openUrl = settings.translateUrl.replace('[[text]]', sentence);
        window.open(openUrl, "__translate");

        if ($('#keepfocus').is(":checked")) {
            self.focus();
        }
    });

    $('#lnkShare').click(function () {
        var currentWord = $('#currentWord').text();
        var openUrl = settings.shareUrl + '/' + escape(currentWord) + '/' + settings.languageId;
        window.open(openUrl, "__sharedWords");

        if ($('#keepfocus').is(":checked")) {
            self.focus();
        }
    });

    $(document).keyup(function (event) {
        var code = (event.keyCode ? event.keyCode : event.which);

        if ($('#textModal').is(":visible")) {
            if (code == 27) {
                closeTextModal();
            } else if(event.ctrlKey && code==13) {
                reader.save();
                closeTextModal();
            }
        } else {
            if (settings.controlsEnabled) {
                if (event.altKey || event.shiftKey || event.ctrlKey) return;

                switch (code) {
                    case settings.restart: $("#jquery_jplayer_1").jPlayer("play", 0); break;
                    case settings.rewind: $("#jquery_jplayer_1").jPlayer("play", parseInt($("#currentTime").text(), 10) - settings.jPlayerTime); break;
                    case settings.stop: $("#jquery_jplayer_1").jPlayer("stop"); settings.isPlaying = false; break;
                    case settings.forward: $("#jquery_jplayer_1").jPlayer("play", parseInt($("#currentTime").text(), 10) + settings.jPlayerTime); break;

                    case settings.play:
                        if (settings.isPlaying) {
                            $("#jquery_jplayer_1").jPlayer("pause");
                        } else {
                            $("#jquery_jplayer_1").jPlayer("play");
                        }

                    default: break;
                }
            }
        }
    });

    $('#removespaces').click(function (event) {
        $('.wsx').toggle();
    });
}

function openDictionary(windowName, url, word) {
    if(word==undefined)
        word = $('#currentWord').text();
    
    var openUrl = url.replace('[[word]]', word);
    window.open(openUrl, windowName);

    if ($('#keepfocus').is(":checked")) {
        self.focus();
    }
}