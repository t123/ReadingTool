@extends('shared.layout')

@section('pageTitle')
My Account - Delete Account
@stop

@section('content')
<div class="alert alert-box alert-danger">
    Seriously, this will delete your account. All your texts, terms and languages will disappear. Permanently. And no, it can't be recovered.
</div>
{{ Form::open(array('action'=>'AccountController@postDeleteAccount', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Permanently Delete Your Account</legend>

<div class="form-group {{ $errors->has('currentPassword') ? 'has-error' : '' }}">
    {{ Form::label('currentPassword', 'Current Password', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::password('currentPassword', array('class'=>'form-control', 'placeholder'=>'your current password')) }}
        {{ $errors->first('currentPassword') }}
    </div>							
</div>
<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Permanently delete your account</button>
        <a href="{{ action('AccountController@index') }}" class="btn btn-default">Back to your account</a>
    </div>
</div>
{{ Form::close() }}
@stop
