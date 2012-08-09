<?xml version="1.0" encoding="utf-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" omit-xml-declaration="yes" />

  <xsl:template match="/timetable">

    <html>
      <head>
        <title>Timetable</title>
        <style type="text/css">
          
          td {
           min-width:150px;
           vertical-align:top;
          }
          
          td.big {
            font-size:x-large;
          }
          
          td.clashes {
            border:solid 1px red;
          }
          
          div {
            margin:1px;
            background-color:#EFEFEF;
          }         
          
      
        </style>
      </head>
      <body>

        <h1>Timetable</h1>

        <table>
          <thead>
            <tr>
              <th />
              <xsl:call-template name="day-header" />
            </tr>
          </thead>

          <tbody>
            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'1'" />              
            </xsl:call-template>

            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'2'" />
            </xsl:call-template>

            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'3'" />
            </xsl:call-template>

            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'4'" />
            </xsl:call-template>

            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'5'" />
            </xsl:call-template>

            <xsl:call-template name="slot">
              <xsl:with-param name="slotNo" select="'6'" />
            </xsl:call-template>
          </tbody>
        </table>

      </body>
    </html>

  </xsl:template>

  <xsl:template name="day-header">
    <xsl:for-each select="day">
      <td class="big">
        <xsl:value-of select="@no"/>
      </td>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="slot">
    <xsl:param name="slotNo" />

    <tr>
      <td class="big">
        <xsl:value-of select="$slotNo"/>
      </td>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'1'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'2'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'3'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'4'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'5'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>

      <xsl:call-template name="day-slot">
        <xsl:with-param name="dayNo" select="'6'" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>
    </tr>

  </xsl:template>

  <xsl:template name="day-slot">
    <xsl:param name="dayNo" />
    <xsl:param name="slotNo" />

    <td>
      <xsl:if test="day[@no = $dayNo]/slot[@no = $slotNo]/@clashes = 'Y'">
        <xsl:attribute name="class">
          <xsl:text>clashes</xsl:text>
        </xsl:attribute>
      </xsl:if>     
      
      <xsl:call-template name="lectures">
        <xsl:with-param name="dayNo" select="$dayNo" />
        <xsl:with-param name="slotNo" select="$slotNo" />
      </xsl:call-template>
    </td>
    
  </xsl:template>

  <xsl:template name="lectures">
    <xsl:param name="dayNo" />
    <xsl:param name="slotNo" />

    <xsl:for-each select="day[@no = $dayNo]/slot[@no = $slotNo]/lecture">

      <div>
        <strong>
          <xsl:value-of select="course"/>
        </strong>
        <br />
        <xsl:value-of select="tutor"/>
        <xsl:text>,</xsl:text>
        <xsl:value-of select="room"/>
      </div>
      
    </xsl:for-each>
    
  </xsl:template>


</xsl:stylesheet>