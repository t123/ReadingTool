<?php

/*
  |--------------------------------------------------------------------------
  | Application Routes
  |--------------------------------------------------------------------------
  |
  | Here is where you can register all of the routes for an application.
  | It's a breeze. Simply tell Laravel the URIs it should respond to
  | and give it the Closure to execute when that URI is requested.
  |
 */

Route::get('/', 'HomeController@index');
Route::get('/sign-out', 'HomeController@signout');
Route::post('/sign-in', 'HomeController@signin');
Route::post('/sign-up', 'HomeController@signup');

Route::group(array('before' => 'auth'), function() {
    Route::get('/my-account', 'AccountController@index');
    Route::post('/my-account', 'AccountController@postIndex');
    Route::get('/my-account/change-password', 'AccountController@changePassword');
    Route::post('/my-account/change-password', 'AccountController@postChangePassword');
    Route::get('/my-account/delete-account', 'AccountController@deleteAccount');
    Route::post('/my-account/delete-account', 'AccountController@postDeleteAccount');
    Route::post('/my-account/css', 'AccountController@postCss');
    Route::get('/my-account/thank-you', 'AccountController@thankyou');

    Route::get('/languages', 'LanguageController@index');
    Route::get('/languages/add', 'LanguageController@add');
    Route::post('/languages/add', 'LanguageController@postAdd');
    Route::get('/languages/edit/{id}', 'LanguageController@edit')->where('id', '[0-9]+');
    Route::post('/languages/edit/{id}', 'LanguageController@postEdit')->where('id', '[0-9]+');
    Route::post('/languages/delete/{id}', 'LanguageController@postDelete')->where('id', '[0-9]+');

    Route::get('/languages/add-dictionary/{id}', 'LanguageController@addDictionary')->where('id', '[0-9]+');
    Route::post('/languages/add-dictionary/{id}', 'LanguageController@postAddDictionary')->where('id', '[0-9]+');
    Route::get('/languages/edit-dictionary/{id}/{did}', 'LanguageController@editDictionary')->where('id', '[0-9]+')->where('did', '[0-9]+');
    Route::post('/languages/edit-dictionary/{id}/{did}', 'LanguageController@postEditDictionary')->where('id', '[0-9]+')->where('did', '[0-9]+');
    Route::post('/languages/delete-dictionary/{id}/{did}', 'LanguageController@postDeleteDictionary')->where('id', '[0-9]+')->where('did', '[0-9]+');

    Route::get('/texts', 'TextController@index');
    Route::post('/texts', 'TextController@postIndex');
    Route::get('/texts/add', 'TextController@add');
    Route::post('/texts/add', 'TextController@postAdd');
    Route::get('/texts/edit/{id}', 'TextController@edit')->where('id', '[0-9]+');
    Route::post('/texts/edit/{id}', 'TextController@postEdit')->where('id', '[0-9]+');
    Route::post('/texts/delete/{id}', 'TextController@postDelete')->where('id', '[0-9]+');
    Route::get('/texts/read/{id}', 'TextController@read')->where('id', '[0-9]+');
    Route::get('/texts/read-parallel/{id}', 'TextController@readParallel')->where('id', '[0-9]+');
    Route::get('/texts/download-pdf/{id}', 'TextController@downloadPdf')->where('id', '[0-9]+');
    Route::get('/texts/download-text/{id}', 'TextController@downloadText')->where('id', '[0-9]+');
    Route::post('/texts/deleteMany', 'TextController@postDeleteMany');
    Route::post('/texts/share', 'TextController@postShare');
    Route::get('/texts/copy-and-edit/{id}', 'TextController@copyAndEdit')->where('id', '[0-9]+');

    Route::get('/terms', 'TermController@index');
    Route::post('/terms', 'TermController@postIndex');
    Route::get('/terms/edit/{id}', 'TermController@edit')->where('id', '[0-9]+');
    Route::post('/terms/edit/{id}', 'TermController@postEdit')->where('id', '[0-9]+');
    Route::get('/terms/export/{id}', 'TermController@exportTerms')->where('id', '[0-9]+');
    Route::post('/terms/delete/{id}', 'TermController@postDelete')->where('id', '[0-9]+');
    
    Route::get('/groups', 'GroupController@index');
    Route::get('/groups/add', 'GroupController@add');
    Route::post('/groups/add', 'GroupController@postAdd');
    Route::get('/groups/edit/{id}', 'GroupController@edit')->where('id', '[0-9]+');
    Route::post('/groups/edit/{id}', 'GroupController@postEdit')->where('id', '[0-9]+');
    Route::post('/groups/delete/{id}', 'GroupController@postDelete')->where('id', '[0-9]+');
    Route::get('/groups/texts/{id}', 'GroupController@texts')->where('id', '[0-9]+');
    Route::post('/groups/texts/{id}', 'GroupController@postTexts')->where('id', '[0-9]+');
    Route::post('/groups/unshare', 'GroupController@postUnshare');
    Route::post('/groups/usernames', 'GroupController@postUsernames');
    Route::get('/groups/read/{groupId}/{textId}', 'GroupController@read')->where('groupId', '[0-9]+')->where('textId', '[0-9]+');
    Route::get('/groups/read-parallel/{groupId}/{textId}', 'GroupController@readParallel')->where('groupId', '[0-9]+')->where('textId', '[0-9]+');
    Route::get('/groups/download-pdf/{groupId}/{textId}', 'GroupController@downloadPdf')->where('groupId', '[0-9]+')->where('textId', '[0-9]+');
    Route::get('/groups/download-text/{groupId}/{textId}', 'GroupController@downloadText')->where('groupId', '[0-9]+')->where('textId', '[0-9]+');
    Route::get('/public-groups', 'GroupController@findGroups');
    Route::post('/public-groups', 'GroupController@postFindGroups');
    Route::post('/public-groups/join/{id}', 'GroupController@postJoinGroup')->where('id', '[0-9]+');

    Route::get('/groups/membership/{id}', 'GroupController@membership')->where('id', '[0-9]+');
    Route::post('/groups/membership/{id}', 'GroupController@postMembership')->where('id', '[0-9]+');
    Route::post('/groups/accept-membership/{id}', 'GroupController@postAcceptMembership')->where('id', '[0-9]+');
    Route::post('/groups/decline-membership/{id}', 'GroupController@postDeclineMembership')->where('id', '[0-9]+');
    Route::post('/groups/leave/{id}', 'GroupController@postLeaveGroup')->where('id', '[0-9]+');

    Route::controller('ajax', 'AjaxController');
});


Route::group(array('before' => 'auth.basic'), function() {
    Route::controller('api', 'ApiController');
});
