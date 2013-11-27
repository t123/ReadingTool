<?php

use RT\Core\FlashMessage;
use App\Models\Term;
use App\Models\Tag;
use App\Models\Language;
use RT\Services\ITermService;

class TermController extends BaseController {

    private $termService;

    public function __construct(ITermService $termService) {
        $this->termService = $termService;

        View::share('currentController', 'Term');
    }

    public function index() {
        return View::make('terms.index');
    }

    public function postIndex() {
        $filter = Input::get('filter');
        $perPage = Input::get('perPage');
        $currentPage = Input::get('page');
        $sort = Input::get('sort');
        $sortDir = Input::get('sortDir');
        $result = $this->termService->findAllByFilter($filter, $perPage, $currentPage, $sort, $sortDir);

        $tarray = array();

        foreach ($result['terms'] as $t) {
            array_push($tarray, array(
                'id' => $t->id,
                'language' => $t->language_name,
                'state' => $t->state,
                'phrase' => $t->phrase,
                'sentence' => $t->sentence,
                'added' => \RT\Core\HumanTime::toReadable($t->created_at),
                'updated' => \RT\Core\HumanTime::toReadable($t->updated_at)
            ));
        }

        $pages = ceil($result['count'] / $perPage);

        $content = View::make('terms.grid')
                ->with('terms', $tarray)
                ->with('currentPage', $currentPage)
                ->with('pages', $pages)
                ->__toString()
        ;

        return $content;
    }

    public function edit($id) {
        $term = Term::find($id);

        return View::make('terms.edit')
                        ->with('term', $term)
        ;
    }

    public function postEdit($id) {
        $returnTags = array();

        $tags = explode(',', Input::get('tags'));
        $term = Term::find($id);

        $term->basePhrase = trim(Input::get('basePhrase'));
        $term->definition = trim(Input::get('definition'));
        $term->sentence = Input::get('sentence');
        $term->state = strtolower(Input::get('state'));
        $term->Tags()->detach();

        foreach ($tags as $tag) {
            $tag = trim(mb_strtolower($tag));

            if (empty($tag) || $tag === '') {
                continue;
            }

            if (in_array($tag, $returnTags)) {
                continue;
            }

            $exists = Tag::where('tag', '=', $tag)->first();

            if ($exists == null) {
                $t = new Tag;
                $t->tag = $tag;
                $t->save();

                $term->Tags()->attach($t->id);
            } else {
                $term->Tags()->attach($exists->id);
            }

            array_push($returnTags, $tag);
        }

        $term->save();

        Session::flash(FlashMessage::MSG, new FlashMessage('Your term has been updated.'));

        return Redirect::action('TermController@edit', array('id' => $id));
    }

    public function postDelete($id) {
        Term::destroy($id);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your term has been deleted.'));
        return Redirect::action('TermController@index');
    }

    public function exportTerms() {
        $termService = App::make('RT\Services\TermService');
        $languageService = App::make('RT\Services\LanguageService');

        $languages = $languageService->findAll();

        $larray = array();
        foreach ($languages as $l) {
            $larray[$l->id] = $l->name;
        }

        $terms = $termService->findAll('unknown');

        $te = array(
            'id' => 'id',
            'state' => 'state',
            'phrase' => 'phrase',
            'basePhrase' => 'basePhrase',
            'definition' => 'definition',
            'created' => 'created_at',
            'updated' => 'updated_at',
            'sentence' => 'sentence',
            'language' => 'language',
            'tags' => 'tags'
        );

        $tsv = implode("\t", $te) . "\n";

        foreach ($terms as $t) {
            $tags = array();

            foreach ($t->tags as $ctag) {
                array_push($tags, $this->fixForTSV($ctag->tag));
            }

            $te = array(
                'id' => $t->id,
                'state' => $t->state,
                'phrase' => $t->phrase,
                'basePhrase' => $this->fixForTSV($t->basePhrase),
                'definition' => $this->fixForTSV($t->definition),
                'created' => $t->created_at,
                'updated' => $t->updated_at,
                'sentence' => $this->fixForTSV($t->sentence),
                'language' => $this->fixForTSV($larray[$t->language_id]),
                'tags' => implode(",", $tags)
            );

            $tsv .= implode("\t", $te) . "\n";
        }

        return Response::make($tsv, 200, array(
                    'Content-Description' => 'File Transfer',
                    'Content-Type' => 'text/html',
                    'Content-Disposition' => 'attachment; filename="terms.tsv"',
                    'Content-Transfer-Encoding' => 'binary',
                    'Expires' => 0,
                    'Cache-Control' => 'must-revalidate, post-check=0, pre-check=0',
                    'Pragma' => 'public',
                    'Content-Length' => mb_strlen($tsv)
        ));
    }

    private function fixForTSV($input) {
        $input = str_replace("\t", "        ", $input);
        $input = str_replace("\n", "<br/>", $input);
        return $input;
    }

}
