<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="no" omit-xml-declaration="yes" />

  <xsl:template match="/root">
    <xsl:apply-templates select="link"/>
    <xsl:apply-templates select="stats"/>
  </xsl:template>

  <xsl:template match="link">
    <xsl:apply-templates select="p[@side='first']"/>
  </xsl:template>

  <xsl:template match="p[@side='first']">
    <xsl:apply-templates select="s"/>
    <!--<xsl:text>\newline</xsl:text>-->
  </xsl:template>

  <xsl:template match="s">
    <xsl:apply-templates select="t"/>
  </xsl:template>

  <xsl:template match="t">
    <xsl:apply-templates select="multi"/>
    <xsl:choose>
      <xsl:when test="@type='word'">
        <xsl:value-of select="@value" />
        <xsl:if test="@state='nkx'">
          \footnote {<xsl:value-of select="@data" />}
        </xsl:if>
      </xsl:when>
      <xsl:when test="@type='space'">
        <xsl:text> </xsl:text>
      </xsl:when>
      <xsl:when test="@type='punctuation'">
        <xsl:value-of select="@value" />
      </xsl:when>
    </xsl:choose>
  </xsl:template>


</xsl:stylesheet>