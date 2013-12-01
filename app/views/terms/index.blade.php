@extends('shared.layout')

@section('pageTitle')
Terms
@stop

@section('topCss')
<style type="text/css">
    td a, td a:hover {
        color: black;
        text-decoration: none;
    }
</style>
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
            placeholder="search for terms, &quot;multi terms&quot;, languages or tags: #known, #notknown, #yourtag" />
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
<div class="clr20"></div>
<div class="btn-group">
    <a onclick="$('#exportFields').show();" href="{{ action("TermController@exportTerms", 0) }}" class="btn btn-default">Export all unknown terms</a>
    @if($languages->count()>1)
        <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
            <span class="caret"></span>
            <span class="sr-only">Toggle Dropdown</span>
        </button>
        <ul class="dropdown-menu" role="menu">
            @foreach($languages as $l)
            <li>
                <a onclick="$('#exportFields').show();" href="{{ action("TermController@exportTerms", $l->id ) }}" class="btn btn-default">Export unknown terms for <strong>{{{ $l->name }}}</strong></a>
            </li>
            @endforeach
        </ul>
    @endif
</div>
<div id="exportFields" style="display:none">
    <div class="clr20"></div>
    <div class="alert alert-info alert-block">
        <button type="button" class="close" aria-hidden="true" onclick="$('#exportFields').hide();">&times;</button>
        The fields of the TSV export are as follows:
        <ul>
            <li>Id</li>
            <li>State</li>
            <li>Phrase</li>
            <li>Base Phrase</li>
            <li>Definition</li>
            <li>Creation Date</li>
            <li>Updated Date</li>
            <li>Sentence</li>
            <li>Language</li>
            <li>Tags</li>
        </ul>
    </div>
    <div class="clr20"></div>
</div>
@stop

@section('bottomJs')
{{ HTML::script('js/webgrid.js') }}
<script language="javascript">
    $(function() {
        window.initWebGrid('/terms');

        $('body').tooltip({
            selector: '[rel=tooltip]',
            html: true,
            animation: false
        });
    });
</script>
@stop
