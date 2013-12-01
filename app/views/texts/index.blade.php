@extends('shared.layout')

@section('pageTitle')
Texts
@stop

@section('content')
<input type="hidden" id="currentPage" value="1"/>
<div class="row">
    <div class="col-md-9">
        <input 
            name="filter"
            id="filter"
            type="text" 
            class="form-control input-sm" 
            placeholder="search for language, title, collection name or these tags: #shared, #audio, #parallel" />
    </div>
    <div class="col-md-2">
        <select class="form-control input-sm" name="rowsPerPage" id="rowsPerPage">
            <option value="5">5</option>
            <option value="10">10</option>
            <option value="25">25</option>
            <option value="50">50</option>
            <option value="100">100</option>
        </select>
    </div>
    <div class="col-md-1">
        <a href="#" id="clearText">
            <i class="icon-remove" title="clear text filter"></i>
        </a>

        <a href="#" id="resetFilter">
            <i class="icon-refresh" title="clear the text filter, sort by and sort direction filter"></i>
        </a>
    </div>
</div>
<div class="clr20"></div>
<div id="grid"></div>
<div class="clr10"></div>
<div class="row">
    <div class="col-md-2">
        <a href="{{ action("TextController@add") }}" class="btn btn-default">Add a new text</a>
    </div>
    @if(sizeof($groups)>0)
        <div class="col-md-5">
            Group
            {{ Form::select('group', $groups, null, array('class'=>'form-control', 'id'=>'group', 'style'=>'width:180px;display:inline')) }}

            {{ Form::open(array('action' => array('TextController@postShare'), 'onsubmit'=>'return update();', 'style'=>'width:180px;display:inline')) }}
                <input type="hidden" name="texts" id="texts" />
                <input type="hidden" name="groupId" id="groupId" />
                <button type="submit" class="btn btn-default">Share selected texts</button>
            {{ Form::close() }}
        </div>
    @endif
    <div class="col-md-2">
        {{ Form::open(array('action' => array('TextController@postDeleteMany'), 'onsubmit'=>'return update2();')) }}
            <input type="hidden" name="texts" id="texts2" />
            <button type="submit" class="btn btn-danger pull-right">Delete selected texts</button>
        {{ Form::close() }}
    </div>
</div>
@stop

@section('bottomJs')
{{ HTML::script('js/webgrid.js') }}
<script language="javascript">
    function update() {
        if($('#GroupId').val()=='') {
            return false;
        }

        var texts = [];
        $('input[type=checkbox]:checked').each(function (index, x) { texts.push($(x).val()); });
        $('#texts').val(texts.join(','));
        $('#groupId').val($('#group').val());
        return true;
    }

    function update2() {
        var texts = [];
        $('input[type=checkbox]:checked').each(function (index, x) { texts.push($(x).val()); });
        $('#texts2').val(texts.join(','));
        return true;
    }

    $(function() {
        window.initWebGrid('/texts');
    });
</script>
@stop
