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

foreach ($file in $files) {
    Write-Host "Formatting $file..."
    $commandArguments = "$FORMAT_ARGUMENTS $file"
    Start-Process -FilePath $FORMAT_COMMAND -ArgumentList $commandArguments -NoNewWindow -Wait
}

Write-Host "Done!"
