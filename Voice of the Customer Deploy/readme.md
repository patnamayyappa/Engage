# Instructions for using scripts to deploy the Survey

1. In the target org, disable all plugins for Survey and Page.
3. Update the Connection String in config.json
4. Run MoveSurvey.ps1 in Powershell.
5. Activate the plugin steps disabled in step 1
6. Test/Preview the Surveys to make sure you changes worked.

## Note
If you forget to turn off plugins for Survey and Page, please manually update the survey to remove unnecessary page and sections or delete the survey, page, and questions and try again.