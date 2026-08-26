param(
    [Parameter(Mandatory = $true)]
    [string]$Root
)

$ErrorActionPreference = 'Stop'

function Patch-File {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Old,
        [Parameter(Mandatory = $true)][string]$New,
        [Parameter(Mandatory = $true)][string]$AlreadyPatchedMarker
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Patch target not found: $Path"
    }

    $content = Get-Content -LiteralPath $Path -Raw

    if ($content.Contains($AlreadyPatchedMarker)) {
        Write-Host "[RDP Imager Patch] Already patched: $Path"
        return
    }

    if (-not $content.Contains($Old)) {
        throw "Expected source block not found in: $Path"
    }

    $content = $content.Replace($Old, $New)
    Set-Content -LiteralPath $Path -Value $content -Encoding UTF8
    Write-Host "[RDP Imager Patch] Patched: $Path"
}

$effectManager = Join-Path $Root 'src\app\avatar\EffectAssetDownloadManager.ts'
$animationManager = Join-Path $Root 'src\app\avatar\animation\AnimationManager.ts'

Patch-File \
    -Path $effectManager \
    -Old '        this._structure.registerAnimation(library.animation);' \
    -New '        if(library.animation) this._structure.registerAnimation(library.animation);' \
    -AlreadyPatchedMarker 'if(library.animation) this._structure.registerAnimation(library.animation);'

$oldAnimationBlock = @'
    public registerAnimation(structure: AvatarStructure, animations: { [index: string]: IAssetAnimation }): boolean
    {
        const animationData = animations[Object.keys(animations)[0]];

        const animation = new Animation(structure, animationData);

        this._animations.add(animationData.name, animation);

        return true;
    }
'@

$newAnimationBlock = @'
    public registerAnimation(structure: AvatarStructure, animations: { [index: string]: IAssetAnimation }): boolean
    {
        if(!animations) return false;

        const animationKeys = Object.keys(animations);

        if(!animationKeys.length) return false;

        const animationData = animations[animationKeys[0]];

        if(!animationData) return false;

        const animation = new Animation(structure, animationData);

        this._animations.add(animationData.name, animation);

        return true;
    }
'@

Patch-File \
    -Path $animationManager \
    -Old $oldAnimationBlock \
    -New $newAnimationBlock \
    -AlreadyPatchedMarker 'if(!animations) return false;'

Write-Host '[RDP Imager Patch] Missing/empty effect animations are now ignored safely.'
