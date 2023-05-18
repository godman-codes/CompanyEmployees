using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CompanyEmployees.Presentation.Helper
{
    public static class PaginationHelper
    {
        public static HttpResponseMessage CreatePaginatedResponse<T>(HttpRequest request, IEnumerable<T> data, int currentPage, int pageSize)
        {
            var totalCount = data.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var previousPage = currentPage > 1 ? currentPage - 1 : (int?)null;
            var nextPage = currentPage < totalPages ? currentPage + 1 : (int?)null;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var linkHeader = new StringBuilder();

            // Add the Link header with links to the previous and next pages
            linkHeader.Append(CreateLink(request, "first", 1, pageSize));

            if (previousPage != null)
                linkHeader.Append(CreateLink(request, "prev", previousPage.Value, pageSize));

            linkHeader.Append(CreateLink(request, "self", currentPage, pageSize));

            if (nextPage != null)
                linkHeader.Append(CreateLink(request, "next", nextPage.Value, pageSize));

            linkHeader.Append(CreateLink(request, "last", totalPages, pageSize));

            response.Headers.Add("Link", linkHeader.ToString());

            return response;
        }

        private static string CreateLink(HttpRequest request, string rel, int page, int pageSize)
        {
            var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            var link = $"<{url}?page={page}&pageSize={pageSize}>; rel=\"{rel}\", ";

            return link;
        }
    }
}
