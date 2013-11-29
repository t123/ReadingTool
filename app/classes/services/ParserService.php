<?php

namespace RT\Services;

use App\Models\Language;
use App\Models\Text;
use App\Models\Term;
use \User;
use RT\Core\Splitter;

interface IParserService {

    public function Parse($asParallel, $l1Language, $l2Language, $terms, $text);
}

class ParserService implements IParserService {

    protected $asParallel;
    protected $l1Language;
    protected $l2Language;
    protected $terms;
    protected $text;
    protected $l1Splitter;
    protected $l2Splitter;
    protected $termTestRegex;

    public function Parse($asParallel, $l1Language, $l2Language, $terms, $text) {
        $this->asParallel = $asParallel;
        $this->l1Language = $l1Language;
        $this->l2Language = $l2Language;
        $this->terms = $terms;
        $this->text = $text;

        $this->l1Splitter = new Splitter("/([^" . $this->l1Language->getSettings()->RegexWordCharacters . "]+)/u");
        $this->l2Splitter = $this->asParallel ? new Splitter("/([^" . $this->l2Language->getSettings()->RegexWordCharacters . "]+)/u") : null;

        $this->termTestRegex = "/([^" . $this->l1Language->getSettings()->RegexWordCharacters . "]+)/u";

        $l1TextWithTitle = $this->buildTextWithTitle($this->text->l1Text);
        $l2TextWithTitle = $this->asParallel ? $this->buildTextWithTitle($this->text->l2Text) : null;

        $l1Split = $this->splitText($l1TextWithTitle, $this->l1Language->getSettings());
        $l2Split = $this->asParallel ? $this->splitText($l2TextWithTitle, $this->l2Language->getSettings()) : "";

        $xml = $this->createTextXml($l1Split, $l2Split);
        $totalTerms = $this->classTerms($xml);
        $this->createStats($totalTerms, $xml);
        $transfrom = $this->applyTransform($xml);

        return $transfrom;
    }

    protected function createTextXml($text, $parallelText) {
        $l1Settings = $this->l1Language->getSettings();
        $l2Settings = $this->l2Language == null ? null : $this->l2Language->getSettings();

        $document = new \DOMDocument('1.0', 'UTF-8');
        //$document->formatOutput = true;
        $xmlRoot = $document->createElement("xml");
        $document->appendChild($xmlRoot);

        $rootNode = $document->createElement("root");
        $rootNode->setAttribute("title", $this->text->title);

        $paragraphs = explode('¶', $text);
        $parallelParagraphs = explode('¶', $parallelText);

        for ($i = 0; $i < sizeof($paragraphs); $i++) {
            $paragraph = $paragraphs[$i];
            $parallelParagraph = $i < sizeof($parallelParagraphs) ? $parallelParagraphs[$i] : "";

            $thisParagraph = $this->createParagraph($document, $paragraph, false, $l1Settings, $this->l1Splitter);
            $thisParallelParagraph = $this->createParagraph($document, $parallelParagraph, true, $l2Settings, $this->l2Splitter);

            if (!$thisParagraph->hasChildNodes() && !$thisParallelParagraph->hasChildNodes()) {
                continue;
            }

            $linkParagraphs = $document->createElement("link");
            $thisParagraph->setAttribute("side", "first");
            $linkParagraphs->appendChild($thisParagraph);
            $thisParallelParagraph->setAttribute("side", "second");
            $linkParagraphs->appendChild($thisParallelParagraph);

            $rootNode->appendChild($linkParagraphs);
        }

        $xmlRoot->appendChild($rootNode);

        return $document;
    }

    protected function createParagraph($document, $paragraph, $asParallel, $l1Settings, $splitter) {
        $thisParagraph = $document->createElement("p");

        if ($l1Settings != null) {
            $thisParagraph->setAttribute("dir", $l1Settings->Direction);
        } else {
            $thisParagraph->setAttribute("dir", "ltr");
        }

        $sentences = explode("\n", $paragraph);

        for ($i = 0; $i < sizeof($sentences); $i++) {
            $sentence = $sentences[$i];

            if ($this->isNullOrEmptyString(rtrim($sentence))) {
                continue;
            }

            $thisSentence = $document->createElement("s");
            $tokens = $splitter->split($sentence);

            $currentLength = 0;

            for ($j = 0; $j < sizeof($tokens); $j++) {
                $token = $tokens[$j];

                if (
                        !isset($token) ||
                        $token == "" ||
                        $token == "\r" ||
                        $token == "\n"
                ) {
                    continue;
                }
                
                if ($token == " ") {
                    $t = $document->createElement("t");
                    $t->setAttribute("type", "space");
                    $t->setAttribute("inSpan", "true");
                } else if(preg_match($this->termTestRegex, $token)==0) {
                    $t = $document->createElement("t");

                    if($asParallel) {
                        $t->setAttribute("type", "parallel");
                    } else {
                        $t->setAttribute("type", "term");
                        $t->setAttribute("lower", mb_strtolower($token));
                        $t->setAttribute("state", "_n");
                    }
                    
                    $t->setAttribute("value", $token);
                } else {
                    $t = $document->createElement("t");
                    $t->setAttribute("type", "punctuation");
                    $t->setAttribute("inSpan", $asParallel ? "false" : "true");
                    $t->setAttribute("value", $token);
                }

                $thisSentence->appendChild($t);
            }

            $t = $document->createElement("t");
            $t->setAttribute("type", "space");
            $t->setAttribute("inSpan", $asParallel ? "false" : "true");
            $thisSentence->appendChild($t);

            $thisParagraph->appendChild($thisSentence);
        }

        return $thisParagraph;
    }

    protected function classTerms($xml) {
        $termsAsDict = array();
        
        foreach($this->terms as $t) {
            if($t->length!=1) {
                continue;
            }
            
            $termsAsDict[$t->phraseLower] = 
                    array(
                        'state'=>$t->state,
                        'definition'=>str_replace("\n", "<br/>", $t->fullDefinition())
                    );
        }
        
        $xpath = new \DomXpath($xml);
        $query = $xpath->query("//t[@type='term']");
        
        foreach ($query as $node) {
            $lower = $node->getAttribute("lower");
            
            if(array_key_exists($lower, $termsAsDict)) {
                $term = $termsAsDict[$lower];
                
                $node->setAttribute("box", "1");
                $node->setAttribute("state", $this->termToStateClass($term['state']));
                $node->setAttribute("data", $term['definition']);
                $node->setAttribute("new", "");
            } else {
                $node->setAttribute("new", "_nw");
            }
        }
        
        return $query->length;
    }

    protected function createStats($totalTerms, $document) {
        $data = $document->createElement("stats");
        
        $totalWordsElement = $document->createElement("totalWords");
        $notseenCountElement = $document->createElement("notseenCount");
        $knownCountElement = $document->createElement("knownCount");
        $unknownCountElement = $document->createElement("unknownCount");
        
        $xpath = new \DomXpath($document);
        $notseenCount = $xpath->query("//t[@type='term' and @state='_n']")->length;
        $knownCount = $xpath->query("//t[@type='term' and @state='_k']")->length;
        $unknownCount = $xpath->query("//t[@type='term' and @state='_u']")->length;
        
        $totalWordsElement->setAttribute('percent', "100.00");
        $totalWordsElement->nodeValue = $totalTerms;
        
        if($totalTerms>0) {
            $notseenCountElement->setAttribute('percent', sprintf("%.2f", $notseenCount/$totalTerms*100));
            $knownCountElement->setAttribute('percent', sprintf("%.2f", $knownCount/$totalTerms*100));
            $unknownCountElement->setAttribute('percent', sprintf("%.2f", $unknownCount/$totalTerms*100));
        } else {
            $notseenCountElement->setAttribute('percent', 0);
            $knownCountElement->setAttribute('percent', 100);
            $unknownCountElement->setAttribute('percent', 0);
        }
        
        $notseenCountElement->nodeValue = $notseenCount;
        $knownCountElement->nodeValue = $knownCount;
        $unknownCountElement->nodeValue = $unknownCount;
        
        $newWords = $xpath->query("//t[@type='term' and @state='_n']");
        $newWordsArray = array();
        
        foreach ($newWords as $node) {
            $lower = $node->getAttribute('lower');
            if(array_key_exists($lower, $newWordsArray)) {
                $newWordsArray[$lower] = $newWordsArray[$lower] + 1;
            } else {
                $newWordsArray[$lower] = 1;
            }
        }
        
        arsort($newWordsArray);
        unset($newWords);
        
        $data->appendChild($totalWordsElement);
        $data->appendChild($notseenCountElement);
        $data->appendChild($knownCountElement);
        $data->appendChild($unknownCountElement);
        
        $unknownWords = $document->createElement("unknownWords");
        foreach(array_slice($newWordsArray,0,20) as $key=>$value) {
            $word = $document->createElement("word");
            $word->nodeValue = $key;
            $word->setAttribute("count", $value);
            $unknownWords->appendChild($word);
        }
        
        $data->appendChild($unknownWords);
        $document->documentElement->appendChild($data);
        
        return $document;
    }
    
    protected function applyTransform($xml) {
        $xsl = new \DOMDocument;
        
        $contents = $this->asParallel 
                ? $xsl->load(public_path() . '/xslt/readParallel.xsl')
                : $xsl->load(public_path() . '/xslt/read.xsl')
                ;

        $proc = new \XSLTProcessor;
        $proc->importStyleSheet($xsl);
        
        return $proc->transformToXML($xml);
    }

    protected function splitText($text, $settings) {
        if ($this->isNullOrEmptyString($text)) {
            return "";
        }

        $text = str_replace("\r\n", "\n", $text);
        $text = str_replace("\n", "¶", $text);
        $text = str_replace("\t", " ", $text);
        $text = trim($text);

        $text = preg_replace("/\s{2,}/", " ", $text);
        $text = str_replace("{", "[", $text);
        $text = str_replace("}", "]", $text);
        $text = trim($text);
        $text = preg_replace("/([" . $settings->RegexSplitSentences . "¶])\s/", "$1\n", $text);

        return $text;
    }

    protected function buildTextWithTitle($inputText) {
        $text = "";

        if ($this->text->collectionNo != null) {
            $text = $this->text->collectionNo . ". ";
        }

        $text .= $this->text->title;
        
        if (!$this->isNullOrEmptyString($this->text->collectionName)) {
            $text .= " (" . $this->text->collectionName . ")";
        }

        if (!$this->isNullOrEmptyString($text)) {
            $text .= "\n\n";
        }

        return $text .= $inputText;
    }

    protected function isNullOrEmptyString($inputText) {
        return (!isset($inputText) || trim($inputText) === '');
    }
    
    protected function termToStateClass($state) {
        $state = trim(strtolower($state));
        
        switch($state) {
            case "known": return "_k";
            case "ignore": return "_i";
            case "ignored": return "_i";
            case "notknown": return "_u";
            case "unknown": return "_u";
            case "notseen": return "_n";
            default: throw new \Exception("Unknown state: " . $state);
        }
    }
}
