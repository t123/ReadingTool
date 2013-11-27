<?php
use RT\Core\FlashMessage;

function hasAudio($url) {
    return isset($url) && strlen($url)>0;
}

$readAction = $asParallel ? "TextController@readParallel" : "TextController@read";

?>
<!DOCTYPE html>
<html>
    <head>
        <title>{{{ $text->title }}}</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <meta charset="utf-8"> 
        <link href='http://fonts.googleapis.com/css?family=Droid+Sans' rel='stylesheet' type='text/css'>
        {{ HTML::style('css/common.css') }}
        {{ HTML::style('css/reading.css') }}
        {{ HTML::style('css/blue.monday/jplayer.blue.monday.css') }}
        {{ HTML::style('css/bootstrap.css') }}
        
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
        {{ HTML::script('js/bootstrap.js') }}
        {{ HTML::script('js/jPlayer/jquery.jplayer.min.js') }}
        {{ HTML::script('js/simplemodal/jquery.simplemodal.1.4.4.min.js') }}
        {{ HTML::script('js/reading/state.js') }}
    </head>
    <body>
        <div class="container">
            <div class="row">
                <div class="col-sm-12">
                    <ul class="nav nav-pills">
                        <li><a href="{{ action('AccountController@index') }}">My account</a></li>
                        <li><a href="{{ action('LanguageController@index') }}">Languages</a></li>
                        <li><a href="{{ action('TextController@index') }}">Texts</a></li>
                        <li><a href="{{ action('TermController@index') }}">Terms</a></li>
                        <li><a href="{{ action('GroupController@index') }}">My Groups</a></li>
                        <li><a href="{{ action('GroupController@findGroups') }}">Public Groups</a></li>
                        
                        @if($nextId!=null)
                        <li class="pull-right">
                            <a href="{{ action($readAction, $nextId) }}">
                                next
                            </a>
                        </li>
                        @endif
                        <li style="margin-top: 2px;" class="pull-right">
                            <div class="btn-group">
                                <button class="btn btn-default dropdown-toggle" data-toggle="dropdown">
                                    more options for this text&nbsp;<span class="caret"></span>
                                </button>
                                <ul class="dropdown-menu">
                                    <li>
                                        <a href="{{ action("TextController@edit", $text->id) }}">edit text</a>
                                        <a href="{{ action("TextController@read", $text->id) }}">read single mode</a>
                                        @if($text->l2_id!=null)
                                            <a href="{{ action("TextController@readParallel", $text->id) }}">read in parallel</a>
                                        @endif
                                        <a href="{{ action("TextController@downloadPdf", $text->id) }}">download as PDF</a>
                                        @if($nextId!=null)
                                            <a href="{{ action("TextController@edit", $nextId) }}">next text</a>
                                        @endif
                                        @if($previousId!=null)
                                            <a href="{{ action("TextController@edit", $previousId) }}">previous text</a>
                                        @endif
                                        <a href="{{ action("LanguageController@edit", $language1->id) }}">edit language</a>
                                        <a href="#stats" onclick="$('#stats').show();">go to statistics</a>
                                        @if(hasAudio($text->audioUrl))
                                            <a href="#" onclick="toggleAudio(); return false;">show/hide audio</a>
                                        @endif
                                    </li>
                                </ul>
                            </div>
                        </li>
                        @if($previousId!=null)
                        <li class="pull-right">
                            <a href="{{ action($readAction, $previousId) }}">
                                previous
                            </a>
                        </li>
                        @endif
                    </ul>
                </div>
            </div>
        </div>
        
        <div id="dictionary-content" style="display: none; height:90%;">
            <iframe id="dictionaryiframe" width="100%" height="100%" scrolling="auto">
            </iframe>
        </div>

        <div id="readingareaouter">
            <div id="readingareainner">
                {{ $parsed }}
            </div>
            <div style="background-color: #fff; padding-left:15px;">
                <div class="clr50"></div>
                @if($previousId!=null)
                    <a href="{{ action($readAction, $previousId) }}" class="btn btn-default">previous text</a>
                @endif
                @if($nextId!=null)
                    <a href="{{ action($readAction, $nextId) }}" class="btn btn-default">next text</a>
                @endif
                <div class="clr10"></div>
                <button type="button" id="btnMarkRemainingAsKnown" class="btn btn-default">mark the remaining words as known</button>
                <div class="clr10"></div>
                <div id="markRemainingAsKnownProgress" class="alert alert-block" style="display: none"></div>
                <div class="clr100"></div>
            </div>
        </div>
        <div id="textModal" style="display: none;">
            <input type="hidden" id="termId" value=""/>
            <div class="row">
                <div class="col-sm-6"><strong><span id="currentWord"></span></strong></div>
                <div class="col-sm-6">
                    <ul class="states">
                        <li class="pull-right"><a id="textModalCloseLink" href="#" onclick="return false;">close</a></li>
                        <li class="pull-right"><a id="increaseWord" href="#" onclick="return false;">increase</a></li>
                        <li class="pull-right"><a id="decreaseWord" href="#" onclick="return false;">decrease</a></li>
                    </ul>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-9">
                    <ul class="dictionaries">
                        @foreach($language1->dictionaries as $d)
                        <li>
                            <a target="{{{ hasAudio($d->windowName) ? $d->windowName : "d_" . time() }}}"
                               class="dictionary"
                               data-id="{{ $d->id }}"
                               data-parameter="{{ $d->sentence ? "true" : "false" }}"
                               data-urlEncode="{{ hasAudio($d->encoding) ? "true" : "false" }}"
                               data-url="{{ $d->url }}"
                               data-autoOpen="{{ $d->autoOpen ? "true" : "false" }}">
                                   {{{ $d->name }}}
                            </a>
                        </li>
                        @endforeach
                    </ul>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-2">&nbsp;</div>
                <div class="col-sm-7">
                    <ul class="states">
                        <li class="known"><input type="radio" name="state" id="knownState" value="known"/>known</li>
                        <li class="notknown"><input type="radio" name="state" id="unknownState" value="unknown"/>not known</li>
                        <li class="notseen"><input type="radio" name="state" id="notseenState" value="notseen"/>not seen</li>
                        <li class="ignore"><input type="radio" name="state" id="ignoredState" value="ignore"/>ignore</li>
                    </ul>
                </div>
            </div>
            <div class="row">
                <div class="col-sm-2">Sentence:</div>
                <div class="col-sm-10"><input type="text" name="currentSentence" id="currentSentence" class="form-control input-sm" style="display:inline; width: 85%"/><a href="#" id="refreshSentence">refresh</a></div>
            </div>
            <div class="clr2"></div>
            <div class="row">
                <div class="col-sm-2">Base word:</div>
                <div class="col-sm-10"><input type="text" name="baseWord" id="baseWord" placeholder="nominative form, nominative, dictionary form etc" class="form-control input-sm" style="width: 85%"/></div>
            </div>
            <div class="clr2"></div>
            <div class="row">
                <div class="col-sm-2">Definition:</div>
                <div class="col-sm-10"><textarea name="definition" id="definition" rows="3" placeholder="your definition of the word" class="form-control input-sm" style="width: 85%"></textarea></div>
            </div>
            <div class="clr2"></div>
            <div class="row">
                <div class="col-sm-2">Tags:</div>
                <div class="col-sm-10"><input type="text" name="tags" id="tags" placeholder="comma separated list of tags" class="form-control input-sm" style="width: 85%"/></div>
            </div>
            <div class="clr2"></div>
            <div class="row">
                <div class="col-sm-9" id="saveMessage">
                </div>
                <div class="col-sm-3">
                    <button id="btnSave" title="ctrl-enter to save" class="btn btn-primary btn-sm">save</button>
                    <button id="btnReset" title="reset" class="btn btn-default btn-sm">reset</button>
                </div>
            </div>
        </div>
        
        @if(hasAudio($text->audioUrl))
            <div id="jplayer" class="jp-jplayer"></div>
            <div id="audiocontainer">
                <a href="#" class="closelink" title="toggle audio player">&rArr;</a>
                <div id="jp_container_1" class="jp-audio">
                    <div class="jp-type-single">
                        <div class="jp-gui jp-interface">
                            <ul class="jp-controls">
                                <li><a href="javascript:;" class="jp-play" tabindex="1">play</a></li>
                                <li><a href="javascript:;" class="jp-pause" tabindex="1">pause</a></li>
                                <li><a href="javascript:;" class="jp-stop" tabindex="1">stop</a></li>
                                <li><a href="javascript:;" class="jp-mute" tabindex="1" title="mute">mute</a></li>
                                <li><a href="javascript:;" class="jp-unmute" tabindex="1" title="unmute">unmute</a></li>
                                <li><a href="javascript:;" class="jp-volume-max" tabindex="1" title="max volume">max volume</a></li>
                            </ul>
                            <div class="jp-progress">
                                <div class="jp-seek-bar">
                                    <div class="jp-play-bar"></div>
                                </div>
                            </div>
                            <div class="jp-volume-bar">
                                <div class="jp-volume-bar-value"></div>
                            </div>
                            <div class="jp-time-holder">
                                <div class="jp-current-time"></div>
                                <div class="jp-duration"></div>

                                <ul class="jp-toggles">
                                    <li><a href="javascript:;" class="jp-repeat" tabindex="1" title="repeat">repeat</a></li>
                                    <li><a href="javascript:;" class="jp-repeat-off" tabindex="1" title="repeat off">repeat off</a></li>
                                </ul>
                            </div>
                        </div>
                        <div class="jp-no-solution">
                            <span>Update Required</span>
                            To play the media you will need to either update your browser to a recent version or update your <a href="http://get.adobe.com/flashplayer/" target="_blank">Flash plugin</a>.
                        </div>
                    </div>
                </div>
            </div>
        @endif
    </body>
    <script language="javascript">
            @if(hasAudio($text->audioUrl))
                function toggleAudio() {
                    $('#jp_container_1').toggle();

                    if($('#jp_container_1').is(':visible')) {
                        $('.closelink').html('&rArr;');
                    } else {
                        $('.closelink').html('&lArr;');
                    }
                }
            @endif
            
            $(function () {
                var routes={
                    ajax: {
                            resetTerm: '/ajax/reset-term',
                            saveTerm: '/ajax/save-term',
                            findTerm: '/ajax/find-term',
                            markRemaingAsKnown: '/ajax/mark-remaining-as-known',
                            encodeTerm: '/ajax/encode-term',
                        }
                };
                
                var settings = {
                    textId: '{{ $text->id }}',
                    languageId: '{{ $language1->id }}',
                    modal: {{ $language1->getSettings()->Modal=="on" ? "true" : "false" }},
                    autoPause: {{ $language1->getSettings()->AutoPause=="on" ? "true" : "false" }},
                    modalBehaviour: '{{ strtolower($language1->getSettings()->ModalBehaviour) }}',
                    hasAudio:  {{ hasAudio($text->audioUrl) ? "true" : "false "}}
                };

                var modalHandler = new ModalHandler(routes, settings);

                @if($language1->getSettings()->ShowSpaces!="on")
                    @if($asParallel)
                         $('td.f .wsx').hide();   
                    @else 
                        $('.sentence .wsx').hide();
                    @endif
                @endif
                
                @if($asParallel && $language1->getSettings()->ShowSpaces!="on")
                    $('td.s .wsx').hide();
                @endif

                @if(hasAudio($text->audioUrl))
                    $("#jplayer").jPlayer({
                        ready: function (event) {
                            $(this).jPlayer("setMedia", {
                                mp3:"{{ $text->audioUrl }}"
                            });
                        },
                        swfPath: "/scripts/jPlayer/",
                        supplied: "mp3",
                        wmode: "window",
                        volume: localStorage['jPlayer_Volume'] || 0.5
                    });

                    $('.closelink').click(function(e) {
                        toggleAudio();
                        return false;
                    });
                @endif
            });
        </script>
</html>
