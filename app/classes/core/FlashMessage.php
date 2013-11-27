<?php
namespace RT\Core;

class FlashMessage {
	const SUCCESS = 'alert-success';
	const INFO = 'alert-info';
	const WARNING = 'alert-warning';
	const DANGER = 'alert-danger';
	
	const MSG = '__FLASH_MESSAGE__';
	
	private $level;
	private $message;
	
	function __construct($message, $level = FlashMessage::INFO) {
		$this->level = $level;
		$this->message = $message;
	}
	
	function getLevel() {
		return $this->level;
	}
	
	function getMessage() {
		return $this->message;
	}
}
