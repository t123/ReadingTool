@extends('shared.layout')

@section('pageTitle')
Term - Edit Term
@stop

@section('content')
{{ Form::model($term, array('action'=>array('TermController@postEdit', $term->id), 'class'=>'form-horizontal', 'role'=>'form')) }}
<legend>Edit Term</legend>

<div class="form-group {{ $errors->has('phrase') ? 'has-error' : '' }}">
    {{ Form::label('phrase', 'Phrase', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('phrase', null, array('class'=>'form-control', 'disabled'=>'disabled')) }}
        {{ $errors->first('phrase') }}
    </div>							
</div>
<div class="form-group {{ $errors->has('basePhrase') ? 'has-error' : '' }}">
    {{ Form::label('basePhrase', 'Base Phrase', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('basePhrase', null, array('class'=>'form-control')) }}
        {{ $errors->first('basePhrase') }}
    </div>
</div>
<div class="form-group {{ $errors->has('sentence') ? 'has-error' : '' }}">
    {{ Form::label('sentence', 'Sentence', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::text('sentence', null, array('class'=>'form-control')) }}
        {{ $errors->first('sentence') }}
    </div>
</div>
<div class="form-group {{ $errors->has('definition') ? 'has-error' : '' }}">
    {{ Form::label('definition', 'Definition', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::textarea('definition', null, array('class'=>'form-control')) }}
        {{ $errors->first('definition') }}
    </div>
</div>
<div class="form-group {{ $errors->has('state') ? 'has-error' : '' }}">
    {{ Form::label('state', 'State', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        {{ Form::select('state', array(
                    'known'=>'Known',
                    'unknown'=>'Unknown',
                    'ignored'=>'Ignored',
                    'notseen'=>'Not seen'
                    ), 
            null, array('class'=>'form-control')) }}
        {{ $errors->first('state') }}
    </div>
</div>
<div class="form-group {{ $errors->has('tags') ? 'has-error' : '' }}">
    {{ Form::label('tags', 'Tags', array('class'=>'col-sm-2 control-label')) }}
    <div class="col-sm-8">
        <?php
        $tags = array();
        foreach($term->tags as $t) {
            array_push($tags, $t->tag);
        }
        ?>
        {{ Form::text('tags', implode(',', $tags), array('class'=>'form-control')) }}
        {{ $errors->first('tags') }}
    </div>
</div>

<div class="form-group">
    <div class="col-sm-offset-2 col-sm-10">
        <button type="submit" class="btn btn-primary">Save changes</button>
    </div>
</div>
{{ Form::close() }}
@stop