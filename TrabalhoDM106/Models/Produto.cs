using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TrabalhoDM106.Models
{
    public class Produto
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string nome { get; set; }

        public string descricao { get; set; }

        public string cor { get; set; }

        [Required(ErrorMessage = "O campo modelo é obrigatório")]
        public string modelo { get; set; }
        
        [Required(ErrorMessage = "O campo modelo é obrigatório")]
        public string codigo { get; set; }

        [Range(10, 999, ErrorMessage = "O preço deverá ser entre 10 e 999.")]
        public decimal preco { get; set; }

        [Range(10, 999, ErrorMessage = "O peso deverá ser entre 10 e 999.")]
        public decimal peso { get; set; }

        [Range(10, 999, ErrorMessage = "O altura deverá ser entre 10 e 999.")]
        public decimal altura { get; set; }

        [Range(10, 999, ErrorMessage = "O largura deverá ser entre 10 e 999.")]
        public decimal largura { get; set; }

        [Range(10, 999, ErrorMessage = "O comprimento deverá ser entre 10 e 999.")]
        public decimal comprimento { get; set; }

        [Range(10, 999, ErrorMessage = "O diametro deverá ser entre 10 e 999.")]
        public decimal diametro { get; set; }

        [StringLength(80, ErrorMessage = "O tamanho máximo da url é 80 caracteres")]
        public string Url { get; set; }

    }
}