<?php

use RT\Core\FlashMessage;
use RT\Services\IUserService;
use RT\Core\Bucket;

class AccountController extends BaseController {

    private $userService;

    public function __construct(IUserService $userService) {
        $this->userService = $userService;
        View::share('currentController', 'Account');
    }

    public function index() {
        return View::make('account.index')
                        ->with('user', Auth::user())
                        ->with('css', $this->userService->getCss())
        ;
    }

    public function postIndex() {
        $rules = array(
            'displayName' => 'max:20|alpha_dash',
            'email' => 'max:50'
        );

        $validator = Validator::make(Input::only('displayName', 'email'), $rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator)->withInput();
        }

        $user = User::find(Auth::user()->id);
        $user->displayName = Input::get('displayName');
        $user->email = Input::get('email');
        $user->save();

        Session::flash(FlashMessage::MSG, new FlashMessage('Your account has been updated.', FlashMessage::SUCCESS));

        return Redirect::action('AccountController@index');
    }

    public function postCss() {
        $css = trim(Input::get('css'));
        $this->userService->saveCss($css);

        Session::flash(FlashMessage::MSG, new FlashMessage('Your CSS has been updated.', FlashMessage::SUCCESS));

        return Redirect::action('AccountController@index');
    }

    public function changePassword() {
        return View::make('account.changepassword');
    }

    public function postChangePassword() {
        $rules = array(
            'currentPassword' => 'required',
            'newPassword' => 'required'
        );

        $validator = Validator::make(Input::only('currentPassword', 'newPassword'), $rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator);
        }

        $user = User::find(Auth::user()->id);

        if (Hash::check(Input::get('currentPassword'), $user->password)) {
            $user->password = Hash::make(Input::get('newPassword'));
            $user->save();

            Session::flash(FlashMessage::MSG, new FlashMessage('Your password was updated', FlashMessage::SUCCESS));

            return Redirect::action('AccountController@changePassword');
        }

        Session::flash(FlashMessage::MSG, new FlashMessage('Your current password was not correct', FlashMessage::DANGER));

        return Redirect::back()->withErrors($validator);
    }

    public function deleteAccount() {
        return View::make('account.deleteaccount');
    }

    public function postDeleteAccount() {
        $rules = array('currentPassword' => 'required');

        $validator = Validator::make(Input::only('currentPassword'), $rules);

        if ($validator->fails()) {
            return Redirect::back()->withErrors($validator);
        }

        $user = User::find(Auth::user()->id);

        if (Hash::check(Input::get('currentPassword'), $user->password)) {
            $this->userService->deleteUser();
            Auth::logout();

            Session::flash(FlashMessage::MSG, new FlashMessage('Your account has been deleted.', FlashMessage::SUCCESS));

            return Redirect::action('HomeController@index');
        }

        Session::flash(FlashMessage::MSG, new FlashMessage('Your current password was not correct', FlashMessage::DANGER));

        return Redirect::back()->withErrors($validator);
    }

    public function thankyou() {
        return View::make('account.thankyou');
    }

}
