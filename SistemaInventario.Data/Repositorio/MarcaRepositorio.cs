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
    public class MarcaRepositorio : Repositorio<Marca>, IMarcaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public MarcaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Actualizar(Marca marca)
        {
            //captura el registro a actualizar
            var MarcaBD = _db.Marcas.FirstOrDefault(b => b.Id == marca.Id);
            if (MarcaBD != null)
            {
                MarcaBD.Nombre = marca.Nombre;
                MarcaBD.Descripcion = marca.Descripcion;
                MarcaBD.Estado = marca.Estado;
                _db.SaveChanges();
            }
        }
    }
}
