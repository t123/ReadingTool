@extends('shared.layout')

@section('pageTitle')
My Account
@stop

@section('content')
{{ Form::model($user, array('action'=>'AccountController@postIndex', 'class'=>'form-horizontal', 'role'=>'form')) }}
<div class="media">
    <a class="pull-left" href="http://www.gravatar.com" style="width:80px; height:80px" target="_blank">
        @if(Auth::user()->email==null)
        <img alt="gravatar" class="media-object" height="80" width="80" src="https://www.gravatar.com/avatar/?s=80&d=mm" title="Get your gravatar"/>
        @else
        <img alt="gravatar" class="media-object" height="80" width="80" src="https://www.gravatar.com/avatar/{{ md5(Auth::user()->email) }}?s=80&d=mm" title="Get your gravatar"/>
        @endif
    </a>
    <div class="media-body">
        <h4 class="media-heading">{{{ Auth::user()->currentName() }}}</h4>
        member since {{ date("d F Y", strtotime(Auth::user()->created)) }}
    </div>
</div>
<div class="clr10"></div>
<legend>Account Details</legend>

<div class="form-group {{ $errors->has('displayName') ? 'has-error' : '' }}">
    {{ Form::label('displayName', 'Display Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('displayName', null, array('class'=>'form-control', 'placeholder'=>'display name, what other users see')) }}
        {{ $errors->first('displayName') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('email') ? 'has-error' : '' }}">
    {{ Form::label('email', 'Email Address', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('email', null, array('class'=>'form-control', 'placeholder'=>'email address, not required')) }}
        {{ $errors->first('email') }}
    </div>							
</div>
<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
        <a href="{{ action('AccountController@changePassword') }}" class="btn btn-default">Change your password</a>
        <a href="{{ action('HomeController@signout') }}" class="btn btn-default">Sign out</a>
        <a href="{{ action('AccountController@deleteAccount') }}" class="btn btn-default">Delete your account</a>
    </div>
</div>
{{ Form::close() }}
<div class="clr20"></div>
{{ Form::model($user, array('action'=>'AccountController@postCss', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Custom CSS for Reading</legend>
<div class="form-group">
    {{ Form::label('css', 'Custom CSS', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::textarea('css', $css, array('id'=>'customCss','class'=>'form-control', 'placeholder'=>'Your custom CSS for the reading screen. This will replace the default CSS. Leave blank to reset to default.')) }}
    </div>
    <div class="clr5"></div>
    <div class="col-sm-8 col-sm-offset-2">
        The original CSS can be <a href="{{URL::asset('css/reading.css')}}">downloaded here</a> (right-click and choose save link as).<br/>
        To use the system CSS leave the above box blank.
    </div>
</div>
<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}
@stop

@section('topCss')
{{ HTML::style('css/codemirror.css') }}
@stop

@section('bottomJs')
{{ HTML::script('js/CodeMirror/codemirror.min.js') }}
{{ HTML::script('js/CodeMirror/css.js') }}
<script language="javascript">
    var cm_css;
    $(function() {
        cm_css = CodeMirror.fromTextArea(document.getElementById("customCss"), {
            mode: "text/css",
            lineNumbers: true,
            lineWrapping: true
        });
    });
</script>
@stop