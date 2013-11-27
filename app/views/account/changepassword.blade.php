@extends('shared.layout')

@section('pageTitle')
My Account - Change Account Password
@stop

@section('content')
{{ Form::open(array('action'=>'AccountController@postChangePassword', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Change your account password</legend>

<div class="form-group {{ $errors->has('currentPassword') ? 'has-error' : '' }}">
    {{ Form::label('currentPassword', 'Current Password', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::password('currentPassword', array('class'=>'form-control', 'placeholder'=>'your current password')) }}
        {{ $errors->first('currentPassword') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('newPassword') ? 'has-error' : '' }}">
    {{ Form::label('newPassword', 'New Password', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::password('newPassword', array('class'=>'form-control', 'placeholder'=>'your new password')) }}
        {{ $errors->first('newPassword') }}
    </div>							
</div>
<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Update your password</button>
        <a href="{{ action('AccountController@index') }}" class="btn btn-default">Back to your account</a>
    </div>
</div>
{{ Form::close() }}
@stop
