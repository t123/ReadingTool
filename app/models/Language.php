<?php

namespace App\Models;

class Language extends \Eloquent {
    public $timestamps = false;
    protected $table = 'languages';
    
    public function user() {
        return $this->belongsTo('User');
    }
    
    public function getSettings() {
        if($this->settings==null) {
            return null;
        }
        
        return json_decode($this->settings);
    }
    
    public function dictionaries() {
        return $this->hasMany('App\\Models\\Dictionary');
    }
}