using CloudSoft.Models;
using CloudSoft.Repositories;

namespace CloudSoft.Services;

public class NewsletterService : INewsletterService
{
    private readonly ISubscriberRepository _subscriberRepository;

    public NewsletterService(ISubscriberRepository subscriberRepository)
    {
        _subscriberRepository = subscriberRepository;
    }

    public async Task<OperationResult> SignUpForNewsletterAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrWhiteSpace(subscriber.Email))
        {
            return OperationResult.Failure("Invalid subscriber information.");
        }

        if (await _subscriberRepository.ExistsAsync(subscriber.Email))
        {
            return OperationResult.Failure("You are already subscribed to our newsletter.");
        }

        var success = await _subscriberRepository.AddAsync(subscriber);

        if (!success)
        {
            return OperationResult.Failure("Failed to add your subscription. Please try again.");
        }

        return OperationResult.Success($"Welcome to our newsletter, {subscriber.Name}! You'll receive updates soon.");
    }

    public async Task<OperationResult> OptOutFromNewsletterAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return OperationResult.Failure("Invalid email address.");
        }

        var subscriber = await _subscriberRepository.GetByEmailAsync(email);

        if (subscriber == null)
        {
            return OperationResult.Failure("We couldn't find your subscription in our system.");
        }

        var success = await _subscriberRepository.DeleteAsync(email);

        if (!success)
        {
            return OperationResult.Failure("Failed to remove your subscription. Please try again.");
        }

        return OperationResult.Success("You have been successfully removed from our newsletter. We're sorry to see you go!");
    }

    public async Task<IEnumerable<Subscriber>> GetActiveSubscribersAsync()
    {
        var subscribers = await _subscriberRepository.GetAllAsync();
        
        // Convert to a concrete List to satisfy the View's model requirements
        return subscribers.ToList();
    }
}