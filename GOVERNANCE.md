# Governance and licensing

## Who decides

ArkaControls is owned and maintained by **DataHub Software**. Maintainers are listed in `.github/CODEOWNERS`. Maintainers merge pull requests, publish releases and enforce the licence and trademark policy. Significant changes (new public API, new dependency, licence or target-framework changes) are discussed in an issue first.

## Why Apache-2.0

The project is deliberately open: anyone may use it, including in commercial products, without fees or activation. Apache-2.0 was chosen over MIT because it adds three things that matter for a public library:

1. **Mandatory attribution.** Redistribution must carry `LICENSE` and `NOTICE`, so derived works refer back to this repository and its owner.
2. **No trademark grant.** The name and logo stay with the owner ([TRADEMARKS.md](TRADEMARKS.md)).
3. **Express patent licence and contributor terms.** Contributions are licensed to the project and to everyone else under the same licence (Section 5), so no separate agreement is required.

## What a licence can and cannot do

Be clear-eyed about this, because it is the honest answer to "how do we stop misuse":

- Nothing published under **any** open-source licence can be technically prevented from being copied. Licences work by giving the owner **legal recourse** against violations.
- Apache-2.0 **allows** forks and modified versions, including renamed ones. It does **not** allow removing the notices, claiming original authorship, or using our name and logo. Those are the enforceable lines.
- If you want to forbid commercial reuse or rebranding of the code itself, you need a source-available licence (for example PolyForm or BUSL). That is a different trade-off: it is not open source, and it prevents the adoption and contributions this project wants.
- Code already published under MIT (the first commits of this repository) remains available under MIT to anyone who obtained it then. Changing the licence affects new releases only.

## What protects the project in practice

| Layer | What it does |
|---|---|
| Copyright headers and SPDX identifiers in every source file | Make removal visible and prove origin |
| `LICENSE` + `NOTICE` shipped inside the NuGet package | Attribution travels with every binary |
| Package metadata: author, company, copyright, repository URL | Anyone inspecting the DLL or package sees the owner and repo |
| Trademark policy | Stops a fork trading under our name |
| Signed release tags and public history | Timestamped proof of authorship |
| Branch protection, CODEOWNERS, review required | Stops unreviewed or malicious changes reaching `main` |
| DCO sign-off on contributions | Every contributor certifies they have the right to submit |
| Clean-room rule (see CONTRIBUTING) | No code from other commercial libraries enters the project |

## If someone violates the licence

1. Save evidence: URLs, archive.org snapshot, package version, screenshots of the missing notices, and the matching commit in this repository.
2. Contact them politely through an issue or email and ask them to restore the notices or rename. Most cases end here.
3. If ignored, file a takedown with the host (GitHub DMCA form, NuGet "Report" on the package page) citing the licence terms.
4. For anything serious, involve a lawyer. Copyright and trademark registration strengthen your position and are worth doing for a product the business depends on.

_This document describes project policy and general practice. It is not legal advice._
