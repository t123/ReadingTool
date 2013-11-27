<?php

use RT\Services\TermService;
use RT\Services\LanguageService;

class ApiController extends BaseController {

    private function makeTerms() {
        $termService = App::make('RT\Services\TermService');
        $languageService = App::make('RT\Services\LanguageService');

        $languages = $languageService->findAll();

        $larray = array();
        foreach ($languages as $l) {
            $larray[$l->id] = $l->name;
        }

        $terms = $termService->findAll();

        $tarray = array();
        foreach ($terms as $t) {
            $te = array(
                'id' => $t->id,
                'state' => $t->state,
                'phrase' => $t->phrase,
                'basePhrase' => $t->basePhrase,
                'definition' => $t->definition,
                'created' => $t->created_at,
                'updated' => $t->updated_at,
                'sentence' => $t->sentence,
                'language' => $larray[$t->language_id]
            );

            array_push($tarray, $te);
        }
        
        return $tarray;
    }
    
    public function getTerms() {
        return Response::json(array('terms'=>$this->makeTerms()));
    }

    public function postTerms() {
        return Response::json(array('terms'=>$this->makeTerms()));
    }
}
