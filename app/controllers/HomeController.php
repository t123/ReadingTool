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
                        ), true
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
}
