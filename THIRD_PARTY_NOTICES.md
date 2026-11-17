# Third-Party Notices

XIIGen's first-party code is licensed under AGPL-3.0-only; see [LICENSE](LICENSE) and [NOTICE](NOTICE). Third-party packages, copied materials and separately deployed services retain their own licenses, copyright notices and attribution requirements. XIIGen's license does not replace them.

## Dependency records

- NuGet references: [platform API project](src/Xiigen.Platform.Api/Xiigen.Platform.Api.csproj) and the other project files under [src/](src) and [tests/](tests).
- Resolved NuGet versions: each project's `packages.lock.json`, for example [platform API lockfile](src/Xiigen.Platform.Api/packages.lock.json).
- Optional service images: [docker-compose.platform.yml](docker-compose.platform.yml).

Project manifests and lockfiles identify dependencies and versions. Consult the exact package metadata and included license or notice files for its terms: NuGet packages can carry a license expression or license file in their metadata, and npm packages can carry a license declaration and accompanying license files. Lockfiles alone are not a complete license record. Container images and external services have their own version-specific terms.

Preserve existing notices when copying or redistributing third-party material. A contribution that adds such material should identify its source and exact version and retain its applicable license and attribution.
