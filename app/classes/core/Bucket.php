<?php
namespace RT\Core;


class Bucket {
    public static function getUserBucketName() {
        if(!Auth::check()) {
            throw new Exception("User must be logged in");
        }
        
        //return "rt." . Auth::user()->id;
        return storage_path() . '/texts/' . Auth::user()->id;
    }
    
    public static function getBucketName($userId) {
//        return "rt." . $userId;
        
        return storage_path() . '/texts/' . $userId;
    }
}
