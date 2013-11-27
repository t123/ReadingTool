<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr>
            <th><input type="checkbox" /></th>
            <th>Owner</th>
            <th>Language</th>
            <th>Title</th>
            <th>Collection Name</th>
            <th style="width:150px">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
        @foreach ($texts as $t)
        <tr>
            <td>
                @if($t['isOwner'])
                    <input type="checkbox" value="{{ $t['id'] }}" />
                @else
                    &nbsp;
                @endif
            </td>
            <td>{{{ $t['owner'] }}}
            <td>{{{ $t['language'] }}}
            <td>
                {{{ $t['title'] }}}
                
                @if( $t['isParallel'] )
                    <i class="icon-pause pull-right" title="text is parallel"> </i>
                @endif
                @if( $t['hasAudio'] )
                    <i class="icon-music pull-right" title="text has audio"> </i>
                @endif
                @if( $t['isShared']&&$t['isOwner'] )
                    <i class="icon-share pull-right" title="text is shared"> </i>
                @endif
            </td>
            <td>{{{ $t['collectionName'] }}}</td>
            <td>
                <div class="btn-group">
                    @if( !$t['isParallel'] )
                        <a href="{{ action("GroupController@read", array($group->id, $t['id'])) }}" class="btn btn-sm btn-default">
                            read
                        </a>
                    @else
                        <a href="{{ action("GroupController@readParallel", array($group->id, $t['id'])) }}" class="btn btn-sm btn-default">
                            read parallel
                        </a>
                        <button type="button" class="btn btn-default btn-sm dropdown-toggle" data-toggle="dropdown">
                            <span class="caret"></span>
                            <span class="sr-only">Toggle Dropdown</span>
                        </button>
                        <ul class="dropdown-menu" role="menu">
                            <li>
                                <a href="{{ action("GroupController@read", array($group->id, $t['id'])) }}" class="btn btn-sm btn-default">
                                    read
                                </a>
                            </li>
                        </ul>
                    @endif
                </div>
            </td>
        </tr>
        @endforeach
    </tbody>
</table>
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