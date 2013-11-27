@extends('shared.layout')

@section('pageTitle')
Languages - Edit Dictionary
@stop

@section('content')
{{ Form::model($dictionary, array('action'=>array('LanguageController@postEditDictionary', $language->id, $dictionary->id), 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Edit Dictionary</legend>

<div class="form-group {{ $errors->has('name') ? 'has-error' : '' }}">
    {{ Form::label('name', 'Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('name', null, array('class'=>'form-control', 'placeholder'=>'your name for the dictionary')) }}
        {{ $errors->first('name') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('encoding') ? 'has-error' : '' }}">
    {{ Form::label('encoding', 'Encoding', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('encoding', null, array('class'=>'form-control', 'placeholder'=>'word encoding for URL')) }}
        {{ $errors->first('encoding') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('windowName') ? 'has-error' : '' }}">
    {{ Form::label('windowName', 'Window Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('windowName', null, array('class'=>'form-control', 'placeholder'=>'name of the window tab')) }}
        {{ $errors->first('windowName') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('url') ? 'has-error' : '' }}">
    {{ Form::label('url', 'URL', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('url', null, array('class'=>'form-control', 'placeholder'=>'URL for dictionary, use ### for word placement')) }}
        {{ $errors->first('url') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('sentence') ? 'has-error' : '' }}">
    {{ Form::label('sentence', 'Send sentence?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('sentence', null) }}
        {{ $errors->first('sentence') }}
    </div>
</div>
<div class="form-group {{ $errors->has('autoOpen') ? 'has-error' : '' }}">
    {{ Form::label('autoOpen', 'Automatically open?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('autoOpen', null) }}
        {{ $errors->first('autoOpen') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
        <a href="{{ action("LanguageController@edit", $language->id) }}" class="btn btn-default">Back to language</a>
    </div>
</div>
{{ Form::close() }}
@stop
