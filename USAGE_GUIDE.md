# Usage Guide

How to start RecipeManager and see the result of the work. Two ways to run it:

- **[Path A — Docker Compose](#path-a--docker-compose-easiest)**
- **[Path B — Run locally](#path-b--run-locally)**

Then jump to **[See the result](#see-the-result-a-2-minute-tour)** for a guided tour.

---

## Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| Docker Desktop | any recent | `docker --version` (and it must be **running**) |
| .NET SDK | 10.0.301 (see `global.json`) | `dotnet --version` |
| Node.js | 24.x | `node --version` |
| PostgreSQL | 17 | only for Path B if you don't use the Docker database |

Path A needs only Docker. Path B needs the .NET SDK + Node (+ a PostgreSQL you can connect to).

---

## Path A — Docker Compose

1. Make sure **Docker Desktop is running**.
2. From the repository root:

   ```bash
   docker compose up --build
   ```

   This builds and starts three containers:
   - `db` — PostgreSQL 17 on `localhost:5432`
   - `api` — the .NET API on `localhost:8080` (waits for the DB to be healthy, then **auto-applies migrations**; the database starts empty)
   - `client` — the Vite dev server on `localhost:5173`

3. Wait until the logs settle (the API prints `Application started`), then open:

   **http://localhost:5173**

4. To stop: press `Ctrl+C`. To remove the containers: `docker compose down`.
   To also wipe the database: `docker compose down -v`.

---

## Path B — Run locally

### 1. Start PostgreSQL

Either use your own local PostgreSQL, or start just the database with Docker:

```bash
docker compose up db
```

The API's default connection string (in `src/RecipeManager.Api/appsettings.json`) is:

```
Host=localhost;Port=5432;Database=recipemanager;Username=postgres;Password=postgres
```

If your PostgreSQL uses a different password, override it (next step). If you
started the DB with `docker compose up db`, its password is `postgres`, so
override the connection string accordingly.

### 2. Configure backend secrets (first time only)

The JWT signing key is **not** committed — it lives in .NET user secrets. Set one:

```bash
dotnet user-secrets set "Jwt:SigningKey" "dev-signing-key-at-least-32-bytes-long!" \
  --project src/RecipeManager.Api
```

Optionally override the database connection string (e.g. to match the Docker DB):

```bash
dotnet user-secrets set "ConnectionStrings:RecipeDb" \
  "Host=localhost;Port=5432;Database=recipemanager;Username=postgres;Password=postgres" \
  --project src/RecipeManager.Api
```

### 3. Run the API

```bash
dotnet run --project src/RecipeManager.Api
```

- Runs in the **Development** environment, so on startup it applies EF
  migrations automatically. 
- Listens on **http://localhost:5027**. The API base path is `/api/v1`.
- The OpenAPI document is served at **http://localhost:5027/openapi/v1.json**.

Leave this terminal running.

### 4. Run the client

In a **second terminal**:

```bash
cd client
cp .env.example .env.local
```

Edit `.env.local` so the client points at your **local** API port (not the
Docker `8080`):

```
VITE_API_URL=http://localhost:5027/api/v1
```

Then:

```bash
npm install
npm run dev
```

Open **http://localhost:5173**.

### Shortcut — start / stop scripts (Windows)

Once the one-time setup is done (PostgreSQL running from step 1, the
`Jwt:SigningKey` secret from step 2, and `client/.env.local` + `npm install`
from step 4), you can launch the API **and** the client together with a single
PowerShell script instead of running steps 3 and 4 by hand:

```powershell
pwsh scripts/start.ps1
```

This opens **two** new PowerShell windows — one running the API on
**http://localhost:5027**, the other running the client on
**http://localhost:5173** — and prints both URLs.

To shut them down again:

```powershell
pwsh scripts/stop.ps1
```

`stop.ps1` stops whatever is listening on ports **5027** and **5173**, so it
cleans up the two windows `start.ps1` opened (and reports if nothing was
running). Both scripts are Windows/PowerShell only; on macOS or Linux use the
manual steps 3 and 4 above.

---

## See the result (a 2-minute tour)

1. **Register.** On the auth screen, switch to *Sign up*, enter a name, email,
   and a password (min 8 characters), and submit. You're logged straight in.
   (Access + refresh tokens are stored in the browser; the client silently
   refreshes the access token on expiry.)

2. **Browse the dashboard.** The category sidebar and recipe grid are populated
   from the API (empty on a brand-new database — run the seed script above, or
   create a recipe). Search and category filtering happen **server-side** with
   pagination. Open the **Filter** panel for difficulty, max prep/cook time,
   min servings, "must contain these ingredients", and sort order.

3. **Create a recipe.** Click *Create Recipe* and fill in:
   - Title, description, category, difficulty, prep/cook time, servings
   - **Ingredients** — name + quantity + unit (new ingredient names are created
     on the fly and reused case-insensitively; their nutrition is looked up and
     cached automatically)
   - **Steps** — added in order; the backend stores them as a linked list
   - A **cover image** (JPG/PNG/WEBP/GIF, up to 10 MB)

   Submit. Behind the scenes the client does: create recipe -> append steps ->
   attach ingredients -> upload image.

4. **Open the recipe.** Click a card to see the detail view: ordered steps with
   a progress checklist, the ingredient list with units, the cover image, the
   average rating, and the **Nutrition** panel.

5. **Rate and favourite.** Give it a 1–5 star rating (the average updates), tap
   the heart to favourite it, and use *Save to Collection* to add it to a named
   collection. Your **Favourites** and **Collections** appear in the sidebar.

6. **Nutrition.** The panel shows per-serving calories/protein/fat/carbs/fibre.
   In **automatic** mode these are computed from the ingredients' cached macros
   (with a note when some couldn't be counted); the owner can switch to a
   **manual** override, or use *Fetch nutrition data* to back-fill ingredients
   that have none. See [Nutrition notes](#nutrition-notes) below.

7. **Profile.** The avatar menu (top-right) opens *My Profile* — your name,
   email, recipe count, and an avatar upload.

### What's real vs. decoration

Backed by the API: **auth, recipes, steps, ingredients, categories, search,
pagination, filters, image upload, per-user ratings, favourites, collections,
nutrition (automatic + manual), and profiles/avatars.**

Visual placeholders only (no backend): **comments** and the **social-login
(Google/Apple) buttons**.

### Nutrition notes

Ingredient macros come from **USDA FoodData Central** and are cached on each
ingredient. The bundled `DEMO_KEY` is heavily rate-limited, so lookups may fail
or be partial; for reliable results set your own free key via
`Nutrition:UsdaApiKey` (user-secret) or `Nutrition__UsdaApiKey` (env), or set
`Nutrition:Enabled=false` to disable external lookups and rely on manual
nutrition. The demo seed script uses manual nutrition, so it works regardless of
the key.

---

## API quick reference

Base URL: `/api/v1` (i.e. `http://localhost:8080/api/v1` in Docker, or
`http://localhost:5027/api/v1` locally).

| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| POST | `/auth/register` | — | Create account, returns tokens |
| POST | `/auth/login` | — | Log in, returns tokens |
| POST | `/auth/refresh` | — | Rotate refresh token |
| POST | `/auth/logout` | — | Revoke refresh token |
| GET | `/recipes` | — | List (paged): `?page=&pageSize=&search=&categoryId=&difficulty=&maxPrepTime=&maxCookTime=&minServings=&ingredientIds=&sortBy=&sortDescending=` |
| GET | `/recipes/{id}` | — | Recipe detail (incl. nutrition) |
| POST | `/recipes` | ✔ | Create recipe |
| POST | `/recipes/{id}/steps` | ✔ | Append a step |
| POST | `/recipes/{id}/ingredients` | ✔ | Attach an ingredient |
| POST | `/recipes/{id}/image` | ✔ | Upload cover image (multipart) |
| PUT | `/recipes/{id}/nutrition` | ✔ | Set automatic or manual nutrition (owner) |
| PUT | `/recipes/{id}/rating` | ✔ | Rate 1–5 |
| DELETE | `/recipes/{id}/rating` | ✔ | Remove your rating |
| GET | `/favorites` | ✔ | Your favourite recipes |
| PUT | `/favorites/{recipeId}` | ✔ | Favourite a recipe |
| DELETE | `/favorites/{recipeId}` | ✔ | Un-favourite |
| GET · POST | `/collections` | ✔ | List / create collections |
| PUT · DELETE | `/collections/{id}/recipes/{recipeId}` | ✔ | Add / remove a recipe |
| GET | `/users/me` | ✔ | Current user's profile |
| POST | `/users/me/avatar` | ✔ | Upload avatar (multipart) |
| GET · POST | `/categories` | — · ✔ | List / create categories |
| GET · POST | `/ingredients` | — · ✔ | List / create ingredients |
| POST | `/ingredients/{id}/nutrition/refresh` | ✔ | Re-fetch an ingredient's macros |

Example — register with `curl`:

```bash
curl -X POST http://localhost:5027/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Ada","lastName":"Lovelace","email":"ada@example.com","password":"Password1"}'
```

---

## Running the tests

```bash
# Backend unit tests (fast, no Docker)
dotnet test tests/RecipeManager.Domain.UnitTests
dotnet test tests/RecipeManager.Application.UnitTests

# Backend integration tests — spins up PostgreSQL via Testcontainers,
# so Docker Desktop must be running
dotnet test tests/RecipeManager.Api.IntegrationTests

# Frontend tests (Vitest)
cd client
npm run test
```

---