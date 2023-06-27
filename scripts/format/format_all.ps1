$FILES_DIRS = @("engine/Assets/Scripts")
$FORMAT_COMMAND = "clang-format"
$FORMAT_ARGUMENTS = "-i", "-style=file"

if (-not (Test-Path -PathType Leaf -Path ".clang-format")) {
    Write-Error "Error: .clang-format not found. Are you in the root of the project?"
    exit 1
}

if (-not (Get-Command -Name "clang-format" -ErrorAction SilentlyContinue)) {
    Write-Error "Error: clang-format not installed. Follow the README instructions to install it."
    exit 1
}

$files = @()
foreach ($dir in $FILES_DIRS) {
    $foundFiles = Get-ChildItem -Path $dir -Recurse -File -Filter "*.cs"
    foreach ($file in $foundFiles) {
        if ($file.Extension -eq ".cs") {
            $files += $file.FullName
        }
    }
}

Write-Host "Found $($files.Count) files."

$jobResults = @()
$jobCount = 5

$formatScriptBlock = {
    param($FormatCommand, $Arguments, $File)
    & $FormatCommand $Arguments $File
}

$jobOptions = @{
    ScriptBlock = $formatScriptBlock
    ArgumentList = @($FORMAT_COMMAND, $FORMAT_ARGUMENTS)
}

$jobs = @()

foreach ($file in $files) {
    $arguments = @($FORMAT_COMMAND, $FORMAT_ARGUMENTS, $file)
    $jobOptions.ArgumentList = $arguments

    Write-Host "Formatting $file"
    $job = Start-Job @jobOptions
    $jobs += $job

    if ($jobs.Count -ge $jobCount) {
        $completedJob = Wait-Job -Job $jobs -Any
        $jobResults += Receive-Job -Job $completedJob
        $jobs = $jobs | Where-Object { $_ -ne $completedJob }
    }
}

$jobResults += $jobs | Wait-Job | Receive-Job
$jobs | Remove-Job -Force

Write-Host "Done!"
