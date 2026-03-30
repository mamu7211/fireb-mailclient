# Job Scheduling Consolidation

## Problem

Two parallel Quartz-based job scheduling systems exist:

| Aspect | `IImapSyncScheduler` | `IJobSettingsScheduler` |
|--------|---------------------|------------------------|
| Scope | IMAP-specific | Generic |
| Schedule type | Interval (`WithSimpleSchedule`) | Cron (`WithCronSchedule`) |
| Execution tracking | None | Full (`JobExecution` entity) |
| Auto-disable | None | After 3 consecutive failures |
| Admin UI | Not visible | JobDetail page |
| Job instances | One per mailbox (dynamic) | One per type (static) |

This creates code duplication, inconsistent behavior, and prevents users from managing their sync jobs.

## Solution

Consolidate into one system: all jobs run through `ManagedJob` infrastructure.

### Data Model Changes

`JobSettings` gains:

| Field | Type | Purpose |
|-------|------|---------|
| `JobType` | `string` (required, max 50) | Registry lookup key (e.g. `"imap-sync"`, `"classification"`) |
| `UserId` | `Guid?` | null = system job, set = user-scoped |
| `ResourceId` | `Guid?` | Generic FK to owning resource |
| `ResourceType` | `string?` (max 50) | e.g. `"Mailbox"` |

- Index on `(JobType, ResourceId)`
- Optional FK `UserId -> User` with `DeleteBehavior.SetNull`
- `JobName` stays unique and parseable (e.g. `imap-sync:{mailboxId}`) but is no longer the registry key

### Registry Change

`ManagedJobRegistry` maps `JobType -> CLR Type` (was `JobName -> Type`). One `JobType` can have many `JobSettings` rows (one per mailbox for IMAP sync).

### API Change

Move from `/api/admin/jobs` to `/api/jobs`:
- Admin sees all jobs
- User sees own jobs + system jobs (where `UserId == null`)
- Admin can edit any job, user can edit their own
- New endpoint: `GET /api/jobs/by-resource/{resourceType}/{resourceId}`

### Mailbox Lifecycle

- **Create mailbox:** auto-create `JobSettings` (`JobType = "imap-sync"`, `Cron = "0 */5 * * * ?"`, bound to user and mailbox)
- **Delete mailbox:** unschedule + remove `JobSettings` and executions (code-side, not DB cascade, since `ResourceId` is generic)
- **Update mailbox:** schedule changes go through job settings UI, not mailbox edit

### UI Component: `JobSettingsPanel`

Reusable Blazor component, parameters: `ResourceType`, `ResourceId`.

Contents:
- `ToggleButtonGroup` with schedule presets (1min, 5min, 15min, hourly, custom)
- Custom cron input (when "custom" selected)
- Enable/disable toggle
- Compact status: last run (relative time), last status (badge)
- Up to 10 green/red dots for recent execution results

Full execution log stays on the admin `JobDetail` page only.

## Implementation Phases

### Phase 1: Schema Evolution
- Add fields to `JobSettings` entity
- Configure in `FeirbDbContext` (index, FK, seed update)
- EF Core migration

### Phase 2: Registry Refactor
- `ManagedJobRegistry`: `HasJob` -> `HasJobType`, `GetJobType` -> `GetJobClrType`
- `ManagedJobExtensions`: `JobName` -> `JobTypeName`
- Update `JobSettingsScheduler`, `JobService`
- Update tests

### Phase 3: Convert ImapSyncJob to ManagedJob
- Extend `ManagedJob` instead of `IJob`
- Override `RunAsync`: parse mailbox ID from job name, call `IImapSyncService`
- Register: `AddManagedJob<ImapSyncJob>("imap-sync")`

### Phase 4: Mailbox Lifecycle + Remove Old Scheduler
- `MailboxEndpoints`: replace `IImapSyncScheduler` with `IJobSettingsScheduler`
- Auto-create/delete `JobSettings` on mailbox CRUD
- Delete `IImapSyncScheduler`, `ImapSyncScheduler`
- Startup safety net for orphaned mailboxes

### Phase 5: API Consolidation
- Move endpoints to `/api/jobs`
- Add role-based filtering
- Add `by-resource` endpoint
- Add `GetForUserAsync` to `IJobService`

### Phase 6: JobSettingsPanel Component
- New `JobSettingsPanel.razor`
- Embed in mailbox edit page
- Update admin pages to new API URLs
- i18n strings

### Phase 7: Cleanup
- Remove `Mailbox.PollIntervalMinutes`
- EF migration to drop column

## Out of Scope

- User-facing job list page (separate work)
- Notification on auto-disable (future dashboard widget)
