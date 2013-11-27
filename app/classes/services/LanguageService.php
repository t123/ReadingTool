<?php

namespace RT\Services;

use App\Models\Language;
use App\Models\SLanguage;
use App\Models\Dictionary;
use \User;
use Illuminate\Support\Facades\Auth as Auth;
use Illuminate\Support\Facades\DB as DB;

interface ILanguageService {
    public function find($id);
    public function findOneByCode($code);
    public function findAll();
    public function delete($id);
    public function findDictionary($id);
    public function deleteDictionary($id);
    public function findAllSystemLanguages();
    public function findOneSystemLanguage($code);
}

class LanguageService implements ILanguageService {
    private $user;
    
    public function __construct() {
        $this->user = Auth::user();
    }

    public function findAllSystemLanguages() {
        return SLanguage::orderBy('language');
    }
    
    public function findOneSystemLanguage($code) {
        return SLanguage::where('code', '=', $code)
                ->first();
    }
    
    public function find($id) {
        return Language::where('id', '=', $id)
                ->where('user_id', '=', $this->user->id)
                ->with('dictionaries')
                ->first();
    }
    
    public function findOneByCode($code) {
        return Language::where('code', '=', $code)
                ->where('user_id', '=', $this->user->id)
                ->with('dictionaries')
                ->first();
    }
    
    public function findAll() {
        return Language::where('user_id', '=', $this->user->id)
                ->orderBy('name', 'ASC')
                ->get();
    }
    
    public function delete($id) {
        $l = $this->find($id);
        
        if($l!=null) {
            DB::delete('delete from group_text where text_id in (select id from texts where l1_id=?)', array($l->id));
            DB::delete('delete from texts where l1_id=?', array($l->id));
            DB::delete('delete from dictionaries where language_id=?', array($l->id));
            $l->delete();
        }
    }
    
    public function findDictionary($id) {
        return Dictionary::find($id);
    }
    
    public function deleteDictionary($id) {
        $d = $this->findDictionary($id);
        $l = $this->find($d->language->id);

        if($d!=null) {
            $d->delete();
        }
    }
}
