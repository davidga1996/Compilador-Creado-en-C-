using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ChumpePP
{
    class Archivos
    {

        public static String Direccion = null;


        public static Boolean Guardar(List<string> Sintaxis , Boolean redir = false)
        {

            if (Direccion == null || redir == true)
            {
                SaveFileDialog save_ = new SaveFileDialog();
                save_.Filter = "(.chumpe)|*.chumpe";


                if (save_.ShowDialog() == DialogResult.OK)
                {
                    if (!save_.CheckFileExists)
                    {
                        Direccion = save_.FileName;
                        return CrearArchivo(Sintaxis);
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("Este archivo ya existe desea sobre-escribir ?"
                            , "UN CHUMPE EXISTENTE", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            Direccion = save_.FileName;
                            return CrearArchivo(Sintaxis);
                        }
                    }
                }
            }
            else
            {
               return  CrearArchivo(Sintaxis);
            }
            
            return false;
        }


        public static string[] AbrirArchivo()
        {
            try
            {
                OpenFileDialog openD = new OpenFileDialog();
                openD.ShowDialog();
                openD.Filter = "(.chumpe)|*.chumpe";
                if (openD.CheckFileExists)
                {
                    Direccion = openD.FileName;
                    return LeerArchivo();
                }
                
            }
            catch { }

            return null;
        }


        private static string[] LeerArchivo()
        {
            List<string> listado = new List<string>();
            string[] arch = null;
            try
            {
                StreamReader read = new StreamReader(Direccion);
                while (!read.EndOfStream)
                {
                    listado.Add(read.ReadLine());
                }
                arch = listado.ToArray();
            }
            catch {
            }
            return arch;
        }


        private static Boolean CrearArchivo(List<string> sintaxis)
        {
            try
            {
               File.Create(Direccion).Close();
               StreamWriter escritura = new StreamWriter(Direccion, false);
               foreach(string data in sintaxis)
               {
                   escritura.WriteLine(data);
               }
               escritura.Close();
            }
            catch {
            }
            return false;
        }




    }
}
