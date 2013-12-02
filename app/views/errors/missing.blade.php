<?php
$currentController = '';
?>
<!DOCTYPE html>
<html>
    <head>
        <title>Reading Tool - 404 - Page Not Found</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <meta charset="utf-8"> 
        {{ HTML::style('css/bootstrap.css') }}
        {{ HTML::style('css/sb-admin.css') }}
        {{ HTML::style('css/common.css') }}
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
                        <li class="dropdown messages-dropdown">
                            <a href="{{ action('HomeController@signout') }}">sign out</a>
                        </li>
                    </ul>
                </div>
            </nav>

            <div id="page-wrapper">
                <div class="clr50"></div>
                <div class="row">
                    <div class="col-lg-10 col-lg-offset-1">
                        <div class="media">
                            <a class="pull-left">
                                <img src="/img/misc-jackie-chan.png" alt="Page not found" title="Page not found" class="media-object" style="width: 250px"/>
                            </a>
                            <div class="media-body">
                                <div class="clr50"></div>
                                <h4 class="media-heading">Page not found.</h4>
                                Sorry, it seems that page does not exist on the site.
                            </div>
                        </div>
                    </div>
                </div>
                <div class="clr50"></div>
            </div>
        </div>	
    </body>
</html>
