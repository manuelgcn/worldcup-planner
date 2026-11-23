# Security policy

## Reporting a vulnerability

Report it privately through GitHub: open the repository's **Security** tab and choose
**Report a vulnerability**, or go straight to
<https://github.com/manuelgcn/worldcup-planner/security/advisories/new>.
Please do not open a public issue or pull request for it.

Include the release or commit you tested, the steps to reproduce it and what an attacker gains.

This is a university project: there is no bug bounty and no guaranteed response time.

## Supported versions

Only the [latest release](https://github.com/manuelgcn/worldcup-planner/releases/latest) is
supported. Older releases are not patched.

## Demo defaults are not vulnerabilities

The project ships as a demo that runs on one machine. These values are public on purpose, so
do not make the app or its database reachable from another machine while they are in place:

- The three accounts created at startup (`admin@copa.com`, `editor@copa.com`,
  `periodista@copa.com`) and their passwords, which are in the README and on the login page.
  Setting `Demo:Enabled` to false stops creating them and hides them from the login page.
- The database `sa` password in `.env.example` and in `appsettings.json`.

Known weaknesses that are already tracked are listed under "Security" in [TODO.md](TODO.md).

## What the project enforces

- **Passwords** are stored as salted PBKDF2-SHA256 with 600,000 iterations and compared in
  constant time. They must have 8 to 128 characters with an upper-case letter, a lower-case
  letter, a digit and a symbol. A hash in an older format is replaced on the next successful login.
  Changing a password from the profile page asks for the current one, and a wrong one counts
  as a failed login.
- **Login**: an unknown email costs the same password check as a wrong password, and five failed
  logins within five minutes lock that email for five minutes. The counter is kept in memory.
- **Sessions** are kept in the browser's `sessionStorage`, encrypted with ASP.NET Core Data
  Protection, with an eight-hour expiry. On every navigation, and again before every action
  that changes or exports data, the app checks the expiry, whether that session was closed from
  another tab, and the user's current roles in the database, so a deleted user or a removed
  role takes effect there. Changing the password ends the account's other sessions.
- **Roles**: public sign-up only creates journalists, and the demo accounts are created only
  when they are missing; existing accounts are never modified at startup.
- **Database access** goes through EF Core queries only; the application builds no SQL text.
- **Audit log**: changes are recorded with their time and the email and id of the user who made
  them. Entries the system writes by itself, such as ranking updates, have no author.
- **Browser**: every response carries a Content-Security-Policy that only lets scripts load from
  the app, and forbids framing, MIME sniffing and the device APIs the app does not use.
- **Uploads**: the team CSV must be UTF-8 and at most 256 KB; a flag must be a PNG, JPEG, WebP
  or SVG image by content, not by file name.
- **CSV export** escapes cells that a spreadsheet would read as a formula.
- **Transport**: the antiforgery middleware is on, and requests are redirected to HTTPS, with
  HSTS outside Development, when the app is given an HTTPS endpoint. The container image
  listens on plain HTTP: put it behind a TLS-terminating proxy before exposing it.
- **Network exposure**: Docker Compose publishes the database and the optional app container
  on `127.0.0.1` only.
- **Container**: the image runs as a non-root user, and `/health` answers `Healthy` or
  `Unhealthy` with no further detail.
- **Dependencies and code**: Dependabot proposes updates weekly, CodeQL analyses every push
  and pull request to `main` and `develop`, and CI runs the full test suite on them.
