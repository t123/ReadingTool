<?php

namespace RT\Services;

use \User;
use RT\Core\Bucket;
use Illuminate\Support\Facades\Auth as Auth;
use Illuminate\Support\Facades\DB as DB;
use Aws\Laravel\AwsFacade as AWS;

interface IUserService {

    public function createUser($username, $password);

    public function deleteUser();
}

class UserService implements IUserService {

    private $user;

    public function __construct() {
        $this->user = Auth::user();
    }

    public function createUser($username, $password) {
        $countQuery = DB::select("select count(id) as counted from users where type='admin'");
        $count = $countQuery[0]->counted;
        $type = $count == 0 ? 'admin' : 'web';

        $user = new \User;
        $user->username = $username;
        $user->password = $password;
        $user->email = '';
        $user->created = date('Y-m-d H:i:s');
        $user->lastlogin = date('Y-m-d H:i:s');
        $user->type = $type;

        $user->save();

        $path = Bucket::getBucketName($user->id);
        
        if (!is_dir($path)) {
            mkdir($path);
        }

//        $s3 = AWS::get('s3');
//        
//        $s3->createBucket(
//                array(
//                        'Bucket'=> Bucket::getBucketName($user->id),
//                        'LocationConstraint' => 'eu-west-1'
//                )
//                );

        return $user;
    }

    public function deleteUser() {
        $groupService = \App::make('RT\Services\IGroupService');
        $languageService = \App::make('RT\Services\ILanguageService');

        $user = User::find(Auth::user()->id);

        if ($user == null) {
            return;
        }

        $groups = $groupService->findAllForUser();

        foreach ($groups as $g) {
            $groupService->delete($g->id);
        }

        DB::delete('delete from group_user where user_id=?', array($user->id));
        DB::delete('delete from tag_term where term_id in ( select id from terms where user_id=? )', array($user->id));
        DB::delete('delete from terms where user_id=?', array($user->id));

        $languages = $languageService->findAll();

        foreach ($languages as $l) {
            $languageService->delete($l->id);
        }

//        $bucketname = Bucket::getBucketName($user->id);
//        $s3 = AWS::get('s3');
//        $s3->clearBucket($bucketname);
//        $s3->deleteBucket(array('Bucket' => $bucketname));

        $path = Bucket::getBucketName($user->id);
        if (is_dir($path)) {
            $this->delTree($path);
        }

        $user->delete();
    }

    private static function delTree($dir) {
        $files = array_diff(scandir($dir), array('.', '..'));
        foreach ($files as $file) {
            (is_dir("$dir/$file")) ? $this->delTree("$dir/$file") : unlink("$dir/$file");
        }
        return rmdir($dir);
    }
}
