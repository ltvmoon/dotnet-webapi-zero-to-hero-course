# Cleaning Migrations in EF Core 10

A hands-on testbed for practicing every EF Core migration cleanup strategy: `remove`, revert, squash, and reset.

## Resources

- **Article**: [Cleaning Migrations in EF Core 10](https://codewithmukesh.com/blog/cleaning-migrations-efcore/)
- **Companion article**: [Running Migrations in EF Core 10](https://codewithmukesh.com/blog/running-migrations-efcore/)
- **Course**: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

## What You'll Practice

- Remove an unapplied migration with `dotnet ef migrations remove`
- Revert applied migrations to a previous point with `dotnet ef database update [name]`
- Squash an entire migration history into a single `InitialCreate`
- Reset (drop database + recreate) for early-development scenarios
- Resolve `__EFMigrationsHistory` after a squash so the schema and history stay in sync

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running PostgreSQL instance (any local install or `docker run -d --name pg -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:17`)
- `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

The connection string in `appsettings.json` points at `localhost:5432`, database `dotnetHero`, user `postgres`, password `password`. Edit it if your setup differs.

## Quick Start

```bash
dotnet build

# From inside MovieApi.Api/
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

Open `http://localhost:5000/scalar/v1` for the API surface.

> Note: this project does NOT auto-apply migrations on startup (unlike the running-migrations-efcore sibling). The whole point is to keep the migration state under your control while you practice each cleanup strategy.

## Hands-On Exercise: Build a Messy Migration History, Then Squash It

The article walks through cleaning up a project that has accumulated dozens of migrations. To feel that pain (and the satisfaction of squashing it), simulate the accumulation here.

### Step 1: Create the InitialCreate migration

```bash
cd MovieApi.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

You now have one migration file and one row in `__EFMigrationsHistory`.

### Step 2: Add a few churn migrations

Add a property to `Models/Movie.cs`, generate a migration, apply it. Repeat 3-4 times to build up history. Example sequence:

1. Add `public string? Description { get; private set; }` to `Movie` (and a setter path through the constructor or `Update`).
   ```bash
   dotnet ef migrations add AddMovieDescription
   dotnet ef database update
   ```
2. Add `public string? Director { get; private set; }`.
   ```bash
   dotnet ef migrations add AddMovieDirector
   dotnet ef database update
   ```
3. Add a unique index on `Title` in `MovieConfiguration.cs` (`builder.HasIndex(m => m.Title).IsUnique();`).
   ```bash
   dotnet ef migrations add MakeTitleIndexUnique
   dotnet ef database update
   ```
4. Add `public int? RuntimeMinutes { get; private set; }`.
   ```bash
   dotnet ef migrations add AddRuntimeMinutes
   dotnet ef database update
   ```

You now have 5 migration files. Imagine 200.

### Step 3: Practice `remove` (last migration, not yet applied)

Add one more property but DO NOT apply it:

```bash
# Add `public string? Studio { get; private set; }` to Movie
dotnet ef migrations add AddStudio
# Realize it was a mistake
dotnet ef migrations remove
```

The migration files are gone and the snapshot is back to 4 migrations.

### Step 4: Practice revert

Roll the database back to `AddMovieDirector`:

```bash
dotnet ef database update AddMovieDirector
dotnet ef migrations list
```

The two migrations after `AddMovieDirector` show as unapplied. Run `dotnet ef migrations remove` twice to drop them, or `dotnet ef database update` to reapply.

### Step 5: Practice squashing (the main event)

Make sure the database is fully migrated:

```bash
dotnet ef database update
dotnet ef migrations list
```

Back up the Migrations folder:

```bash
cp -r Migrations Migrations_backup
```

Connect to PostgreSQL and clear history:

```sql
DELETE FROM app."__EFMigrationsHistory";
```

> The default schema for this project is `app` (set in `MovieDbContext.OnModelCreating`), so the history table lives at `app.__EFMigrationsHistory`. If you change the schema, update the SQL accordingly.

Delete the Migrations folder and create a fresh initial migration:

```bash
rm -rf Migrations/
dotnet ef migrations add InitialCreate
```

Generate the SQL script and copy the final `INSERT` statement:

```bash
dotnet ef migrations script
```

The last line looks like:

```sql
INSERT INTO app."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260428120000_InitialCreate', '10.0.0');
```

Run that statement against your database. Verify:

```bash
dotnet ef migrations list
```

You should see exactly one migration, marked applied. Done. The schema is unchanged, but the history is squashed.

### Step 6: Practice reset (nuclear option)

If you do not care about preserving data:

```bash
rm -rf Migrations/
dotnet ef database drop --force
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Use this only in early development or for throwaway environments. Never in production.

## Project Layout

```
cleaning-migrations-efcore/
├── MovieApi.slnx
├── README.md
└── MovieApi.Api/
    ├── DTOs/
    │   ├── CreateMovieDto.cs
    │   ├── MovieDto.cs
    │   └── UpdateMovieDto.cs
    ├── Endpoints/
    │   └── MovieEndpoints.cs
    ├── Models/
    │   ├── EntityBase.cs
    │   └── Movie.cs
    ├── Persistence/
    │   ├── Configurations/
    │   │   └── MovieConfiguration.cs
    │   └── MovieDbContext.cs
    ├── Services/
    │   ├── IMovieService.cs
    │   └── MovieService.cs
    ├── Program.cs
    ├── MovieApi.Api.csproj
    ├── appsettings.json
    └── appsettings.Development.json
```

The starting state has zero migrations on disk. You generate them as you walk through the exercise.

## Troubleshooting

- **"The model backing the context has changed since the database was created"** - You squashed but skipped Step 5's `INSERT` into `__EFMigrationsHistory`. Run the insert.
- **`__EFMigrationsHistory` not found** - Check the schema. This project uses `app` as the default schema, so the table is `app.__EFMigrationsHistory`.
- **`dotnet ef` not found** - Install the global tool: `dotnet tool install --global dotnet-ef`.
- **Cannot connect to PostgreSQL** - Verify Postgres is running on port 5432 and the credentials in `appsettings.json` match your instance.

## License

MIT
