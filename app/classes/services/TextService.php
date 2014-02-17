<?php

namespace RT\Services;

use App\Models\Text;
use \User;
use Illuminate\Support\Facades\Auth as Auth;
use Illuminate\Support\Facades\DB as DB;
use Aws\Laravel\AwsFacade as AWS;
use RT\Core\Bucket;

interface ITextService {

    public function find($id);

    public function findForGroupUser($id, $groupId);

    public function findAll();

    public function findAllByFilter($filter, $perPage, $currentPage, $sort, $sortDir);

    public function delete($id);

    public function nextText($text);

    public function previousText($text);

    public function save($text, $l1, $l2);
    
    public function findCollectionsForUser();
}

class TextService implements ITextService {

    private $user;

    public function __construct() {
        $this->user = Auth::user();
    }

    public function find($id) {
        $text = Text::where('id', '=', $id)
                ->where('user_id', '=', $this->user->id)
                ->first();

        return $this->loadTexts($text);
    }

    private function loadTexts($text) {
        if ($text == null) {
            return;
        }

        if ($text != null) {
//            $s3 = AWS::get('s3');
//
//            $bucketname = Bucket::getBucketName($text->user_id);
//            $l1 = $s3->getObject(array('Bucket' => $bucketname, 'Key' => $id . '.l1'));
//            $text->l1Text = $l1['Body'];
//
//            if ($text->l2_id != null) {
//                $l2 = $s3->getObject(array('Bucket' => $bucketname, 'Key' => $id . '.l2'));
//                $text->l2Text = $l2['Body'];
//            }
            $path = Bucket::getBucketName($text->user_id);
            $text->l1Text = $this->file_get_contents_utf8($path . '/' . $text->id . '.l1');

            if ($text->l2_id != null) {
                $text->l2Text = $this->file_get_contents_utf8($path . '/' . $text->id . '.l2');
            }
        }

        return $text;
    }

    public function findCollectionsForUser() {
        $query = "
            select 
                distinct(concat(b.name, ' - ', a.collectionName)) as cName
            from 
                texts a, languages b 
            where 
                a.l1_id=b.id and a.user_id=? and b.archived=0 
            order by cName";
        
        $collections = DB::select($query, array($this->user->id));
        return $collections;
    }
    
    public function findForGroupUser($id, $groupId) {
        $text = Text::where('texts.id', '=', $id)
                        ->join('group_text', 'texts.id', '=', 'group_text.text_id')
                        ->where('group_text.group_id', '=', $groupId)
                        ->first();
        
        return $this->loadTexts($text);
    }

    public function findAll() {
        return Text::where('user_id', '=', $this->user->id)
                        ->orderBy('collectionName', 'ASC')
                        ->orderBy('collectionNo', 'ASC')
                        ->orderBy('title', 'ASC')
                        ->with('language1')
                        ->with('language2')
                        ->get()
        ;
    }

    public function findAllByFilter($filter, $perPage, $currentPage, $sort, $sortDir = 0) {
        $filter = mysql_real_escape_string($filter);
        if (empty($currentPage) || !is_numeric($currentPage)) {
            $currentPage = 1;
        }

        if (empty($perPage) || !is_numeric($perPage)) {
            $perPage = 10;
        }

        $query = "
            select 
                a.*, c.name as language_name, exists(select * from group_text where text_id=a.id) as isShared
            from texts a, users b, languages c
            where 
                a.user_id=b.id and 
                b.id=" . $this->user->id . " and 
                a.l1_id=c.id 
            ";
        $xtra = "";
        $having = "";
        $pieces = 0;
        
        foreach (explode(' ', $filter) as $piece) {
            $piece = trim(mb_strtolower($piece));

            if (empty($piece)) {
                continue;
            }

            if (substr($piece, 0, 1) == '#') {
                $tag = substr($piece, 1);
                if ($tag == 'parallel') {
                    $xtra .= " and a.l2_id is not null ";
                } else if ($tag == 'audio') {
                    $xtra .= ' and length(a.audioUrl)>0 ';
                } else if ($tag == 'shared') {
                    $having = ' having isShared=1 ';
                } else if($tag == 'unread') {
                    $xtra .= ' and a.lastread is null ';
                } else if($tag == 'read') {
                    $xtra .= ' and a.lastread is not null ';
                }
            } else {
                $xtra .= " and (a.title like '%$piece%' or a.collectionName like '%$piece%' or c.name='$piece') ";
                $pieces++;
            }
        }

        if($pieces==0) {
            $xtra .= " and c.archived=0 ";
        }
        
        $query .= $xtra . $having;
        $startPage = $perPage * ($currentPage - 1);
        $sortDirQ = empty($sortDir) || $sortDir == 0 ? " ASC" : " DESC";

        if (empty($sort)) {
            $orderBy = " order by c.name $sortDirQ, a.collectionName ASC, a.collectionNo ASC, a.title ASC ";
        } else {
            switch ($sort) {
                case "language":
                    $orderBy = " order by c.name $sortDirQ, a.collectionName ASC, a.collectionNo ASC, a.title ASC ";
                    break;

                case "title":
                    $orderBy = " order by a.title $sortDirQ, c.name ASC, a.collectionName ASC, a.collectionNo ASC ";
                    break;

                case "collectioname":
                    $orderBy = " order by a.collectionName $sortDirQ, a.collectionNo ASC, c.name ASC,a. ";
                    break;

                case "lastread":
                    $orderBy = " order by a.lastread $sortDirQ, a.collectionName ASC, a.collectionNo ASC, c.name ASC, a.title ";
                    break;

                default:
                    $orderBy = " order by c.name $sortDirQ, a.collectionName ASC, a.collectionNo ASC, a.title ASC ";
                    break;
            }
        }
        $texts = DB::select($query . $orderBy . " limit $startPage, $perPage");

        $countQuery = DB::select("select count(*) as counted from ($query) xxx");
        $count = $countQuery[0]->counted;

        return array('count' => $count, 'texts' => $texts);
    }

    public function delete($id) {
        $l = $this->find($id);

        if ($l != null) {
            DB::delete('delete from group_text where text_id=?', array($id));
            $l->delete();
        }
    }

    public function nextText($text) {
        $query = Text::where('user_id', '=', $text->user_id)
                ->where('l1_id', '=', $text->l1_id)
                ->where('collectionName', '=', $text->collectionName);

        if ($text->collectionNo != null) {
            $query = $query->where('collectionNo', '>', $text->collectionNo);
        }

        return $query->orderBy('collectionNo', 'ASC')->first();
    }

    public function previousText($text) {
        $query = Text::where('user_id', '=', $text->user_id)
                ->where('l1_id', '=', $text->l1_id)
                ->where('collectionName', '=', $text->collectionName);

        if ($text->collectionNo != null) {
            $query = $query->where('collectionNo', '<', $text->collectionNo);
        }

        return $query->orderBy('collectionNo', 'DESC')->first();
    }

    public function save($text, $l1, $l2) {
        unset($text->l1Text);
        unset($text->l2Text);
        $text->save();

        $path = Bucket::getBucketName($text->user_id);

        if(!is_dir($path)) {
            mkdir($path);
        }
        
        file_put_contents($path . '/' . $text->id . '.l1', $l1);

        if ($text->l2_id != null) {
            file_put_contents($path . '/' . $text->id . '.l2', $l2);
        }
        
//        $s3 = AWS::get('s3');
//
//        $s3->putObject(
//                array(
//                    'Bucket' => Bucket::getBucketName($text->user_id),
//                    'Key' => $text->id . ".l1",
//                    'Body' => $l1
//                )
//        );
//
//        if ($text->l2_id != null) {
//            $s3->putObject(
//                    array(
//                        'Bucket' => Bucket::getBucketName($text->user_id),
//                        'Key' => $text->id . ".l2",
//                        'Body' => $l2
//                    )
//            );
//        }
    }

    private function file_get_contents_utf8($fn) {
        if (file_exists($fn)) {
            $content = file_get_contents($fn);
        } else {
            $content = "";
        }

        return mb_convert_encoding($content, 'UTF-8', mb_detect_encoding($content, 'UTF-8, ISO-8859-1', true));
    }

}
