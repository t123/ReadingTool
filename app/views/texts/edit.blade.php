@extends('shared.layout')

@section('pageTitle')
Texts - Edit Text
@stop

@section('content')
{{ Form::model($text, array('action'=>array('TextController@postEdit', $text->id), 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Edit Text</legend>

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
    <div class="col-sm-3">
        {{ Form::select('l1_id', $languages, null, array('class'=>'form-control')) }}
        {{ $errors->first('l1_id') }}
    </div>
    <div class="col-sm-1"></div>
    {{ Form::label('l2_id', 'Language 2', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-3">
        {{ Form::select('l2_id', array(''=>'None')+$languages, null, array('class'=>'form-control')) }}
        {{ $errors->first('l2_id') }}
    </div>
</div>

<div class="form-group {{ $errors->has('l1Text') ? 'has-error' : '' }}">
    <input type="button" class="btn btn-default btn-sm col-sm-offset-2" value="Toggle Line Wrapping" id="l1_wrap"/>
    <input type="button" class="btn btn-default btn-sm col-sm-offset-2" value="Toggle Height" id="l1_height"/>
    <div class="clr10"></div>
    <div class="col-sm-6" id="l1textcontrol">
        {{ Form::textarea('l1Text', null, array('class'=>'form-control', 'id'=>'l1Text')) }}
        {{ $errors->first('l1Text') }}
    </div>
    <?php
    $style = $text->l2_id==null ? "display:none" : "";
    ?>
    <div class="col-sm-6" style="{{ $style }}" id="l2textcontrol">
        {{ Form::textarea('l2Text', null, array('class'=>'form-control', 'id'=>'l2Text')) }}
        {{ $errors->first('l2Text') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
        <a href="{{ action("TextController@copyAndEdit", $text->id) }}" class="btn btn-default">
            Create copy & edit
        </a>
        <a href="{{ action("TextController@read", $text->id) }}" class="btn btn-default">
            Read
        </a>
        @if($text->l2_id!=null)
            <a href="{{ action("TextController@readParallel", $text->id) }}" class="btn btn-default">
                Read parallel
            </a>
        @endif
        <a href="{{ action("TextController@downloadPdf", $text->id) }}" class="btn btn-default">
            Download PDF
        </a>
    </div>
</div>
{{ Form::close() }}
@stop

@section('topCss')
{{ HTML::style('css/codemirror.css') }}
@stop

@section('bottomJs')
{{ HTML::script('js/CodeMirror/codemirror.min.js') }}
<script language="javascript">
    function checkL2() {
        if ($('#l2_id').val() == '') {
            $('#l2textcontrol').hide();
            $('#l1textcontrol').removeClass('col-sm-6').addClass('col-sm-12');
        } else {
            $('#l2textcontrol').show();
            $('#l1textcontrol').removeClass('col-sm-12').addClass('col-sm-6');
        }
    }
    
    $('#l2_id').change(function() {
        checkL2();
    });
    
    $('#l1_wrap').click(function() {
        cm_l1.setOption('lineWrapping', !cm_l1.getOption('lineWrapping'));
        cm_l2.setOption('lineWrapping', !cm_l2.getOption('lineWrapping'));
    });
    
    $('#l1_height').click(function() {
        if(height) {
            cm_l1.setSize(null, 500);
            cm_l2.setSize(null, 500);
        } else {
            cm_l1.setSize(null, 300);
            cm_l2.setSize(null, 300);
        }
        
        height = !height;
    });
    
    var cm_l1, cm_l2, height = true;
    $(function() {
        cm_l1 = CodeMirror.fromTextArea(document.getElementById("l1Text"), {
            mode: "plain/text",
            lineNumbers: true,
            lineWrapping: false
        });

        cm_l2 = CodeMirror.fromTextArea(document.getElementById("l2Text"), {
            mode: "plain/text",
            lineNumbers: true,
            lineWrapping: false
        });
        
        checkL2();
    });
</script>
@stop