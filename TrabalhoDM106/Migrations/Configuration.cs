namespace TrabalhoDM106.Migrations
{
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TrabalhoDM106.Models.TrabalhoDM106Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
        protected override void Seed(TrabalhoDM106.Models.TrabalhoDM106Context context)
        {
            context.Produtoes.AddOrUpdate(
                p => p.Id,
                new Produto { Id = 1, nome = "Produto 01", descricao = "Descrição 01", cor = "Branco", modelo = "Modelo 01", codigo = "COD01", preco = 10, peso = 5, altura = 5, largura = 5, comprimento = 5, diametro = 5, Url = "foto01.jpg" },
                new Produto { Id = 2, nome = "Produto 02", descricao = "Descrição 02", cor = "Preto", modelo = "Modelo 02", codigo = "COD02", preco = 10, peso = 5, altura = 5, largura = 5, comprimento = 5, diametro = 5, Url = "foto02.jpg" },
                new Produto { Id = 3, nome = "Produto 03", descricao = "Descrição 03", cor = "Azul", modelo = "Modelo 03", codigo = "COD03", preco = 10, peso = 5, altura = 5, largura = 5, comprimento = 5, diametro = 5, Url = "foto03.jpg" }
            );
        }
    }
}
