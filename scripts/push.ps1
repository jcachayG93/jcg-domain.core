﻿Get-ChildItem ../src/jcg.Domain/bin/Debug -Filter "*.nupkg" | ForEach-Object {
        Write-Host "push.ps1: Pushing $($_.Name)"
       # dotnet nuget push $_ --source $Env:NUGET_URL --api-key $Env:NUGET_API_KEY
       # if ($lastexitcode -ne 0) {
       #     throw ("Exec: " + $errorMessage)
       # }
    }