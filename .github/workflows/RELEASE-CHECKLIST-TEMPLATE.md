---
name: "[MAINTAINERS ONLY] Release Checklist"
about: Entry point for releasing a new version of NXPorts
title: Release Checklist for `v{{ env.INTENDEDVERSION }}`
labels: maintenance
assignees: MeikTranel

---

- [ ] Verify correct version in [Global Properties](https://github.com/MeikTranel/NXPorts/blob/master/Directory.Build.props) to `{{ env.INTENDEDVERSION }}`
- [ ] Verify that the [Changelog](https://github.com/MeikTranel/NXPorts/blob/master/CHANGES.MD)
  - [ ] List all closed issues in this release
  - [ ] Check for updated licenses in dependencies and add package dependency changes to CHANGES.md
  - [ ] List all contributors who helped with this release
- [ ] Wait for latest canary build to succeed
  - [ ] Automated tests succeed on all supported platforms
  - [ ] Manual testing on the canary build
- [ ] Run manual action "create-release"
- [ ] Wait for tagged release build to finish
  - [ ] Approve deployment to `NuGet Gallery`
