<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="text" indent="no" omit-xml-declaration="yes" />

    <xsl:template match="/">
        <xsl:text>\documentclass[8pt]{article}
\usepackage[papersize={90mm,120mm},margin=2mm]{geometry}
\usepackage[kerning=true]{microtype}
\usepackage[utf8]{inputenc}
\usepackage[T1]{fontenc}
\usepackage[charter]{mathdesign}
\usepackage[normalmargins]{savetrees}
\usepackage[hidelinks]{hyperref}
\usepackage[pdftex,
    pdfauthor={</xsl:text><xsl:value-of select="//root/@author"/><xsl:text>},
    pdftitle={</xsl:text><xsl:value-of select="//root/@title"/><xsl:text>},
    pdfcreator={ReadingTool}]{hyperref}
\sloppy
\pagestyle{empty}
\begin{document}
</xsl:text>
<xsl:apply-templates select="//root/link"/>
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
                <xsl:if test="@state='_u'">
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