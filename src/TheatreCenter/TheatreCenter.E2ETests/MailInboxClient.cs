using System.Text.RegularExpressions;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;

namespace TheatreCenter.E2ETests;

public class MailInboxClient
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _imapHost;
    private readonly int _imapPort;
    private readonly SecureSocketOptions _secureSocketOptions;

    public MailInboxClient()
    {
        _username = Environment.GetEnvironmentVariable("E2E_MAIL_USERNAME") ?? "testnataly@mail.ru";
        _password = Environment.GetEnvironmentVariable("E2E_MAIL_PASSWORD")
            ?? throw new InvalidOperationException("E2E_MAIL_PASSWORD is not configured.");
        _imapHost = Environment.GetEnvironmentVariable("E2E_MAIL_IMAP_HOST") ?? "imap.mail.ru";
        _imapPort = int.TryParse(Environment.GetEnvironmentVariable("E2E_MAIL_IMAP_PORT"), out var parsedPort)
            ? parsedPort
            : 993;
        _secureSocketOptions = SecureSocketOptions.SslOnConnect;
    }

    public async Task<string> WaitForCodeAsync(string subjectContains, DateTime sinceUtc, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 25; attempt++)
        {
            var code = await TryReadCodeAsync(subjectContains, sinceUtc, cancellationToken);
            if (!string.IsNullOrWhiteSpace(code))
            {
                return code;
            }

            await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);
        }

        throw new InvalidOperationException("Verification code was not received via email.");
    }

    private async Task<string?> TryReadCodeAsync(string subjectContains, DateTime sinceUtc, CancellationToken cancellationToken)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(_imapHost, _imapPort, _secureSocketOptions, cancellationToken);
        await client.AuthenticateAsync(_username, _password, cancellationToken);

        var folders = new List<IMailFolder> { client.Inbox };
        try
        {
            var subFolders = await client.Inbox.GetSubfoldersAsync(false, cancellationToken);
            folders.AddRange(subFolders);
        }
        catch
        {
            // ignore if subfolders cannot be loaded
        }

        foreach (var folder in folders.Where(f => f != null).Distinct())
        {
            await folder.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

            var query = SearchQuery.SubjectContains(subjectContains)
                .And(SearchQuery.DeliveredAfter(sinceUtc.AddMinutes(-30)));

            var uids = await folder.SearchAsync(query, cancellationToken);
            foreach (var uid in uids.OrderByDescending(u => u.Id).Take(25))
            {
                var message = await folder.GetMessageAsync(uid, cancellationToken);
                var body = message.TextBody ?? message.HtmlBody ?? string.Empty;

                var match = Regex.Match(body, @"\b(\d{6})\b");
                if (match.Success)
                {
                    await folder.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                    await client.DisconnectAsync(true, cancellationToken);
                    return match.Groups[1].Value;
                }
            }
        }

        await client.DisconnectAsync(true, cancellationToken);
        return null;
    }

}
