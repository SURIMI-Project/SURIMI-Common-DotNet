# No parameters. Script determines project directory automatically.

# The directory where this script lives (NuGet tools folder)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Host "Script Directory: $ScriptDir"

# FIX: The PROJECT directory is the current working directory
# MSBuild invokes Exec from the project folder
$ProjectDir = (Get-Location).ProviderPath
Write-Host "Project Directory: $ProjectDir"

# The SOLUTION directory is the parent of the project directory
$SolutionDir = Split-Path -Parent $ProjectDir
Write-Host "Solution Directory: $SolutionDir"

# Build Dockerfile path
$DockerfilePath = Join-Path $ProjectDir "Dockerfile"

# Ensure Dockerfile exists
if (-not (Test-Path $DockerfilePath)) {
    Write-Error "Dockerfile not found at: $DockerfilePath"
    exit 1
}

Write-Host "Dockerfile found at: $DockerfilePath"

# Extract normalized project name (no dashes, lowercase)
$ProjectName = Split-Path $ProjectDir -Leaf
Write-Host "Project Name (raw): '$ProjectName'"

# Normalize: remove dashes and convert to lowercase
$Normalized = $ProjectName -replace '-', ''
$Normalized = $Normalized.ToLowerInvariant()
Write-Host "Normalized Name: '$Normalized'"

# Validate normalized name
if ([string]::IsNullOrWhiteSpace($Normalized)) {
    Write-Error "Failed to determine project name from directory: $ProjectDir"
    exit 1
}

# Compose full image name
$ImageName = "ghcr.io/official-ewe/${Normalized}:latest"
Write-Host "Docker image name: '$ImageName'"

# Load auth tokens
$githubToken = $env:SURIMI_DOCKER_BUILD_GITHUB_TOKEN
$bsrToken = $env:SURIMI_DOCKER_BUILD_BSR_TOKEN

if ([string]::IsNullOrEmpty($githubToken) -or [string]::IsNullOrEmpty($bsrToken)) {
    Write-Error @"
Missing environment variables:

SURIMI_DOCKER_BUILD_GITHUB_TOKEN
SURIMI_DOCKER_BUILD_BSR_TOKEN

Set them with:
[System.Environment]::SetEnvironmentVariable('SURIMI_DOCKER_BUILD_GITHUB_TOKEN', 'your-token', 'User')
[System.Environment]::SetEnvironmentVariable('SURIMI_DOCKER_BUILD_BSR_TOKEN', 'your-token', 'User')
"@
    exit 1
}

Write-Host "Tokens loaded."

# Build Docker image
Write-Host "Building Docker image..."

docker build `
    -f $DockerfilePath `
    --build-arg GITHUB_TOKEN=$githubToken `
    --build-arg BSR_TOKEN=$bsrToken `
    -t $ImageName `
    $SolutionDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker build failed (exit $LASTEXITCODE)"
    exit $LASTEXITCODE
}

Write-Host "Image '$ImageName' built successfully."

# Login to GitHub Container Registry
Write-Host "Logging in to ghcr.io..."
$githubToken | docker login ghcr.io -u official-ewe --password-stdin

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker login failed (exit $LASTEXITCODE)"
    exit $LASTEXITCODE
}

# Push Docker image
Write-Host "Pushing Docker image..."

docker push $ImageName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker push failed (exit $LASTEXITCODE)"
    exit $LASTEXITCODE
}

Write-Host "Docker image '$ImageName' pushed successfully."
exit 0