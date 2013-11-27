<?php

use RT\Core\FlashMessage;
use App\Models\Group;
use App\Models\Language;
use RT\Services\IGroupService;
use RT\Services\ITextService;
use RT\Services\ILanguageService;
use RT\Services\ITermService;
use RT\Services\IParserService;

class GroupController extends BaseController {
    private $groupService;
    private $textService;
    private $languageService;
    private $termService;
    private $parserService;
    
    private $rules = array(
            'name' => 'max:50|required',
            'description' => 'max:1000',
            'type' => 'required|in:public,private'
        );
    
    public function __construct(
            IGroupService $groupService, 
            ITextService $textService, 
            ILanguageService $languageService, 
            ITermService $termService,
            IParserService $parserService
            ) 
                {
        $this->groupService = $groupService;
        $this->textService = $textService;
        $this->languageService = $languageService;
        $this->termService = $termService;
        $this->parserService = $parserService;
        
        View::share('currentController', 'Group');
    }
    
    public function index() {
        $groups = $this->groupService->findAllForUser();

        return View::make('groups.index')
                ->with('groups', $groups)
                ;
    }
    
    public function add() {
        return View::make('groups.add');
    }
    
    public function postAdd() {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $group = new Group();
        $group->name = Input::get('name');
        $group->description = Input::get('description');
        $group->type = Input::get('type');
        $group->save();
        
        $group->members()->attach(Auth::user()->id, array('membership'=>'owner'));
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your group has been added.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@index');
    }
    
    public function edit($id) {
        $group = $this->groupService->findOneByOwner($id);
        
        if($group==null) {
            throw new Exception;
        }
        
        return View::make('groups.edit')
                ->with('group', $group)
                ;
    }
    
    public function postEdit($id) {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $group = $this->groupService->findOneByOwner($id);
        $group->name = Input::get('name');
        $group->description = Input::get('description');
        $group->type = Input::get('type');
        
        $group->save();
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your group has been updated.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@edit', array('id'=>$id));
    }
    
    public function postDelete($id) {
        $this->groupService->delete($id);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your group has been deleted.', FlashMessage::SUCCESS));
        return Redirect::action('GroupController@index');
    }
    
    public function membership($id) {
        $group = $this->groupService->findOneByOwnerOrModerator($id);
        $members = $this->groupService->findMembers($id);
        
        return View::make('groups.membership')
                ->with('group', $group)
                ->with('members', $members);
    }
    
    public function postMembership($id) {
        $group = $this->groupService->findOneByOwnerOrModerator($id);
        $members = $this->groupService->findMembers($id);
        
        $mlist = Input::get('membership');
        $usernames = Input::get('usernames');
        
        if(!empty($mlist)) {
            foreach($mlist as $uid=>$type) {
                $this->groupService->updateMembership($uid, $group->id, $type);
            }
        }
        
        if(!empty($usernames)) {
            foreach(explode(' ', $usernames) as $username) {
                $this->groupService->inviteUser($group->id, $username);
            }
        }
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your group membership has been updated.', FlashMessage::SUCCESS));

        return Redirect::action('GroupController@membership', array('id'=>$id));
    }
    
    public function postAcceptMembership($id) {
        $userId = Auth::user()->id;
        $this->groupService->updateMembership($userId, $id, 'member');
        Session::flash(FlashMessage::MSG, new FlashMessage('You have accepted the invitation to the group.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@index');
    }
    
    public function postDeclineMembership($id) {
        $userId = Auth::user()->id;
        $this->groupService->updateMembership($userId, $id, 'delete');
        Session::flash(FlashMessage::MSG, new FlashMessage('Your have declined the invitation to the group.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@index');
    }
    
    public function postLeaveGroup($id) {
        $userId = Auth::user()->id;
        $this->groupService->updateMembership($userId, $id, 'delete');
        Session::flash(FlashMessage::MSG, new FlashMessage('Your have left this group.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@index');
    }
    
    public function texts($id) {
        $group = $this->groupService->findOneByOwnerModeratorOrMember($id);
        
        if($group==null) {
            return Redirect::action('GroupController@index');
        }
        
        return View::make('groups.texts')
                ->with('group', $group)
                ;
    }
    
    public function postTexts($id) {
        $group = $this->groupService->findOneByOwnerModeratorOrMember($id);
        
        if($group==null) {
            return "";
        }
        
        $filter = Input::get('filter');
        $perPage = Input::get('perPage');
        $currentPage = Input::get('page');
        
        $result = $this->groupService->findAllTextsByFilter($id, $filter, $perPage, $currentPage);
        
        $tarray = array();
        
        foreach($result['texts'] as $t) {
            array_push($tarray, array(
                'id' => $t->id,
                'language' => $t->language_name,
                'title' => $t->title,
                'collectionName' => $t->collectionName,
                'collectionNo' => $t->collectionNo,
                'isParallel' => $t->l2_id!=null,
                'hasAudio' => !empty($t->audioUrl) && trim($t->audioUrl)!='',
                'isShared' => $t->isShared==1,
                'userId' => $t->user_id,
                'isOwner' => $t->user_id==Auth::user()->id,
                'owner' => !empty($t->displayName) && trim($t->displayName)!='' ? $t->displayName : $t->username
            ));
        }
        
        $pages = ceil($result['count']/$perPage);

        $content = View::make('groups.grid')
                ->with('group', $group)
                ->with('texts', $tarray)
                ->with('currentPage', $currentPage)
                ->with('pages', $pages)
                ->__toString()
                ;
        
        return $content;
    }
    
    public function postUnshare() {
        $groupId = Input::get('groupId');
        $group = $this->groupService->findOneByOwnerModeratorOrMember($groupId);
        
        if($group==null) {
            Session::flash(FlashMessage::MSG, new FlashMessage('You do not have access to this group.', FlashMessage::WARNING));
            return Redirect::action('GroupController@index');
        }
        
        foreach(explode(',', Input::get('texts')) as $id) {
            $this->groupService->unshareText($group->id, $id, false);
        }
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your texts have been unshared.', FlashMessage::SUCCESS));
        
        return Redirect::action('GroupController@texts', array('id'=>$groupId));
    }
    
    public function read($groupId, $textId) {
        $group = $this->groupService->findOneByOwnerModeratorOrMember($groupId);
        
        if($group==null) {
            return Redirect::action('GroupController@index');
        }
        
        $text = $this->textService->findForGroupUser($textId, $groupId);
        $language1 = $this->languageService->findOneByCode($text->language1->code);
        
        if(!$text->shareAudioUrl) {
            $text->audioUrl = null;
        }
        
        if($language1==null) {
            $slanguage = $this->languageService->findOneSystemLanguage($text->language1->code);
            Session::flash(FlashMessage::MSG, new FlashMessage('Sorry, you do not have the language ' . $slanguage->language . ' ('. $text->language1->code . ') in your languages.', FlashMessage::WARNING));
            return Redirect::action('GroupController@texts', array('id'=>$groupId));
        }
        
        $terms = $this->termService->findAllForLanguage($language1->id);
        
        $parsed = $this->parserService->parse(
                FALSE,
                $language1,
                null,
                $terms,
                $text
                );
        
        return View::make('groups.read')
                ->with('parsed', $parsed)
                ->with('asParallel', FALSE)
                ->with('text', $text)
                ->with('language1', $language1)
                ->with('language2', null)
                ->with('user', Auth::user())
                ->with('nextId', null)
                ->with('previousId', null)
                ->with('groupId', $groupId)
                ;
    }
    
    public function readParallel($groupId, $textId) {
        $group = $this->groupService->findOneByOwnerModeratorOrMember($groupId);
        
        if($group==null) {
            return Redirect::action('GroupController@index');
        }
        
        $text = $this->textService->findForGroupUser($textId, $groupId);
        
        if(!$text->shareAudioUrl) {
            $text->audioUrl = null;
        }
        
        $language1 = $this->languageService->findOneByCode($text->language1->code);
        $language2 = $this->languageService->findOneByCode($text->language2->code);

        if($language1==null) {
            $slanguage = $this->languageService->findOneSystemLanguage($text->language1->code);
            Session::flash(FlashMessage::MSG, new FlashMessage('Sorry, you do not have the language ' . $slanguage->language . ' ('. $text->language1->code . ') in your languages.', FlashMessage::WARNING));
            return Redirect::action('GroupController@texts', array('id'=>$groupId));
        }
        
        if($language2==null) {
            $slanguage = $this->languageService->findOneSystemLanguage($text->language2->code);
            Session::flash(FlashMessage::MSG, new FlashMessage('Sorry, you do not have the language ' . $slanguage->language . ' ('. $text->language2->code . ') in your languages.', FlashMessage::WARNING));
            return Redirect::action('GroupController@texts', array('id'=>$groupId));
        }

        $terms = $this->termService->findAllForLanguage($language1->id);
        
        $parsed = $this->parserService->parse(
                TRUE,
                $language1,
                $language2,
                $terms,
                $text
                );
        
        return View::make('groups.read')
                ->with('parsed', $parsed)
                ->with('asParallel', TRUE)
                ->with('text', $text)
                ->with('language1', $language1)
                ->with('language2', $language2)
                ->with('user', Auth::user())
                ->with('nextId', null)
                ->with('previousId', null)
                ->with('groupId', $groupId)
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
    
    public function findGroups() {
        View::share('currentController', 'PublicGroup');
        return View::make('groups.find');
    }
    
    public function postFindGroups() {
        $filter = Input::get('filter');
        $perPage = Input::get('perPage');
        $currentPage = Input::get('page');
        
        $result = $this->groupService->findAllGroupsByFilter($filter, $perPage, $currentPage);
        
        $tarray = array();
        
        foreach($result['groups'] as $g) {
            array_push($tarray, array(
                'id' => $g->id,
                'name' => $g->name
            ));
        }
        
        $pages = ceil($result['count']/$perPage);

        $content = View::make('groups.findgrid')
                ->with('groups', $tarray)
                ->with('currentPage', $currentPage)
                ->with('pages', $pages)
                ->__toString()
                ;
        
        return $content;
    }
    
    public function postJoinGroup($id) {
        $this->groupService->updateMembership(Auth::user()->id, $id, 'pending');
        Session::flash(FlashMessage::MSG, new FlashMessage('A request has been sent to the group for moderation.', FlashMessage::SUCCESS));
        return Redirect::action('GroupController@findGroups');
    }
}
