<?php
namespace App\Models;

use App\Models\Language;

class Text extends \Eloquent {
    protected $table = 'texts';
    
    public function __construct() {
        return $this->belongsTo('User');
    }
    
    public function language1() {
        return $this->belongsTo('App\\Models\\Language', 'l1_id');
    }
    
    public function language2() {
        return $this->belongsTo('App\\Models\\Language', 'l2_id');
    }
}