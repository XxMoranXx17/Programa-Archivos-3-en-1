using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;

namespace WindowsServerCarpetas
{
    partial class Archivos : ServiceBase

    {
        bool bandera = false;
        static string NombreConcatenado = "";
        static string LastLine = "";
        //Direccion de las carpetas de entrada y salida
        static string[] Configuracion = File.ReadAllLines(@"C:\Windows\ConfiguracionCarpetas");
        //File en donde se gurdan logs
        //StreamWriter sw = new StreamWriter(Configuracion[2].Replace("Dirección de Salida Log:", "") + "\\" + GetFileName() + ".txt");
        string SourceFileName = Configuracion[0].Replace("Dirección de Carga:", "");
        string DestineFileName = Configuracion[1].Replace("Dirección de Entrada:", "");
        string DestineFileLog = Configuracion[2].Replace("Dirección de Salida Log:", "");
        List<string> NombreSplit = Configuracion[3].Split(',').ToList();
        List<string> CadenaLog = new List<string>();
        static string NombreLog = GetFileName();

        public Archivos()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            stLapso.Start();
        }

        protected override void OnStop()
        {
            stLapso.Stop();
        }

        private void stLapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                EventLog.WriteEntry("Se inicio proceso de copiado", EventLogEntryType.Information);
                if (Directory.GetFiles(SourceFileName).Length != 0)
                {
                    MoverArchivos(Directory.GetFiles(SourceFileName).Length);
                }
                else
                {
                    Thread.Sleep(60000);
                }


            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
                //sw.Close();
            }

            bandera = false;

        }
    

 

        public void MoverArchivos(int Cantidad)
        {

            string Cadena = "";
            bool BanderaUnica = true;
            //Lista que utilizamos para juntar los datos dentro de los archivos
            List<String> Concatenar = new List<String>();
            //Muestra la direccion de todos los archivos dentro de la carpeta de entrada
            string[] files = Directory.GetFiles(SourceFileName, "*");
            //Se reccore toda la carpeta
            for (int i = 0; i < Cantidad; i++)
            {

                try
                {
                    //Se obtiene el nombre para verificar si la tenemos que concatenar
                    string NombreArchivo = files[i].Replace(Configuracion[0].Replace("Dirección de Carga:", ""), "").Replace("\\", "");
                    bool ValidarNombre = NombreSplit.Contains(NombreArchivo);


                    if (ValidarNombre)
                    {
                        if (BanderaUnica)
                        {
                            BanderaUnica = false;
                            NameEmbosser(NombreArchivo);
                        }
                        List<string> array = ConcatenateFiles(files[i]);
                        for (int j = 0; j < array.Count; j++)
                        {
                            //Agreamos a la lista linea por linea
                            Concatenar.Add(array[j]);
                        }
                        //Se sobreescribe sobre el log y se elimina de la carpeta de entrada
                        File.Delete(files[i]);
                        CadenaLog.Add(DateTime.Now + " - " + "Se movio el siguiente archivo: " + files[i]);
                    }
                    else
                    {
                        //Direccion donde este mi archivo -> Direccion donde va a para mi archivo
                        File.Copy(files[i], DestineFileName + files[i].Replace(Configuracion[0].Replace("Dirección de Carga:", ""), ""));
                        //Se sobreescribe sobre el log y se elimina de la carpeta de entrada
                        File.Delete(files[i]);
                        CadenaLog.Add(DateTime.Now + " - " + "Se movio el siguiente archivo: " + files[i]);
                    }
                }
                catch (Exception ex)
                {
                    CadenaLog.Add(DateTime.Now + " - " + "Exception: " + ex.Message);
                }
            }
            int ConcatenarCount = Concatenar.Count();
            //Agregamos la ultima linea del nuevo archivo 
            Concatenar.Add(LastLineModify(LastLine, ConcatenarCount));
            //Creamos el nuevo archivo concatenado 
            System.IO.File.WriteAllLines(DestineFileName + "\\" + NombreConcatenado, Concatenar);
            CadenaLog.Add(DateTime.Now + " - " + "Se creo un nuevo archivo: " + DestineFileName + "\\" + NombreConcatenado);
            System.IO.File.WriteAllLines(DestineFileLog + "\\" + NombreLog, CadenaLog);


        }
        static string GetFileName()
        {
            string nombre = "";
            nombre = "Log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + "_" + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
            return nombre;
        }

        //Leemos lo que este dentro de un archivo y agregamos a una lista ommitiendo la ultima linea en todos los casos
        static List<string> ConcatenateFiles(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            List<String> salida = new List<String>();

            for (int i = 0; i < lines.Length - 1; i++)
            {
                salida.Add(lines[i]);
            }
            LastLine = lines[lines.Length - 1];
            return salida;
        }
        //Se crea la nueva linea dependiendo su nuemero de lineas y su nuevo nombre
        static string LastLineModify(string lastline, int Cant)
        {
            string NumerCount = "000000000" + (Cant + 1).ToString();
            string FirstLine = lastline.Substring(0, 17) + NombreConcatenado + ".TXT         " + NumerCount.Substring(NumerCount.Length - 9, 9);
            return FirstLine;
        }
        static void NameEmbosser(string nombre)
        {
            if (nombre.Length == 14)
            {
                NombreConcatenado = "G510000CEMBOSS";
            }
            else
            {
                NombreConcatenado = "G510000CEMBOSS" + nombre.Substring(14, 2);
            }
        }

       
    }
}
