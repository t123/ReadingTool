<?php

namespace RT\Services;

use App\Models\Language;
use App\Models\Text;
use App\Models\Term;
use \User;
use RT\Core\Splitter;

class LatextParserService extends ParserService {
    protected function classTerms($xml) {
        $termsAsDict = array();
        
        foreach($this->terms as $t) {
            if($t->length!=1 || $t->state!='unknown') {
                continue;
            }
            
            $termsAsDict[$t->phraseLower] = 
                    array(
                        'state'=>$t->state,
                        'definition'=>$t->fullDefinition()
                    );
        }
        
        $xpath = new \DomXpath($xml);
        $query = $xpath->query("//t[@type='term']");
        
        foreach ($query as $node) {
            $lower = $node->getAttribute("lower");
            
            if(array_key_exists($lower, $termsAsDict)) {
                $term = $termsAsDict[$lower];
                
                $node->setAttribute("state", $this->termToStateClass($term['state']));
                $node->setAttribute("data", $term['definition']);
            }
        }
        
        return $query->length;
    }

    protected function createStats($totalTerms, $document) {
        return $document;
    }
    
    protected function applyTransform($xml) {
        $xsl = new \DOMDocument;
        
        $contents = $xsl->load(public_path() . '/xslt/latex.xsl');
        $proc = new \XSLTProcessor;
        $proc->importStyleSheet($xsl);
        
        return $proc->transformToXML($xml);
    }
}
