module CampusManagement.localization {

	export function getResourceString(key) {
		let rwindow: any = window;
		let resourceName: string = "cmc_/Resources/ResourceStrings";
		if (window.opener != undefined  ) 
			return window.opener.Xrm.Utility.getResourceString(resourceName, key);	
		else if (window.parent!=undefined)
			return window.parent.Xrm.Utility.getResourceString(resourceName, key);
		else
			return window.Xrm.Utility.getResourceString(resourceName, key);
	}

}