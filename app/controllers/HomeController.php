<?php

use RT\Core\FlashMessage;
use RT\Services\IUserService;

class HomeController extends BaseController {
    private $userService;

    public function __construct(IUserService $userService) {
        $this->userService = $userService;
    }

    public function index() {
        if(Auth::check()) {
            return Redirect::action('AccountController@index');
        }

        return View::make('home.index');
    }

    public function signup() {
        $rules = array(
            'signup_username' => 'required|min:3|max:18|alpha_dash|unique:users,username',
            'signup_password' => 'required'
        );

        $validator = Validator::make(Input::only('signup_username', 'signup_password'), $rules);

        if ($validator->fails()) {
            return Redirect::to('/')->withErrors($validator)->withInput();
        }
        
        $user = $this->userService->createUser(Input::get('signup_username'), Hash::make(Input::get('signup_password')));
        
        if($user!=null) {
            Auth::login($user);
        }

        return Redirect::to('my-account/thank-you');
    }

    public function signin() {
        $rules = array(
            'signin_username' => 'required',
            'signin_password' => 'required'
        );

        $validator = Validator::make(Input::only('signin_username', 'signin_password'), $rules);

        if ($validator->fails()) {
            return Redirect::to('/')->withErrors($validator)->withInput();
        }

        if (!Auth::attempt(
                        array(
                            'username' => Input::get('signin_username'),
                            'password' => Input::get('signin_password')
                        )
                )
        ) {
            Session::flash(FlashMessage::MSG, new FlashMessage('Either your username or password is incorrect.', FlashMessage::DANGER));
            return Redirect::to('/')->withErrors($validator)->withInput();
        }

        return Redirect::to('my-account');
    }

    public function signout() {
        Auth::logout();
        return Redirect::to('/');
    }

    function file_get_contents_utf8($fn) {
     $content = file_get_contents($fn);
      return mb_convert_encoding($content, 'UTF-8',
          mb_detect_encoding($content, 'UTF-8, ISO-8859-1', true));
}

function toDate($string) {
    // Parse the date into timestamp string and timezone string
$date =  $string;
preg_match('/(\d{10})(\d{3})([\+\-]\d{4})/', $date, $matches);

// Get the timestamp as the TS tring / 1000
$ts = (int) $matches[1];

// Get the timezone name by offset
$tz = (int) $matches[3];
$tz = timezone_name_from_abbr("", $tz / 100 * 3600, false);
$tz = new DateTimeZone($tz);

// Create a new DateTime, set the timestamp and the timezone
$dt = new DateTime();
$dt->setTimestamp($ts);
$dt->setTimezone($tz);

// Echo the formatted value 
return $dt->format('Y-m-d H:i:s');
}

public function guid($str) {
    $str = substr_replace($str, "-", 8, 0);
    $str = substr_replace($str, "-", 13, 0);
    $str = substr_replace($str, "-", 18, 0);
    $str = substr_replace($str, "-", 23, 0);
    
    return $str;
}

    public function temp() {
        $ts = App::make('RT\Services\ITextService');
        
        $contents = $this->file_get_contents_utf8('/home/travis/Downloads/lessonsforimport.json');
        $json = json_decode($contents);
        $error = json_last_error();

        dd($error);
        
        $languageService = new \RT\Services\LanguageService;
        $textService = new \RT\Services\TextService;
        $uid = Auth::user()->id;
        
        $lookup = array();
        
        $languages = $languageService->findAll();
        
        foreach($languages as $l) {
            $lookup[$l->name] = $l->id;
        }
        
        foreach($json->Items as $t) {
            $text = new \App\Models\Text();
            $text->title = $t->Title;
            $text->collectionName = "Polish in 4 Weeks";
            $text->collectionNo = isset($t->CollectionNo) ? $t->CollectionNo : null;
            $text->l1_id = $lookup[$t->Language];
            $text->l2_id = $lookup['English'];
            $text->audioUrl = '';
            $text->user_id = $uid;
            $text->shareAudioUrl = false;
            //$text->created_at = $this->toDate($t->Created);
            //$text->updated_at = $this->toDate($t->Modified);
            //$text->l1Text = file_exists($l1) ? $this->file_get_contents_utf8($l1) : "";
            //$text->l2Text = file_exists($l2) ? $this->file_get_contents_utf8($l2) : "";
            
            $ts->save(
                    $text, 
                    $t->L1Text,
                    $t->L2Text
                );
        }
        
//        foreach($json->Languages as $l) {
//            $language = new \App\Models\Language();
//            $language->name = $l->Name;
//            $language->code = $l->Code;
//            $language->user_id = $uid;
//            $language->settings = json_encode(
//                array(
//                    'ModalBehaviour' => 1,
//                    'Modal' => null,
//                    'RegexWordCharacters' => "a-zA-ZÀ-ÖØ-öø-ȳ\-\'",
//                    'RegexSplitSentences' => ".!?:;",
//                    'Direction' => "ltr",
//                    'ShowSpaces' => "on",
//                    'AutoPause' => "on",
//                )
//                );
//            
//            $lookup[$language->name] = $language;
//            $language->save();
//        }
//        
//        $counter = 0;
//        foreach($json->Texts as $t) {
//            $l1 = "/home/travis/Downloads/account/texts/" . $this->guid($t->TextId) . ".l1";
//            $l2 = "/home/travis/Downloads/account/texts/" . $this->guid($t->TextId) . ".l2";
//            
//            $text = new \App\Models\Text();
//            $text->title = $t->Title;
//            $text->collectionName = $t->CollectionName;
//            $text->collectionNo = isset($t->CollectionNo) ? $t->CollectionNo : null;
//            $text->l1_id = $lookup[$t->Language1]->id;
//            $text->l2_id = empty($t->Language2) ? null : $lookup[$t->Language2]->id;
//            $text->audioUrl = '';
//            $text->user_id = $uid;
//            $text->shareAudioUrl = false;
//            $text->created_at = $this->toDate($t->Created);
//            $text->updated_at = $this->toDate($t->Modified);
//            //$text->l1Text = file_exists($l1) ? $this->file_get_contents_utf8($l1) : "";
//            //$text->l2Text = file_exists($l2) ? $this->file_get_contents_utf8($l2) : "";
//            
//            $ts->save(
//                    $text, 
//                    file_exists($l1) ? $this->file_get_contents_utf8($l1) : "",
//                    file_exists($l2) ? $this->file_get_contents_utf8($l2) : ""
//                );
//            
//            $counter++;
//            if($counter>20) {
//                break;
//            }
//        }
        
        exit;
        
        return View::make('home.index');
    }
}
