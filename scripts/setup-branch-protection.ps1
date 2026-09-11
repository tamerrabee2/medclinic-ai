param(
    [Parameter(Mandatory=$false)]
    [string]$GitHubToken = $env:GITHUB_TOKEN,
    
    [Parameter(Mandatory=$false)]
    [string]$Repo = "tamerrabee2/medclinic-ai",

    [Parameter(Mandatory=$false)]
    [string]$Branch = "main"
)

if ([string]::IsNullOrWhiteSpace($GitHubToken)) {
    Write-Host "Please provide a GitHub Personal Access Token (PAT) with 'repo' scope." -ForegroundColor Yellow
    Write-Host "Usage: .\scripts\setup-branch-protection.ps1 -GitHubToken 'ghp_xxxx'" -ForegroundColor Cyan
    Write-Host "Or set `$env:GITHUB_TOKEN = 'ghp_xxxx' before running." -ForegroundColor Cyan
    exit 1
}

$headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept"        = "application/vnd.github.v3+json"
    "User-Agent"    = "MedClinic-Setup-Script"
}

$protectionBody = @{
    required_status_checks = @{
        strict = $true
        contexts = @(
            "Backend (.NET 10) Build & Tests",
            "Frontend (Next.js & Playwright) Build & Tests"
        )
    }
    enforce_admins = $true
    required_pull_request_reviews = @{
        dismiss_stale_reviews = $true
        require_code_owner_reviews = $false
        required_approving_review_count = 1
        require_last_push_approval = $false
    }
    restrictions = $null
    required_linear_history = $true
    allow_force_pushes = $false
    allow_deletions = $false
    required_conversation_resolution = $true
} | ConvertTo-Json -Depth 5

$url = "https://api.github.com/repos/$Repo/branches/$Branch/protection"

Write-Host "Applying Branch Protection Rules to '$Repo' on branch '$Branch'..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $url -Method Put -Headers $headers -Body $protectionBody -ContentType "application/json"
    Write-Host "[SUCCESS] Branch protection enabled successfully on '$Branch'!" -ForegroundColor Green
    Write-Host "  - Require Pull Request before merge: Enabled (1 approval)" -ForegroundColor Gray
    Write-Host "  - Require branch up to date before merging: Enabled" -ForegroundColor Gray
    Write-Host "  - Require conversation resolution: Enabled" -ForegroundColor Gray
    Write-Host "  - Block force pushes: Enabled" -ForegroundColor Gray
    Write-Host "  - Block branch deletion: Enabled" -ForegroundColor Gray
    Write-Host "  - Enforce for Administrators: Enabled" -ForegroundColor Gray
} catch {
    Write-Host "[ERROR] Failed to enable branch protection: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "Details: $($reader.ReadToEnd())" -ForegroundColor Red
    }
    exit 1
}
