using CloudSoft.Models;
using System.Collections.Concurrent;

namespace CloudSoft.Repositories;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    // Vi använder ConcurrentDictionary för att vara säkra på att koden inte kraschar
    // om flera användare prenumererar samtidigt (Thread safety).
    // StringComparer.OrdinalIgnoreCase gör att "Test@Hej.se" och "test@hej.se" räknas som samma.
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers = new(StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<Subscriber>> GetAllAsync()
    {
        // Vi returnerar datan som en Task för att matcha interfacet
        return Task.FromResult(_subscribers.Values.AsEnumerable());
    }

    public Task<Subscriber?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<Subscriber?>(null);
        }

        _subscribers.TryGetValue(email, out var subscriber);
        return Task.FromResult(subscriber);
    }

    public Task<bool> AddAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // TryAdd returnerar true om det gick att lägga till, 
        // false om e-postadressen redan fanns.
        return Task.FromResult(_subscribers.TryAdd(subscriber.Email, subscriber));
    }

    public Task<bool> UpdateAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        if (!_subscribers.ContainsKey(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // AddOrUpdate ser till att uppdateringen sker säkert
        _subscribers.AddOrUpdate(
            subscriber.Email,
            subscriber,
            (key, oldValue) => subscriber
        );

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        // TryRemove returnerar true om den lyckades ta bort något
        return Task.FromResult(_subscribers.TryRemove(email, out _));
    }

    public Task<bool> ExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_subscribers.ContainsKey(email));
    }
}