using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Data.Repositorio.IRepositorio
{
    public interface IRepositorio<T> where T : class
    {
        Task<T> Get(int id);

        Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>> filtro = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null,
            bool isTracking = true
        );

        Task<T> GetFirst(
            Expression<Func<T, bool>> filtro = null,
            string includeProperties = null,
            bool isTracking = true
        );

        Task Add( T entidad );        
        void Delete( T entidad );
        void DeleteRange(IEnumerable<T> entidad );

    }
}
