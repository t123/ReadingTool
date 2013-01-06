<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" indent="no" omit-xml-declaration="yes" />

  <xsl:template match="/root">
    <xsl:text>\documentclass[12pt]{article}

</xsl:text>
    <xsl:text>\usepackage[papersize={108mm,144mm},margin=2mm]{geometry}

</xsl:text>
    <xsl:text>\usepackage[utf8]{inputenc}

</xsl:text>
    <xsl:text>\usepackage{perpage}

</xsl:text>
    <xsl:text>\MakePerPage{footnote}

</xsl:text>
    <xsl:text>\sloppy

</xsl:text>
    <xsl:text>\pagestyle{empty}

</xsl:text>
    <xsl:text>\usepackage[scaled]{helvet}

</xsl:text>
    <xsl:text>\renewcommand{\familydefault}{\sfdefault}

</xsl:text>
    <xsl:text>\setlength\parindent{0pt}

</xsl:text>
    <xsl:text>\begin{document}

</xsl:text>
    <xsl:apply-templates select="link"/>
    <xsl:text>\end{document}
</xsl:text>
  </xsl:template>

  <xsl:template match="link">
    <xsl:apply-templates select="p[@side='first']"/>
  </xsl:template>

  <xsl:template match="p[@side='first']">
    <xsl:text>\paragraph{</xsl:text>
    <xsl:apply-templates select="s"/>
    <xsl:text>}

</xsl:text>
  </xsl:template>

  <xsl:template match="s">
    <xsl:apply-templates select="t"/>
  </xsl:template>

  <xsl:template match="t">
    <xsl:apply-templates select="multi"/>
    <xsl:choose>
      <xsl:when test="@type='term'">
        <xsl:value-of select="@value" />
        <xsl:if test="@state='nkx'">
          <xsl:text> \protect\footnote{</xsl:text>
          <xsl:value-of select="@data" />
          <xsl:text>}</xsl:text>
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