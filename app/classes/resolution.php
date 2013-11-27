<?php

namespace RT\Core;

use Illuminate\Support\ServiceProvider;

use RT\Services\IUserService;
use RT\Services\UserService;
use RT\Services\ILanguageService;
use RT\Services\LanguageService;
use RT\Services\ITextService;
use RT\Services\TextService;
use RT\Services\ITermService;
use RT\Services\TermService;
use RT\Services\IParserService;
use RT\Services\ParserService;
use RT\Services\IGroupService;
use RT\Services\GroupService;

class RTServiceProvider extends ServiceProvider {
	public function register() {
            $this->app->bind('RT\Services\IUserService', function() {
                return new UserService();
            });
            
            $this->app->bind('RT\Services\ILanguageService', function() {
                return new LanguageService();
            });
            
            $this->app->bind('RT\Services\ITextService', function() {
                return new TextService();
            });
            
            $this->app->bind('RT\Services\ITermService', function() {
                return new TermService();
            });
            
            $this->app->bind('RT\Services\IParserService', function() {
                return new ParserService();
            });
            
            $this->app->bind('RT\Services\IGroupService', function() {
                return new GroupService();
            });
	}
}
