<?xml version='1.0' ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output encoding="us-ascii" method="html" indent="yes"  />
  <xsl:template match="/ArrayOfIHeldDeviceItem">
    <table class="ms-listviewtable" width="100%" border="0" cellSpacing="0" cellPadding="0">
      <tbody>
        <tr class="ms-viewheadertr ms-vhltr">
          <th class="ms-vh2">Username</th>
          <th class="ms-vh2">Name</th>
        </tr>
        <xsl:apply-templates select="IHeldDeviceItem">
          <xsl:sort select="UserIdFriendly" />
        </xsl:apply-templates>
      </tbody>
    </table>
  </xsl:template>
  <xsl:template match="IHeldDeviceItem">
    <tr class="ms-itmhover" style="cursor: default">
      <td class="ms-vb-title ms-vb-firstCell">
        <xsl:value-of select="UserIdFriendly"/>
      </td>
      <td class="ms-vb2 ms-vb-lastCell" style="cursor: default">
        <xsl:value-of select="UserDisplayName"/>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
