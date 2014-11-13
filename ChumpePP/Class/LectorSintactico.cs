using System;
using System.IO;
using System.Collections;

namespace ChumpePP
{
	/// <summary>
	/// Descripcion lenguaje sintactico
	/// </summary>
	public class LectorSintaxis
	{

		private string Archivo;
		private  ArrayList Llaves = new ArrayList();
		private ArrayList  Funciones = new ArrayList();
		private ArrayList  Comentario = new ArrayList();
        private ArrayList Separadores = new ArrayList();
        
        public ArrayList GetLlaves() { return this.Llaves; }

        public ArrayList GetFunciones() { return this.Funciones; }

        public ArrayList GetComentarios() { return this.Comentario; }

        public ArrayList GetSeparadores() { return this.Separadores; }

		public LectorSintaxis(string archivo)
		{
			FileStream fs = new FileStream(archivo, FileMode.Open, FileAccess.Read);
			StreamReader sr = new StreamReader(fs);
			Archivo = sr.ReadToEnd();
			sr.Close();
			fs.Close();
			FiltrarArreglo();
		}

		public void FiltrarArreglo()
		{
			StringReader sr = new StringReader(Archivo);
			string SigLinea;

			SigLinea = sr.ReadLine();
			SigLinea = SigLinea.Trim();

			while (SigLinea != null)
			{
				if (SigLinea == "[FUNCIONES]")
				{
					SigLinea = sr.ReadLine();
					if (SigLinea != null)
						SigLinea = SigLinea.Trim();
					while (SigLinea != null && SigLinea[0] != '[' 
						)
					{
						Funciones.Add(SigLinea);
                        Funciones.Add(SigLinea.ToUpper());
						SigLinea = "";
						while (SigLinea != null && SigLinea == "")
						{
							SigLinea = sr.ReadLine();
							if (SigLinea != null)
								SigLinea = SigLinea.Trim();
						}
					}
				}

				if (SigLinea == "[CLAVES]")
				{
					
					SigLinea = sr.ReadLine();
					if (SigLinea != null)
						SigLinea = SigLinea.Trim();
					while (SigLinea != null && SigLinea[0] != '[' 
						)
					{
						Llaves.Add(SigLinea);
                        Llaves.Add(SigLinea.ToUpper());
						SigLinea = "";
						while (SigLinea != null && SigLinea == "")
						{
							SigLinea = sr.ReadLine();
							if (SigLinea != null)
							SigLinea = SigLinea.Trim();
						}
						
					}
				}

				if (SigLinea == "[COMENTARIO]")
				{
					
					SigLinea = sr.ReadLine();
					if (SigLinea != null)
						SigLinea = SigLinea.Trim();
					while (SigLinea != null && SigLinea[0] != '[' 
						)
					{
						Comentario.Add(SigLinea);

						SigLinea = "";
						while (SigLinea != null && SigLinea == "")
						{
							SigLinea = sr.ReadLine();
							if (SigLinea != null)
								SigLinea = SigLinea.Trim();
						}
						
					}
				}

                if (SigLinea == "[SEPARADORES]")
                {

                    SigLinea = sr.ReadLine();
                    if (SigLinea != null)
                        SigLinea = SigLinea.Trim();
                    while (SigLinea != null && SigLinea[0] != '['
                        )
                    {
                        Separadores.Add(@SigLinea);

                        SigLinea = "";
                        while (SigLinea != null && SigLinea == "")
                        {
                            SigLinea = sr.ReadLine();
                            if (SigLinea != null)
                                SigLinea = SigLinea.Trim();
                        }

                    }
                }

				if (SigLinea != null && SigLinea.Length > 0 && SigLinea[0] == '[')
				{
				}
				else
				{
					SigLinea = sr.ReadLine();
					if (SigLinea != null)
						SigLinea = SigLinea.Trim();
				}
			}

			Llaves.Sort();
			Funciones.Sort();
			Comentario.Sort();
            Separadores.Sort();
											
		}

		public bool IsLlave(string s)
		{
			int index = Llaves.BinarySearch(s);
			if (index >= 0)
				return true;

			return false;
		}

		public bool IsFuncion(string s)
		{
			int index = Funciones.BinarySearch(s);
			if (index >= 0)
				return true;

			return false;
		}

		public bool IsCommentario(string s)
		{
			int index = Comentario.BinarySearch(s);
			if (index >= 0)
				return true;

			return false;
		}

        public bool IsSeparador(string s)
        {

            int index = Separadores.BinarySearch(s);
            if (index >= 0)
                return true;

            return false;
        }


	}
}
