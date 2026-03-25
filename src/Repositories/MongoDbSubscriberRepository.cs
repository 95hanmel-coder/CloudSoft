using CloudSoft.Models;
using MongoDB.Driver;

namespace CloudSoft.Repositories;

public class MongoDbSubscriberRepository : ISubscriberRepository
{
    private readonly IMongoCollection<Subscriber> _subscribers;

    public MongoDbSubscriberRepository(IMongoCollection<Subscriber> subscribers)
    {
        _subscribers = subscribers;
    }

    public async Task<IEnumerable<Subscriber>> GetAllAsync()
    {
        try
        {
            return await _subscribers.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "GetAllAsync");
            return new List<Subscriber>();
        }
    }

    public async Task<Subscriber?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) return null;

        try
        {
            return await _subscribers.Find(s => s.Email == email).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "GetByEmailAsync");
            return null;
        }
    }

    public async Task<bool> AddAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email)) return false;

        try
        {
            var existingSubscriber = await GetByEmailAsync(subscriber.Email);
            if (existingSubscriber != null) return false;

            await _subscribers.InsertOneAsync(subscriber);
            return true;
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "AddAsync");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email)) return false;

        try
        {
            var result = await _subscribers.ReplaceOneAsync(
                s => s.Email == subscriber.Email,
                subscriber,
                new ReplaceOptions { IsUpsert = false });

            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "UpdateAsync");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        try
        {
            var result = await _subscribers.DeleteOneAsync(s => s.Email == email);
            return result.DeletedCount > 0;
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "DeleteAsync");
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;

        try
        {
            return await _subscribers.CountDocumentsAsync(s => s.Email == email) > 0;
        }
        catch (Exception ex)
        {
            LogErrorCode(ex, "ExistsAsync");
            return false;
        }
    }

    private void LogErrorCode(Exception ex, string methodName)
    {
        Console.WriteLine($"\n[DATABASE ERROR] in {methodName}: {ex.Message}");
        if (ex.InnerException != null) 
        {
            Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message}");
        }
    }
}