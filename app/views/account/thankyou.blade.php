@extends('shared.layout')

@section('pageTitle')
	Thank you for registering
@stop

@section('content')
<div class="alert alert-block alert-success">
    Thank you for registering.
</div>
<div class="clr10"></div>
<h3>Getting Started</h3>
<ol>
    <li>
        First you must <a href="{{ action('LanguageController@add') }}">add a new language</a>.
    </li>
    <li>
        Next, you should add some dictionaries to that language. You'll need the URL of the dictionary, and replace the word in the URL with
        <strong>###</strong>.
        <br/>
        For example if you look up <strong>bonjour</strong> on Word Reference, the URL is <a href="http://www.wordreference.com/fren/bonjour" target="_blank">http://www.wordreference.com/fren/bonjour</a>.
        Therefore when you add the URL of the dictionary you would use: http://www.wordreference.com/fren/###
        <br/>
        Alternatively you can use a software dictionary like <a href="http://goldendict.org/" target="_blank">Golden Dict</a>.
    </li>
    <li>
        Finally you can <a href="{{ action('TextController@add') }}">add some new texts</a>. Once you've added the texts use the read option to read them.
        Words you have never seen before are <span style="background-color:#DAE5EB">higlighted in blue</span>. Words that you do not know are 
        <span style="background-color: #F5B8A9">highighted in pink</span> and words you know are not highlighted at all.
    </li>
    <li>
        All your words are stored and are accessible from the <a href="{{ action('TermController@index') }}">terms page.</a> You can export your 
        unknown words as a TSV file, or access them via the <a href="{{ action('ApiController@getIndex') }}">API.</a>
    </li>
</ol>
@stop
