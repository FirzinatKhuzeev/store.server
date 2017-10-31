namespace StoreServer
{
    using System;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [EnableCors("AllowAll")]
    public class ProductController : Controller
    {
        private readonly IDocumentDbRepository productRepository;

        public ProductController(IDocumentDbRepository repository)
        {
            productRepository = repository;
        }

        [HttpGet]
        [Route("all")]
        public IActionResult Get()
        {
            var persons = productRepository.GetAsync<Product>().Result;
            return Ok(persons);
        }

        [HttpGet("{id}")]
        [ValidateProductExistsFilter]
        public IActionResult Get(string id)
        {
            var person = productRepository.GetAsync<Product>(id).Result;
            return Ok(person);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Product productModel)
        {
            productModel.Id = Guid.NewGuid();
            var person = productRepository.PostAsync(productModel).Result;
            return Ok(person);
        }

        [HttpPut("{id}")]
        [ValidateProductExistsFilter]
        public IActionResult Put(string id, [FromBody] Product productModel)
        {
            var person = productRepository.PutAsync(id, productModel);
            return Ok(person.Result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var res = productRepository.DeleteAsync<Product>(id);
            return Ok(res.Status);
        }
    }
}
