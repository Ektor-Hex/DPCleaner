$rootPath = "\TestZone"
$targetFile = "@Ekt0r.txt"

# Limpiar si existe
if (Test-Path $rootPath) {
    Remove-Item $rootPath -Recurse -Force
}

New-Item -Path $rootPath -ItemType Directory -Force | Out-Null

Write-Host "Generando datos de prueba en: $rootPath" -ForegroundColor Cyan

# Función para crear archivos random
function Create-RandomFile($path, $name) {
    $content = "Contenido de prueba para $name - " + (Get-Date).ToString()
    $fullPath = Join-Path $path $name
    Set-Content -Path $fullPath -Value $content
}

# Generar estructura
for ($i = 1; $i -le 5; $i++) {
    $subPath = Join-Path $rootPath "Folder_$i"
    New-Item -Path $subPath -ItemType Directory -Force | Out-Null
    
    # Crear archivo objetivo
    Create-RandomFile $subPath $targetFile
    
    # Crear archivos normales
    Create-RandomFile $subPath "data_$i.json"
    Create-RandomFile $subPath "notes_$i.txt"

    # Subcarpetas
    for ($j = 1; $j -le 3; $j++) {
        $deepPath = Join-Path $subPath "SubFolder_$j"
        New-Item -Path $deepPath -ItemType Directory -Force | Out-Null
        
        # Crear archivo objetivo en algunas subcarpetas
        if ($j % 2 -ne 0) {
            Create-RandomFile $deepPath $targetFile
        }
        
        Create-RandomFile $deepPath "image_$j.png"
        Create-RandomFile $deepPath "config_$j.xml"
    }
}

# Crear algunos en la raíz
Create-RandomFile $rootPath "readme.md"
Create-RandomFile $rootPath $targetFile

$count = (Get-ChildItem $rootPath -Recurse -Filter $targetFile).Count
Write-Host "Generación completada." -ForegroundColor Green
Write-Host "Se crearon $count archivos '$targetFile' para probar." -ForegroundColor Yellow
