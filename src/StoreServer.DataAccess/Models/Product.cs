namespace StoreServer
{
    using System;
    using Newtonsoft.Json;

    public class Product
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "unitPrice")]
        public int UnitPrice { get; set; }
        [JsonProperty(PropertyName = "brand")]
        public string Brand { get; set; }
        [JsonProperty(PropertyName = "category")]
        public string Category { get; set; }
        [JsonProperty(PropertyName = "sku")]
        public string SKU { get; set; }
        [JsonProperty(PropertyName = "productiondate")]
        public DateTime ProductionDate { get; set; }
    }
}
