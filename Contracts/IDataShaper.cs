using System.Dynamic;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<ExpandoObject> ShapedData(IEnumerable<T> entities, string fieldsString);
        ExpandoObject ShapedData(T entity, string fieldsString);
    }
}
