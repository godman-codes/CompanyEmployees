using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace CompanyEmployees.Presentation.Helper
{
    

    public class HttpResponseMessageCustom : IActionResult
    {
        private readonly HttpResponseMessage _response;

        public HttpResponseMessageCustom(HttpResponseMessage response)
        {
            _response = response;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            var httpContext = context.HttpContext;

            httpContext.Response.StatusCode = (int)_response.StatusCode;

            foreach (var header in _response.Headers)
                httpContext.Response.Headers.Add(header.Key, header.Value.ToArray());

            if (_response.Content != null)
            {
                foreach (var header in _response.Content.Headers)
                    httpContext.Response.Headers.Add(header.Key, header.Value.ToArray());

                var content = await _response.Content.ReadAsStringAsync();
                await httpContext.Response.WriteAsync(content);
            }
        }
    }
}
