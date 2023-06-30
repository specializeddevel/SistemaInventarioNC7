using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using SistemaInventario.Data.Repositorio.IRepositorio;
using SistemaInventario.Model;
using SistemaInventario.Model.ViewModels;
using SistemaInventario.Utils;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductoController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;
        /*pemite acceso al directorio root para la s imagenes*/
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnvironment)
        {
            _unidadTrabajo = unidadTrabajo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Categoria"),
                MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Marca"),          
                PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Producto")
            };

            if (id == null)
            {
                //crear nuevo producto
                productoVM.Producto.Estado = true;
                return View(productoVM);
            }
            else
            {
                productoVM.Producto = await _unidadTrabajo.Producto.Obtener(id.GetValueOrDefault());
                if(productoVM.Producto == null)
                {
                    return NotFound();
                }
                return View(productoVM);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductoVM productoVM)
        {
            if(ModelState.IsValid)
            {
                /*Aqui se almacenaran los archivos que se envien por el formularios
                 * en este caso la foto*/
                var files = HttpContext.Request.Form.Files;
                /*obtiene el directorio raiz del servidor web*/
                string webRooTPath = _webHostEnvironment.WebRootPath;
                /*obtiene la ruta para imagenes en el servidor web*/
                string upload = webRooTPath + DS.ImagenRuta;
                /*creamos uin nuevo nombre para el archivo*/
                string fileName = Guid.NewGuid().ToString();
                

                if (productoVM.Producto.Id == 0)
                {
                    /*obtenenos su extension*/
                    string extension = Path.GetExtension(files[0].FileName).ToLower();
                    /*Es un producto nuevo*/
                    /*Se almacena la imagen mediante un steam en un nuevo archivo creadoen la carpeta del servidor
                     * con los datos generados y se actualiza el campo en la BD*/
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productoVM.Producto.ImagenURL = fileName+extension;
                    await _unidadTrabajo.Producto.Adicionar(productoVM.Producto);
                }
                else
                {
                    
                    /*Actualizacion del producto*/
                    var objProducto = await _unidadTrabajo.Producto.ObtenerPrimero(p => p.Id == productoVM.Producto.Id, isTracking: false);
                    /*Comprobamos si se esta enviando un nuevo achivo de foto*/
                    if (files.Count > 0)
                    {
                        /*obtenenos su extension*/
                        string extension = Path.GetExtension(files[0].FileName).ToLower();
                        var anterioFile = Path.Combine(upload, objProducto.ImagenURL);
                        if(System.IO.File.Exists(anterioFile))
                        {
                            System.IO.File.Delete(anterioFile);
                        }
                        using(var fileStream = new FileStream(Path.Combine(upload, fileName+extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        /*actuialza el nombre de la nueva imagen en la BD*/
                        productoVM.Producto.ImagenURL = fileName+extension;
                    }
                    else
                    {
                        /*no cambia la imagen*/
                        productoVM.Producto.ImagenURL = objProducto.ImagenURL;
                    }
                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);
                }
                await _unidadTrabajo.Guardar();
                TempData[DS.Exitosa] = "Tansacción exitosa";
                return View("Index");
            } // if not valid
            //se cargan nuevamente las listas y se retorna el modelo con las nuevas listas
            productoVM.CategoriaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Categoria");
            productoVM.MarcaLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Marcas");
            productoVM.PadreLista = _unidadTrabajo.Producto.ObtenerTodosDropdownLista("Producto");
            return View(productoVM);
        }

        

        #region API //metodos que se llaman desde JS

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Producto.ObtenerTodos(includeProperties:"Categoria,Marca");
            return Json(new { data = todos });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDb = await _unidadTrabajo.Producto.Obtener(id);
            if (productoDb == null)
            {
                return Json(new { success = false, message = "Error al eliminar el producto"});
            }

            /*eliminamos el archivo fisico de la imagen del producto*/
            string rutaUpload = _webHostEnvironment.WebRootPath + DS.ImagenRuta;
            /*La instrucción Path.Combine en C# se utiliza para combinar múltiples 
             * segmentos de una ruta de archivo o directorio en una única ruta completa
             * y válida paa cualquier sistema operativo*/
            var anteriorFile = Path.Combine(rutaUpload, productoDb.ImagenURL);
            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }

            _unidadTrabajo.Producto.Eliminar(productoDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto eliminado exitosamente" });
        }

        //El actionName permite que se pueda llamar con ese nombre este metodo desde JS
        //Este metodo controla que no se tenga mismo numero de serie para diferentes productos
        [ActionName("ValidarSerie")]
        public async Task<IActionResult> ValidarSerie(string serie, int id = 0)
        {
            var valor = false;
            var listaProductos = await _unidadTrabajo.Producto.ObtenerTodos();
            if (id == 0)
            {
                if (serie != null)
                {
                    valor = listaProductos.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim());
                }
            }
            else
            {
                if (serie != null)
                {
                    //Controla que no se trate del objeto que se esta editando
                    valor = listaProductos.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim() && b.Id != id);
                }
            }
            if (valor)
            {
                return Json(new { data = true });
            }
            return Json(new { data = false });
        }

        #endregion



    }
}
