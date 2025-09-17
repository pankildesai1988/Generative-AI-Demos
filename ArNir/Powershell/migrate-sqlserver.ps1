param(
    [string]$Name = "NewMigration"
)

Write-Host "🔄 Running SQL Server migration: $Name"

dotnet ef migrations add $Name `
    --context ArNirDbContext `
    --project ./ArNir.Data `
    --startup-project ./ArNir.Admin `
    -o Migrations/SqlServer

dotnet ef database update `
    --context ArNirDbContext `
    --project ./ArNir.Data `
    --startup-project ./ArNir.Admin

Write-Host "✅ SQL Server migration $Name applied successfully!"
