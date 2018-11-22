# Instructions for manually deploying the portal

1. Download XrmToolbox and install the Portal Records Mover tab.
2. Open XrmToolbox.
3. Connect to the Portal source organization.
4. Open a Portal Records Mover tab.
5. Click Load Items and wait for Portal Components to load.
6. Click Settings : Load Settings and select CampusPortalExportSettings.xml in the file chooser dialog.
7. Click Retrieve Records
8. In the bottom part of the screen, press the Select all link.
9. Click Export records. Save the file with a name of Deploy_[DateTime].xml.
10. In the target org, disable all plugins for Web Page
11. In the target org, make sure that JS is not a blocked file extension under System Settings.
12. Go back to the XrmToolbox Home tab and open a new tab for Portal Records Mover
13. At the bottom of the window, click the Connected to text and Create/Change connection to the destination org.
14. In the popup asking which tab to update the connection for, select the last Portal Records Mover tab.
15. In the new Portal Records Mover tab for the destination org, click Import Records.
16. Select the file created in step 9.
17. Click Import
18. Activate the plugin steps disabled in step 10
19. Clear the cache

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