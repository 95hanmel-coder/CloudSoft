using CloudSoft.Models;
using CloudSoft.Services;
using Xunit;

namespace CloudSoft.Services.UnitTests;

public class NewsletterServiceTests
{
    private readonly INewsletterService _sut;

    public NewsletterServiceTests()
    {
        // sut = System Under Test
        _sut = new NewsletterService();
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_WithValidSubscriber_ReturnsSuccess()
    {
        var subscriber = new Subscriber { Name = "Test User", Email = "user@example.com" };
        var result = await _sut.SignUpForNewsletterAsync(subscriber);

        Assert.True(result.IsSuccess);
        Assert.Contains("Welcome to our newsletter", result.Message);
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_WithDuplicateEmail_ReturnsFailure()
    {
        var subscriber1 = new Subscriber { Name = "Test User 1", Email = "duplicate@example.com" };
        var subscriber2 = new Subscriber { Name = "Test User 2", Email = "duplicate@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber1);

        var result = await _sut.SignUpForNewsletterAsync(subscriber2);

        Assert.False(result.IsSuccess);
        Assert.Contains("already subscribed", result.Message);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_WithExistingEmail_ReturnsSuccess()
    {
        var subscriber = new Subscriber { Name = "Test User", Email = "optoutuser@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber);

        var result = await _sut.OptOutFromNewsletterAsync("optoutuser@example.com");

        Assert.True(result.IsSuccess);
        Assert.Contains("successfully removed", result.Message);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_WithNonexistentEmail_ReturnsFailure()
    {
        var result = await _sut.OptOutFromNewsletterAsync("nonexistent@example.com");

        Assert.False(result.IsSuccess);
        Assert.Contains("couldn't find your subscription", result.Message);
    }

    [Fact]
    public async Task GetActiveSubscribersAsync_ReturnsAllSubscribers()
    {
        var subscriber1 = new Subscriber { Name = "Test User 1", Email = "test1@example.com" };
        var subscriber2 = new Subscriber { Name = "Test User 2", Email = "test2@example.com" };
        await _sut.SignUpForNewsletterAsync(subscriber1);
        await _sut.SignUpForNewsletterAsync(subscriber2);

        var subscribers = await _sut.GetActiveSubscribersAsync();

        Assert.True(subscribers.Count() >= 2);
        Assert.Contains(subscribers, s => s.Email == "test1@example.com");
        Assert.Contains(subscribers, s => s.Email == "test2@example.com");
    }
}