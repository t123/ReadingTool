@extends('shared.layout')

@section('pageTitle')
Languages - Edit Language
@stop

@section('content')
<?php
$json = json_decode($language==null ? "" : $language->settings);
?>
{{ Form::model($language, array('action'=>array('LanguageController@postEdit', $language->id), 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Edit Language</legend>

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
                $json->ModalBehaviour, array('class'=>'form-control')) }}
        {{ $errors->first('ModalBehaviour') }}
    </div>
</div>
<div class="form-group {{ $errors->has('Modal') ? 'has-error' : '' }}">
    {{ Form::label('Modal', 'Open dictionaries in modal instead of tab?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('Modal', null, $json->Modal) }} If you have 2 or more monitors use tabs works better. Modals may be better with one monitor.
        {{ $errors->first('Modal') }}
    </div>
</div>
<div class="form-group {{ $errors->has('archived') ? 'has-error' : '' }}">
    {{ Form::label('archived', 'Archive language?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('archived', null) }}
        {{ $errors->first('archived') }}
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
        {{ Form::text('RegexWordCharacters', $json->RegexWordCharacters, array('class'=>'form-control', 'placeholder'=>'word test regular expression, add \-\&#39; for French')) }}
        {{ $errors->first('RegexWordCharacters') }}
    </div>
</div>

<div class="form-group {{ $errors->has('RegexSplitSentences') ? 'has-error' : '' }}">
    {{ Form::label('RegexSplitSentences', 'Regex Split Sentences', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('RegexSplitSentences', $json->RegexSplitSentences, array('class'=>'form-control', 'placeholder'=>'end of sentence characters')) }}
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
                $json->Direction, array('class'=>'form-control')) }}
        {{ $errors->first('Direction') }}
    </div>
</div>

<div class="form-group {{ $errors->has('ShowSpaces') ? 'has-error' : '' }}">
    {{ Form::label('ShowSpaces', 'Display Spaces?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('ShowSpaces', null, $json->ShowSpaces) }} Display spaces between the words
        {{ $errors->first('ShowSpaces') }}
    </div>
</div>

<div class="form-group {{ $errors->has('AutoPause') ? 'has-error' : '' }}">
    {{ Form::label('AutoPause', 'Pause audio when word modal opens?', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::checkbox('AutoPause', null, $json->AutoPause) }} Pause the audio when a word is clicked. Automatically resumes when modal closes.
        {{ $errors->first('AutoPause') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}

<div class="clr20"></div>

@if(sizeof($language->dictionaries)==0)
    <strong>There are no dictionaries defined yet.</strong>
@else
<table class="table table-striped table-bordered table-bordered table-condensed">
    <caption>Dictionaries</caption>
    <thead>
        <tr> 
            <th>Name</th>
            <th>URL</th>
            <th>Auto Open?</th>
            <th>Sentence</th>
            <th class="form">&nbsp;</th>
            <th class="form">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
    @foreach ($language->dictionaries as $d)
        <tr>
            <td>{{{ $d->name }}}</td>
            <td>{{{ $d->url}}}</td>
            <td>{{{ $d->autoOpen ? "Yes" : "No" }}}</td>
            <td>{{{ $d->sentence ? "Yes" : "No" }}}</td>
            <td><a href="{{ action("LanguageController@editDictionary", array($d->language_id, $d->id)) }}" class="btn btn-default">edit</a></td>
            <td>
                {{ Form::open(array('action' => array('LanguageController@postDeleteDictionary', $d->language_id, $d->id))) }}
                    <button type="submit" class="btn btn-danger">delete</button>
                {{ Form::close() }}
            </td>
        </tr>
    @endforeach
    </tbody>
</table>
@endif
<div class="clr20"></div>
<a href="{{ action("LanguageController@addDictionary", $language->id) }}" class="btn btn-default">Add a new dictionary</a>

@stop