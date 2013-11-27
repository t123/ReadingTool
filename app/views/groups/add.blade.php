@extends('shared.layout')

@section('pageTitle')
Languages - Add Group
@stop

@section('content')
{{ Form::open(array('action'=>'GroupController@postAdd', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Add Group</legend>

<div class="form-group {{ $errors->has('name') ? 'has-error' : '' }}">
    {{ Form::label('name', 'Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('name', null, array('class'=>'form-control', 'placeholder'=>'your name for the group')) }}
        {{ $errors->first('name') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('description') ? 'has-error' : '' }}">
    {{ Form::label('description', 'Description', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::textarea('description', null, array('class'=>'form-control', 'placeholder'=>'a description of the group, max 1000 characters.')) }}
        {{ $errors->first('description') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('type') ? 'has-error' : '' }}">
    {{ Form::label('type', 'Type', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('type', array(''=>'Please choose', 'public'=>'Public', 'private'=>'Private'), null, array('class'=>'form-control')) }}
        {{ $errors->first('type') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}
@stop
