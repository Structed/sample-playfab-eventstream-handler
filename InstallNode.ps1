$nodeVersion = "14.7.0"

# Check for NPM and minimum version
if ($null -ne (Get-Command "node" -ErrorAction SilentlyContinue)) {
    Write-Host "Node is already installed."
    # if ("" -ne (Get-Command "node" -ErrorAction SilentlyContinue))
    # exit
}

$nodeDownloadUri = "https://nodejs.org/dist/v$nodeVersion/node-v$nodeVersion-x64.msi ..."

$url = $nodeDownloadUri
$output = "$PSScriptRoot\node.msi"
$start_time = Get-Date

# Write-Host "Downloading node.js v$nodeVersion from $nodeDownloadUri"
# $wc = New-Object System.Net.WebClient
# $wc.DownloadFile($url, $output)

# Write-Output "Time taken: $((Get-Date).Subtract($start_time).Seconds) second(s)"

$file = Get-ChildItem("node.msi")
$DataStamp = get-date -Format yyyyMMddTHHmmss
$logFile = '{0}-{1}.log' -f $file.fullname,$DataStamp

$MSIArguments = @(
    # "/i"
    "/ju" # Only for current user
    ('"{0}"' -f $file.fullname)
    "/quiet"
    "/norestart"
    "/L*v"
    $logFile
)
Start-Process "msiexec.exe" -ArgumentList $MSIArguments -Wait -NoNewWindow 