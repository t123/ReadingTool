<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr>
            <th>Name</th>
            <th class="form">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
    @foreach ($groups as $g)
        <tr>
            <td>
                <a title="{{ $g['description'] }}" rel="tooltip">
                    {{{ $g['name'] }}}
                </a>
            </td>
            <td>
                {{ Form::open(array('action' => array('GroupController@postJoinGroup', $g['id']))) }}
                    <button type="submit" class="btn btn-default">Join group</button>
                {{ Form::close() }}
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
