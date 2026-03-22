# Feature 2.7: Profile Editing

## Goal

Implement the profile editing pages: Personal Information and Security & Password, with supporting API endpoints.

**Depends on:** Feature 2.6 (Breadcrumb & Drill-Down Navigation)

## Deliverables

### Personal Information (`/settings/personal-information`)

- Avatar: dashed circle placeholder (no upload functionality)
- Editable fields: username, email
- Username validation: unique, 2–100 characters, "username already taken" error is acceptable
- Email validation: unique, direct update without verification
- Save button

### Security & Password (`/settings/security`)

Two visually separated content sections (using the content section component from Feature 2.6):

**Change Password:**
- Current password, new password, confirm new password
- Submit button

**Sessions:**
- "Log out all sessions" button
- Invalidates the user's refresh token
- No additional password confirmation required (JWT auth is sufficient)

### API Endpoints

```
GET  /api/settings/profile            → current user's username and email
PUT  /api/settings/profile            → update username and/or email
PUT  /api/settings/profile/password   → change password (requires current password)
POST /api/settings/profile/logout-all → invalidate refresh token
```

- All endpoints identify the user from the JWT token (no user ID in URL)
- Admin editing other users remains at `/api/admin/users/{id}`

### DTOs (Feirb.Shared)

- Profile response, update profile request, change password request — exact shapes determined during implementation

## i18n

All new strings localized in `en-US`, `de-DE`, `fr-FR`, `it-IT`.

## Acceptance Criteria

- [ ] Personal Information page displays username and email with avatar placeholder
- [ ] Username can be changed (unique, 2–100 chars)
- [ ] Email can be changed (unique, no verification)
- [ ] Password can be changed with current password verification
- [ ] "Log out all sessions" invalidates refresh token
- [ ] Content sections are visually separated on the Security page
- [ ] Breadcrumb shows correct path
- [ ] All strings localized in 4 locales
- [ ] API validation returns appropriate error messages
