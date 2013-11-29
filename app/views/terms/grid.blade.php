<table class="table table-striped table-bordered table-bordered table-condensed">
    <thead>
        <tr>
            <th><a href="?sort=language">Language</a></th>
            <th><a href="?sort=state">State</a></th>
            <th><a href="?sort=basephrase">Base Phrase</a></th>
            <th><a href="?sort=phrase">Phrase</a></th>
            <th>Sentence</th>
            <th><a href="?sort=added">Added</a></th>
            <th><a href="?sort=changed">Changed</a></th>
            <th style="width:100px;">&nbsp;</th>
        </tr>
    </thead>
    <tbody>
    @foreach ($terms as $t)
        <tr>
            <td>{{{ $t['language'] }}}</td>
            <td>{{{ $t['state'] }}}</td>
            <?php
            $tooltip = (empty($t['basePhrase']) ? $t['phrase'] : $t['basePhrase']) . "<br/>" . str_replace("\n", "<br/>", $t['definition']);
            ?>
            <td>
                <a rel="tooltip" title="{{{ $tooltip }}}">
                    {{{ empty($t['basePhrase']) ? $t['phrase'] : $t['basePhrase'] }}}
                </a>
                <a href="http://www.forvo.com/search-{{{ $t['code'] }}}/{{{ $t['basePhrase'] }}}" target="__forvo__"><i class="icon-play pull-right" title="search forvo"> </i></a>
            </td>
            <td>
                <a rel="tooltip" title="{{{ $tooltip }}}">
                    {{{ $t['phrase'] }}}
                </a>
                <a href="http://www.forvo.com/search-{{{ $t['code'] }}}/{{{ $t['phrase'] }}}" target="__forvo__"><i class="icon-play pull-right" title="search forvo"> </i></a>
            </td>
            <td>
                <span style="font-size: smaller">
                    {{{ $t['sentence'] }}}
                </span>
                <a href="https://translate.google.com/#{{{ $t['code'] }}}/en/{{{ $t['sentence'] }}}" target="__gt___">
                    <i class="icon-search pull-right" title="search google translate"> </i></a>
                </a>
            </td>
            <td>{{{ $t['added'] }}}</td>
            <td>{{{ $t['updated'] }}}</td>
            <td>
                <div class="btn-group">
                    <a href="{{ action("TermController@edit", $t['id']) }}" class="btn btn-default">edit</a>
                    <button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown">
                        <span class="caret"></span>
                        <span class="sr-only">Toggle Dropdown</span>
                    </button>
                    <ul class="dropdown-menu" role="menu">
                        <li>
                            {{ Form::open(array('action' => array('TermController@postDelete', $t['id']))) }}
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
