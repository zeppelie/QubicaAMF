param(
    [Parameter(Mandatory)][ValidateSet('Catalog', 'Bookings', 'Identity')][string]$Service,
    [string]$Server = '(localdb)\MSSQLLocalDB'
)

$database = @{ Catalog = 'CinemaCatalog'; Bookings = 'CinemaBooking'; Identity = 'CinemaIdentity' }[$Service]
$source = "$(Split-Path $PSScriptRoot -Parent)\src\$Service\CinemaBooking.$Service"

if ($Service -eq 'Identity') {
    $project = "$source.Api"
    $entities = "$source.Api\Entities"
    $entitiesNamespace = "CinemaBooking.Identity.Api.Entities"
    $contextNamespace = "CinemaBooking.Identity.Api.Persistence"
}
else {
    $project = "$source.Infrastructure"
    $entities = "$source.Core\Entities"
    $entitiesNamespace = "CinemaBooking.$Service.Core.Entities"
    $contextNamespace = "CinemaBooking.$Service.Infrastructure.Persistence"
}

dotnet ef dbcontext scaffold `
    "Server=$Server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --project $project `
    --context "${Service}DbContext" `
    --context-dir Persistence `
    --context-namespace $contextNamespace `
    --output-dir $entities `
    --namespace $entitiesNamespace `
    --no-onconfiguring `
    --force
