<?php
namespace App\Models;

class Group extends \Eloquent {
    protected $table = 'groups';
    
    public function __construct() {
    }
    
    public function members() {
        return $this->belongsToMany('User')->withPivot('membership');
    }
}