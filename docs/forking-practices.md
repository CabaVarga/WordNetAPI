# Forking Best Practices

Standard operating procedure when forking a GitHub repo and making substantial changes, especially when the original is no longer maintained.

## 1. Declare Intent / Archive Context

Document that your fork is now the active maintained version:

- Update `README.md` to indicate this is an actively maintained fork of the original repo
- Note what diverged and why
- Link back to the original for historical context

## 2. Rename / Rebrand (Optional)

If your changes are substantial enough, consider renaming the project to distinguish it from the original. This avoids user confusion about which repo is the canonical version.

## 3. Update Package Metadata

- Change the package name in `pubspec.yaml`, `package.json`, `pyproject.toml`, etc.
- Update author/maintainer fields
- Update the repository URL to point to your fork
- Bump the version (usually a minor or major bump) to signal divergence from the original

## 4. Publish to Package Registries

Publish under your own name or organization to `pub.dev`, `npm`, `PyPI`, etc. so users can depend on your version directly rather than using a git dependency.

## 5. Redirect Users from the Original

- Open a PR or issue on the original repo pointing to your fork as the active maintained version
- Add a notice in the original's issue tracker if you have permissions

## 6. Set Up Your Own CI/CD

Ensure your fork has its own GitHub Actions or other CI pipelines, since the original's workflows may have bitrotted or reference resources you don't control.

## 7. Update Documentation

- Update all internal links that reference the original repo URL
- Update `CHANGELOG.md` to document your changes from the fork point forward
- Make clear in docs which version/fork users should be referencing
