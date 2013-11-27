@extends('shared.layout')

@section('pageTitle')
Texts - Add Text
@stop

@section('content')
{{ Form::open(array('action'=>'TextController@postAdd', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Add Text</legend>

<div class="form-group {{ $errors->has('name') ? 'has-error' : '' }}">
    {{ Form::label('title', 'Title', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('title', null, array('class'=>'form-control', 'placeholder'=>'title of your text')) }}
        {{ $errors->first('title') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('collectionName') ? 'has-error' : '' }}">
    {{ Form::label('collectionName', 'Collection Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('collectionName', null, array('class'=>'form-control', 'placeholder'=>'collection name for grouping texts')) }}
        {{ $errors->first('collectionName') }}
    </div>
</div>
<div class="form-group {{ $errors->has('collectionNo') ? 'has-error' : '' }}">
    {{ Form::label('collectionNo', 'Collection Number', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('collectionNo', null, array('class'=>'form-control', 'placeholder'=>'collection number for ordering texts in a collection')) }}
        {{ $errors->first('collectionNo') }}
    </div>
</div>
<div class="form-group {{ $errors->has('audioUrl') ? 'has-error' : '' }}">
    {{ Form::label('audioUrl', 'Audio URL', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('audioUrl', null, array('class'=>'form-control', 'placeholder'=>'URL of the audio')) }}
        {{ $errors->first('audioUrl') }}
    </div>
</div>
<div class="form-group {{ $errors->has('shareAudioUrl') ? 'has-error' : '' }}">
    {{ Form::label('shareAudioUrl', 'Share the audio?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('shareAudioUrl', null) }}
        {{ $errors->first('shareAudioUrl') }}
    </div>
</div>
<div class="form-group {{ $errors->has('l1_id') ? 'has-error' : '' }}">
    {{ Form::label('l1_id', 'Language 1', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('l1_id', $languages, null, array('class'=>'form-control')) }}
        {{ $errors->first('l1_id') }}
    </div>
</div>

<div class="form-group {{ $errors->has('l1Text') ? 'has-error' : '' }}">
    {{ Form::label('l1Text', 'Text', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::textarea('l1Text', null, array('class'=>'form-control')) }}
        {{ $errors->first('l1Text') }}
    </div>
</div>

<div class="form-group {{ $errors->has('l2_id') ? 'has-error' : '' }}">
    {{ Form::label('l2_id', 'Language 2', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('l2_id', array(''=>'None')+$languages, null, array('class'=>'form-control')) }}
        {{ $errors->first('l2_id') }}
    </div>
</div>

<div class="form-group {{ $errors->has('l2Text') ? 'has-error' : '' }}" id="l2textcontrol" style="display:none">
    {{ Form::label('l2Text', 'Parallel Text', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::textarea('l2Text', null, array('class'=>'form-control')) }}
        {{ $errors->first('l2Text') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}
@stop

@section('bottomJs')
<script language="javascript">
    function checkL2() {
        if ($('#l2_id').val() == '') {
            $('#l2textcontrol').hide();
        } else {
            $('#l2textcontrol').show();
        }
    }
    $('#l2_id').change(function() {
        checkL2();
    });
    checkL2();
</script>
@stop