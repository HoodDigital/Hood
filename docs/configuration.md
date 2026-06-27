# Hood configuration (`appsettings.json`)

Reference for the `Hood` configuration section a consumer app provides. Only settings you need to
override have to be present; sensible defaults apply otherwise.

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
