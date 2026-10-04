param(
    [Parameter(Mandatory)][ValidateSet('Catalog', 'Booking')][string]$Service,
    [string]$Server = '(localdb)\MSSQLLocalDB'
)

$root = Split-Path $PSScriptRoot -Parent
$infrastructure = "$root\src\$Service\CinemaBooking.$Service.Infrastructure"
$entities = "$root\src\$Service\CinemaBooking.$Service.Core\Entities"

dotnet ef dbcontext scaffold `
    "Server=$Server;Database=Cinema$Service;Trusted_Connection=True;TrustServerCertificate=True" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --project $infrastructure `
    --context "${Service}DbContext" `
    --context-dir Persistence `
    --context-namespace "CinemaBooking.$Service.Infrastructure.Persistence" `
    --output-dir $entities `
    --namespace "CinemaBooking.$Service.Core.Entities" `
    --no-onconfiguring `
    --force
