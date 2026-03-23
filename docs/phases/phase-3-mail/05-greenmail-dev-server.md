# Feature 3.5: GreenMail as Dev Mail Server

## Goal

Replace Mailpit with GreenMail as the development mail server, providing IMAP + SMTP + REST API in a single container. Enables full mail workflow testing during development.

**Related issue:** #58

## Deliverables

### Aspire AppHost Changes

- Remove `CommunityToolkit.Aspire.Hosting.MailPit` NuGet package and Mailpit container
- Add GreenMail as generic container resource (`greenmail/standalone`)
- Ports:
  - SMTP: 3025
  - IMAP: 3143
  - REST API / OpenAPI UI: 8080
- Mount `./seeding/mails/` read-only into container
- Configure via `GREENMAIL_OPTS`:
  - `-Dgreenmail.preload.dir=/preload`
  - `-Dgreenmail.setup.test.all`
  - `-Dgreenmail.api.port=8080`
  - `-Dgreenmail.api.hostname=0.0.0.0`

### Mail Preloading

Git-managed directory at repo root:

```
seeding/mails/
  admin@feirb.local/
    INBOX/
      01-welcome.eml
      02-meeting.eml
      ... (10 plain text .eml files)
  alice@feirb.local/
    INBOX/
      01-project-update.eml
      02-question.eml
      ... (10 plain text .eml files)
  bob@feirb.local/
    INBOX/
      01-newsletter.eml
      02-receipt.eml
      ... (10 plain text .eml files)
```

- 10 emails per account, 30 total
- Plain text only — no HTML, no attachments
- Realistic variety (different senders, subjects, dates)
- GreenMail loads into memory at startup, no write-back to filesystem
- Bob has mail in GreenMail but no Feirb account (orphan test case)

### DB Seeder Changes

**System SMTP settings** point to GreenMail:
- Host: `localhost`, Port: `3025`, TLS: off, Auth: off
- FromAddress: `noreply@feirb.local`

**Seeded users:**
- Admin: `admin@feirb.local` / `admin@feirb.local`
- Alice: `alice@feirb.local` / `alice@feirb.local`

**Seeded mailboxes** (one per user):
- IMAP: host `localhost`, port `3143`, username = email, password = email (encrypted via Data Protection API)
- SMTP: host `localhost`, port `3025`, username = email, password = email (encrypted via Data Protection API)
- BadgeColor: distinct hex values per mailbox

### Documentation Updates

- Update `CLAUDE.md` development services section (ports, GreenMail instead of Mailpit)
- Update `README.md` and `docs/SETUP.md` if they reference Mailpit

## Acceptance Criteria

- [ ] Mailpit fully removed (container, NuGet package, all references)
- [ ] GreenMail container starts via Aspire with SMTP, IMAP, and API ports
- [ ] OpenAPI UI accessible at port 8080 from Aspire dashboard
- [ ] 30 preloaded `.eml` files loaded into GreenMail on startup
- [ ] DB seeder creates admin + alice users with mailbox entities pointing to GreenMail
- [ ] System SMTP settings point to GreenMail
- [ ] Bob's mail exists in GreenMail but is not accessible in Feirb (no account)
- [ ] Credentials stored encrypted via Data Protection API
- [ ] All documentation updated
