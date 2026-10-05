# Cinema Booking

Seat booking for a cinema, done as three small REST services on .NET 10 and SQL Server.

A customer can look at the programme, see which seats are still free for a show, book some of them (for more than one show at a time if they want) and cancel. An admin adds movies and schedules shows.

## How it is split

There are three services and each one has its own database.

- **Identity** (port 5103) knows the users. It registers customers, checks passwords and hands out a JWT.
- **Catalog** (port 5101) knows halls, seats, movies and shows.
- **Bookings** (port 5102) knows who booked which seat for which show.

Nobody reads the tables of somebody else. Bookings has to know when a show starts and which seats the hall has, and it gets that by calling Catalog over HTTP. If Catalog is down the booking fails with a `503` and a message that says so.

Catalog and Bookings do not call Identity either: they only check the signature of the token. The code for that sits in `src/Shared/CinemaBooking.Security`, which is the only thing the services have in common.

Inside Catalog and Bookings the code is in three projects: `Core` has the entities and the rules and knows nothing about EF Core or ASP.NET, `Infrastructure` has the database access (and the HTTP client towards Catalog), `Api` has the controllers. Identity is one project only. It has two endpoints and a single table, three projects for that would have been mostly empty folders.

## Running it

### With Docker

Copy `.env.example` to `.env`, fill it in (the comments in the file say what each value is) and then:

```
docker compose up -d --build
```

SQL Server starts first, a one-shot container runs the database scripts, then the three services come up. The API reference of each service is at `/scalar`:

- http://localhost:5103/scalar
- http://localhost:5101/scalar
- http://localhost:5102/scalar

The same thing can be done from Visual Studio: every API project has a **Docker Compose (up)** launch profile that runs that command and opens the Scalar page, and a **Docker Compose (down)** one that stops everything.

SQL Server is reachable from the host on `localhost,14330` with the `sa` password you put in `.env`.

### From Visual Studio

You need Visual Studio 2026 and SQL Server LocalDB. Create the databases once, from the `database` folder:

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 1.0.0.sql
```

Then open `CinemaBooking.slnx`, choose the **All services** launch profile and start. Connection strings and the signing key for local use are in each `appsettings.Development.json`. Outside Development there are no defaults: a service that is not given a connection string and a key refuses to start, which I prefer to a service that quietly runs with a key everybody can read on GitHub.

### Trying it

There is one user in the seed data, `admin` with password `Cinema.Admin.2026`, plus two halls, three movies and three shows for tomorrow. It is a throwaway password for a demo database.

The quickest way to see everything working is the Postman collection in `postman/`. Run the folders in order (Identity, Catalog, Bookings): the first one registers two customers and keeps their tokens, the second adds a movie and a show, the third books and cancels. Every request has its own check, so the Collection Runner tells you straight away if something is off.

## The booking rules

A booking is a list of items, one per show. For each item you either say which seats you want or just how many, and in the second case the service picks them: it tries to keep people next to each other on the same row and, if no row has room, gives the first free seats.

The booking is all or nothing. If one seat of one show cannot be had, nothing is saved.

You cannot book a show that has already started. You only see and cancel your own bookings, and a booking of someone else looks exactly like one that does not exist. Cancelling puts the seats back on sale, and it is only possible until the first show of the booking starts: after that the whole booking stays as it is.

On the Catalog side, a show cannot start in the past and cannot overlap another show in the same hall.

## Database

The database is written by hand and the EF Core classes are generated from it, not the other way round.

`database/1.0.0.sql` is the script for version 1.0.0. It is a `sqlcmd` script that pulls in the files under `1.0.0/ddl` (databases and tables) and then the ones under `1.0.0/dml` (seed data). It can be run again on a database that already exists.

When a table changes, the classes are regenerated with:

```
dotnet tool restore
./database/scaffold.ps1 -Service Catalog
```

The generated classes are partial, and anything I add to an entity lives in a separate file (`Booking.Rules.cs`, `Show.Rules.cs`), otherwise the next scaffold would delete it.

## Tests

```
dotnet test CinemaBooking.slnx
```

The tests on the rules (seat allocation, booking, cancelling, scheduling, accounts) need nothing and run in a second. The ones marked `Category=Integration` start the real API in memory and use a real SQL Server, LocalDB unless `ConnectionStrings__Catalog`, `ConnectionStrings__Bookings` and `ConnectionStrings__Identity` say otherwise. I did not use an in-memory database for those because the interesting behaviour, two people after the same seat, is enforced by a SQL Server index.

```
dotnet test CinemaBooking.slnx --filter "Category!=Integration"
dotnet test CinemaBooking.slnx --filter "Category=Integration"
```

## Logs

The services log with Serilog to the console and to a file. There is one file per day, named after the date (`logs/20261004.log`), in a `logs` folder next to the service. Besides one line per HTTP request, the log says what happened in business terms: booking made, booking refused and why, booking cancelled, show scheduled, failed sign-in, Catalog not answering.

In Docker each service keeps its `logs` folder in a volume, so the files survive a rebuild:

```
docker compose exec bookings ls logs
```

Levels and sinks are in the `Serilog` section of `appsettings.json`.

## Jenkins

Jenkins is in the same compose file under the `ci` profile, so it does not start unless you ask:

```
docker compose --profile ci up -d --build
```

It is at http://localhost:8090, user `admin`, password from `.env`. The job `cinema-booking` is already there because the server is configured from `jenkins/casc.yaml`. Press **Build Now**: it builds, runs the unit tests, runs the integration tests against the SQL Server of the compose file and, only if everything is green, builds the three Docker images.

The job reads the `main` branch of this folder, mounted read-only in the container. So it builds what is committed, not what is on disk.

## Things worth discussing

Two people booking the same seat at the same moment. Reading the free seats and then saving is not safe on its own, so the database has the last word: `BookedSeat` has a unique index on show and seat that ignores cancelled rows. The slower request gets a duplicate key error, which becomes a `409`. There is a test that fires eight requests at once and expects exactly one to win. The same trick covers two admins scheduling a show in the same hall at the same time, though only when the start time is identical; two overlapping shows with different start times, sent in the same instant, could still both get in. Closing that properly needs a lock on the hall, and I left it out.

Bookings calling Catalog synchronously. It is simple and never stale, but it means no Catalog, no bookings, and no cancellations either, because cancelling has to check when the shows start. The usual answer is for Bookings to keep its own copy of shows and seats, updated by events from Catalog. That is a message broker and a fair amount of code, too much for this exercise.

One signing key shared by the three services. Any service that can check a token could also forge one. With real users I would sign with a private key that only Identity has.

`ShowId` and `UserId` in the bookings database are plain numbers with no foreign key, since the rows they point at are in other databases. The show is checked through Catalog when the booking is made; the user id comes from the token.

The services connect to SQL Server as `sa` in the compose file. Fine on a laptop, not anywhere else.

Not done: prices and payments, holding a seat for a few minutes while the customer decides, editing or removing movies and shows, paging, an API gateway, pushing the images to a registry.
