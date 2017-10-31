namespace StoreServer
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Newtonsoft.Json;

    public class DocumentDbRepositoryBase : IDocumentDbRepository
    {
        private static string databaseId = string.Empty;
        private static string collectionId = string.Empty;
        private static DocumentClient documentClient;
        private readonly CosmoDbSettings cosmoDbSettings;

        public DocumentDbRepositoryBase(IOptions<CosmoDbSettings> cosmoDbSettingSnapshot)
        {
            cosmoDbSettings = cosmoDbSettingSnapshot.Value;
            databaseId = cosmoDbSettings.DatabaseId;
            collectionId = cosmoDbSettings.CollectionId;

            documentClient = new DocumentClient(new Uri(cosmoDbSettings.Endpoint), cosmoDbSettings.Key);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await documentClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await documentClient.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await documentClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await documentClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<TModel>> GetAsync<TModel>() where TModel : class
        {
            var documents = documentClient
                .CreateDocumentQuery<TModel>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    new FeedOptions { MaxItemCount = -1 }).AsDocumentQuery();
            var persons = new List<TModel>();
            while (documents.HasMoreResults)
            {
                persons.AddRange(await documents.ExecuteNextAsync<TModel>());
            }

            return persons;
        }

        public async Task<TModel> GetAsync<TModel>(string id) where TModel : class
        {
            try
            {
                Document doc =
                    await documentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));

                return JsonConvert.DeserializeObject<TModel>(doc.ToString());
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<TModel> PostAsync<TModel>(TModel item) where TModel : class
        {
            try
            {
                var document =
                    await documentClient.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
                var res = document.Resource;
                var person = JsonConvert.DeserializeObject<TModel>(res.ToString());

                return person;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TModel> PutAsync<TModel>(string id, TModel item) where TModel : class
        {
            try
            {
                var document =
                    await documentClient.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
                var data = document.Resource.ToString();
                var person = JsonConvert.DeserializeObject<TModel>(data);

                return person;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<TModel> DeleteAsync<TModel>(string id) where TModel : class
        {
            try
            {
                var document =
                    await documentClient.DeleteDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, id));

                var res = document.Resource;
                var person = JsonConvert.DeserializeObject<TModel>(res.ToString());

                return person;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
