namespace TheatreCenter.Services.Options;

public class EmailOptions
{
    public string From { get; set; } = "testnataly@mail.ru";
    public string FromName { get; set; } = "TheatreCenter";
    public string SmtpHost { get; set; } = "smtp.mail.ru";
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = "testnataly@mail.ru";
    public string? Password { get; set; }
    public bool DisableDelivery { get; set; }
}
