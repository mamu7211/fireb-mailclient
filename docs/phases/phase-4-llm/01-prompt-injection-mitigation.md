# Feature 4.1: Prompt Injection Mitigation

## Goal

Protect all LLM pipelines from prompt injection attacks via email content. Emails are attacker-controlled input — any text in subject, body, or headers could attempt to hijack the LLM's behavior.

**Depends on:** Phase 3 (synced messages available for processing)

## Threat Model

### Attack Surface

- **Email subject** — short text, easy to craft injection payloads
- **Email body** (plain text and HTML-stripped) — large text field, can embed hidden instructions
- **Sender name / headers** — often displayed alongside LLM output, can inject context
- **Attachments** (future) — filenames and extracted text could carry payloads

### Attack Goals

- **Misclassification** — force the LLM to assign a specific category (e.g., mark phishing as "Important")
- **Context leakage** — trick the LLM into including content from other emails in its output
- **Output manipulation** — inject text that appears as system-generated UI (fake badges, fake warnings)
- **Reply hijacking** — manipulate smart reply drafts to include attacker-chosen content

## Deliverables

### Input Sanitization Layer

A service that preprocesses email content before it reaches the LLM prompt:

- **Delimiter enforcement** — wrap user content in clear delimiters that the system prompt instructs the LLM to treat as data, not instructions
- **Instruction stripping** — detect and neutralize common injection patterns (e.g., "ignore previous instructions", "you are now", "system:", role-switching attempts)
- **Length truncation** — cap input length to prevent context window stuffing
- **Character normalization** — normalize Unicode homoglyphs and zero-width characters that could hide instructions

### Prompt Architecture

- **System prompt hardening** — system prompts must explicitly instruct the model to treat delimited content as data only
- **Single-email context** — never include multiple emails in the same prompt to prevent cross-email leakage
- **Structured output enforcement** — require JSON output with a strict schema; reject free-text LLM responses
- **Role separation** — use clear role boundaries (system vs user) in the prompt structure

### Output Validation

- **Schema validation** — validate LLM JSON output against expected schema before use
- **Category allowlist** — only accept categories from a predefined set; reject unknown values
- **Content-length limits** — reject summaries or replies that exceed expected length bounds
- **HTML/markup stripping** — strip any markup from LLM output before rendering in the UI

### Monitoring

- **Anomaly logging** — log when input sanitization triggers (pattern detected) or output validation rejects a response
- **Classification confidence** — if the model reports low confidence, flag for manual review rather than auto-categorizing

## i18n

No user-facing strings in this feature (backend-only pipeline).

## Acceptance Criteria

- [ ] Input sanitization service exists with delimiter wrapping, pattern detection, length limits, and Unicode normalization
- [ ] System prompts use hardened templates with clear data delimiters
- [ ] Each email is processed in isolation (no multi-email context)
- [ ] LLM output is validated against JSON schema before use
- [ ] Categories are checked against an allowlist
- [ ] Injection attempts are logged for monitoring
- [ ] Unit tests cover common injection patterns (role switching, instruction override, delimiter escape)
- [ ] Integration test confirms end-to-end: injected email does not alter classification of a clean email
