using CloudSoft.Models;

namespace CloudSoft.Repositories;

public interface ISubscriberRepository
{
    // Hämtar alla prenumeranter
    Task<IEnumerable<Subscriber>> GetAllAsync();

    // Hittar en specifik prenumerant via e-post
    Task<Subscriber?> GetByEmailAsync(string email);

    // Lägger till en ny prenumerant (returnerar true om det gick bra)
    Task<bool> AddAsync(Subscriber subscriber);

    // Uppdaterar en befintlig prenumerant
    Task<bool> UpdateAsync(Subscriber subscriber);

    // Tar bort en prenumerant baserat på e-post
    Task<bool> DeleteAsync(string email);

    // En snabbkoll för att se om en e-post redan finns i systemet
    Task<bool> ExistsAsync(string email);
}