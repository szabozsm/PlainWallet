

pushd ..\Resources\Logos



# Scale-SVGs.ps1
# Iterates all SVGs in the current folder, queries their height via Inkscape,
# calculates the scale ratio to reach 600px, then transforms and exports them.

$InkscapePath = "C:\Program Files\Inkscape\bin\inkscape.com"
$TargetHeight  = 200

Get-ChildItem -File | Rename-Item -NewName { $_.Name.ToLower() }

$svgFiles = Get-ChildItem -Filter "*.svg" -File

if ($svgFiles.Count -eq 0) {
    Write-Host "No SVG files found in: $PSScriptRoot" -ForegroundColor Yellow
    exit
}


foreach ($svg in $svgFiles) {

    Write-Host "`nProcessing: $($svg.Name)" -ForegroundColor Cyan

    # ── 3. Build output filename ─────────────────────────────────────────────
    # Places output_<original>.svg next to the source file so files don't clash.
    $outputName = "output.svg"
    $outputPath = Join-Path $svg.DirectoryName $outputName


    # ── 1. Query height ──────────────────────────────────────────────────────
    $heightOutput = & $InkscapePath --actions="query-height" $svg.FullName 2>&1

    # Inkscape prints lines; grab the last non-empty line that looks like a number
    $heightLine = ($heightOutput | Where-Object { $_ -match '^\s*[\d.]+\s*$' } | Select-Object -Last 1)

    if (-not $heightLine) {
        Write-Warning "  Could not parse height for '$($svg.Name)'. Skipping."
        Write-Host   "  Raw output: $heightOutput"
        continue
    }

    $currentHeight = [double]($heightLine.Trim())
    Write-Host "  Current height : $currentHeight px"

    if ($currentHeight -eq 0) {
        Write-Warning "  Height is 0 for '$($svg.Name)'. Skipping."
        continue
    }

    # ── 2. Calculate scale ratio ─────────────────────────────────────────────
    $scale = $TargetHeight / $currentHeight
    $scale = [math]::Round($scale, 6)
    Write-Host "  Scale ratio    : $scale  ($TargetHeight px)"


    # ── 4. Run transform + export ────────────────────────────────────────────
    $actions = "select-all; selection-group; transform-scale:$scale; fit-canvas-to-selection; fit-canvas-to-selection; export-filename:$outputPath; export-do;"

    Write-Host "  Exporting to   : $outputName"
    & $InkscapePath --actions="$actions" $svg.FullName 2>&1 | ForEach-Object { Write-Host "  [inkscape] $_" }

    if (Test-Path $outputPath) {
		Move-Item -Path $outputPath -Destination $svg.FullName  -Force
		
	
	& "C:\Users\szabo\AppData\Roaming\Python\Python312\Scripts\scour.exe"  -i $svg.FullName -o output.svg --enable-viewboxing --enable-id-stripping --enable-comment-stripping --shorten-ids --indent=none
	
	Move-Item -Path $outputPath -Destination $svg.FullName  -Force
        Write-Host "  Done: $outputName" -ForegroundColor Green
    } else {
        Write-Warning "  Output file not found after export - check Inkscape output above."
    }
}

Write-Host "All done."


popd