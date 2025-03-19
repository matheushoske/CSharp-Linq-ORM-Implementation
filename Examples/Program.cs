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
            using (var mapa = new MapaBD())
            {
                int id = 1;
                var produtos = mapa.Produtos.Where(c => c.Id == 1 || c.Id == 2);
                var lista = produtos.ToList();
            }
        }
    }
}
