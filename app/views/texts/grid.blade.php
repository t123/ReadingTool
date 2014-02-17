<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr>
            <th><input type="checkbox" /></th>
            <th><a href="?sort=language">Language</a></th>
            <th><a href="?sort=title">Title</a></th>
            <th><a href="?sort=collectionname">Collection Name</a></th>
            <th><a href="?sort=lastread">Last Read</a></th>
            <th style="width:150px">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
        @foreach ($texts as $t)
        <tr>
            <td><input type="checkbox" value="{{ $t['id'] }}" /></td>
            <td>{{{ $t['language'] }}}
            <td>
                @if( $t['collectionNo']==null )
                    {{{ $t['title'] }}}
                @else
                    {{{ $t['collectionNo'] }}}. {{{ $t['title'] }}}
                @endif
                
                @if( $t['isParallel'] )
                    <i class="icon-pause pull-right" title="text is parallel"> </i>
                @endif
                @if( $t['hasAudio'] )
                    <i class="icon-music pull-right" title="text has audio"> </i>
                @endif
                @if( $t['isShared'] )
                    <i class="icon-share pull-right" title="text is shared"> </i>
                @endif
            </td>
            <td>
                {{{ $t['collectionName'] }}}
            </td>
            <td>{{ $t['lastRead'] }}</td>
            <td>
                <div class="btn-group">
                    @if( !$t['isParallel'] )
                        <a href="{{ action("TextController@read", $t['id']) }}" class="btn btn-sm btn-default">
                            read
                        </a>
                    @else
                        <a href="{{ action("TextController@readParallel", $t['id']) }}" class="btn btn-sm btn-default">
                            read parallel
                        </a>
                    @endif
                    <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown">
                        <span class="caret"></span>
                        <span class="sr-only">Toggle Dropdown</span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li>
                            <a href="{{ action("TextController@edit", $t['id']) }}" class="btn btn-sm btn-default">edit</a>
                        </li>
                        <li>
                            <a href="{{ action("TextController@read", $t['id']) }}" class="btn btn-sm btn-default">read</a>
                        </li>
                        @if($t['isParallel'])
                            <li>
                                <a href="{{ action("TextController@readParallel", $t['id']) }}" class="btn btn-sm btn-default">read parallel</a>
                            </li>
                        @endif
                        <li class="divider"></li>
                        <li>
                            {{ Form::open(array('action' => array('TextController@postDelete', $t['id']))) }}
                                <button type="submit" class="btn btn-sm btn-danger">delete</button>
                            {{ Form::close() }}
                        </li>
                    </ul>
                </div>
            </td>
        </tr>
        @endforeach
    </tbody>
</table>
<div class="text-center">
    <div class="row">
        <div class="col-md-4">
            Total Matching Texts: <strong>{{ $count }}</strong>
        </div>
    </div>
</div>
<div class="text-center">
    <div class="pagination pagination-sm">
        @if($currentPage==1)
            <li class="active"><a href="#">first page</a></li>
            <li class="disabled"><a href="#">previous</a></li>
        @else
            <li><a href="#" data-page="1">first page</a></li>
            <li><a href="#" data-page="{{ $currentPage-1 }}">previous</a></li>
        @endif

        <?php
            $start = $currentPage-7;
            $end = $currentPage+7;

            if($start<1) {
                $start =  1;
            }

            if($end>$pages-1) {
                $end = $pages-1;
            }
        ?>

        @for($i=$start; $i<=$end; $i++)
            @if($i==$currentPage)
                <li class="active"><a href="#">{{ $i }}</a></li>
            @else
                <li class=""><a href="#" data-page="{{ $i }}">{{ $i }}</a></li>
            @endif
        @endfor

        @if($currentPage==$pages)
            <li class="active"><a href="#">last page ({{ $pages }})</a></li>
            <li class="disabled"><a href="#">next</a></li>
        @else
            <li><a href="#" data-page="{{ $pages }}">last page ({{ $pages }})</a></li>
            <li><a href="#" data-page="{{ $currentPage+1 }}">next</a></li>
        @endif

        <li class="disabled"><a href="#" title="{{$pages}} pages in total">{{ $pages }}</a></li>
    </div>
</div>
<script language="javascript">
    $('#grid table thead tr th:first').html('<input type="checkbox" id="checkAll" value="" />');

    $('#checkAll').live('click', function (e) {
        updateCheckboxes();
    });
    
    function updateCheckboxes() {
        var state = $('#checkAll').attr('checked');
        if (state == undefined) state = false;
        $('.table input[type=checkbox]').each(function () {
            $(this).attr('checked', state);
        });
    }
</script>