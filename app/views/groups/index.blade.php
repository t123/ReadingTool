@extends('shared.layout')

@section('pageTitle')
My Groups
@stop

@section('content')
<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr> 
            <th>Name</th>
            <th>Group Type</th>
            <th>Membership Type</th>
            <th style="width:200px;">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
        @foreach ($groups as $g)
        <tr>
            <td>{{{ $g->name }}}</td>
            <td>{{{ $g->type }}}</td>
            <td>{{ $g->membership }}</td>
            <td>
                <div class="btn-group">
                    @if($g->membership=='invited')
                        {{ Form::open(array('action' => array('GroupController@postAcceptMembership', $g->id), 'style'=>'display:inline', 'class'=>'pull-left')) }}
                            <button type="submit" class="btn btn-default">Accept invitation</button>
                        {{ Form::close() }}
                        <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
                            <span class="caret"></span>
                            <span class="sr-only">Toggle Dropdown</span>
                        </button>
                        <ul class="dropdown-menu" role="menu">
                            {{ Form::open(array('action' => array('GroupController@postDeclineMembership', $g->id))) }}
                                <button type="submit" class="btn btn-danger">Decline invitation</button>
                            {{ Form::close() }}
                        </ul>
                    @elseif($g->membership=='pending')
                        {{ Form::open(array('action' => array('GroupController@postLeaveGroup', $g->id))) }}
                            <button type="submit" class="btn btn-danger">Leave group</button>
                        {{ Form::close() }}
                    @else
                        <a href="{{ action("GroupController@texts", $g->id) }}" class="btn btn-default">
                            View texts
                        </a>
                    
                    <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
                        <span class="caret"></span>
                        <span class="sr-only">Toggle Dropdown</span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        @if($g->membership=='owner')
                            <li>
                                <a href="{{ action("GroupController@edit", $g->id) }}" class="btn btn-default">
                                    Edit group
                                </a>
                            </li>
                            <li><a href="{{ action("GroupController@membership", $g->id) }}" class="btn btn-default">Membership</a></li>
                            <li>
                                {{ Form::open(array('action' => array('GroupController@postDelete', $g->id))) }}
                                    <button type="submit" class="btn btn-danger">Delete group</button>
                                {{ Form::close() }}
                            </li>
                        @elseif($g->membership=='moderator')
                            <li><a href="{{ action("GroupController@membership", $g->id) }}" class="btn btn-default">Membership</a></li>
                            <li>
                                {{ Form::open(array('action' => array('GroupController@postLeaveGroup', $g->id))) }}
                                    <button type="submit" class="btn btn-danger">Leave group</button>
                                {{ Form::close() }}
                            </li>
                        @else
                            <li>
                                {{ Form::open(array('action' => array('GroupController@postLeaveGroup', $g->id))) }}
                                        <button type="submit" class="btn btn-danger">Leave group</button>
                                {{ Form::close() }}
                            </li>
                        @endif
                    </ul>
                    @endif
                </div>
            </td>
        </tr>
        @endforeach
    </tbody>
</table>
<a href="{{ action("GroupController@add") }}" class="btn btn-default">Add a new group</a>
@stop
