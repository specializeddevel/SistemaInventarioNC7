using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using SistemaInventario.Data.Repositorio.IRepositorio;
using SistemaInventario.Model;
using SistemaInventario.Utils;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriaController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;

        public CategoriaController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            //se obtine el registro a modificar o se crea una nueva instancia vacia
            Categoria categoria = new Categoria();

            if (id == null)
            {
                //se trata de un registro nuevo
                categoria.Estado = true;
                return View(categoria);
            }
            //se trata de una actualizacion a un registro existente
            categoria = await _unidadTrabajo.Categoria.Obtener(id.GetValueOrDefault()); //GetValueOrDefault() permite la recepcion del id nulo
            if (categoria == null)
            {
                //si la bodega no se encontro se retorna notfound
                return NotFound();
            }
            return View(categoria);
        }

        /******
        * creacion y actulizacion de los datos
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                if (categoria.Id == 0)
                {
                    //verificamos si el nombre de Bodega existe
                    var resultadoJson = await ValidarNombre(categoria.Nombre);
                    var jsonResult = (JsonResult)resultadoJson;                    
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if(existe)
                    {
                        TempData[DS.Error] = "Ya existe una categoria con ese nombre!";
                        return View(categoria);
                    } else {
                        await _unidadTrabajo.Categoria.Adicionar(categoria);
                        //muestra el toast con el mensaje enviado a la TempData de la vista parcial _Notificaiones
                        TempData[DS.Exitosa] = "Categoria creada exitosamente!";
                    }                    
                } else
                {
                    var resultadoJson = await ValidarNombre(categoria.Nombre, categoria.Id);
                    var jsonResult = (JsonResult)resultadoJson;
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if (existe)
                    {
                        TempData[DS.Error] = "Ya existe una categoria con ese nombre!";
                        return View(categoria);
                    }
                    else
                    {
                        _unidadTrabajo.Categoria.Actualizar(categoria);
                        TempData[DS.Exitosa] = "Categoria actualizada exitosamente!";
                    }
                }
                await _unidadTrabajo.Guardar();
                //
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar la Categoria!";
            return View(categoria);
        }


        #region API //metodos que se llaman desde JS

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Categoria.ObtenerTodos();
            return Json(new { data = todos });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var categoriaDb = await _unidadTrabajo.Categoria.Obtener(id);
            if (categoriaDb == null)
            {
                return Json(new { success = false, message = "Error al eliminar la Categoria"});
            }            
            _unidadTrabajo.Categoria.Eliminar(categoriaDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Categoria eliminada exitosamente" });
        }

        //El actionName permite que se pueda llamar con ese nombre este metodo desde JS
        //Este metodo controla que no se tenga una misma bodega con el mismo nombre
        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre, int id = 0)
        {
            var valor = false;
            var listaCategorias = await _unidadTrabajo.Categoria.ObtenerTodos();
            if (id == 0)
            {
                if (nombre != null)
                {
                    valor = listaCategorias.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
                }
            }
            else
            {
                if (nombre != null)
                {
                    //Controla que no se trate del objeto que se esta editando
                    valor = listaCategorias.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
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
