@extends('shared.layout')

@section('pageTitle')
My Groups - {{{ $group->name }}} membership
@stop

@section('content')
<?php
$isOwner = false;
?>
<div class="alert alert-block alert-info">
    Changing a user to a status of <strong>pending</strong>, <strong>invited</strong> or <strong>banned</strong> or <strong>removing them</strong> will automatically unshare their texts.
</div>
<h3>{{{ $group->name }}}</h3>
{{ Form::model($group, array('action'=>array('GroupController@postMembership', $group->id), 'class'=>'form-horizontal', 'role'=>'form')) }}
<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr> 
            <th>Member Name</th>
            <th>Invited</th>
            <th>Pending</th>
            <th>Moderator</th>
            <th>Member</th>
            <th>Banned</th>
            <th>Remove</th>
        </tr>
    </thead>
    <tbody>
        @foreach ($members as $m)
        <tr>
            <td>{{{ $m->displayName=='' ? $m->username : $m->displayName }}}</td>
            @if($m->membership=='owner')
                @if($m->user_id==Auth::user()->id)
                    <?php
                        $isOwner = true;
                    ?>
                @endif
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
            @elseif($m->membership=='invited')
                <td><input type="radio" name="membership[{{$m->id}}]" value="invited" checked="checked"/></td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="delete"/></td>
            @else
                <td>&nbsp;</td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="pending" {{ $m->membership=='pending' ? 'checked="checked"' : '' }} /></td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="moderator" {{ $m->membership=='moderator' ? 'checked="checked"' : '' }} /></td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="member" {{ $m->membership=='member' ? 'checked="checked"' : '' }} /></td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="banned" {{ $m->membership=='banned' ? 'checked="checked"' : '' }} /></td>
                <td><input type="radio" name="membership[{{$m->id}}]" value="delete"/></td>
            @endif
        </tr>
        @endforeach
    </tbody>
</table>
@if($group->type=='private')
<h4>Invite these users to the group:</h4>
{{ Form::textarea('usernames', null, array('class'=>'form-control', 'placeholder'=>'type in the usernames of users to invite to this group. Separate their names with spaces.')) }}
@endif
<div class="clr10"></div>
<button type="submit" class="btn btn-primary">Save changes</button>
@if($isOwner)
    <a href="{{ action("GroupController@edit", $group->id) }}" class="btn btn-small btn-default">
        Edit group
    </a>
@endif
<a href="{{ action("GroupController@texts", $group->id) }}" class="btn btn-small btn-default">
    View texts
</a>
{{ Form::close() }}
@stop
