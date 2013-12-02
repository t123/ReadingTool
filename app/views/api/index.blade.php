<!DOCTYPE html>
<html>
    <head>
        <title>Reading Tool - API</title>
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <meta charset="utf-8"> 
        {{ HTML::style('css/bootstrap.css') }}
        {{ HTML::style('css/common.css') }}
    </head>
    <body>
        <div id="wrapper">
            <div class="row">
                <div class="col-sm-12 col-sm-offset-1">
                    <h1>API Description</h1>
                    <div class="clr10"></div>
                    <div class="alert alert-block alert-info">
                        <ul>
                            <li>Currently only <strong>GET</strong> is supported.</li>
                            <li>The <strong>basic authentication</strong> protocol is used.</li>
                            <li>The result is always <strong>JSON</strong>.</li>
                            <li>HTTPS is available but the certificate is self-signed.</li>
                        </ul>
                    </div>
                    <h4>Languages</h4>
                    <ul>
                        <li>GET <a href="{{ action("ApiController@getLanguages") }}">http://readingtool.net/api/languages</a></li>
                        <li>GET http://readingtool.net/api/languages/{id}</li>
                    </ul>
                    <div class="clr10"></div>
                    <h4>Texts</h4>
                    <ul>
                        <li>GET <a href="{{ action("ApiController@getTexts") }}">http://readingtool.net/api/texts</a></li>
                        <li>GET http://readingtool.net/api/texts/{id}</li>
                        <li><strong>Note:</strong>The index does not return the L1 & L2 text, only fetching the individual text does so.
                    </ul>
                    <div class="clr10"></div>
                    <h4>Terms</h4>
                    <ul>
                        <li>GET <a href="{{ action("ApiController@getTerms") }}">http://readingtool.net/api/terms</a></li>
                        <li>GET http://readingtool.net/api/terms/{id}</li>
                    </ul>
                </div>
            </div>
        </div>	
    </body>
</html>
