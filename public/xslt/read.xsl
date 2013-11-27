<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" omit-xml-declaration="yes" />
    
    <xsl:template match="/root">
        <xsl:apply-templates select="link"/>
        <xsl:apply-templates select="stats"/>
    </xsl:template>

    <xsl:template match="link">
        <xsl:apply-templates select="p[@side='first']"/>
    </xsl:template>

    <xsl:template match="p[@side='first']">
        <p>
            <xsl:attribute name="dir">
                <xsl:value-of select="@dir" />
            </xsl:attribute>
      
            <xsl:apply-templates select="s"/>
        </p>
    </xsl:template>

    <xsl:template match="s">
        <span class="sentence">
            <xsl:apply-templates select="t"/>
        </span>
    </xsl:template>

    <xsl:template match="t">
        <xsl:apply-templates select="multi"/>
        <xsl:choose>
            <xsl:when test="@type='tag'">
                <xsl:value-of select="concat('&lt;', @value,'&gt;')" disable-output-escaping="yes"/>
            </xsl:when>
            <xsl:when test="@type='term'">
                <span>
                    <xsl:attribute name="class">
                        <xsl:value-of select="@state" />
                        <xsl:text> </xsl:text>
                        <xsl:value-of select='@lower' />
                        <xsl:text> </xsl:text>
                        <xsl:value-of select='@box' />
                        <xsl:text> </xsl:text>
                        <xsl:value-of select='@new' />
                        <xsl:text> </xsl:text>
                    </xsl:attribute>
                    <xsl:choose>
                        <xsl:when test="@data">
                            <a>
                                <xsl:attribute name="title">
                                    <xsl:value-of select="@data" />
                                </xsl:attribute>
                                <xsl:value-of select="@value" />
                            </a>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:value-of select="@value" />
                        </xsl:otherwise>
                    </xsl:choose>
                </span>
            </xsl:when>
            <xsl:when test="@type='parallel'">
                <xsl:value-of select="@value" />
            </xsl:when>
            <xsl:when test="@type='space'">
                <xsl:choose>
                    <xsl:when test="@inSpan='true'">
                        <span>
                            <xsl:attribute name="class">wsx</xsl:attribute>
                            <xsl:text> </xsl:text>
                        </span>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:text> </xsl:text>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
            <xsl:when test="@type='punctuation'">
                <xsl:choose>
                    <xsl:when test="@inSpan='true'">
                        <span>
                            <xsl:attribute name="class">pcx</xsl:attribute>
                            <xsl:value-of select="@value" />
                        </span>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="@value" />
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:when>
        </xsl:choose>
    </xsl:template>

    <xsl:template match="multi">
        <sup>
            <span>
                <xsl:attribute name="class">
                    mxx
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="@state" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="@id" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select='@box' />
                </xsl:attribute>
                <xsl:attribute name="data-phrase">
                    <xsl:value-of select="." />
                </xsl:attribute>
                <xsl:attribute name="data-id">
                    <xsl:value-of select="@id" />
                </xsl:attribute>
                <xsl:choose>
                    <xsl:when test="@data">
                        <a>
                            <xsl:attribute name="title">
                                <xsl:value-of select="@data" />
                            </xsl:attribute>
                            <xsl:value-of select="@length" />
                        </a>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:value-of select="@length" />
                    </xsl:otherwise>
                </xsl:choose>
            </span>
        </sup>
    </xsl:template>

    <xsl:template match="stats">
        <div style="margin-top: 30px;">
            <button class="btn btn-default" onclick="$('#stats').toggle();">
                Show some statistics for this text
            </button>
            <br/>
            <br/>
            <div id="stats" style="display: none;">
                <table class="table table-striped table-bordered table-condensed table-hover">
                    <caption>Some stats for this text</caption>
                    <tbody>
                        <tr>
                            <td>Total Words</td>
                            <td>
                                <xsl:attribute name="id">totalWords</xsl:attribute>
                                <xsl:attribute name="data-value">
                                    <xsl:value-of select="./totalWords" />
                                </xsl:attribute>
                                <xsl:value-of select="./totalWords" />
                            </td>
                            <td>
                                <xsl:value-of select="./totalWords/@percent" />%
                            </td>
                        </tr>
                        <tr>
                            <td>New Words</td>
                            <td>
                                <xsl:value-of select="./notseenCount" />
                            </td>
                            <td>
                                <xsl:value-of select="./notseenCount/@percent" />%
                            </td>
                        </tr>
                        <tr>
                            <td>Known Words</td>
                            <td>
                                <xsl:value-of select="./knownCount" />
                            </td>
                            <td>
                                <xsl:value-of select="./knownCount/@percent" />%
                            </td>
                        </tr>
                        <tr>
                            <td>Unknown Words</td>
                            <td>
                                <xsl:value-of select="./unknownCount" />
                            </td>
                            <td>
                                <xsl:value-of select="./unknownCount/@percent" />%
                            </td>
                        </tr>
                    </tbody>
                </table>
                <br/>
                <table class="table table-striped table-bordered table-condensed table-hover">
                    <caption>Most common new words in this text</caption>
                    <tbody>
                        <xsl:apply-templates select="unknownWords" />
                    </tbody>
                </table>
            </div>
        </div>
    </xsl:template>

    <xsl:template match="word">
        <tr>
            <td>
                <xsl:value-of select="@count" />
            </td>
            <td>
                <xsl:value-of select="." />
            </td>
        </tr>
    </xsl:template>
</xsl:stylesheet>