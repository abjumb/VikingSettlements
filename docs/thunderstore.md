# Publishing to Thunderstore

Everything package-side is already prepared: the zip that CI builds contains
`manifest.json`, `icon.png` (256×256), `README.md` (the package page body),
`CHANGELOG.md` (rendered as the page's Changelog tab), `LICENSE` and
`plugins/VikingSettlements.dll`. What remains needs the repository owner's
browser — Thunderstore has no way to delegate account or team creation.

## One-time setup

1. **Make the GitHub repo public** (Settings → General → Danger Zone →
   Change visibility). The manifest's `website_url` and every guide link on
   the package page point at `github.com/abjumb/VikingSettlements`; on a
   private repo those links 404 for everyone but you.

2. **Create a Thunderstore account** — go to
   <https://thunderstore.io/c/valheim/> and sign in (GitHub login works).

3. **Create a team** — <https://thunderstore.io/settings/teams/> → *Create
   team*. The team name becomes the package namespace, the `Author-Name`
   part of the package identity (e.g. team `abjumb` publishes
   `abjumb-VikingSettlements`). Team names can't easily change later, so
   pick the name you want to be known by.

## Publishing a release

1. Grab the zip from the GitHub release you want to publish —
   `VikingSettlements-vX.Y.Z.zip` from
   <https://github.com/abjumb/VikingSettlements/releases/latest>
   (both assets are identical; the versioned name is just clearer to keep
   around).

2. Go to <https://thunderstore.io/c/valheim/create/> and:
   - **Team:** the team you created.
   - **Community:** Valheim (preselected when you start from the Valheim
     community page).
   - **Categories:** *Mods* fits; *Gameplay* / *World Generation* also apply
     if offered.
   - **NSFW:** no.
   - Drop the zip in the upload box and submit.

3. Thunderstore validates the manifest, icon and README server-side. If it
   rejects the upload, the error names the file — the most common causes are
   a namespace/team mismatch or re-uploading a `version_number` that was
   already published (versions are immutable; bump the version and rebuild
   instead).

That's it — the page goes live at
`https://thunderstore.io/c/valheim/p/<team>/VikingSettlements/` immediately.
Verify the three tabs: Details (README), Changelog (CHANGELOG.md), Versions.

Promo art for posts and embeds lives in `docs/brand/`: the pixel banner
(three sizes), the VS shield logo (32–512 px), and a composed
`promo-1200x630.png` social card.

## Updating later

Every update is the same two steps: tag `vX.Y.Z` on GitHub (CI builds and
attaches the zip, with the tag↔manifest↔plugin↔changelog consistency gate
making version drift impossible), then upload that zip on the same create
page. Thunderstore stacks it as a new version of the existing package —
users with a mod manager get the update automatically.

The dependency pins in `manifest.json`
(`denikson-BepInExPack_Valheim-5.4.2333`, `ValheimModding-Jotunn-2.29.2`)
mean mod managers install both requirements alongside the mod — players
never need to hunt for BepInEx or Jötunn themselves.

## Automated publishing (wired into CI)

The release workflow has a **Publish to Thunderstore** job that pushes the
exact zip the GitHub release carries to Thunderstore as a new version of
`morosmods-VikingSettlements`. It activates itself once a single secret
exists; until then it skips with a note and manual uploads keep working.

One-time activation:

1. On Thunderstore: *Teams → morosmods → Service Accounts → Add service
   account* (name it e.g. `github-actions`). Copy the token it shows — it
   is displayed only once.
2. On GitHub: *VikingSettlements → Settings → Secrets and variables →
   Actions → New repository secret*, name `THUNDERSTORE_API_KEY`, paste
   the token.

From then on, every release (tag push or workflow dispatch) publishes to
Thunderstore automatically after the GitHub release is created. Two
properties worth knowing:

- **Thunderstore versions are immutable.** Re-running a release whose
  version already published will fail the Thunderstore job (the GitHub
  release job still succeeds) — bump the version for any fix.
- The version gate upstream guarantees the zip's manifest version matches
  the tag, so CI can never publish a mismatched version.
