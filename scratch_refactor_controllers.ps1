$controllers = Get-ChildItem -Path "d:\Code\TechGearAuction\TechGearAuction.API\Controllers" -Filter *.cs

foreach ($file in $controllers) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Simple regex to replace try-catch in controllers
    # This might be tricky because of nested braces. Let's do a more robust approach or just use C# Roslyn.
    # Actually, a regex might suffice if the controllers are simple.
    
    # Let's inspect a controller first before doing wild regex.
}

