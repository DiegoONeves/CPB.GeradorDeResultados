using System;

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
