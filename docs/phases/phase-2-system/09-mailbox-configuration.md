# Feature 2.9: Per-User Mailbox Configuration

## Goal

Allow users to configure personal mailboxes with IMAP and SMTP settings, including secure credential storage and connection testing.

**Depends on:** Feature 2.6 (Breadcrumb, Drill-Down, Toolbar), #41 (Data Protection Key Persistence)

## Deliverables

### Data Model

- `Mailbox` entity belonging to a `User` (FK, CASCADE delete)
- Fields: Name, EmailAddress, DisplayName (optional)
- IMAP settings: Host, Port (default 993), Username, EncryptedPassword, UseTls
- SMTP settings: Host, Port (default 587), Username, EncryptedPassword, UseTls, RequiresAuth
- No unique constraint on email address
- 0..N mailboxes per user, no limit
- Passwords encrypted via Data Protection API

### Mailbox List (`/settings/mailboxes`)

- Drill-down items showing Name + Email as subtitle
- Alphabetical sorting by name
- "Add Mailbox" button

### Mailbox Detail (`/settings/mailboxes/<guid>` and `/settings/mailboxes/new`)

Three content sections:

- **General** — Name, email address, display name (optional)
- **IMAP** — Host, port, username, password, TLS toggle, test connection button
- **SMTP** — Host, port, username, password, TLS toggle, auth required toggle, test connection button

Toolbar actions: Save (always), Delete (edit only, with modal confirmation)

### Connection Testing

- Separate test buttons in IMAP and SMTP sections
- Multi-step error reporting: DNS/TCP → TLS → Authentication → Success
- Tests use form values directly, no saved mailbox required

### API

```
GET    /api/settings/mailboxes          → List (name + email, no passwords)
POST   /api/settings/mailboxes          → Create
GET    /api/settings/mailboxes/{id}     → Detail (no passwords)
PUT    /api/settings/mailboxes/{id}     → Update (null/empty password = keep existing)
DELETE /api/settings/mailboxes/{id}     → Delete
POST   /api/mail/test/imap             → IMAP connection test (JWT required)
POST   /api/mail/test/smtp             → SMTP connection test (JWT required)
```

All settings endpoints identify the user from JWT. Passwords are never returned in responses.

### Stitch Reference

- Project: "Login Page"
- Screen: "Settings Page (Vertical Layout)" — shows mailbox list with configured accounts

## i18n

All strings localized in `en-US`, `de-DE`, `fr-FR`, `it-IT`.

## Acceptance Criteria

- [ ] User can create, read, update, and delete mailboxes
- [ ] IMAP and SMTP credentials stored encrypted
- [ ] Passwords never returned in API responses
- [ ] Connection test with multi-step error messages
- [ ] Test endpoints require JWT authentication
- [ ] Delete requires modal confirmation
- [ ] Mailbox list sorted alphabetically
- [ ] Default ports pre-filled
- [ ] CASCADE delete when user is deleted
- [ ] All strings localized in 4 locales
