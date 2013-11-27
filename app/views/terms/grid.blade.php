<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr>
            <th><a href="?sort=language">Language</a></th>
            <th><a href="?sort=state">State</a></th>
            <th><a href="?sort=phrase">Phrase</a></th>
            <th>Sentence</th>
            <th><a href="?sort=added">Added</a></th>
            <th><a href="?sort=changed">Changed</a></th>
            <th class="form">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
    @foreach ($terms as $t)
        <tr>
            <td>{{{ $t['language'] }}}</td>
            <td>{{{ $t['state'] }}}</td>
            <td>{{{ $t['phrase'] }}}</td>
            <td>{{{ $t['sentence'] }}}</td>
            <td>{{{ $t['added'] }}}</td>
            <td>{{{ $t['updated'] }}}</td>
            <td>
                <a href="{{ action("TermController@edit", $t['id']) }}" class="btn btn-default">edit</a>
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
