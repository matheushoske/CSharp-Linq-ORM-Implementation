using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemplo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var produto = new Produto();
            //produto.Id = 1;
            using (var mapa = new MapaBD())
            {
                int numero1 = 1;
                var a = mapa.Produtos.Where(c => c.Id == 1 || c.Id == 2);
                var list = a.ToList();
                ;
            }
        }
    }
}
