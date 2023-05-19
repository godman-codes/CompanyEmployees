using Entities.Models;
using System.Dynamic;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<Entity> ShapedData(IEnumerable<T> entities, string fieldsString);
        Entity ShapedData(T entity, string fieldsString);
    }
}
