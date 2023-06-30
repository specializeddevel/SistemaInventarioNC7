using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Model
{
    public class Producto
    {

        [Key] public int Id { get; set; }

        [Required(ErrorMessage = "Número de Serie es un campo requerido")]
        [MaxLength(60)]
        public string NumeroSerie { get; set; }

        [Required(ErrorMessage = "Descripción es un campo requerido")]
        [MaxLength(60)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Precio es un campo requerido")]
        public double Precio { get; set; }

        [Required(ErrorMessage = "Costo es un campo requerido")]
        public double Costo { get; set; }

        public string ImagenURL { get; set; }

        [Required(ErrorMessage = "Estado es un campo requerido")]
        public bool Estado { get; set; }

        [Required(ErrorMessage = "Categoría es un campo requerido")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        [Required(ErrorMessage = "Marca es un campo requerido")]
        public int MarcaId { get; set; }

        [ForeignKey("MarcaId")]
        public Marca Marca { get; set;}

        public int? PadreId { get; set; }
        public virtual Producto Padre { get; set; }

    }
}
