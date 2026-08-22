param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$libDir = Join-Path $root 'Resources\libs'
New-Item -ItemType Directory -Force -Path $libDir | Out-Null

# These two assemblies are legacy PlusEMU dependencies. They are restored only
# from the canonical public PlusEMU repository, pinned to an immutable commit
# and verified against the Git blob SHA recorded by GitHub.
$dependencies = @(
    @{
        Name = 'AStar.dll'
        Url = 'https://raw.githubusercontent.com/Sledmore/PlusEMU/ba70b05e36dbda8a96e5ab0dfc0f502fe2c94127/Resources/libs/AStar.dll'
        GitBlobSha1 = '13a5f4c63d88366be1282b573d03ddf46dec829b'
    },
    @{
        Name = 'HabboEncryption.dll'
        Url = 'https://raw.githubusercontent.com/Sledmore/PlusEMU/ba70b05e36dbda8a96e5ab0dfc0f502fe2c94127/Resources/libs/HabboEncryption.dll'
        GitBlobSha1 = '294b1a58a9d59c2a93d654db8765dfd3ae3e81f7'
    }
)

function Get-GitBlobSha1([string]$Path) {
    [byte[]]$content = [System.IO.File]::ReadAllBytes($Path)
    [byte[]]$header = [System.Text.Encoding]::ASCII.GetBytes("blob $($content.Length)`0")
    [byte[]]$payload = New-Object byte[] ($header.Length + $content.Length)
    [System.Buffer]::BlockCopy($header, 0, $payload, 0, $header.Length)
    [System.Buffer]::BlockCopy($content, 0, $payload, $header.Length, $content.Length)

    $sha1 = [System.Security.Cryptography.SHA1]::Create()
    try {
        [byte[]]$hash = $sha1.ComputeHash($payload)
        return (($hash | ForEach-Object { $_.ToString('x2') }) -join '')
    }
    finally {
        $sha1.Dispose()
    }
}

foreach ($dependency in $dependencies) {
    $destination = Join-Path $libDir $dependency.Name

    if (Test-Path $destination) {
        $existingHash = Get-GitBlobSha1 $destination
        if ($existingHash -eq $dependency.GitBlobSha1) {
            Write-Host "$($dependency.Name) already verified."
            continue
        }
        Remove-Item -Force $destination
    }

    $temporary = "$destination.download"
    Remove-Item -Force $temporary -ErrorAction SilentlyContinue
    Invoke-WebRequest -UseBasicParsing -Uri $dependency.Url -OutFile $temporary

    $actualHash = Get-GitBlobSha1 $temporary
    if ($actualHash -ne $dependency.GitBlobSha1) {
        Remove-Item -Force $temporary -ErrorAction SilentlyContinue
        throw "$($dependency.Name) integrity check failed. Expected Git blob $($dependency.GitBlobSha1), got $actualHash."
    }

    Move-Item -Force $temporary $destination
    Write-Host "$($dependency.Name) restored and verified ($actualHash)."
}
