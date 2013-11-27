<?php

namespace App\Models;

class Dictionary extends \Eloquent {
    public $timestamps = false;
    protected $table = 'dictionaries';
    
    public function language() {
        return $this->belongsTo('\App\Models\Language');
    }
}