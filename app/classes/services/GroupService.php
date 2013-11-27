<?php

namespace RT\Services;

use App\Models\Group;
use \User;
use Illuminate\Support\Facades\DB as DB;
use Illuminate\Support\Facades\Auth as Auth;

interface IGroupService {
    public function findAllForUser();
    public function findOneByOwner($id);
    public function findOneByOwnerOrModerator($id);
    public function findOneByOwnerModeratorOrMember($id);
    public function delete($id);
    public function isOwner($id);
    public function isOwnerOrModerator($id);
    public function isOwnerModeratorOrMember($id);
    public function findMembers($id);
    public function updateMembership($userId, $groupId, $type);
    public function inviteUser($groupId, $username);
    public function shareText($groupId, $textId, $checkOwner);
    public function unshareText($groupId, $textId, $checkOwner);
    public function findAllTextsByFilter($groupId, $filter, $perPage, $currentPage);
    public function findAllGroupsByFilter($filter, $perPage, $currentPage);
}

class GroupService implements IGroupService {
    private $user;
    private $uid;
    
    public function __construct() {
        $this->user = Auth::user();
        $this->uid = $this->user->id;
    }

    public function shareText($groupId, $textId, $checkOwner = false) {
        if($checkOwner) {
            $group = $this->findOneByOwnerModeratorOrMember($groupId);
            
            if($group==null) {
                return;
            }
        }
        
        $result = DB::select('select count(*) as counted from group_text where group_id=? and text_id=?', array($groupId, $textId));
        if($result[0]->counted==0) {
            DB::insert('insert into group_text select id, ? from texts where user_id=? and id=?', array($groupId, $this->uid, $textId));
        }
    }
    
    public function unshareText($groupId, $textId, $checkOwner = false) {
        if($checkOwner) {
            $group = $this->findOneByOwnerModeratorOrMember($groupId);
            
            if($group==null) {
                return;
            }
        }
        
        DB::delete('delete from group_text where group_id=? and text_id=?', array($groupId, $textId));
    }
    
    public function findAllTextsByFilter($groupId, $filter, $perPage, $currentPage) {
        if(empty($currentPage) || !is_integer($currentPage)) {
            $currentPage = 1;
        }
        
        if(empty($perPage) || !is_integer($perPage)) {
            $perPage = 10;
        }
        
        $filter = mysql_real_escape_string($filter);
        
        $query = "
            select 
                a.*, c.name as language_name, b.displayName as displayName, b.username as username, exists(select * from group_text where text_id=a.id and group_id=$groupId) as isShared
            from texts a, users b, languages c
            where 
                a.user_id=b.id and 
                a.l1_id=c.id 
            ";
        
        $xtra = "";
        foreach(explode(' ', $filter) as $piece ) {
            $piece = trim(mb_strtolower($piece));
            
            if(empty($piece)) {
                continue;
            }
            
            if(substr($piece, 0, 1)=='#') {
                $tag = substr($piece, 1);
                if($tag=='parallel') {
                    $xtra .= " and a.l2_id is not null ";
                } else if($tag=='audio') {
                    $xtra .= ' and length(a.audioUrl)>0 ';
                }
            } else {
                $xtra .= " and (a.title like '%$piece%' or a.collectionName like '%$piece%' or c.name='$piece') ";
            }
        }
        
        $query .= $xtra . ' having isShared=1 ';
        $startPage = $perPage*($currentPage-1);
        
        $texts = DB::select(
                $query . " 
                order by c.name ASC, a.collectionName ASC, a.collectionNo ASC, a.title ASC
                limit $startPage, $perPage"
            );
        
        $countQuery = DB::select("select count(*) as counted from ($query) xxx");
        $count = $countQuery[0]->counted;
        
        return array('count'=>$count, 'texts'=>$texts);
    }

    public function findAllGroupsByFilter($filter, $perPage, $currentPage) {
        if(empty($currentPage) || !is_integer($currentPage)) {
            $currentPage = 1;
        }
        
        if(empty($perPage) || !is_integer($perPage)) {
            $perPage = 10;
        }
        
        $filter = mysql_real_escape_string($filter);
        
        $query = "
            select 
                a.*, exists(select * from group_user where group_id=a.id and user_id=$this->uid) as isMember
            from groups a
            where 
                a.type='public' 
            ";
        
        $xtra = "";
        foreach(explode(' ', $filter) as $piece ) {
            $piece = trim(mb_strtolower($piece));
            $xtra .= " and (a.name like '%$piece%') ";
        }
        
        $query .= $xtra . ' having isMember=0 ';
        $startPage = $perPage*($currentPage-1);
        
        $groups = DB::select(
                $query . " 
                order by a.name ASC
                limit $startPage, $perPage"
            );
        
        $countQuery = DB::select("select count(*) as counted from ($query) xxx");
        $count = $countQuery[0]->counted;
        
        return array('count'=>$count, 'groups'=>$groups);
    }
    
    public function findAllForUser() {
        $groups = DB::table('groups')
                ->join('group_user', 'groups.id', '=', 'group_user.group_id')
                ->where('group_user.user_id', '=', $this->user->id)
                ->whereIn('group_user.membership', array('member','moderator','owner', 'pending', 'invited'))
                ->get();
        
        return $groups;
    }
    
    public function updateMembership($userId, $groupId, $type) {
        if($type=='delete') {
            DB::delete("delete from group_user where user_id=? and group_id=?", array($userId, $groupId));
            DB::delete("delete from group_text where group_id=? and text_id in (select id from texts where user_id=?)", array( $groupId, $userId));
        } else {
            DB::update("update group_user set membership=? where user_id=? and group_id=?", array($type, $userId, $groupId));
            
            if($type=='banned' || $type=='invited' || $type=='pending') {
                DB::delete("delete from group_text where group_id=? and text_id in (select id from texts where user_id=?)", array( $groupId, $userId));
            }
            
            if($type=='invited' || $type=='pending') {
                $count = DB::select("select count(*) as counted from group_user where group_id=? and user_id=?", array($groupId, $userId));
        
                if($count[0]->counted==0) {
                    DB::insert('insert into group_user (group_id,user_id,membership) values (?,?,?)', array($groupId, $userId, $type));
                }
            }
        }
    }
    
    public function findMembers($id) {
        $members = DB::select("select * from groups a, group_user b, users c where a.id=$id and a.id=b.group_id and b.user_id=c.id order by b.membership");
        return $members;
    }
    
    public function isOwner($id) {
        $groups = DB::select("select a.id from groups a, group_user b where a.id=$id and a.id=b.group_id and b.user_id=$this->uid and b.membership='owner' limit 1");
        return sizeof($groups)==1;
    }
    
    public function isOwnerOrModerator($id) {
        $groups = DB::select("select a.id from groups a, group_user b where a.id=$id and a.id=b.group_id and b.user_id=$this->uid and (b.membership='owner' or b.membership='moderator') limit 1");
        return sizeof($groups)==1;
    }
    
    public function isOwnerModeratorOrMember($id) {
        $groups = DB::select("select a.id from groups a, group_user b where a.id=$id and a.id=b.group_id and b.user_id=$this->uid and (b.membership='owner' or b.membership='moderator' or b.membership='member') limit 1");
        return sizeof($groups)==1;
    }
    
    public function findOneByOwner($id) {
        $group = Group::find($id);
        
        if($this->isOwner($id)) {
            return $group;
        }
        
        return null;
    }
    
    public function findOneByOwnerOrModerator($id) {
        $group = Group::find($id);
        
        if($this->isOwnerOrModerator($id)) {
            return $group;
        }
        
        return null;
    }
    
    public function findOneByOwnerModeratorOrMember($id) {
        $group = Group::find($id);
        
        if($this->isOwnerModeratorOrMember($id)) {
            return $group;
        }
        
        return null;
    }
    
    public function delete($id) {
        $g = $this->findOneByOwner($id);
        
        if($g!=null) {
            DB::delete('delete from group_user where group_id=?', array($id));
            DB::delete('delete from group_text where group_id=?', array($id));
            $g->delete();
        }
    }
    
    public function inviteUser($groupId, $username) {
        $user = User::where('username','=',$username)->first();

        if($user==null) {
            return;
        }
        
        $count = DB::select("select * from group_user where group_id=? and user_id=?", array($groupId, $user->id));
        
        if(sizeof($count)!=0) {
            return;
        }
        
        DB::insert('insert into group_user (group_id,user_id,membership) values (?,?,?)', array($groupId, $user->id, 'invited'));
    }
}
