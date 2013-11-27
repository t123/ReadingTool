<?php

namespace App\Models;

use App\Models\Term;

class Tag extends \Eloquent {
    public $timestamps = false;
    protected $table = 'tags';
    
//    public function Terms() {
//        return $this->belongsToMany('App\\Model\\Term');
//    }
}