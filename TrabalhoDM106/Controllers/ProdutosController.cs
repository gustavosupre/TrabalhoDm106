using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrabalhoDM106.Models;

namespace TrabalhoDM106.Controllers
{
    [Authorize]
    public class ProdutosController : ApiController
    {
        private TrabalhoDM106Context db = new TrabalhoDM106Context();

        // GET: api/Produtos
        public IQueryable<Produto> GetProdutoes()
        {
            return db.Produtoes;
        }

        // GET: api/Produtos/5
        [ResponseType(typeof(Produto))]
        public IHttpActionResult GetProduto(int id)
        {
            Produto produto = db.Produtoes.Find(id);
            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        // PUT: api/Produtos/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduto(int id, Produto produto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != produto.Id)
            {
                return BadRequest();
            }

            // recupera o produto do id
            Produto temp = new TrabalhoDM106Context().Produtoes.Find(id);
            // confere se alterou os dados e se ja existe no banco um valor igual
            if (((temp.codigo != produto.codigo) && (ProdutoPorCodigo(produto.codigo))) ||
                 ((temp.modelo != produto.modelo) && (ProdutoPorModelo(produto.modelo))))
            {
                return BadRequest("Não foi possivel alterar o produto!");
            }
            else
            {
                db.Entry(produto).State = EntityState.Modified;
                db.SaveChanges();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Produtos
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Produto))]
        public IHttpActionResult PostProduto(Produto produto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (ProdutoPorCodigo(produto.codigo) || ProdutoPorModelo(produto.modelo))
            {
                return BadRequest("Não foi possivel inserir o produto!");
            }
            else
            {
                db.Produtoes.Add(produto);
                db.SaveChanges();
            }

            return CreatedAtRoute("DefaultApi", new { id = produto.Id }, produto);
        }

        // DELETE: api/Produtos/5
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Produto))]
        public IHttpActionResult DeleteProduto(int id)
        {
            Produto produto = db.Produtoes.Find(id);
            if (produto == null)
            {
                return NotFound();
            }

            db.Produtoes.Remove(produto);
            db.SaveChanges();

            return Ok(produto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProdutoExists(int id)
        {
            return db.Produtoes.Count(e => e.Id == id) > 0;
        }

        private bool ProdutoPorModelo(string modelo)
        {
            return db.Produtoes.Count(e => e.modelo == modelo) > 0;
        }

        private bool ProdutoPorCodigo(string codigo)
        {
            return db.Produtoes.Count(e => e.codigo == codigo) > 0;
        }
    }
}