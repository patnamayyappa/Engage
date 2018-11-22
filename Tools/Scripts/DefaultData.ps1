[CmdletBinding(SupportsShouldProcess=$false)]
Param (
    [string]$exportUrl,
    [string]$exportUserName,
    [string]$exportPassword,
    [string]$dataConfigPath,
    [string]$dataFilePath,
    [string]$importUrl,
    [string]$importUserName,
    [string]$importPassword
)

function buildConnectionString([string]$url, [string]$userName, [string]$password) {
    return "AuthType=Office365;Url=$($url);Username=$($username);Password=$($password)";
}

Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
$old_ErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Stop"

try
{
    # Import Modules
    ###############################################################
    Import-Module $PSScriptRoot\DefaultData\SonomaPartners.Xrm.DefaultData.Powershell.dll
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12

    # Run default data
    #################################################################

    Write-Host "$(Get-Date) - Running default data" -ForegroundColor Green

    Write-Host ("$(Get-Date) - Exporting default data...") -ForegroundColor Green

    $exportConnectionString = buildConnectionString $exportUrl $exportUserName $exportPassword

    Write-Host ("$(Get-Date) - Config path: {0}" -f $dataConfigPath) -ForegroundColor Green
    Write-Host ("$(Get-Date) - Data path: {0}" -f $dataFilePath) -ForegroundColor Green

    
    Export-DefaultData -DataConfigFile $dataConfigPath -DataFile $dataFilePath -ConnectionString $exportConnectionString
    

    if (-not (Test-Path $dataFilePath)) {
        Write-Host ("$(Get-Date) - Error: Could not find the exported data file with path - {0}" -f $dataFilePath) -ForegroundColor Red
        return
    }

    return;
    Write-Host ("$(Get-Date) - Importing {0} default data to {1}..." -f $defaultDataConfig.name, $importConnection) -ForegroundColor Green

    $importConnectionString = buildConnectionString $importUrl $importUserName $importPassword

    Import-DefaultData -DataFile $dataFilePath -ConnectionString $importConnectionString

    Write-Host "$(Get-Date) - Default data complete" -ForegroundColor Green
    

	# Wrap up
    #################################################################

    Write-Host "$(Get-Date) - Default Data push complete" -ForegroundColor Green
}
Catch
{
    $errorMessage = $_.Exception.Message
    Write-Host "$(Get-Date) - Default Data script error: $errorMessage" -ForegroundColor Red
    
    if ($_.Exception.InnerException) { 
        Write-Host ("$(Get-Date) - Inner Exception: {0}" -f $_.Exception.InnerException.Message) -ForegroundColor Red
    }

    if ($_.Exception.Source) { 
        Write-Host ("$(Get-Date) - Source: {0}" -f $_.Exception.Source) -ForegroundColor Red
    }

    if ($_.Exception.StackTrace) { 
        Write-Host ("$(Get-Date) - StackTrace: {0}" -f $_.Exception.StackTrace) -ForegroundColor Red
    }

    Break
}
Finally
{
    $ErrorActionPreference = $old_ErrorActionPreference
}
