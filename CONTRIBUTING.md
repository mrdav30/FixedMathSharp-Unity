# Contributing

For substantial API, architecture, or serialization changes, open an issue or
discussion before implementation so the intended contract is clear. Focused
fixes can proceed directly through a pull request.

Please note we have a code of conduct, please follow it in all your interactions
with the project.

## Source of Truth

Edit shared package assets under `Build/Base`, then run the package sync. This
includes the shared runtime, editor tooling, samples, legal files, and
changelog. Do not hand-edit synchronized copies inside the two
`com.mrdav30.fixedmathsharp*` package folders.

Package manifests, package READMEs, asmdefs, plugins, and other package-specific
assets remain in their package folders.

## Pull Request Process

1. Keep changes focused and preserve package-variant boundaries.
2. Add or update Unity tests for behavior changes.
3. Add concise XML documentation for public runtime APIs and update the relevant
   package changelog when behavior changes.
4. Sync canonical Build files into the package variants before review.
5. Exclude generated Unity, DocFX, test, and package-export artifacts.

## Validation

From this repository root, run the package sync through the parent Unity
project:

```bash
"$UNITY_EDITOR" \
  -batchmode -quit \
  -projectPath "$(cd ../.. && pwd)" \
  -executeMethod FixedMathSharp.Build.Editor.FixedMathSharpPackageSync.SyncPackagesBatchMode \
  -logFile -
```

Set `UNITY_EDITOR` to the Unity executable. Run the command a second time when
the first pass copied files; a clean sync reports `Copied 0 files`.

Useful lightweight checks:

```bash
dotnet tool restore
git diff --check
```

The `build-and-test` workflow runs EditMode tests and sample-import smoke checks
for both package variants on Unity 6000.5.0f1. It also builds the hosted API
site from the Standard job; deployment occurs only after successful pushes to
`main`.

## Code of Conduct

### Our Pledge

In the interest of fostering an open and welcoming environment, we as
contributors and maintainers pledge to making participation in our project and
our community a harassment-free experience for everyone, regardless of age, body
size, disability, ethnicity, gender identity and expression, level of
experience, nationality, personal appearance, race, religion, or sexual identity
and orientation.

### Our Standards

Examples of behavior that contributes to creating a positive environment
include:

- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

Examples of unacceptable behavior by participants include:

- The use of sexualized language or imagery and unwelcome sexual attention or
  advances
- Trolling, insulting/derogatory comments, and personal or political attacks
- Public or private harassment
- Publishing others' private information, such as a physical or electronic
  address, without explicit permission
- Other conduct which could reasonably be considered inappropriate in a
  professional setting

### Our Responsibilities

Project maintainers are responsible for clarifying the standards of acceptable
behavior and are expected to take appropriate and fair corrective action in
response to any instances of unacceptable behavior.

Project maintainers have the right and responsibility to remove, edit, or reject
comments, commits, code, wiki edits, issues, and other contributions that are
not aligned to this Code of Conduct, or to ban temporarily or permanently any
contributor for other behaviors that they deem inappropriate, threatening,
offensive, or harmful.

### Scope

This Code of Conduct applies both within project spaces and in public spaces
when an individual is representing the project or its community. Examples of
representing a project or community include using an official project e-mail
address, posting via an official social media account, or acting as an appointed
representative at an online or offline event. Representation of a project may be
further defined and clarified by project maintainers.

### Enforcement

Instances of abusive, harassing, or otherwise unacceptable behavior may be
reported by contacting the project team at `david.oravsky@gmail.com`. All
complaints will be reviewed and investigated and will result in a response that
is deemed necessary and appropriate to the circumstances. The project team is
obligated to maintain confidentiality with regard to the reporter of an
incident. Further details of specific enforcement policies may be posted
separately.

Project maintainers who do not follow or enforce the Code of Conduct in good
faith may face temporary or permanent repercussions as determined by other
members of the project's leadership.

### Attribution

This Code of Conduct is adapted from the [Contributor Covenant][homepage],
version 1.4, available at [http://contributor-covenant.org/version/1/4][version]

[homepage]: http://contributor-covenant.org
[version]: http://contributor-covenant.org/version/1/4/
