<?php

use RT\Core\FlashMessage;
use App\Models\Language;
use App\Models\SLanguage;
use App\Models\Dictionary;
use RT\Services\ILanguageService;
use RT\Services\LanguageService;

class LanguageController extends BaseController {
    private $languageService;
    private $dictRules = array(
            'name' => 'max:25|required',
            'windowName' => 'max:25|alpha_dash',
            'url' => 'required|max:100',
            'encoding' => 'max:10',
        );
    
    private $rules = array(
            'name' => 'max:20|required',
            'code' => 'max:2|required|alpha',
            'ModalBehaviour' => 'required',
            'RegexWordCharacters' => 'required',
            'RegexSplitSentences' => 'required',
            'Direction' => 'required'
        );
    
    public function __construct(ILanguageService $languageService) {
        $this->languageService = $languageService;
        View::share('currentController', 'Language');
    }
    
    public function index() {
        $languages = $this->languageService->findAll();
        return View::make('languages.index')->with('languages', $languages);
    }
    
    public function add() {
        return View::make('languages.add')
                ->with('slanguages', $this->languageService->findAllSystemLanguages()->lists('language', 'code'))
                ;
    }
    
    public function postAdd() {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $language = new Language();
        $language->name = Input::get('name');
        $language->code = Input::get('code');
        $language->user_id = Auth::user()->id;
        $language->settings = json_encode(
                array(
                    'ModalBehaviour' => Input::get('ModalBehaviour'),
                    'Modal' => Input::get('Modal'),
                    'RegexWordCharacters' => Input::get('RegexWordCharacters'),
                    'RegexSplitSentences' => Input::get('RegexSplitSentences'),
                    'Direction' => Input::get('Direction'),
                    'ShowSpaces' => Input::get('ShowSpaces'),
                    'AutoPause' => Input::get('AutoPause'),
                )
                );
        $language->save();
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your language has been added.', FlashMessage::SUCCESS));
        
        return Redirect::action('LanguageController@index');
    }
    
    public function edit($id) {
        $language = $this->languageService->find($id);
        return View::make('languages.edit')
                ->with('language', $language)
                ->with('slanguages', $this->languageService->findAllSystemLanguages()->lists('language', 'code'))
                ;
    }
    
    public function postEdit($id) {
        $validator = Validator::make(Input::all(), $this->rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $language = $this->languageService->find($id);
        $language->name = Input::get('name');
        $language->code = Input::get('code');
        $language->archived = Input::get('archived') == "on";
        $language->settings = json_encode(
                array(
                    'ModalBehaviour' => Input::get('ModalBehaviour'),
                    'Modal' => Input::get('Modal'),
                    'RegexWordCharacters' => Input::get('RegexWordCharacters'),
                    'RegexSplitSentences' => Input::get('RegexSplitSentences'),
                    'Direction' => Input::get('Direction'),
                    'ShowSpaces' => Input::get('ShowSpaces'),
                    'AutoPause' => Input::get('AutoPause'),
                )
                );
        $language->save();
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your language has been updated.', FlashMessage::SUCCESS));
        
        return Redirect::action('LanguageController@edit', array('id'=>$id));
    }
    
    public function postDelete($id) {
        $this->languageService->delete($id);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your language has been deleted.', FlashMessage::SUCCESS));
        return Redirect::action('LanguageController@index');
    }
    
    public function addDictionary($id) {
        $language = $this->languageService->find($id);
        
        return View::make('languages.addDictionary')
                ->with('language', $language)
                ;
    }
    
    public function postAddDictionary($id) {
        $validator = Validator::make(Input::all(), $this->dictRules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $dict = new Dictionary();
        $dict->name = Input::get('name');
        $dict->encoding= Input::get('encoding');
        $dict->windowName = Input::get('windowName');
        $dict->url = Input::get('url');
        $dict->sentence = Input::get('sentence')=="on";
        $dict->autoOpen = Input::get('autoOpen')=="on";
        $dict->language_id = $id;
        
        $dict->save();
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your dictionary has been added.', FlashMessage::SUCCESS));
        
        return Redirect::action('LanguageController@edit', array('id'=>$id));
    }
    
    public function editDictionary($id, $did) {
        $language = $this->languageService->find($id);
        $dictionary = $this->languageService->findDictionary($did);
        
        return View::make('languages.editDictionary')
                ->with('language', $language)
                ->with('dictionary', $dictionary)
                ;
    }
    
    public function postEditDictionary($id, $did) {
        $validator = Validator::make(Input::all(), $this->dictRules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }
        
        $dict = $this->languageService->findDictionary($did);
        $dict->name = Input::get('name');
        $dict->encoding= Input::get('encoding');
        $dict->windowName = Input::get('windowName');
        $dict->url = Input::get('url');
        $dict->sentence = Input::get('sentence')=="on";
        $dict->autoOpen = Input::get('autoOpen')=="on";
        
        $dict->save();
        
        Session::flash(FlashMessage::MSG, new FlashMessage('Your dictionary has been updated.', FlashMessage::SUCCESS));
        
        return Redirect::action('LanguageController@editDictionary', array('id'=>$id, 'did'=>$did));
    }
    
    public function postDeleteDictionary($id, $did) {
        $this->languageService->deleteDictionary($did);
        Session::flash(FlashMessage::MSG, new FlashMessage('Your dictionary has been deleted.', FlashMessage::SUCCESS));
        return Redirect::action('LanguageController@edit', array('id'=>$id ));
    }
}
