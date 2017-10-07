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

            try
            {
                GeradorResultadoService geradorResultado = new GeradorResultadoService();
                geradorResultado.Iniciar();
            }
            catch (Exception ex)
            {
                //gravar log
            }

            Console.ReadLine();
        }

    }
}
