namespace HotelListingAPI.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<T> GetAsync(int? id);
        public Task<List<T>> GetAllAsync();
        public Task<T> AddAsync(T entity);
        public Task DeleteAsync(int id);
        public Task UpdateAsync(T entity);
        public Task<bool> Exists(int id);
    }

}
