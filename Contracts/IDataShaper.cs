using Entities.Models;
using System.Dynamic;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<ShapedEntity> ShapedData(IEnumerable<T> entities, string fieldsString);
        ShapedEntity ShapedData(T entity, string fieldsString);
    }
}
