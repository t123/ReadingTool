<?php
namespace RT\Core;

class Splitter {
    private $regex;

    public function __construct($regex) {
        $this->regex = $regex;
    }
    
    public function split($text) {
        $splitted = preg_split($this->regex, $text, -1, PREG_SPLIT_DELIM_CAPTURE);
        return $splitted;
    }
}