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
    public class BodegaRepositorio : Repositorio<Bodega>, IBodegaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public BodegaRepositorio(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Actualizar(Bodega bodega)
        {
            //captura el registro a actualizar
            var bodegaBD = _db.Bodegas.FirstOrDefault(b => b.Id == bodega.Id);
            if (bodegaBD != null)
            {
                bodegaBD.Nombre = bodega.Nombre;
                bodegaBD.Descripcion = bodega.Descripcion;
                bodegaBD.Estado = bodega.Estado;
                _db.SaveChanges();
            }
        }
    }
}
