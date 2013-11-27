@extends('shared.layout')

@section('pageTitle')
Languages
@stop

@section('content')
<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr> 
            <th>Name</th>
            <th>Code</th>
            <th class="form">&nbsp;</th>
            <th class="form">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
    @foreach ($languages as $l)
        <tr>
            <td>{{{ $l->name }}}</td>
            <td>{{{ $l->code }}}</td>
            <td>
                <a href="{{ action("LanguageController@edit", $l->id) }}" class="btn btn-default">edit</a>
            </td>
            <td>
                {{ Form::open(array('action' => array('LanguageController@postDelete', $l->id))) }}
                    <button type="submit" class="btn btn-danger">delete</button>
                {{ Form::close() }}
            </td>
        </tr>
    @endforeach
    </tbody>
</table>
<a href="{{ action("LanguageController@add") }}" class="btn btn-default">Add a new language</a>
@stop
