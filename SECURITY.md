# Security policy

## Reporting a vulnerability

Please **do not** open a public issue. Use GitHub's private reporting:

https://github.com/DataHub-Software/ArkaControls/security/advisories/new

Include the affected version, a description, and steps to reproduce. We aim to acknowledge reports within 5 working days and to publish a fix and advisory once a patch is available.

## Supported versions

Only the latest released version receives security fixes while the project is pre-1.0.

## Scope

ArkaControls is a UI library: it does no networking and stores no data. Relevant reports include unsafe handling of text in `ArkaHtmlLabel`, native-interop problems, and resource exhaustion (handle leaks).
