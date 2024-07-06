<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="/profile/@version">
    <xsl:attribute name="version">
      <xsl:text>1.9</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Remove sources -->
  <xsl:template match="/profile/sources">
    <sources />
  </xsl:template>

  <!-- Set lrudstate='None' for setting action sets -->
  <xsl:template match="appliesto/@lrudstate">
    <xsl:attribute name="lrudstate">
      <xsl:text>None</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Copy attribute values for Internal control from appliesto node -->
  <xsl:template match="actionset[@eventtype='KxInternalSettingEventArgs']">
    <xsl:copy>
      <xsl:apply-templates select="@* | actionlist | appliesto/@*"/>
    </xsl:copy>
  </xsl:template>

  <!-- Rename controltype -->
  <xsl:template match="@controltype">
    <xsl:attribute name="controltype">
      <xsl:choose>
        <xsl:when test=". = 'ControllerButtons'">
          <xsl:text>Button</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'ControllerThumb'">
          <xsl:text>Stick</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'ControllerTrigger'">
          <xsl:text>Trigger</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'ControllerDPad'">
          <xsl:text>DPad</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="." />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:attribute>
    <xsl:apply-templates select="@*"/>
  </xsl:template>
  
  <!-- Rename eventtype values -->
  <xsl:template match="@eventtype">
    <xsl:attribute name="eventtype">
      <xsl:text>Control</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Rename valueids -->
  <xsl:template match="@valueids">
    <xsl:attribute name="state">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>

  <!-- Replace buttonstate with controlid -->
  <xsl:template match="node()[@side='None']/@buttonstate">
    <xsl:attribute name="controlid">
      <xsl:choose>
        <xsl:when test=". = 'None'">
          <xsl:text>1</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'A'">
          <xsl:text>1</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'B'">
          <xsl:text>2</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'X'">
          <xsl:text>3</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'Y'">
          <xsl:text>4</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'LeftShoulder'">
          <xsl:text>5</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'RightShoulder'">
          <xsl:text>6</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'Back'">
          <xsl:text>7</xsl:text>
        </xsl:when>
        <xsl:when test=". = 'Start'">
          <xsl:text>8</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:attribute>
  </xsl:template>

  <!-- Remove buttonstate where side is specified -->
  <xsl:template match="node()[@side!='None']/@buttonstate">
  </xsl:template>

  <!-- Replace side -->
  <xsl:template match="@side">
    <xsl:choose>
      <xsl:when test=". = 'Left'">
        <xsl:attribute name="controlid">1</xsl:attribute>
      </xsl:when>
      <xsl:when test=". = 'Right'">
        <xsl:attribute name="controlid">2</xsl:attribute>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>