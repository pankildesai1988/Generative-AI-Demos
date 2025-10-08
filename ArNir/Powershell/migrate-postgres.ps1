param(
    [string]$Name = "NewMigration"
)

Write-Host "🔄 Running Postgres migration: $Name"

dotnet ef migrations add $Name `
    --context VectorDbContext `
    --project ./ArNir.Data `
    --startup-project ./ArNir.Admin `
    --msbuildprojectextensionspath obj `
    -o Migrations/Postgres

dotnet ef database update `
    --context VectorDbContext `
    --project ./ArNir.Data `
    --startup-project /ArNir.Admin `
    --msbuildprojectextensionspath obj

Write-Host "✅ Postgres migration $Name applied successfully!"
