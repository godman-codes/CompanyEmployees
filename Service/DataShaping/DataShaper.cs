using Contracts;
using Entities.Models;
using System.Dynamic;

using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Service.DataShaping
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }
        public DataShaper()
        {
            // constructors getting the property name of the model class
            Properties = typeof(T).GetProperties(BindingFlags.Public |
                BindingFlags.Instance);
        }
        public IEnumerable<Entity> ShapedData(IEnumerable<T> entities, 
            string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchData(entities, requiredProperties);
        }

        public Entity ShapedData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchDataForEntity(entity, requiredProperties);
        }

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            // create a list of requiredproperties
            var requiredProperties = new List<PropertyInfo>();

            // check if the fieldstring is null or a white space
            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                // split by comma
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var field in fields)
                {
                    // check if the passed field is a property of the model class
                    var property = Properties
                        .FirstOrDefault(pi => pi.Name.Equals(field.Trim(),
                        StringComparison.InvariantCultureIgnoreCase));
                    if (property == null)
                        continue;
                    //add it to the list of propertyInfo
                    requiredProperties.Add(property);
                }
            }
            else
            {
                // if the fieldstring is empty or white space
                // return a list of default properties of the model class
                requiredProperties = Properties.ToList();
            }
            return requiredProperties;
        }
        private IEnumerable<Entity> FetchData(IEnumerable<T> entities,
            IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<Entity>();

            
            foreach (var entity in entities)
            {
                // fetch the custon object 
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                // add it to the list
                shapedData.Add(shapedObject);
            }
            return shapedData;
        }

        private Entity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            // new object with dynamic properties
            var shapedObject = new Entity();

            // loop through the properties to return 
            foreach (var property in requiredProperties)
            {
                // get the value of the wanted property in the entity object 
                var objectPropertyValue = property.GetValue(entity);
                // add the property name and value to the dynamic object 
                shapedObject.TryAdd(property.Name, objectPropertyValue);
            }
            return shapedObject;

        }

    }
}
