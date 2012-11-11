<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/root">
    <table>
      <xsl:apply-templates select="link"/>
    </table>
    <xsl:apply-templates select="stats"/>
  </xsl:template>

  <xsl:template match="link">
    <tr>
      <xsl:apply-templates select="p"/>
    </tr>
  </xsl:template>

  <xsl:template match="p[@side='first']">
    <td>
      <xsl:attribute name="class">f</xsl:attribute>
      <p>
        <xsl:apply-templates select="s"/>
      </p>
    </td>
  </xsl:template>

  <xsl:template match="p[@side='second']">
    <td>
      <xsl:attribute name="class">s</xsl:attribute>
      <p>
        <xsl:apply-templates select="s"/>
      </p>
    </td>
  </xsl:template>

  <xsl:template match="s">
    <span class="sentence">
      <xsl:apply-templates select="t"/>
    </span>
  </xsl:template>

  <xsl:template match="t">
    <xsl:apply-templates select="multi"/>
    <xsl:choose>
      <xsl:when test="@type='word'">
        <span>
          <xsl:attribute name="class">
            <xsl:value-of select="@state" />
            <xsl:text> </xsl:text>
            <xsl:value-of select='@lower' />
            <xsl:text> </xsl:text>
            <xsl:value-of select='@box' />
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
      <button onclick="$('#stats').toggle();">
        text statistics
      </button>
      <br/>
      <br/>
      <div id="stats" style="display: none;">
        <table class="texts" width="95%" cellspacing="0" cellpadding="0">
          <thead>
            <tr>
              <th colspan="3">Some stats</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>Total Words</td>
              <td>
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
        <table class="texts" width="95%" cellspacing="0" cellpadding="0">
          <thead>
            <tr>
              <th colspan="2" dir="ltr">Most common new words</th>
            </tr>
          </thead>
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