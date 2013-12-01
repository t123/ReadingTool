<?php

use RT\Services\TermService;
use RT\Services\LanguageService;

class ApiController extends BaseController {

    private function createTerm($t, $larray) {
        $tags = array();

        foreach ($t->tags as $ctag) {
            array_push($tags, $this->$ctag->tag);
        }
        
        return array(
            'id' => $t->id,
            'state' => $t->state,
            'phrase' => $t->phrase,
            'basePhrase' => $t->basePhrase,
            'definition' => $t->definition,
            'created' => $t->created_at,
            'updated' => $t->updated_at,
            'sentence' => $t->sentence,
            'language' => $larray[$t->language_id],
            'text_id' => $t->text_id,
            'tags' => implode(",", $tags)
        );
    }
    
    private function createText($t, $larray) {
        return array(
            'id' => $t->id,
            'title' => $t->title,
            'collectionName' => $t->collectionName,
            'language1' => $larray[$t->l1_id],
            'language2' => $t->l2_id==null ? "" : $larray[$t->l2_id],
            'created' => $t->created_at,
            'updated' => $t->updated_at,
            'lastRead' => $t->lastRead,
            'audioUrl' => $t->audioUrl,
            'l1Text' => $t->l1Text,
            'l2Text' => $t->l2Text
        );
    }

    private function makeTerms($id) {
        $termService = App::make('RT\Services\TermService');
        $languageService = App::make('RT\Services\LanguageService');

        $languages = $languageService->findAll();

        $larray = array();
        $tarray = array();

        foreach ($languages as $l) {
            $larray[$l->id] = $l->name;
        }

        if ($id == null) {
            $terms = $termService->findAll();

            foreach ($terms as $t) {
                array_push($tarray, $this->createTerm($t, $larray));
            }
        } else {
            $terms = $termService->find($id);

            if ($terms != null) {
                array_push($tarray, $this->createTerm($terms, $larray));
            }
        }

        return $tarray;
    }
    
    private function makeTexts($id) {
        $textService = App::make('RT\Services\TextService');
        $languageService = App::make('RT\Services\LanguageService');

        $languages = $languageService->findAll();

        $larray = array();
        $tarray = array();

        foreach ($languages as $l) {
            $larray[$l->id] = $l->name;
        }

        if ($id == null) {
            $texts = $textService->findAll();

            foreach ($texts as $t) {
                array_push($tarray, $this->createText($t, $larray));
            }
        } else {
            $texts = $textService->find($id);

            if ($texts != null) {
                array_push($tarray, $this->createText($texts, $larray));
            }
        }

        return $tarray;
    }

    public function getTerms($id = null) {
        return Response::json(array('terms' => $this->makeTerms($id)));
    }

    public function getLanguages($id = null) {
        $languageService = App::make('RT\Services\LanguageService');
        $larray = array();

        if ($id == null) {
            $languages = $languageService->findAll();
            
            foreach ($languages as $l) {
                array_push($larray, array(
                    'id' => $l->id,
                    'name' => $l->name,
                    'code' => $l->code,
                    'settings' => $l->settings
                ));
            }
        } else {
            $l = $languageService->find($id);
            
            if($l!=null) {
                array_push($larray, array(
                    'id' => $l->id,
                    'name' => $l->name,
                    'code' => $l->code,
                    'settings' => $l->settings
                ));
            }
        }

        return Response::json(array('languages' => $larray));
    }

    public function getTexts($id = null) {
        return Response::json(array('texts' => $this->makeTexts($id)));
    }

}
