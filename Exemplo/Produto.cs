using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemplo
{
    [Table("produtos")]
    internal class Produto
    {
        [Key]
        [Column(name:"id")]
        public int? Id { get; set; }

        [Column(name: "nome")]
        public string Nome { get; set; }
    }
}
