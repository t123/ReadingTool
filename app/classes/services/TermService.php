<?php

namespace RT\Services;

use App\Models\Term;
use \User;
use Illuminate\Support\Facades\Auth as Auth;
use Illuminate\Support\Facades\DB as DB;

interface ITermService {

    public function find($id);

    public function findAll($state = null);

    public function delete($id);

    public function findAllForLanguage($languageId, $state=null);

    public function findByPhraseAndLanguage($phrase, $languageId);

    public function findAllByFilter($filter, $perPage, $currentPage, $sort, $sortDir);
}

class TermService implements ITermService {

    private $user;

    public function __construct() {
        $this->user = Auth::user();
    }

    public function find($id) {
        return Term::where('id', '=', $id)
                        ->where('user_id', '=', $this->user->id)
                        ->first();
    }

    public function findAll($state = null) {
        if ($state == null) {
            return Term::where('user_id', '=', $this->user->id)
                            ->orderBy('phraseLower', 'ASC')
                            ->get();
        } else {
            return Term::where('user_id', '=', $this->user->id)
                            ->where('state', '=', $state)
                            ->orderBy('phraseLower', 'ASC')
                            ->get();
        }
    }

    public function findAllForLanguage($languageId, $state=null) {
        if ($state == null) {
            return Term::where('user_id', '=', $this->user->id)
                        ->where('language_id', '=', $languageId)
                        ->orderBy('phraseLower', 'ASC')
                        ->get();
        } else {
            return Term::where('user_id', '=', $this->user->id)
                        ->where('language_id', '=', $languageId)
                        ->where('state', '=', $state)
                        ->orderBy('phraseLower', 'ASC')
                        ->get();
        }
    }

    public function delete($id) {
        $l = $this->find($id);

        if ($l != null) {
            $l->delete();
        }
    }

    public function findByPhraseAndLanguage($phrase, $languageId) {
        return Term::where('language_id', '=', $languageId)
                        ->where('user_id', '=', $this->user->id)
                        ->where('phraseLower', '=', mb_strtolower($phrase))
                        ->first();
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
                a.*, c.name as language_name, c.code as language_code, texts.collectionName, texts.title, texts.collectionNo
            from (terms a, languages c)
            left join texts on a.text_id=texts.id
            where 
                a.user_id=" . $this->user->id . " and 
                a.language_id=c.id 
            ";

        $xtra = "";
        $having = "";

        foreach (explode(' ', $filter) as $piece) {
            $piece = trim(mb_strtolower($piece));

            if (empty($piece)) {
                continue;
            }

            if (substr($piece, 0, 1) == '#') {
                $tag = substr($piece, 1);
                if ($tag == 'known') {
                    $xtra .= " and a.state='known' ";
                } else if ($tag == 'notknown' || $tag == 'unknown') {
                    $xtra .= " and a.state='unknown' ";
                } else if ($tag == 'notseen') {
                    $xtra .= " and a.state='notseen' ";
                } else if ($tag == 'ignored') {
                    $xtra .= " and a.state='ignored' ";
                } else {
                    $xtra = " and a.id in ( select z.term_id from tag_term z where z.tag_id in ( select y.id from tags y where y.tag='$tag' ) ) ";
                }
            } else {
                $xtra .= " and (a.phraseLower like '%$piece%') ";
            }
        }

        $query .= $xtra . $having;
        $startPage = $perPage * ($currentPage - 1);
        $sortDirQ = empty($sortDir) || $sortDir == 0 ? " ASC" : " DESC";

        if (!empty($sort)) {
            switch ($sort) {
                case "language":
                    $orderBy = " order by c.name $sortDirQ, a.phraseLower ASC ";
                    break;

                case "state":
                    $orderBy = " order by a.state $sortDirQ, c.name ASC, a.phraseLower ASC ";
                    break;

                case "phrase":
                    $orderBy = " order by a.phraseLower $sortDirQ, c.name ASC ";
                    break;
                
                case "basephrase":
                    $orderBy = " order by a.basePhrase $sortDirQ, c.name ASC ";
                    break;

                case "added":
                    $orderBy = " order by a.created_at $sortDirQ, c.name ASC, a.phraseLower ASC ";
                    break;

                case "changed":
                    $orderBy = " order by a.updated_at $sortDirQ, c.name ASC, a.phraseLower ASC ";
                    break;

                default:
                    $orderBy = " order by c.name $sortDirQ, a.phraseLower ASC ";
                    break;
            }
        } else {
            $orderBy = " order by c.name ASC, a.phraseLower ASC ";
        }

        $terms = DB::select($query . $orderBy . " limit $startPage, $perPage");

        $countQuery = DB::select("select count(*) as counted from ($query) xxx");
        $count = $countQuery[0]->counted;

        return array('count' => $count, 'terms' => $terms);
    }

}
