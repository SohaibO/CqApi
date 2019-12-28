﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SW.PrimitiveTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi;

namespace SW.CqApi
{

    [CqApiExceptionFilter]
    [Route("cqapi")]
    [ApiController]
    public class CqApiController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<CqApiController> logger;

        public CqApiController(IServiceProvider serviceProvider, ILogger<CqApiController> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        //[HttpGet]
        //public IActionResult ListModels()
        //{
        //    var sd = serviceProvider.GetRequiredService<ServiceDiscovery>();
        //    return new OkObjectResult(sd.ListModels());
        //}

        //[HttpOptions("{resourceName}")]
        //public IActionResult ListServicesForModel(string resourceName)
        //{
        //    Response.Headers.Add("X-Total-Count", "20");

        //    return Ok();
        //    //return new add(serviceDiscovery.GetServices(resourceName));
        //}

        //[HttpGet("{resourceName}/_lookup")]
        //public async Task<IActionResult> LookupList(string resourceName,
        //    [FromQuery(Name = "search")] string searchPhrase,
        //    [FromQuery(Name = "filter")] string[] filters,
        //    [FromQuery(Name = "sort")] string[] sorts,
        //    [FromQuery(Name = "size")] int pageSize,
        //    [FromQuery(Name = "page")] int pageIndex,
        //    [FromQuery(Name = "count")] bool countRows
        //    )
        //{
        //    var mapiService = serviceProvider.ResolveService(typeof(ILookable<>), resourceName, "LookupList");

        //    var searchyRequest = new SearchyRequest(filters, sorts, pageSize, pageIndex, countRows);

        //    return Ok(await (dynamic)mapiService.Method.Invoke(mapiService.Instance, new object[] { searchPhrase, searchyRequest }));
        //}

        //[HttpGet("{resourceName}/_lookup/{key}")]
        //public async Task<IActionResult> LookupValue(string resourceName, string key)
        //{
        //    var mapiService = serviceProvider.ResolveService(typeof(ILookable<>), resourceName, "LookupValue");

        //    var result = await (dynamic)mapiService.Method.Invoke(mapiService.Instance, new object[] { key });
        //    if (result == null) return NotFound();
        //    return Ok(result);
        //}

        [HttpGet("swagger.json")]
        public ActionResult<string> GetOpenApiDocument()
        {
            var sd = serviceProvider.GetService<ServiceDiscovery>();
            return Ok(sd.GetOpenApiDocument());
        }

        //[HttpGet("{resourceName}/_filter")]
        //public async Task<IActionResult> GetFilterConfigs(string resourceName)
        //{
        //    var mapiService = serviceProvider.ResolveService(resourceName, "get/key");

        //    return Ok(await (dynamic)mapiService.Method.Invoke(mapiService.Instance, null));
        //}

        [HttpGet("{resourceName}")]
        public async Task<IActionResult> Search(
            string resourceName,
            [FromQuery(Name = "filter")] string[] filters,
            [FromQuery(Name = "sort")] string[] sorts,
            [FromQuery(Name = "size")] int pageSize,
            [FromQuery(Name = "page")] int pageIndex,
            [FromQuery(Name = "count")] bool countRows,
            [FromQuery(Name = "search")] string searchPhrase,
            [FromQuery(Name = "lookup")] bool lookup)
        {
            var handlerInfo = serviceProvider.ResolveHandler(resourceName, "get");

            var searchyRequest = new SearchyRequest(filters, sorts, pageSize, pageIndex, countRows);

            var result = await (dynamic)handlerInfo.Method.Invoke(handlerInfo.Instance, new object[] { searchyRequest, lookup, searchPhrase });

            return Ok(result);
        }

        [HttpGet("{resourceName}/{key}")]
        public async Task<IActionResult> Get(string resourceName, string key, [FromQuery(Name = "lookup")] bool lookup)
        {
            var handlerInfo = serviceProvider.ResolveHandler(resourceName, "get/key");

            var param = Object.ConvertValue(key, handlerInfo.ArgumentTypes.First());

            var result = await (dynamic)handlerInfo.Method.Invoke(handlerInfo.Instance, new object[] { param, lookup });

            if (result == null) return NotFound(key);

            return Ok(result);
        }

        [HttpPost("{resourceName}")]
        public async Task<IActionResult> Post(string resourceName, [FromBody]object body)
        {
            var handlerInfo = serviceProvider.ResolveHandler(resourceName, "post");

            var typedParam = JsonConvert.DeserializeObject(body.ToString(), handlerInfo.ArgumentTypes.First());
            //new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = await (dynamic)handlerInfo.Method.Invoke(handlerInfo.Instance, new object[] { typedParam });

            if (result == null) return NoContent();

            return Ok(result);
            //return Created(new Uri(new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}"), $"/mapi/{resourceName}/{key}"), null);
            //new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}"),
        }

        [HttpPut("{resourceName}/{key}")]
        public async Task<IActionResult> Put(string resourceName, string key, [FromBody]object body)
        {

            var handlerInfo = serviceProvider.ResolveHandler(resourceName, "put/key");

            var keyParam = Object.ConvertValue(key, handlerInfo.ArgumentTypes[0]);
            var typedParam = JsonConvert.DeserializeObject(body.ToString(), handlerInfo.ArgumentTypes[1]);
            //new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = await (dynamic)handlerInfo.Method.Invoke(handlerInfo.Instance, new object[] { keyParam, typedParam });

            if (result == null) return NoContent();

            return result;// (new Uri(new Uri($"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}"), $"/mapi/{resourceName}/{key}"), await svc.Create(typedParam));
        }

        [HttpPost("{resourceName}/{key}/{command}")]
        public async Task<IActionResult> PostWithKey(string resourceName, string key, string command, [FromBody]object body)
        {

            var handlerInfo = serviceProvider.ResolveHandler(resourceName, $"post/key/{command}");

            var keyParam = Object.ConvertValue(key, handlerInfo.ArgumentTypes[0]);
            var typedParam = JsonConvert.DeserializeObject(body.ToString(), handlerInfo.ArgumentTypes[1]);
            //new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = await (dynamic)handlerInfo.Method.Invoke(handlerInfo.Instance, new object[] { keyParam, typedParam });

            if (result == null) return NoContent();

            return result;// (new Uri(new Uri($"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}"), $"/mapi/{resourceName}/{key}"), await svc.Create(typedParam));
        }

        //[HttpDelete("{resourceName}/{key}")]
        //public async Task<IActionResult> Delete(string resourceName, string key)
        //{
        //    var mapiService = serviceProvider.ResolveService(typeof(IUpdatable<>), resourceName, "Delete");

        //    await (dynamic)mapiService.Method.Invoke(mapiService.Instance, new object[] { key });

        //    return NoContent();// (new Uri(new Uri($"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}"), $"/mapi/{resourceName}/{key}"), await svc.Create(typedParam));
        //}


        T GetFromQueryString<T>() where T : new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var valueAsString = Request.Query[ property.Name].FirstOrDefault();
                var value = Object.ConvertValue(valueAsString, property.PropertyType);

                if (value == null)
                    continue;

                property.SetValue(obj, value, null);
            }
            return obj;
        }

    }
}

