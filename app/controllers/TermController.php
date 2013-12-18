<?php

use RT\Core\FlashMessage;
use App\Models\Term;
use App\Models\Tag;
use App\Models\Language;
use RT\Services\ITermService;
use RT\Services\ILanguageService;

class TermController extends BaseController {

    private $termService;
    private $languageService;

    public function __construct(ITermService $termService, RT\Services\ILanguageService $languageService) {
        $this->termService = $termService;
        $this->languageService = $languageService;
        
        View::share('currentController', 'Term');
    }

    public function index() {
        $languages = $this->languageService->findAll();

        return View::make('terms.index')
                        ->with('languages', $languages)
        ;
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
            $source = empty($t->collectionNo) ? "" : $t->collectionNo . ". ";
            $source .= empty($t->collectionName) ? "" : $t->collectionName . ": ";
            $source .= $t->title;

            array_push($tarray, array(
                'id' => $t->id,
                'language' => $t->language_name,
                'code' => $t->language_code,
                'basePhrase' => $t->basePhrase,
                'definition' => $t->definition,
                'state' => $t->state,
                'phrase' => $t->phrase,
                'sentence' => $t->sentence,
                'added' => \RT\Core\HumanTime::toReadable($t->created_at),
                'updated' => \RT\Core\HumanTime::toReadable($t->updated_at),
                'source' => $source,
                'text_id' => $t->text_id,
                'l1_id' => $t->l1_id,
                'l2_id' => $t->l2_id,
                'group_id' => $t->group_id
            ));
        }

        $pages = ceil($result['count'] / $perPage);

        $content = View::make('terms.grid')
                ->with('terms', $tarray)
                ->with('currentPage', $currentPage)
                ->with('pages', $pages)
                ->with('count', $result['count'])
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
        $this->termService->delete($id);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your term has been deleted.'));
        return Redirect::action('TermController@index');
    }

    public function exportTerms($id) {
        $termService = App::make('RT\Services\TermService');
        $languageService = App::make('RT\Services\LanguageService');

        $languages = $languageService->findAll();

        $larray = array();
        foreach ($languages as $l) {
            $larray[$l->id] = $l->name;
        }

        if($id<=0) {
            $terms = $termService->findAll('unknown');
        } else {
            $terms = $termService->findAllForLanguage($id, 'unknown');
        }
        
//        $te = array(
//            'id' => 'id',
//            'state' => 'state',
//            'phrase' => 'phrase',
//            'basePhrase' => 'basePhrase',
//            'definition' => 'definition',
//            'created' => 'created_at',
//            'updated' => 'updated_at',
//            'sentence' => 'sentence',
//            'language' => 'language',
//            'tags' => 'tags'
//        );

//        $tsv = implode("\t", $te) . "\n";
        $tsv = "";
        
        foreach ($terms as $t) {
            $tags = array();
            
            foreach ($t->tags as $ctag) {
                array_push($tags, $this->fixForTSV($ctag->tag));
            }

            $source = empty($t->collectionNo) ? "" : $t->collectionNo . ". ";
            $source .= empty($t->collectionName) ? "" : $t->collectionName . ": ";
            $source .= $t->title;
            
            $te = array(
                'id' => $t->id,
                'state' => $t->state,
                'phrase' => $t->phrase,
                'basePhrase' => $this->fixForTSV($t->basePhrase),
                'definition' => $this->fixForTSV($t->definition),
                'created' => $t->created_at,
                'updated' => $t->updated_at,
                'sentence' => preg_replace("/$t->phrase/iu", "<strong>\$0</strong>", $this->fixForTSV($t->sentence)),
                'language' => $this->fixForTSV($larray[$t->language_id]),
                'tags' => implode(",", $tags),
                'source' => $this->fixForTSV($source),
                'text_id' => $t->text_id
            );

            $tsv .= implode("\t", $te) . "\n";
        }

        if(strlen($tsv)>0) {
            $tsv = substr($tsv,0,-1);
        }

        return Response::make($tsv, 200, array(
                    'Content-Description' => 'File Transfer',
                    'Content-Type' => 'text/csv',
                    'Content-Disposition' => 'attachment; filename="terms.tsv"',
                    'Content-Transfer-Encoding' => 'binary',
                    'Expires' => 0,
                    'Cache-Control' => 'must-revalidate, post-check=0, pre-check=0',
                    'Pragma' => 'public',
                    'Content-Length' => strlen($tsv)
        ));
    }

    private function fixForTSV($input) {
        $input = str_replace("\t", "        ", $input);
        $input = str_replace("\n", "<br/>", $input);
        return $input;
    }

}
