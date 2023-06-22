using SistemaInventario.Data.Data;
using SistemaInventario.Data.Repositorio.IRepositorio;
using SistemaInventario.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Data.Repositorio
{
    public class CategoriaRepositorio : Repositorio<Categoria>, ICategoriaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public CategoriaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Actualizar(Categoria categoria)
        {
            //captura el registro a actualizar
            var CategoriaBD = _db.Categorias.FirstOrDefault(b => b.Id == categoria.Id);
            if (CategoriaBD != null)
            {
                CategoriaBD.Nombre = categoria.Nombre;
                CategoriaBD.Descripcion = categoria.Descripcion;
                CategoriaBD.Estado = categoria.Estado;
                _db.SaveChanges();
            }
        }
    }
}
