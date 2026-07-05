# Hood configuration (`appsettings.json`)

Reference for the `Hood` and `Identity` configuration sections a consumer app provides. Only
settings you need to override have to be present; sensible defaults apply otherwise.

## Minimal consumer `appsettings.json`

The only setting every consumer must provide is the connection string. Everything else in this
document is an optional override — a fresh site can start from:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=…;Database=…;User Id=…;Password=…;TrustServerCertificate=True"
  }
}
```

Add your own provider/integration keys (SendGrid, Google, Auth0, …) as you turn features on —
most of those live in the admin **Settings** area (persisted to the database), not
`appsettings.json`. [`Hood.Development/appsettings.json`](../projects/Hood.Development/appsettings.json)
is the working example: it only sets the handful of values that genuinely differ from Hood's
defaults (its local connection string, a couple of password-policy overrides, and `BypassCDN` for
the in-repo dev loop).

## The site owner

The site owner is simply the account you create on first run at `/install` — there is no
configuration key for it. If you're upgrading a consumer that used to set `Hood:SuperAdminEmail`,
remove it from `appsettings.json`; the key is no longer read and no other action is needed.

## Identity

The whole `Identity` section is optional. Every setting below has a working default; only add a
key when you need something other than Hood's default.

| Setting | Default | Notes |
|---|---|---|
| `Identity:Password:RequireDigit` | `true` | |
| `Identity:Password:RequireLowercase` | `false` | |
| `Identity:Password:RequireUppercase` | `false` | |
| `Identity:Password:RequireNonAlphanumeric` | `true` | |
| `Identity:Password:RequiredLength` | `6` | |
| `Identity:Cookies:Name` | `hoodcms` | Prefixed onto the auth/antiforgery/session/consent cookie names. |
| `Identity:Cookies:Domain` | *(unset)* | Cookie domain; unset scopes cookies to the current host. |
| `Identity:Cookies:ConsentRequired` | `true` | Whether the cookie-consent banner gates non-essential cookies. |
| `Identity:LoginPath` | `/account/login` | |
| `Identity:LogoutPath` | `/account/logout` | |
| `Identity:AccessDeniedPath` | `/account/access-denied` | |

### Auth0 (optional)

`Identity:Auth0` is entirely optional. Leave it out (or leave `Domain`/`ClientId` unset) and Hood
runs on the standard ASP.NET Identity/password backend — no Auth0 configuration is required to run
Hood at all. Set both `Identity:Auth0:Domain` and `Identity:Auth0:ClientId` to switch the site to
the Auth0 backend instead.

## CDN asset delivery

Hood's admin and UI CSS/JS are served from a CDN so a consumer doesn't have to publish Hood's
front-end assets with its app. By default they load from jsDelivr:

```
https://cdn.jsdelivr.net/npm/hoodcms@{version}/{path}
```

`{version}` matches the Hood assembly you're running, **including the `-rc.N` prerelease tag** — so
apps on a prerelease build resolve the matching npm package rather than 404-ing on a stable version
that isn't published yet.

Three `Hood` settings control where assets load from. Set **at most one**; they resolve in the order
`BypassCDN` → `CdnFullPath` → `CdnPath` → default.

| Setting | Effect | Use when |
|---|---|---|
| `BypassCDN: true` | Serve Hood's assets from your app's own `wwwroot`. | You publish Hood's `wwwroot` assets with your app and want no external dependency. |
| `CdnPath` | Override the CDN **base** (host + package). Hood still appends `@{version}{path}`. | You mirror the `hoodcms` package on another CDN but want to keep Hood's versioning. |
| `CdnFullPath` | A **complete** base URL used **verbatim** — Hood appends only `{path}`, never a version segment. **You own the version pin.** | You self-host or mirror the assets at a fixed location, or want to pin a specific version, without overriding the `_Scripts` / `_Styles` views. |

### Example — self-hosted, version-pinned copy

```jsonc
"Hood": {
  "CdnFullPath": "https://assets.example.com/hoodcms@7.0.0"
  // → https://assets.example.com/hoodcms@7.0.0/src/css/admin.css
}
```

### Notes

- **`asp-append-version` is a no-op on CDN URLs.** The ASP.NET Core tag helper can only hash local
  files under `wwwroot`, not remote CDN URLs, so it emits no version suffix on Hood resources — don't
  rely on it for cache-busting. Content-hashed cache-busting is planned with the manifest pipeline
  (≥ Hood 7.1).
