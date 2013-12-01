<?php

use RT\Core\FlashMessage;
use App\Models\Term;
use App\Models\Tag;
use App\Models\Text;
use App\Models\Language;
use RT\Services\ITextService;
use RT\Services\ITermService;
use RT\Services\ILanguageService;
use RT\Services\IParserService;

class AjaxController extends BaseController {
    private $textService;
    private $termService;
    private $parserService;
    private $languageService;
    
    public function __construct(ITextService $textService, ITermService $termService, IParserService $parserService, ILanguageService $languageService) {
        $this->textService = $textService;
        $this->termService = $termService;
        $this->parserService = $parserService;
        $this->languageService = $languageService;
    }
    
    private function termToStateClass($state) {
        $state = trim(strtolower($state));
        
        switch($state) {
            case "known": return "_k";
            case "ignore": return "_i";
            case "ignored": return "_i";
            case "notknown": return "_u";
            case "unknown": return "_u";
            case "notseen": return "_n";
            default: throw new \Exception("Unknown state: " . $state);
        }
    }
    
    private function termStateToDesription($state) {
        $state = trim(strtolower($state));
        
        switch($state) {
            case "known": return "Known";
            case "ignore": return "Ignored";
            case "ignored": return "Ignored";
            case "notknown": return "Unknown";
            case "unknown": return "Unknown";
            case "notseen": return "Not Seen";
            default: throw new \Exception("Unknown state: " . $state);
        }
    }
    
    public function postResetTerm() {
        $termId = Input::get('termId');
        
        $term = $this->termService->find($termId);
        
        if($term==null) {
            return Response::json();
        }
        
        $term->state = "unknown";
        $term->save();
        
        return Response::json(
                array(
                    "basePhrase" => $term->basePhrase,
                    "definition" => $term->definition,
                    "phrase" => $term->phraseLower,
                    "sentence" => $term->sentence,
                    "state" => $term->state,
                    "tags" => array(),
                    "termId" => $term->id,
                    "box" => 1,
                    "message" => "Term updated to <strong>" . $this->termStateToDesription($term->state),
                    "stateClass" => $this->termToStateClass($term->state)
                ));
    }
    
    private function termLength($phrase) {
        if(empty($phrase)) {
            return 0;
        }
        
        $ex = explode(' ', trim($phrase));
        
        return sizeof($ex);
    }
    
    public function postSaveTerm() {
        $rules = array(
            'phrase' => 'max:50',
            'basePhrase' => 'max:50',
            'sentence' => 'max:500',
            'definition' => 'max:500'
        );
        
        $validator = Validator::make(Input::all(), $rules);
        
        if($validator->fails()) {
            return Response::json(array('state'=>'failed', 'message'=>'<strong>WARNING</strong>: Term did not save, invalid input'));
        }
        
        $length = $this->termLength(Input::get('phrase'));
        $message = '';
        $termId = Input::get('termId');
        $tags = explode(',', Input::get('tags'));
        $returnTags = array();
        
        if(!isset($termId) || empty($termId)|| $termId<=0) {
            $term = new Term;
            $term->basePhrase = trim(Input::get('basePhrase'));
            $term->definition =trim(Input::get('definition'));
            $term->language_id = Input::get('languageId');
            $term->user_id = Auth::user()->id;
            $term->phrase = trim(Input::get('phrase'));
            $term->phraseLower = mb_strtolower($term->phrase);
            $term->sentence = trim(Input::get('sentence'));
            
            $state = Input::get('state');
            
            switch(strtolower($state)) {
                case "known": $term->state = 'known'; break;
                case "unknown": $term->state = 'unknown'; break;
                case "notseen": $term->state = 'notseen'; break;
                case "ignore": $term->state = 'ignored'; break;
                default: throw new \Exception('Unknown state: ' . $state);
            }
            
            $term->text_id = Input::get('textId');
            $term->length = $length;
            $term->save();
            
            foreach($tags as $tag) {
                $tag = trim(mb_strtolower($tag));
                
                if(empty($tag) || $tag==='') {
                    continue;
                }
                
                if(in_array($tag, $returnTags)) {
                    continue;
                }
                
                $exists = Tag::where('tag','=', $tag)->first();
                
                if($exists==null) {
                    $t = new Tag;
                    $t->tag = $tag;
                    $t->save();
                    
                    $term->Tags()->attach($t->id);
                } else {
                    $term->Tags()->attach($exists->id);
                }
                
                array_push($returnTags, $tag);
            }
            
            //$term->save();

            $message = "Term created, state is <strong>" . $this->termStateToDesription($term->state) . "</strong>";
        } else {
            $term = $this->termService->find($termId);
            
            if($term==null) {
                return Response::json(array('state'=>'failed', 'message'=>'<strong>WARNING</strong>: Term did not save, term not found'));    
            }

            $term->basePhrase = trim(Input::get('basePhrase'));
            $term->definition =trim(Input::get('definition'));
            
            $sentence = trim(Input::get('sentence'));
            
            if(strcmp(trim($term->sentence), $sentence)!=0) {
                $term->sentence = $sentence;
                $term->text_id = Input::get('textId');
            }
            
            $state = Input::get('state');
            
            switch(strtolower($state)) {
                case "known": $term->state = 'known'; break;
                case "unknown": $term->state = 'unknown'; break;
                case "notseen": $term->state = 'notseen'; break;
                case "ignore": $term->state = 'ignored'; break;
                default: throw new \Exception('Unknown state: ' . $state);
            }
            
            $term->Tags()->detach();
            
            foreach($tags as $tag) {
                $tag = trim(mb_strtolower($tag));
                
                if(empty($tag) || $tag==='') {
                    continue;
                }
                
                if(in_array($tag, $returnTags)) {
                    continue;
                }
                
                $exists = Tag::where('tag','=', $tag)->first();
                
                if($exists==null) {
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
            
            $message = "Term updated, state is <strong>" . $this->termStateToDesription($term->state) . "</strong>";
        }
        
        return Response::json(
                array(
                    "basePhrase" => $term->basePhrase,
                    "definition" => $term->definition,
                    "phrase" => $term->phraseLower,
                    "sentence" => $term->sentence,
                    "state" => $term->state,
                    "tags" => implode(',', $returnTags),
                    "termId" => $term->id,
                    "message" => $message,
                    "box" => 1,
                    "stateClass" => $this->termToStateClass($term->state),
                    "exists" => true
                ));
    }
    
    public function postFindTerm() {
        $termId = Input::get('termId');
        $languageId = Input::get('languageId');
        $spanTerm = Input::get('spanTerm');
        
        $length = $this->termLength($spanTerm);
        
        $term = null;
        
        if(!isset($termId) || empty($termId) || $termId<=0) {
            $term = $this->termService->findByPhraseAndLanguage($spanTerm, $languageId);
        } else {
            $term = $this->termService->find($termId);
        }
        
        if($term==null) {
            return Response::json(array(
                    'phrase' => $spanTerm,
                    'length' => $length,
                    'state' => 'unknown',
                    'message' => 'New word, default to <strong>UNKNOWN</strong>',
                    'exists' => false
            ));
        } else {
            $tags = array();
            foreach($term->tags as $t) {
                array_push($tags, $t->tag);
            }
            
            return Response::json(array(
                    'basePhrase' => $term->basePhrase,
                    'definition' => $term->definition,
                    'phrase' => $term->phrase,
                    'sentence' => $term->sentence,
                    'state' => $term->state,
                    'termId' => $term->id,
                    'message' => '',
                    'length' => $term->length,
                    'box' => 1,
                    'tags' => implode(',',$tags),
                    'exists' => true
            ));
        }
    }
    
    public function postMarkRemainingAsKnown() {
        $languageId = Input::get('languageId');
        $textId = Input::get('textId');
        $userId = Auth::user()->id;
        $terms = Input::get('terms');
        
        $language = $this->languageService->find($languageId);
        $termTestRegex = "/([^" . $language->getSettings()->RegexWordCharacters . "]+)/";
        
        $processed = array();
        
        foreach($terms as $term) {
            $term = trim($term);
            $lower = mb_strtolower($term);
            
            if(empty($term) || $term==='' || array_key_exists($lower, $processed)) {
                continue;
            }
            
            if(preg_match($termTestRegex, $term)==0) {
                $t = new Term;
                $t->language_id = $languageId;
                $t->user_id = $userId;
                $t->phrase = $term;
                $t->phraseLower = $lower;
                $t->text_id = $textId;
                $t->state = 'known';
                $t->length = 1;
                $t->save();
                
                $processed[$lower] = true;
            }
        }
        
        return "OK";
    }
    
    public function postEncodeTerm() {
        $languageId = Input::get('languageId');
        $dictionaryId = Input::get('dictionaryId');
        $input = Input::get('input');
        
        //$language = $this->languageService->find($languageId);
        $dictionary = $this->languageService->findDictionary($dictionaryId);
        $word = mb_convert_encoding($input, "$dictionary->encoding", 'UTF-8');
        
        return Response::json(str_replace('###', urlencode($word), $dictionary->url));
    }
}
