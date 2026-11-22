# IndieVault — Progress Log

**Project:** IndieVault — Indie Game Discovery, Upload and Recommendation Platform
**Course:** SEL-311 Software Construction, BSE-5 Morning (Instructor: Engr. Saad Mazhar Khan)
**Team:** Umer Iftikhar [09-131242-089], Qasim Abdul Qadeer Khan [09-131242-75]
**Workflow:** feature branch → pull request → review → merge into `develop`; `main` and `develop` are protected branches.

This log is generated directly from the repository's merge history (`github.com/Umer-Iftikhar/IndieVault`), not written from memory, so dates, authors and PR numbers below are exact.

---

## Merged Work (on `develop`)

| Date | PR | Feature | Contributor |
|---|---|---|---|
| 2026-10-12 | #3 | Initial build workflow / CI setup | Umer Iftikhar |
| 2026-10-12 | #4 | Fixed project structure | Umer Iftikhar |
| 2026-10-12 | #5 | CI: run build only on PRs into `main` | Umer Iftikhar |
| 2026-10-12 | #6 | Logging and error handling (initial) | Qasim Abdul Qadeer Khan |
| 2026-10-13 | #7 | Error-handling controllers and custom error pages | Qasim Abdul Qadeer Khan |
| 2026-11-03 | #8 | Fix: restore `develop` build (Serilog packages, namespaces, root route) | Qasim Abdul Qadeer Khan |
| 2026-11-04 | #9 | Test project setup — middleware and error-controller tests | Umer Iftikhar |
| 2026-11-06 | #10 | Domain models | Qasim Abdul Qadeer Khan |
| 2026-11-06 | #11 | Data layer: `AppDbContext`, Fluent API configurations, migrations, Identity | Qasim Abdul Qadeer Khan |
| 2026-11-08 | #12 | Generic repository and lookup repositories | Umer Iftikhar |
| 2026-11-10 | #13 | Game and activity repositories | Qasim Abdul Qadeer Khan |
| 2026-11-11 | #14 | Database seeder with Bogus sample data | Umer Iftikhar |
| 2026-11-13 | #15 | Game edit and delete with image handling *(first attempt)* | Qasim Abdul Qadeer Khan |
| 2026-11-14 | #16 | Revert of PR #15 (regression found after merge) | Umer Iftikhar |
| 2026-11-14 | #17 | Site layout, dark theme and shared scripts | Umer Iftikhar |
| 2026-11-14 | #18 | User authentication: registration, login, logout, access-denied | Umer Iftikhar |
| 2026-11-15 | #19 | Game upload and "My Games" for developers | Qasim Abdul Qadeer Khan |
| 2026-11-15 | #20 | Game edit and delete with image handling *(corrected re-submission)* | Qasim Abdul Qadeer Khan |
| 2026-11-16 | #21 | Game details page *(first attempt)* | Umer Iftikhar |
| 2026-11-16 | #22 | Revert of PR #21 (regression found after merge) | Umer Iftikhar |
| 2026-11-16 | #24 | CI: `main`-source-check workflow for PR validation | Umer Iftikhar |
| 2026-11-17 | #23 | Game details page *(corrected re-submission)* | Umer Iftikhar |
| 2026-11-17 | #25 | Game browsing — filtering, sorting and pagination | Qasim Abdul Qadeer Khan |
| 2026-11-18 | #26 | Review system — one review per user, owner/admin delete | Qasim Abdul Qadeer Khan |
| 2026-11-19 | #27 | Wishlist with AJAX add/remove | Umer Iftikhar |
| 2026-11-19 | #28 | Admin dashboard and featured-games toggle | Qasim Abdul Qadeer Khan |
| 2026-11-20 | #29 | Admin genre management | Umer Iftikhar |
| 2026-11-20 | #30 | Developer profile with GitHub integration and profile editing | Qasim Abdul Qadeer Khan |

**27 numbered pull requests merged** (#3–#30, with #1 and #2 unaccounted for — see Notes) **plus one direct, unnumbered commit** (the initial logging/error-handling work), spanning 2026-10-12 to 2026-11-20. This includes two revert-and-redo cycles: one on the game edit/delete feature (#15 → #16 revert → #20 redo) and one on the game details page (#21 → #22 revert → #23 redo).


## Notes

- Two features were reverted and redone rather than patched in place, which is why each appears twice in the table above: game edit/delete (PR #15 → reverted by #16 → corrected re-submission #20) and the game details page (PR #21 → reverted by #22 → corrected re-submission #23, which — despite the lower number — merged a day *after* the admin dashboard PR #28, since #23 was the number originally reserved when the branch was first opened on 2026-11-16, before the revert).
- PR numbers **#1 and #2 do not appear anywhere in the repository's commit history** (checked across every branch, not just `develop`). GitHub shares one number sequence between issues and pull requests, so these may be issues rather than PRs, or PRs that were closed without merging — this can't be confirmed from git history alone; check directly on GitHub's Issues/Pull requests tabs if it matters for grading.
- Commit attribution above is taken from each merged pull request's source branch; two git identities on record ("Umer-Iftikhar" and "Umer Iftkihar") are the same contributor.
- **This log reflects only what is merged or open on GitHub** 
