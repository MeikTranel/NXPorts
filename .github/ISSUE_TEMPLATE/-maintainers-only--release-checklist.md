---
name: "[MAINTAINERS ONLY] Release Checklist"
about: Entry point for releasing a new version of NXPorts
title: Release Checklist For Release vX.X
labels: maintenance
assignees: MeikTranel

---

- [ ] Verify correct version in [Global Properties](../Directory.Build.props) to `vNext`
- [ ] Verify that the [Changelog](../CHANGES.MD)
  - [ ] List all closed issues in this release
  - [ ] List all contributors who helped with this release
- [ ] Wait for latest canary build to succeed
  - [ ] Automated tests succeed on all supported platforms
  - [ ] Manual testing on the canary build
- [ ] Run manual action "create-release"
- [ ] Wait for tagged release build to finish
  - [ ] Approve deployment to `NuGet Gallery`

<details>
  <summary>Glossary</summary>
* `vNext` -> Name of the version **to be released**
* `vCurrent` -> Name of the latest **released** Version
</details>
