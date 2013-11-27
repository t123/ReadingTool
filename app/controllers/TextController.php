<?php

use RT\Core\FlashMessage;
use App\Models\Text;
use App\Models\Language;
use RT\Services\ITextService;
use RT\Services\ITermService;
use RT\Services\ILanguageService;
use RT\Services\IGroupService;
use RT\Services\IParserService;

class TextController extends BaseController {
    private $textService;
    private $termService;
    private $parserService;
    private $languageService;
    private $groupService;
    
    private $rules = array(
            'title' => 'max:100|required',
            'collectionName' => 'max:100',
            'collectionNo' => 'integer',
            'audioUrl' => 'max:250',
            'l1_id' => 'required',
            'l1Text' => 'required'
        );
    
    public function __construct(
            ITextService $textService, 
            ITermService $termService, 
            IParserService $parserService, 
            ILanguageService $languageService,
            IGroupService $groupService
        ) {
        $this->textService = $textService;
        $this->termService = $termService;
        $this->parserService = $parserService;
        $this->languageService = $languageService;
        $this->groupService = $groupService;
        
        View::share('currentController', 'Text');
    }
    
    public function index() {
        $groups = $this->groupService->findAllForUser();
        $garray = array();
        
        foreach($groups as $g) {
            $garray[$g->id] = $g->name;
        }
        
        return View::make('texts.index')
                ->with('groups', $garray);
    }
    
    public function postIndex() {
        $filter = Input::get('filter');
        $perPage = Input::get('perPage');
        $currentPage = Input::get('page');
        $sort = Input::get('sort');
        $sortDir = Input::get('sortDir');
        $result = $this->textService->findAllByFilter($filter, $perPage, $currentPage, $sort, $sortDir);
        
        $tarray = array();
        
        foreach($result['texts'] as $t) {
            array_push($tarray, array(
                'id' => $t->id,
                'language' => $t->language_name,
                'title' => $t->title,
                'collectionName' => $t->collectionName,
                'collectionNo' => $t->collectionNo,
                'lastRead' => \RT\Core\HumanTime::toReadable($t->lastRead),
                'isParallel' => $t->l2_id!=null,
                'hasAudio' => !empty($t->audioUrl) && trim($t->audioUrl)!='',
                'isShared' => $t->isShared==1,
                'userId' => $t->user_id,
                'isOwner' => $t->user_id==Auth::user()->id
            ));
        }
        
        $pages = ceil($result['count']/$perPage);

        $content = View::make('texts.grid')
                ->with('texts', $tarray)
                ->with('currentPage', $currentPage)
                ->with('pages', $pages)
                ->__toString()
                ;
        
        return $content;
    }
    
    public function add() {
        $languages = $this->languageService->findAll()->lists('name', 'id');
        
        return View::make('texts.add')
                ->with('languages', $languages)
                ;
    }
    
    public function postAdd() {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $text = new Text();
        $text->user_id = Auth::user()->id;
        $text->title = Input::get('title');
        $text->collectionName = Input::get('collectionName');
        
        $collectionNo = Input::get('collectionNo');
        $text->collectionNo = empty($collectionNo) ? null : $collectionNo;
        
        $text->audioUrl = Input::get('audioUrl');
        $text->shareAudioUrl = Input::get('shareAudioUrl')=="on";
        $text->l1_id = Input::get('l1_id');
        
        $l2id = Input::get('l2_id');
        
        if(empty($l2id)) {
            $text->l2_id = null;
            $l2Text = "";
        } else {
            $text->l2_id = $l2id;
            $l2Text = Input::get('l2Text');
        }            
        
        $this->textService->save($text, Input::get('l1Text'), $l2Text);
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your text has been added.', FlashMessage::SUCCESS));
        
        return Redirect::action('TextController@index');
    }
    
    public function edit($id) {
        $text = $this->textService->find($id);
        $languages = $this->languageService->findAll()->lists('name', 'id');
        
        return View::make('texts.edit')
                ->with('text', $text)
                ->with('languages', $languages)
                ;
    }
    
    public function postEdit($id) {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $text = $this->textService->find($id);
        $text->title = Input::get('title');
        $text->collectionName = Input::get('collectionName');
        
        $collectionNo = Input::get('collectionNo');
        $text->collectionNo = empty($collectionNo) ? null : $collectionNo;

        $text->audioUrl = Input::get('audioUrl');
        $text->shareAudioUrl = Input::get('shareAudioUrl')=="on";
        $text->l1_id = Input::get('l1_id');
        $l2id = Input::get('l2_id');
        
        if(empty($l2id)) {
            $text->l2_id = null;
            $l2Text = "";
        } else {
            $text->l2_id = $l2id;
            $l2Text = Input::get('l2Text');
        }            
        
        $this->textService->save($text, Input::get('l1Text'), $l2Text);
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your text has been updated.', FlashMessage::SUCCESS));
        
        return Redirect::action('TextController@edit', array('id'=>$id));
    }
    
    public function postDelete($id) {
        $this->textService->delete($id);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your text has been deleted.', FlashMessage::SUCCESS));
        return Redirect::action('TextController@index');
    }
    
    public function read($id) {
        $text = $this->textService->find($id);
        $terms = $this->termService->findAllForLanguage($text->language1->id);
        
        $parsed = $this->parserService->parse(
                FALSE,
                $text->language1,
                null,
                $terms,
                $text
                );
        
        $text->lastRead = date("Y-m-d H:i:s");
        unset($text->l1Text);
        unset($text->l2Text);
        $text->save();
        $next = $this->textService->nextText($text);
        $previous = $this->textService->previousText($text);
        
        return View::make('texts.read')
                ->with('parsed', $parsed)
                ->with('asParallel', FALSE)
                ->with('text', $text)
                ->with('language1', $text->language1)
                ->with('language2', null)
                ->with('user', Auth::user())
                ->with('nextId', $next==null ? null : $next->id)
                ->with('previousId', $previous==null ? null : $previous->id)
                ;
    }
    
    public function readParallel($id) {
        $text = $this->textService->find($id);
        $terms = $this->termService->findAllForLanguage($text->language1->id);
        
        $parsed = $this->parserService->parse(
                TRUE,
                $text->language1,
                $text->language2,
                $terms,
                $text
                );
        
        $text->lastRead = date("Y-m-d H:i:s");
        unset($text->l1Text);
        unset($text->l2Text);
        $text->save();
        $next = $this->textService->nextText($text);
        $previous = $this->textService->previousText($text);
        
        return View::make('texts.read')
                ->with('parsed', $parsed)
                ->with('asParallel', TRUE)
                ->with('text', $text)
                ->with('language1', $text->language1)
                ->with('language2', $text->language2)
                ->with('user', Auth::user())
                ->with('nextId', $next==null ? null : $next->id)
                ->with('previousId', $previous==null ? null : $previous->id)
                ;
    }
    
    public function downloadPdf($id) {
        $text = $this->textService->find($id);
        $terms = $this->termService->findAllForLanguage($text->language1->id);
        
        $parserService = new \RT\Services\LatextParserService;
        $parsed = $parserService->parse(
                FALSE,
                $text->language1,
                null,
                $terms,
                $text
                );
        
        $path = storage_path() . "/pdf";
        $tmp = tempnam($path, "rt-latex");
        $latex = $tmp . ".tex";
        $pdf = $tmp . ".pdf";
        
        file_put_contents($latex, $parsed);
        
        $cmd = "/usr/bin/pdflatex -output-directory $path --interaction batchmode $latex";
        $output = shell_exec($cmd);
        
        return Response::download($pdf, $text->title . ".pdf", 
                    array('Content-Type' => 'application/pdf')
                );
    }
    
    public function postShare() {
        $groupId = Input::get('groupId');
        $group = $this->groupService->findOneByOwnerModeratorOrMember($groupId);
        
        if($group==null) {
            Session::flash(FlashMessage::MSG, new FlashMessage('You do not have access to this group.', FlashMessage::WARNING));
            return Redirect::action('TextController@index');
        }
        
        foreach(explode(',', Input::get('texts')) as $id) {
            $this->groupService->shareText($group->id, $id, false);
        }
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your texts have been shared.', FlashMessage::SUCCESS));
        return Redirect::action('TextController@index');
    }
    
    public function postDeleteMany() {
        foreach(explode(',', Input::get('texts')) as $id) {
            $this->textService->delete($id);
        }
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your texts have been deleted.', FlashMessage::SUCCESS));
        return Redirect::action('TextController@index');
    }
}
