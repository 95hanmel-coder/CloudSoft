using CloudSoft.Models;
using CloudSoft.Repositories;
using CloudSoft.Services;
using Xunit; 

namespace CloudSoft.Tests;

public class NewsletterServiceTests
{
    private readonly NewsletterService _sut;
    private readonly InMemorySubscriberRepository _repository;

    public NewsletterServiceTests()
    {
        // Fresh instance for each test to prevent data leakage between tests
        _repository = new InMemorySubscriberRepository();
        _sut = new NewsletterService(_repository);
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_ValidSubscriber_ReturnsSuccess()
    {
        var subscriber = new Subscriber { Name = "Test User", Email = "test@test.com" };

        var result = await _sut.SignUpForNewsletterAsync(subscriber);

        Assert.True(result.IsSuccess);
        Assert.Contains("Welcome", result.Message);
    }

    [Fact]
    public async Task SignUpForNewsletterAsync_DuplicateEmail_ReturnsFailure()
    {
        var subscriber = new Subscriber { Name = "Test User", Email = "test@test.com" };
        await _sut.SignUpForNewsletterAsync(subscriber); 

        var result = await _sut.SignUpForNewsletterAsync(subscriber);

        Assert.False(result.IsSuccess);
        Assert.Contains("already subscribed", result.Message);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_ExistingSubscriber_ReturnsSuccess()
    {
        var subscriber = new Subscriber { Name = "Test User", Email = "test@test.com" };
        await _sut.SignUpForNewsletterAsync(subscriber);

        var result = await _sut.OptOutFromNewsletterAsync("test@test.com");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OptOutFromNewsletterAsync_NonExistingSubscriber_ReturnsFailure()
    {
        var result = await _sut.OptOutFromNewsletterAsync("nobody@test.com");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetActiveSubscribersAsync_ReturnsAllSubscribers()
    {
        await _sut.SignUpForNewsletterAsync(new Subscriber { Name = "User 1", Email = "u1@test.com" });
        await _sut.SignUpForNewsletterAsync(new Subscriber { Name = "User 2", Email = "u2@test.com" });

        var result = await _sut.GetActiveSubscribersAsync();

        Assert.Equal(2, result.Count());
    }
}