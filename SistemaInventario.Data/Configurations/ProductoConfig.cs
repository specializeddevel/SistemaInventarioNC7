using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaInventario.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Data.Configurations
{
    public class ProductoConfig : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.NumeroSerie).IsRequired().HasMaxLength(60);
            builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Estado).IsRequired();
            builder.Property(x => x.Precio).IsRequired();
            builder.Property(x => x.Costo).IsRequired();
            builder.Property(x => x.CategoriaId).IsRequired();
            builder.Property(x => x.MarcaId).IsRequired();
            builder.Property(x => x.ImagenURL).IsRequired(false);
            builder.Property(x => x.PadreId).IsRequired(false);

            /*relaciones*/
            /*relacion uno a muchos categoria*/
            builder.HasOne(x => x.Categoria).WithMany()
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Marca).WithMany()
                .HasForeignKey(x => x.MarcaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Padre).WithMany()
                .HasForeignKey(x => x.PadreId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
