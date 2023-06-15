using Microsoft.EntityFrameworkCore;
using SistemaInventario.Data.Data;
using SistemaInventario.Data.Repositorio.IRepositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Data.Repositorio
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {

        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repositorio(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public async Task Add(T entidad)
        {
            await dbSet.AddAsync(entidad);
        }

        public async Task<T> Get(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> filtro = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null, bool isTracking = true)
        {
            IQueryable<T> query = dbSet;
            if( filtro != null )
            {
                query = query.Where( filtro );  //Select * from where filtro
            }
            if(includeProperties != null)
            {
                foreach (var incluirProp in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incluirProp);  //ejemplo incluir: categoria, marca
                }
            }
            if(orderBy != null)
            {
                query = orderBy(query);
            }
            if(!isTracking)
            {
                query = query.AsNoTracking();
            }
            return query;
        }

        public async Task<T> GetFirst(Expression<Func<T, bool>> filtro = null, string includeProperties = null, bool isTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (filtro != null)
            {
                query = query.Where(filtro);  //Select * from where filtro
            }
            if (includeProperties != null)
            {
                foreach (var incluirProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incluirProp);  //ejemplo incluir: categoria, marca
                }
            }            
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public Task Update(T entidad)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entidad)
        {
            dbSet.Remove(entidad);
        }

        public void DeleteRange(IEnumerable<T> entidad)
        {
            dbSet.RemoveRange(entidad);
        }
        
    }
}
