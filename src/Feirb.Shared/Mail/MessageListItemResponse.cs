namespace Feirb.Shared.Mail;

public record MessageListItemResponse(
    Guid Id,
    string MailboxName,
    string? MailboxBadgeColor,
    string FromName,
    string FromEmail,
    string Subject,
    DateTimeOffset Date,
    bool HasAttachments);
