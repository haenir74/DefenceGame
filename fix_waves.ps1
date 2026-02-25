$wavesDir = "E:\Develop\Unity Editor\Project one\Assets\_DungeonDefence\Resources\Data\Waves"
$replacements = @{
    "eb2dc55f82b9224438bcb40910617288" = "e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6" # Militia
    "4461b380c3c563b4f9c5688b0d1450bb" = "f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7" # Recruit
    "45f25e304c0f8634da90e48a68add732" = "b6c7d8e9f0a1b2c3d4e5f6a8b9c0d1e2" # Bandit King
    "958d03f712514e1498d3c748b10e5398" = "c7d8e9f0a1b2c3d4e5f6a8b9c0d1e2f3" # Grand Knight Commander
}

Get-ChildItem -Path $wavesDir -Filter WaveData_*.asset | ForEach-Object {
    $content = Get-Content $_.FullName
    $changed = $false
    foreach ($old in $replacements.Keys) {
        if ($content -contains "*$old*") { # Simple check, but replace is safer
            # We use -replace which works on arrays of strings (lines)
        }
        $newContent = $content -replace $old, $replacements[$old]
        if ($newContent -ne $content) {
            $content = $newContent
            $changed = $true
        }
    }
    if ($changed) {
        Set-Content $_.FullName $content
        Write-Host "Updated $($_.Name)"
    }
}
