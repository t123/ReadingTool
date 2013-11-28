@extends('shared.layout')

@section('pageTitle')
Terms
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
<a href="{{ action("TermController@exportTerms") }}" class="btn btn-default">Export unknown terms</a>
@stop

@section('bottomJs')
{{ HTML::script('js/webgrid.js') }}
<script language="javascript">
    $(function() {
        window.initWebGrid('/terms');
    });
</script>
@stop
