# Instructions for using scripts to deploy the portal

1. In the target org, disable all plugins for Web Page.
2. In the target org, make sure that JS is not a blocked file extension under System Settings.
3. Update the Connection String in config.json
4. Run MovePortal.ps1 in Powershell.
5. Activate the plugin steps disabled in step 1
6. Clear the cache at [portal_url]/_services/about

## First Time Deploy Steps
1. Navigate to Portal : Web Link Sets
2. Open Footer and navigate to the Web Link related grid.
3. Deactivate the following links.
    1. Support
    2. Forum
4. Open the Web Link Set Primary Navigation.
5. Deactivate the following links.
    1. Forums
    2. My Support
6. Navigate to Portal : Web Files
7. Deactivate the theme.css Web File owned by System.