<?php

use RT\Core\FlashMessage;
?>
<!DOCTYPE html>
<html>
    <head>
        <title>Reading Tool - @yield('pageTitle')</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <meta charset="utf-8"> 
        {{ HTML::style('css/bootstrap.css') }}
        {{ HTML::style('css/sb-admin.css') }}
        {{ HTML::style('font-awesome/css/font-awesome.min.css') }}
        {{ HTML::style('css/common.css') }}
        @yield('topCss')
    </head>
    <body>
        <div id="wrapper">
            <nav class="navbar navbar-inverse navbar-fixed-top" role="navigation">
                <div class="navbar-header">
                    <a title="Reading Tool &copy {{ date("Y") }} AGPL 3" class="navbar-brand" href="{{ action('AccountController@index') }}">Reading Tool - {{{ Auth::user()->currentName() }}}</a>
                    
                </div>

                <div class="collapse navbar-collapse navbar-ex1-collapse">
                    <ul class="nav navbar-nav side-nav">
                        <li {{ $currentController=="Account" ? "class=\"active\"" : "" }}><a href="{{ action('AccountController@index') }}">My account</a></li>
                        <li {{ $currentController=="Language" ? "class=\"active\"" : "" }}><a href="{{ action('LanguageController@index') }}">Languages</a></li>
                        <li {{ $currentController=="Text" ? "class=\"active\"" : "" }}><a href="{{ action('TextController@index') }}">Texts</a></li>
                        <li {{ $currentController=="Term" ? "class=\"active\"" : "" }}><a href="{{ action('TermController@index') }}">Terms</a></li>
                        <li {{ $currentController=="Groups" ? "class=\"active\"" : "" }}><a href="{{ action('GroupController@index') }}">My Groups</a></li>
                        <li {{ $currentController=="PublicGroups" ? "class=\"active\"" : "" }}><a href="{{ action('GroupController@findGroups') }}">Public Groups</a></li>
                    </ul>

                    <ul class="nav navbar-nav navbar-right navbar-user">
                        @if(!Request::secure())
                            <li class="dropdown messages-dropdown">
                                <a href="{{ str_replace('http://', 'https://', Request::url()) }}" class="https">switch to https</a>
                            </li>
                        @endif
                        <li class="dropdown messages-dropdown">
                            <a href="{{ action('HomeController@signout') }}">sign out</a>
                        </li>
                    </ul>
                </div>
            </nav>

            <div id="page-wrapper">
                @if(Session::has(FlashMessage::MSG))
                <div class="alert {{ Session::get(FlashMessage::MSG)->getLevel() }} alert-dismissable">
                    <button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>
                    {{ Session::get(FlashMessage::MSG)->getMessage() }}
                </div>
                @endif

                @yield('content')
                
                <div class="clr50"></div>
            </div>
        </div>	
    </body>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    {{ HTML::script('js/bootstrap.js') }}
    @yield('topJs')
    @yield('bottomJs')
</html>
