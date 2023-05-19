
using System.Reflection;
using System.Text;

namespace Repository.Extensions.Utility
{
    public static class OrderQueryBuilder
    {
        public static string CreateOrderQuery<T>(string orderByQueryString)
        {
            // split the order by comma the last one may contain desc
            var orderParams = orderByQueryString.Trim().Split(',');

            // get the property name of the generic model class
            var propertyInfos = typeof(T)
                .GetProperties(BindingFlags.Public |
                BindingFlags.Instance);

            // create a new string builder
            var orderQueryBuilder = new StringBuilder();

            // loop through the list of query parameters
            foreach (var param in orderParams)
            {
                // check null
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                // split by space and get the first element beacuse 
                // the last param might contain the order description
                var propertyFromQueryName = param.Split(" ")[0];
                // check if the params property match thoes of the genric model class 
                // and map it to it 
                var objectProperty = propertyInfos.FirstOrDefault(pi =>
                pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                // check if there is no property that match the model class
                if (objectProperty == null)
                    continue;
                //check if the param ends with desc if not it will be in ascending order
                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                //append to stringBuilder
                orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {direction},"
                );
            }
            // trim the last comma
            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            return orderQuery;
        }
    }
}
