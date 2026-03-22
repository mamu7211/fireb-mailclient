# Feature 2.10: System Settings — Outgoing SMTP

## Goal

Allow admins to view and edit the system SMTP configuration after initial setup. Used for password reset mails (#21) and future admin notifications (quota warnings, job failures).

**Depends on:** Feature 2.1 (System Setup — initial SMTP config created during setup)

## Deliverables

### Data Model

- Extend existing `SmtpSettings` entity with `FromAddress` and `FromName`
- Migration required for the two new fields
- Setup flow populates `FromAddress`/`FromName` from the created admin user

### API

```
GET /api/admin/system-settings/smtp   → Current config (without password)
PUT /api/admin/system-settings/smtp   → Update (null/empty password = keep existing)
POST /api/settings/test/smtp          → Connection test (existing endpoint)
```

All admin endpoints behind `RequireAdmin` policy. Password is never returned in responses.

### UI (`/admin/system-settings/outgoing-smtp`)

Replace the "Coming Soon" stub with a form:

- **Fields:** Host, Port, UseTls toggle, RequiresAuth toggle, Username, Password (empty, overwrite-only), FromAddress, FromName
- **Test connection button** reusing the existing SMTP test flow from mailbox configuration
- **Save button** in toolbar

### Password Handling

- Password field is always empty when loading the form
- Password is only overwritten when the admin enters a new value
- Decrypted password is never sent to the client

## i18n

All strings localized in `en-US`, `de-DE`, `fr-FR`, `it-IT`.

## Acceptance Criteria

- [ ] Admin can view current SMTP settings (without password)
- [ ] Admin can update SMTP settings
- [ ] Password only overwritten when explicitly provided
- [ ] FromAddress and FromName editable
- [ ] Setup flow populates FromAddress/FromName from admin user
- [ ] Connection test works from the settings form
- [ ] All endpoints require admin authorization
- [ ] All strings localized in 4 locales
