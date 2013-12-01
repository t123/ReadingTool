<?php

use RT\Core\FlashMessage;
?>
<!DOCTYPE html>
<html>
    <head>
        <title>Reading Tool</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <meta charset="utf-8"> 
        {{ HTML::style('css/bootstrap.css'); }}
        {{ HTML::style('css/sb-admin.css'); }}
        {{ HTML::style('css/common.css'); }}
    </head>
    <body>
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <h1><a href="{{ URL::to('/') }}" title="Reading Tool &copy {{ date("Y") }} AGPL 3">Reading Tool</a></h1>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="clr20"></div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    {{ Form::open(array('action'=>'HomeController@signin', 'class'=>'form-horizontal', 'role'=>'form')) }}

                    <legend>Sign in to your account</legend>

                    @if(Session::has(FlashMessage::MSG))
                    <div class="alert {{ Session::get(FlashMessage::MSG)->getLevel() }}">
                        {{ Session::get(FlashMessage::MSG)->getMessage() }}
                    </div>
                    @endif

                    <div class="form-group {{ $errors->has('signin_username') ? 'has-error' : '' }}">
                        {{ Form::label('signin_username', 'Username', array('class'=>'col-sm-2 control-label')) }}
                        <div class="col-sm-8">
                            {{ Form::text('signin_username', Input::old('signin.username'), array('class'=>'form-control', 'placeholder'=>'Your username')) }}
                            {{ $errors->first('signin_username') }}
                        </div>							
                    </div>
                    <div class="form-group {{ $errors->has('signin_password') ? 'has-error' : '' }}">
                        {{ Form::label('signin_password', 'Password', array('class'=>'col-sm-2 control-label')) }}
                        <div class="col-sm-8">
                            {{ Form::password('signin_password', array('class'=>'form-control')) }}
                            {{ $errors->first('signin_password') }}
                        </div>							
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <button type="submit" class="btn btn-default">Sign in to your account</button>
                        </div>
                    </div>

                    {{ Form::close() }}
                </div>
                <div class="col-md-6">
                    {{ Form::open(array('action'=>'HomeController@signup', 'class'=>'form-horizontal', 'role'=>'form', 'autocomplete'=>'off')) }}

                    <legend>Create a new account</legend>

                    <div class="form-group {{ $errors->has('signup_username') ? 'has-error' : '' }}">
                        {{ Form::label('signup_username', 'Username', array('class'=>'col-sm-2 control-label')) }}
                        <div class="col-sm-8">
                            {{ Form::text('signup_username', Input::old('signup.username'), array('class'=>'form-control', 'placeholder'=>'Pick a username')) }}
                            {{ $errors->first('signup_username') }}
                        </div>							
                    </div>
                    <div class="form-group {{ $errors->has('signup_password') ? 'has-error' : '' }}">
                        {{ Form::label('signup_password', 'Password', array('class'=>'col-sm-2 control-label')) }}
                        <div class="col-sm-8">
                            {{ Form::password('signup_password', array('class'=>'form-control')) }}
                            {{ $errors->first('signup_password') }}
                        </div>							
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <button type="submit" class="btn btn-default">Create your new account</button>
                        </div>
                    </div>

                    {{ Form::close() }}
                </div>
            </div>
            <div class="navbar navbar-inverse navbar-fixed-bottom">
                <div class="navbar-inner">
                    <div class="row">
                        <div class="col-sm-11 col-sm-offset-1" style="line-height: 50px">
                            <a rel="license" href="http://www.gnu.org/licenses/agpl-3.0.txt" target="_blank">&copy; {{ date("Y") }} released under the AGPL</a>
                            <span style="color:#428bca">&nbsp;&nbsp;&dash;&nbsp;&nbsp;</span>
                            <a target="_blank" href="http://www.github.com/t123/readingtool/">source code</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </body>
</html>
