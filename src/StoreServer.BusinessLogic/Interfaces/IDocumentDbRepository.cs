namespace StoreServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDocumentDbRepository
    {
        Task<IEnumerable<TModel>> GetAsync<TModel>() where TModel : class;

        Task<TModel> GetAsync<TModel>(string id) where TModel : class;

        Task<TModel> PostAsync<TModel>(TModel item) where TModel : class;

        Task<TModel> PutAsync<TModel>(string id, TModel item) where TModel : class;

        Task<TModel> DeleteAsync<TModel>(string id) where TModel : class;
    }
}
