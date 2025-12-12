using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.DTOs.Payment;
using TheatreCenter.Services.Options;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;

namespace TheatreCenter.UnitTests.Tests.E2ETests;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.E2E)]
public class PaymentE2ETests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly TheatreFixture _theatreFixture = new();
    private readonly MusicalFixture _musicalFixture = new();
    private readonly ShowFixture _showFixture = new();

    private PaymentMockServerHost? _mockServer;
    private PaymentService? _paymentService;
    private TheatreService? _theatreService;
    private MusicalService? _musicalService;
    private ShowService? _showService;
    private AppDbContext? _context;

    public PaymentE2ETests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _mockServer = await PaymentMockServerHost.StartAsync();
        _context = await _fixture.CreateTransactionalContextAsync();

        var theatreRepository = new TheatreRepository(_context);
        var musicalRepository = new MusicalRepository(_context);
        var showRepository = new ShowRepository(_context);

        _theatreService = new TheatreService(theatreRepository, new AccountRepository(_context), new NullLogger<TheatreService>());
        _musicalService = new MusicalService(musicalRepository, theatreRepository, new AccountRepository(_context), new NullLogger<MusicalService>());
        _showService = new ShowService(showRepository, musicalRepository, new NullLogger<ShowService>());

        var paymentOptions = Options.Create(new PaymentOptions
        {
            Mode = "mock",
            MockBaseUrl = _mockServer.BaseUrl
        });

        var gateway = new CloudPaymentsClient(new HttpClient(), paymentOptions, new NullLogger<CloudPaymentsClient>());
        _paymentService = new PaymentService(gateway, showRepository, paymentOptions, new NullLogger<PaymentService>());
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.RollbackTransactionAsync();
            await _context.DisposeAsync();
        }

        if (_mockServer != null)
        {
            await _mockServer.DisposeAsync();
        }
    }

    [Fact]
    public async Task PaymentScenario_Succeeds_AndRefunds()
    {
        var theatre = _theatreFixture.CreateTheatre(name: "Payment Theatre");
        var createdTheatre = await _theatreService!.CreateTheatreAsync(theatre);

        var musical = _musicalFixture.CreateMusical(title: "Payment Musical", theatreId: createdTheatre.Id);
        var createdMusical = await _musicalService!.CreateMusicalAsync(musical);

        var show = _showFixture.CreateShow(musicalId: createdMusical.Id, date: DateTime.UtcNow.AddDays(2));
        var createdShow = await _showService!.CreateAsync(show);

        var request = new PaymentRequestDto
        {
            ShowId = createdShow.Id,
            Amount = 1500m,
            Currency = "RUB",
            Description = "Test payment",
            PaymentToken = "mock-token",
            MockForceStatus = "completed"
        };

        var chargeResult = await _paymentService!.ChargeAsync(request);
        Assert.Equal(PaymentStatus.Completed, chargeResult.Status);
        Assert.False(string.IsNullOrWhiteSpace(chargeResult.TransactionId));

        var status = await _paymentService.GetStatusAsync(chargeResult.TransactionId);
        Assert.Equal(PaymentStatus.Completed, status.Status);

        var refund = await _paymentService.RefundAsync(new RefundRequestDto
        {
            TransactionId = chargeResult.TransactionId,
            Amount = request.Amount
        });

        Assert.Equal(PaymentStatus.Refunded, refund.Status);
    }

    [Fact]
    public async Task PaymentScenario_Declined_WhenForced()
    {
        var theatre = _theatreFixture.CreateTheatre(name: "Payment Theatre Decline");
        var createdTheatre = await _theatreService!.CreateTheatreAsync(theatre);

        var musical = _musicalFixture.CreateMusical(title: "Payment Musical Decline", theatreId: createdTheatre.Id);
        var createdMusical = await _musicalService!.CreateMusicalAsync(musical);

        var show = _showFixture.CreateShow(musicalId: createdMusical.Id, date: DateTime.UtcNow.AddDays(1));
        var createdShow = await _showService!.CreateAsync(show);

        var request = new PaymentRequestDto
        {
            ShowId = createdShow.Id,
            Amount = 500m,
            Currency = "RUB",
            Description = "Test decline",
            PaymentToken = "mock-token",
            MockForceStatus = "declined"
        };

        var chargeResult = await _paymentService!.ChargeAsync(request);
        Assert.Equal(PaymentStatus.Declined, chargeResult.Status);

        var status = await _paymentService.GetStatusAsync(chargeResult.TransactionId);
        Assert.Equal(PaymentStatus.Declined, status.Status);
    }
}
