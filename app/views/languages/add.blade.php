@extends('shared.layout')

@section('pageTitle')
Languages - Add Language
@stop

@section('content')
{{ Form::open(array('action'=>'LanguageController@postAdd', 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Add Language</legend>

<div class="form-group {{ $errors->has('name') ? 'has-error' : '' }}">
    {{ Form::label('name', 'Name', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('name', null, array('class'=>'form-control', 'placeholder'=>'your name for the language')) }}
        {{ $errors->first('name') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('code') ? 'has-error' : '' }}">
    {{ Form::label('code', 'Language Code', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('code', $slanguages, null, array('class'=>'form-control')) }}
        {{ $errors->first('code') }}
    </div>
</div>
<div class="form-group {{ $errors->has('ModalBehaviour') ? 'has-error' : '' }}">
    {{ Form::label('ModalBehaviour', 'When does the modal open?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('ModalBehaviour', array(
                        ''=>'Please choose', 
                        '1'=>'Left Click', 
                        '2'=>'Ctrl Left Click',
                        '3'=>'Shift Left Click',
                        '4'=>'Middle Click',
                        '5'=>'Right Click'
                        ),
                '1', array('class'=>'form-control')) }}
        {{ $errors->first('ModalBehaviour') }}
    </div>
</div>
<div class="form-group {{ $errors->has('Modal') ? 'has-error' : '' }}">
    {{ Form::label('Modal', 'Open dictionaries in modal instead of tab?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('Modal', null) }}
        {{ $errors->first('Modal') }}
    </div>
</div>

<legend>Parser Settings</legend>
<div class="alert alert-block alert-info">
    For a language like French, if you want to treat words like est-ce, là-bas or s'il as a single word, 
    use the following expression <strong>a-zA-ZÀ-ÖØ-öø-ȳ\-\'</strong> instead.
</div>
<div class="form-group {{ $errors->has('RegexWordCharacters') ? 'has-error' : '' }}">
    {{ Form::label('RegexWordCharacters', 'Regex Test Words', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('RegexWordCharacters', 'a-zA-ZÀ-ÖØ-öø-ȳ', array('class'=>'form-control', 'placeholder'=>'word test regular expression, add \-\&#39; for French')) }}
        {{ $errors->first('RegexWordCharacters') }}
    </div>
</div>

<div class="form-group {{ $errors->has('RegexSplitSentences') ? 'has-error' : '' }}">
    {{ Form::label('RegexSplitSentences', 'Regex Split Sentences', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('RegexSplitSentences', '.!?:;', array('class'=>'form-control', 'placeholder'=>'end of sentence characters')) }}
        {{ $errors->first('RegexSplitSentences') }}
    </div>
</div>

<div class="form-group {{ $errors->has('Direction') ? 'has-error' : '' }}">
    {{ Form::label('Direction', 'Direction', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('Direction', array(
                        null=>'Please choose', 
                        'ltr'=>'Left to Right', 
                        'rtl'=>'Right to Left'
                        ),
                'ltr', array('class'=>'form-control')) }}
        {{ $errors->first('Direction') }}
    </div>
</div>

<div class="form-group {{ $errors->has('ShowSpaces') ? 'has-error' : '' }}">
    {{ Form::label('ShowSpaces', 'Display Spaces?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('ShowSpaces', null, true) }}
        {{ $errors->first('ShowSpaces') }}
    </div>
</div>

<div class="form-group {{ $errors->has('AutoPause') ? 'has-error' : '' }}">
    {{ Form::label('AutoPause', 'Pause audio when word moodal opens?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('AutoPause', null, true) }}
        {{ $errors->first('AutoPause') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}
@stop
