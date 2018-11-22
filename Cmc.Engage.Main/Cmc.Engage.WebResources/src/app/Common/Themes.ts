

module CampusManagement.themes {

  export function getDefaultTheme() {
      //Test Comment Added for checking
			var fetchXml = [
				'<fetch>',
				'<entity name="theme">',
				'<attribute name="backgroundcolor" />',
				'<attribute name="controlborder" />',
				'<attribute name="controlshade" />',
				'<attribute name="defaultcustomentitycolor" />',
				'<attribute name="defaultentitycolor" />',
				'<attribute name="globallinkcolor" />',
				'<attribute name="headercolor" />',			
				'<attribute name="navbarbackgroundcolor" />',
				'<attribute name="navbarshelfcolor" />',
				'<attribute name="processcontrolcolor" />',			
				'<attribute name="selectedlinkeffect" />',
				'<attribute name="panelheaderbackgroundcolor" />',
				'<attribute name="hoverlinkeffect" />',
				'<attribute name="accentcolor" />',
				'<attribute name="pageheaderbackgroundcolor" />',
				'<attribute name="maincolor" />',
				'<filter>',
				'<condition operator="eq" attribute="isdefaulttheme" value="true" />',
				'</filter>',
				'</entity>',
				'</fetch>'
			].join('');
			return Xrm.WebApi.retrieveMultipleRecords('theme', '?fetchXml=' + fetchXml);
							
	}

}
