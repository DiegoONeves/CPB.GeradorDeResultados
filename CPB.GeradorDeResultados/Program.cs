using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPB.GeradorDeResultados
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Iniciando servidor - {DateTime.Now.ToLocalTime()}");

            GeradorResultadoService geradorResultado = new GeradorResultadoService();
            geradorResultado.Iniciar();

            Console.ReadLine();
        }

    }
}
