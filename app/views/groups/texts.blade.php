@extends('shared.layout')

@section('pageTitle')
{{{ $group->name }}} texts
@stop

@section('content')
<h4>{{{ $group->name }}}</h4>
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
        {{ Form::open(array('action' => array('GroupController@postUnshare'), 'onsubmit'=>'return update();')) }}
            <input type="hidden" name="texts" id="texts" />
            <input type="hidden" name="groupId" id="groupId" value="{{{$group->id}}}" />
            <button type="submit" class="btn btn-default pull-right">Unshare selected texts</button>
        {{ Form::close() }}
    </div>
</div>
@stop

@section('bottomJs')
{{ HTML::script('js/webgrid.js') }}
<script language="javascript">
    function update() {
        var texts = [];
        $('input[type=checkbox]:checked').each(function (index, x) { texts.push($(x).val()); });
        $('#texts').val(texts.join(','));
        return true;
    }

    $(function() {
        window.initWebGrid('/groups/texts/{{{$group->id}}}');
    });
</script>
@stop
