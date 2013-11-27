<?php
namespace App\Models;

use App\Models\Language;
use App\Models\Text;
use App\Models\Tag;

class Term extends \Eloquent {
    protected $table = 'terms';
    
    public function __construct() {
    }
    
    public function language() {
        return $this->belongsTo('App\\Models\\Language', 'language_id');
    }
    
    public function user() {
        return $this->belongsTo('\User', 'user_id');
    }
    
    public function text() {
        return $this->belongsTo('App\\Models\\Text', 'text_id');
    }
    
    public function fullDefinition() {
        $definition = '';
        
        $bp = trim($this->basePhrase);
        $def = trim($this->definition);
        
        if(!empty($bp)) {
            $definition .= $bp . "\n";
        }
        if(!empty($def)) {
            $definition .= $def . "\n";
        }
        
        return $definition;
    }
    
    public function Tags() {
        return $this->belongsToMany('App\\Models\\Tag');
    }
}