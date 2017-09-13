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
using System.Security.Principal;
using TrabalhoDM106.br.com.correios.ws;
using TrabalhoDM106.CRMCliente;

namespace TrabalhoDM106.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private TrabalhoDM106Context db = new TrabalhoDM106Context();

        [ResponseType(typeof(Order))]
        [HttpPut]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(int id)
        {
            // recupera o pedido pelo ID
            Order order = db.Orders.Find(id);

            // se nao existir o pedido
            if (order == null)
            {
                return BadRequest("Pedido não cadastrado!");
            }
            else
            {

                // confere se é o admin ou o dono
                if (CheckPropOrder(User, order))
                {

                    if (order.statusPedido != "novo")
                    {
                        return BadRequest("Pedido não está com status de novo!");
                    }


                    if (order.OrderItems.Count == 0)
                    {
                        return BadRequest("Pedido não possui itens!");
                    }


                    CRMRestClient crmClient = new CRMRestClient();
                    Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);

                    // zera as medidas
                    decimal diametro = 0;
                    decimal altura = 0;
                    decimal comprimento = 0;
                    decimal largura = 0;

                    // confere se achou o cliente
                    if (customer != null)
                    {
                        order.precoTotal = 0;
                        order.pesoTotal = 0;
                        // soma medida dos itens
                        foreach (OrderItem temp in order.OrderItems)
                        {
                            // somatorio do preço
                            order.precoTotal += temp.Product.preco * temp.ProductQtd;

                            // somatorio do peso
                            order.pesoTotal += temp.Product.peso * temp.ProductQtd;


                            // -- considerando que os produtos estaram um do lado do outro, 
                            //               somo a largura e pego a maior altura , comprimento e diametro -- //

                            // somatorio da largura
                            largura += temp.Product.largura * temp.ProductQtd;

                            if (temp.Product.altura > altura)
                            {
                                altura = temp.Product.altura;
                            }
                            if (temp.Product.diametro > diametro)
                            {
                                diametro = temp.Product.diametro;
                            }
                            if (temp.Product.comprimento > comprimento)
                            {
                                comprimento = temp.Product.comprimento;
                            }

                        }

                        // Considerando que a loja esteja em São Luís - MA => Rua Vila Anselmo - 65040-101

                        CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
                        // (  string nCdEmpresa, string sDsSenha, string nCdServico, string sCepOrigem, 
                        //    string sCepDestino, string nVlPeso, int nCdFormato, decimal nVlComprimento, 
                        //    decimal nVlAltura, decimal nVlLargura, decimal nVlDiametro, string sCdMaoPropria, 
                        //    decimal nVlValorDeclarado, string sCdAvisoRecebimento) 

                        cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "65040101", customer.zip,
                            order.pesoTotal.ToString(), 1, comprimento, altura, largura, diametro,
                            "N", order.precoTotal, "S");

                        if (resultado.Servicos[0].Erro.Equals("0"))
                        {
                            // ajusta preço do frete
                            order.precoFrete = Convert.ToDecimal(resultado.Servicos[0].Valor) / 100;
                            // atualiza o preço do pedido , somando o valor do frete
                            order.precoTotal += order.precoFrete;
                            // considerar a data da entrega = data pedido mais prazo entrega
                            order.dataEntrega = order.dataPedido.AddDays(Convert.ToDouble(resultado.Servicos[0].PrazoEntrega));
                            //modificações são persistidas no banco de dados
                            db.SaveChanges();

                            return Ok("Valor do frete: R$ " + resultado.Servicos[0].Valor + " , entrega em aproximadamente " + resultado.Servicos[0].PrazoEntrega + " dia(s)");
                        }
                        else
                        {
                            return BadRequest("Ocorreu um erro " + resultado.Servicos[0].Erro + " , " + resultado.Servicos[0].MsgErro);
                        }
                    }
                    else
                    {
                        return BadRequest("Impossibilidade ou erro ao acessar o serviço de CRM!");
                    }
                }
                else
                {
                    return BadRequest("Acesso não permitido!");
                }
            }
        }

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult ObtemCEP()
        {
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);

            if (customer != null)
            {
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha ao consultar o CRM");
            }
        }

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public IQueryable<Order> GetOrders()
        {
            return db.Orders;
        }

        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (CheckPropOrder(User, order))
            {
                return Ok(order);
            }
            else
            {
                return BadRequest("Acesso não autorizado");
            }

            
        }

        // GET: api/Orders/email
        [ResponseType(typeof(Order))]
        [HttpGet]
        [Route("byemail")]
        public IHttpActionResult GetOrderEmail(String email)
        {
            IQueryable orders = db.Orders.Where(p => p.userEmail == email);
            
            if (CheckEmailOrder(User, email))
            {
                return Ok(orders);
            }
            else
            {
                return BadRequest("Acesso não autorizado!");
            }            
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }            

            order.statusPedido = "novo";
            order.pesoTotal = 0;
            order.precoFrete = 0;
            order.precoTotal = 0;
            order.dataPedido = DateTime.Now;

            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        [HttpPut]
        [Route("api/fecharOrder")]
        [ResponseType(typeof(Order))]
        public IHttpActionResult FecharOrder(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return BadRequest("Pedido não cadastrado!");
            }

            // Se nao for admin acesso nao autorizado e se nao for o dono no pedido nao autorizado
            if (CheckEmailOrder(User, order.userEmail))
            {

                // se preco do frete ainda nao calculado
                if (order.precoFrete == 0)
                {
                    return BadRequest("Pedido não pode ser finalizado, pedido não teve o preço de frete calculado!");
                }
                else
                {
                    order.statusPedido = "fechado";
                    db.Entry(order).State = EntityState.Modified;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        return BadRequest("Acesso não permitido!");
                    }
                }

            }
            else
            {
                return BadRequest("Acesso não permitido!");
            }


            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {

            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            else
            {

                if (CheckEmailOrder(User, order.userEmail))
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();
                }
                else
                {
                    return BadRequest("Acesso não permitido!");
                }
            }
            return Ok(order);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }

        private bool CheckEmailOrder(IPrincipal user, string email)
        {
            return ((user.Identity.Name.Equals(email)) || (user.IsInRole("ADMIN")));
        }

        private bool CheckPropOrder(IPrincipal user, Order order)
        {
            return ((user.Identity.Name.Equals(order.userName)) || (user.IsInRole("ADMIN")));
        }
    }
}