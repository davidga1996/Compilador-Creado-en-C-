using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows;
using System.Text.RegularExpressions;

namespace ChumpePP.Class
{
    class AnalizadorLexico
    {

        public static List<string> PALABRAS_RESERVADAS = null;

        public static List<string> OPERADORES_ACEPTADOS = null;

        public static List<string> DELIMITADORES_EXPRESIONES = null;

        public static List<string> EXPRESIONES = null;

        private static List<string> ListaExpresion = new List<string>(
               new string[] {
                   "clase",
                   "entero",
                   "decimal",
                   "objeto",
                   "flotante",
                   "cadena"
               }
            );

        /// <summary>
        /// 
        /// </summary>
        private static List<string> ListaNivel = new List<string>(new string[] { "privado", "publico", "protegido", "virtual" });

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static List<string> SISTEMA_RESERVADO()
        {
            List<string> e = new List<string>();
            try
            {
                e.AddRange(OPERADORES_ACEPTADOS);
                e.AddRange(PALABRAS_RESERVADAS);
                e.AddRange(ListaNivel);
                e.AddRange(ListaExpresion);
            }
            catch { }
            return e;
        }

       
        
        /// <summary>
        /// EXPRESION <NIVEL><OBJETO | CLASE | FUNCION ><NOMBRE>
        /// </summary>
        /// <param name="expresion"></param>
        /// <returns></returns>
        public static int ANALIZAR_SINTAXIS_DENIVEL(string expresion)
        {
       
            try
            {
                string[] partir = expresion.Split(' ');
                partir = partir.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                int correcto = 0;
                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 0:
                            foreach(string nivel in ListaNivel)
                                if (partir[i] == nivel)
                                {
                                    correcto++;
                                    break;
                                }
                            break;
                        case 1:
                            foreach(string ex in ListaExpresion)
                                if (partir[i] == ex)
                                {
                                    correcto++;
                                    break;
                                }
                            break;
                        case 2:
                            foreach (string ex in ListaExpresion)
                                if (partir[i] == ex) {
                                    correcto--;
                                    break; 
                                }
                            foreach (string nivel in ListaNivel)
                                if (partir[i] == nivel)
                                {
                                    correcto--;
                                    break;
                                }
                            correcto++;
                            break;
                    }
                }
                if (correcto == 3) return 1;
                else return 0;
            }
            catch { return 0; }
        }


        /// <summary>
        //EXPRESION <TIPO><NOMBRE>;
        //EXPRESION <TIPO><NOMBRE> = <VALOR>;
        //EXPRESION <NIVEL><TIPO><NOMBRE> = <VALOR>;
        /// </summary>
        /// <![CDATA[Parche dentro de esta funcion agregado 30/10/2014]]>
        /// <param name="expresion">cadena</param>
        /// <returns></returns>
        public static int ANALIZAR_SINTAXIS_DIMENSIONALES(string expresion )
        {
            try
            {
                List<string> AnalisisDimensional = new List<string>();
                string[] t = expresion.Split(new string[] { " " } , StringSplitOptions.RemoveEmptyEntries);
                if (t.Length >= 3)
                {

                    int val = 0;
                    string[] tv;
                    string new_val = "";
                    //PARCHE PARA VARIABLES TIPO CADENA ... 
                    //ERROR GENERADO AL MOMENTO DE TENER UNA CADENA MULTIPLE...
                    if (t[0].ToLower() == "cadena")
                    {
                        new_val = "";
                        tv = new string[2];
                        tv[0] = t[0];
                        for (val = 1; val < t.Length; val++)
                            new_val +=  t[val].ToString();
                        tv[1] = new_val;
                        t = tv;
                    }
                    else if (t[1].ToLower() == "cadena")
                    {
                        new_val = "";
                        tv = new string[3];
                        tv[0] = t[0];
                        tv[1] = t[1];
                        for (val = 2; val < t.Length; val++)
                            new_val += t[val].ToString();
                        tv[2] = new_val;
                        t = tv;
                    }
                }
                //FIN DEL PARCHE

                List<string> Trozos = new List<string>();
                int delimitador = t[t.Length - 1].IndexOf(";");
                if (delimitador != -1)
                {
                    Trozos.AddRange(t);
                    Trozos[Trozos.Count - 1] = Trozos[Trozos.Count - 1].Replace(';',' ');
                    int index = -1;
                    index = t[t.Length - 1].IndexOf("=");
                    if (index >= 0)
                    {
                        int count = Trozos.Count - 1;
                        string valor = Trozos[count];
                        Trozos.RemoveAt(count);
                        string[] emplace = valor.Replace('=', ' ').Split(' ');
                        Trozos.AddRange(emplace);
                        for (int i = 0; i < Trozos.Count; i++)
                        {
                            if (Trozos[i] == "" || string.IsNullOrEmpty(Trozos[i]))
                            {
                                string antes = Trozos[i - 1];
                                Trozos[i-1] = "=";
                                Trozos[i] = antes;
                                
                                break;
                            }
                        }
                    }
                }
                else
                   return 2;

                string[] Sintaxis = Trozos.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                bool expresion_encontrada = false;
                for (int i = 0; i < Sintaxis.Length; i++)
                {
                    switch (i)
                    {
                        //ANALISIS <TIPO>
                        //<NIVEL>
                        case 0:
                            foreach (string nivel in ListaNivel)
                                if (Sintaxis[i] == nivel)
                                {
                                    AnalisisDimensional.Add("<NIVEL>");
                                    break;
                                }
                            if (AnalisisDimensional.IndexOf("<NIVEL>") == -1)
                            {
                                foreach (string tipo in ListaExpresion)
                                {
                                    if (tipo != "clase")
                                        if (Sintaxis[i] == tipo)
                                        {
                                            AnalisisDimensional.Add("<TIPO>");
                                            break;
                                        }
                                }
                            }
                            break;
                            //ANALISIS <TIPO><NOMBRE>
                            //<NIVEL><TIPO>
                        case 1:
                            expresion_encontrada = false;
                            foreach (string tipo in ListaExpresion)
                            {
                                if (tipo != "clase")
                                    if (Sintaxis[i] == tipo)
                                    {
                                        AnalisisDimensional.Add("<TIPO>");
                                        expresion_encontrada = true;
                                        break;
                                    }
                            }
                            if (!expresion_encontrada)
                            {
                                int nombre = ListaExpresion.IndexOf(Sintaxis[i]);
                                if (nombre == -1)
                                {
                                    AnalisisDimensional.Add("<NOMBRE>");
                                }
                            }
                            break;
                            //ANALISIS <TIPO><NOMBRE><=>
                            //ANALISIS <NIVEL><TIPO><NOMBRE>
                        case 2:
                            if (Sintaxis[i] == "=")
                            {
                                AnalisisDimensional.Add("<=>");
                            }
                            else
                            {
                                int nombre_ = ListaExpresion.IndexOf(Sintaxis[i]);
                                if (nombre_ == -1)
                                    AnalisisDimensional.Add("<NOMBRE>");
                            }
                            break;
                            //ANALISIS <NIVEL><TIPO><NOMBRE><=>
                        case 3:
                            if (Sintaxis[i] == "=")
                            {
                                AnalisisDimensional.Add("<=>");
                            }
                            else
                            {
                                int v = ListaExpresion.IndexOf(Sintaxis[i]);
                                if (v == -1)
                                    AnalisisDimensional.Add("<VALOR>");
                            }
                            break;
                        //ANALISIS <NIVEL><TIPO><NOMBRE><=><VALOR>
                        case 4:
                            int valor = ListaExpresion.IndexOf(Sintaxis[i]);
                            if (valor == -1)
                            {
                                AnalisisDimensional.Add("<VALOR>");

                            }
                            break;

                    }
                }

                string glue = string.Concat(AnalisisDimensional);
                switch (glue)
                {
                    case "<TIPO><NOMBRE>":
                        return 1;
                    case "<NIVEL><TIPO><NOMBRE>":
                        return 1;
                    case "<NIVEL><TIPO><NOMBRE><=><VALOR>":
                        return 1;
                    case "<TIPO><NOMBRE><=><VALOR>":
                        return 1;
                    default:
                        return 0;
                }
            }
            catch { return 0; }
          
        }


        
        /// <summary>
        /// //EXPRESION <CLASE><NOMBRE>
        /// </summary>
        /// <param name="expresion"></param>
        /// <returns></returns>
        public static int ANALIZAR_SINTAXIS_CLASE(string expresion)
        {

            string[] partir = expresion.Split(' ');
            partir = partir.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            int class_ok = 0;
            foreach (string exp in partir)
            {
                switch (exp.ToUpper())
                {
                    case "CLASE":
                        class_ok++;
                        break;
                    default:
                        foreach (string val in SISTEMA_RESERVADO())
                        {
                            if (val == exp)
                                return 0;
                        }
                        class_ok++;
                        break;
                }
            }
            if (class_ok == 2)
                return 1;
            else
                return 0;
        }


      
        /// <summary>
        ///   //EXPRESION CONDICION Si <SI><(><CONDICION><)><{><FUNCIONES><}><ENTONCES><{><FUNCIONES><}>
        /// </summary>
        /// <param name="expresiones"></param>
        /// <param name="ExpresionError"></param>
        /// <returns></returns>
        public static int ANALIZADOREXPRESIONES_RESERVADAS(Stack<string> expresiones , out string ExpresionError)
        {
            int ini_si = 0, ini_para = 0, ini_mientras = 0;
            int fin_si = 0, fin_para=0, fin_mientras = 0;
            int err = 1 , ini_entonces = 0;
            ExpresionError = "";
            while (expresiones.Count >= 1)
            {
                string pop_exp = expresiones.Pop();
                bool bandera = false;
                foreach (string delimitador in DELIMITADORES_EXPRESIONES)
                {
                    if (delimitador == pop_exp)
                    {
                        switch (delimitador)
                        {
                            case "finsi":
                                fin_si++;
                                break;
                            case "finpara":
                                fin_para++;
                                break;
                            case "finmientras":
                                fin_mientras++;
                                break;
                        }
                        bandera = true;
                    }
                }
                if (bandera != true)
                {
                    foreach (string exp in EXPRESIONES)
                    {
                        int exist = pop_exp.IndexOf(exp);
                        if (exist != -1)
                        {
                            switch (exp)
                            {
                                case "si":
                                    ini_si++;
                                    err = ANALIZAR_CONDICION_EXPRESIONES(pop_exp , 0);
                                    break;
                                case "entonces":
                                    ini_entonces ++;
                                    err = ANALIZAR_CONDICION_EXPRESIONES(pop_exp, 1);
                                    break;
                                case "para":
                                    ini_para++;
                                    err = ANALIZAR_CONDICION_EXPRESIONES(pop_exp,2);
                                    break;
                                case "mientras":
                                    ini_mientras++;
                                    err = ANALIZAR_CONDICION_EXPRESIONES(pop_exp,3);
                                    break;
                            }

                            if (err != 1)
                            {
                                ExpresionError = pop_exp;
                                return err;
                            }
                            else break;

                        }
                    }
                }
            }

            if (fin_si != ini_si) return 3;
            if (ini_entonces >= 1)
                if (ini_si != ini_entonces) return 7;
            if (fin_para != ini_para) return 4;
            if (fin_mientras != ini_mientras) return 5;
            return 1;
        }


       
        /// <summary>
        ///  //EXPRESION <DATO><=><VALOR><;>
        //CONDICION DEL TIPO DECLARADO
        /// </summary>
        /// <param name="dimensiones_analizar"></param>
        /// <param name="codigo"></param>
        /// <param name="expresion_error"></param>
        /// <param name="err_dimension"></param>
        /// <returns></returns>
        public static int ANALIZADOR__DIMENSIONES_EXACTAS(Dictionary<string, string[]> dimensiones_analizar,
            List<string> codigo , out List<string> expresion_error , out List<int> err_dimension)
        {
            expresion_error = new List<string>();
            err_dimension = new List<int>();


            //^(valor\=[a-z]{1,50}|[A-Z]{1,50}|[0-9]{1,50})\;
            foreach (KeyValuePair<string, string[]> diccionario in dimensiones_analizar)
            {
                string[] valor = diccionario.Value;
                string nombre = diccionario.Key;
                string tipo = valor[0];
                string data = valor[1];
                bool pass_valor = ANALIZAR_TIPO_DIMENSIONES(tipo, data);
                if (pass_valor)
                {
                    foreach (string code in codigo)
                    {
                        string new_code = code;
                        int d = new_code.IndexOf(tipo);
                        if (d == -1)
                        {
                            bool is_regex = Regex.IsMatch(new_code, @"^([a-z]{1,50}|[A-Z]{1,50}|[0-9]{1,50})\s?\=\s?([a-z]{1,50}|[A-Z]{1,50}|[0-9]{1,50})\;");
                            if (is_regex)
                            {
                                string[] val = new_code.Split('=');
                                string patron = @"^(" + nombre + ")";
                                bool is_n = Regex.IsMatch(val[0], patron);
                                if (is_n)
                                {
                                    bool pass_val = ANALIZAR_TIPO_DIMENSIONES(tipo, val[1].Replace(';', ' ').Replace(" ", ""));
                                    if (!pass_val)
                                    {
                                        switch (tipo)
                                        {
                                            case "entero":
                                                err_dimension.Add(2);
                                                break;
                                            case "decimal":
                                                err_dimension.Add(4);
                                                break;
                                            case "flotante":
                                                err_dimension.Add(3);
                                                break;
                                            case "cadena":
                                                err_dimension.Add(1);
                                                break;
                                            case "objeto":
                                                err_dimension.Add(5);
                                                break;
                                        }
                                        expresion_error.Add(nombre);
                                    }
                                }
                            }
                            else
                            {
                                string[] pat = new_code.Split('=');
                                if (pat.Length >= 2)
                                {
                                    string patron_exp = @"^(si|para|mientras|imprimir|leer)";
                                    if (Regex.IsMatch(pat[0], patron_exp) != true)
                                    {
                                        if (!pat[1].Contains(";"))
                                        {
                                            err_dimension.Add(6);
                                            expresion_error.Add(nombre);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    switch (tipo)
                    {
                        case "entero":
                            err_dimension.Add(2);
                            break;
                        case "decimal":
                            err_dimension.Add(4);
                            break;
                        case "flotante":
                            err_dimension.Add(3);
                            break;
                        case "cadena":
                            err_dimension.Add(1);
                            break;
                        case "objeto":
                            err_dimension.Add(5);
                            break;
                    }
                    expresion_error.Add(nombre);
                }
            }
            if (err_dimension.Count >= 1) return 12;
            else  return 1;
        }


        
        /// <summary>
        /// //INVOCA LA CONDICION SI ES DEL MISMO TIPO EN CUALQUIER PARTE DEL CONTEXTO
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="dato"></param>
        /// <returns></returns>
        private static bool ANALIZAR_TIPO_DIMENSIONES(string tipo, string dato)
        {
            switch (tipo.ToLower())
            {
                case "entero":
                    int try_int;
                    return int.TryParse(dato, out  try_int);
                case "decimal":
                    decimal try_decimal;
                    return decimal.TryParse(dato, out  try_decimal);
                case "flotante":
                    float try_float;
                    return float.TryParse(dato, out try_float);
                case "cadena":
                    return Regex.IsMatch(dato, "(^(\"\")|(\".+?\")|('.+?'))" );
                case "objeto":
                    break;

            }

            return false;
        }

        /// <summary>
        /// //EXPRESION DENTRO DE <CONDICION> 
        /// </summary>
        /// <param name="funcion"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static int ANALIZAR_CONDICION_EXPRESIONES(string funcion , int tipo)
        {
            int p1 = funcion.IndexOf("(");
            int p2 = funcion.IndexOf(")");
            if (p1 != -1 && p2 != -1)
            {
                try
                {
                    string[] delimitadores = new string[] 
                    {
                        "==",
                        ">",
                        ">=",
                        "<",
                        "<=",
                        "!",
                        "!="
                    };

                    string[] condicionales = new string[]
                                {
                                    "&&",
                                    "||"
                                };


                    string expresion = funcion.Replace(" ", "");
                    string[] partir;
                    switch (tipo)
                    {
                        case 0:
                        case 3:
                            partir = expresion.Split(condicionales, StringSplitOptions.RemoveEmptyEntries);
                            if (partir.Length >= 2)
                            {
                                int cant = partir.Length;
                                int tot = 0;
                                foreach (string trozo in partir)
                                {
                                    string[] t = trozo.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
                                    if (t.Length == 2) tot++;
                                }
                                if (tot == cant) return 1;
                                else return 7;
                            }
                            else
                            {
                                string[] simple = partir[0].Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
                                if (simple.Length == 2) return 1;
                                else return 7;
                            }
                        case 1:
                            partir = expresion.Split('{');
                            if (partir.Length >= 1 && partir.Length <= 2) return 1;
                            else return 7;
                        case 2:
                            partir = expresion.Split(';');
                            if (partir.Length == 3)
                            {
                                string[] operadores = new string[] { 
                                    ">",
                                    ">=",
                                    "<",
                                    "<="
                                 };
                                string[] incrementadores = new string[] {
                                    "++",
                                    "--"
                                };
                                int i1 = partir[0].IndexOf("=");
                                if (i1 != -1)
                                {
                                    foreach (string op in operadores)
                                    {
                                        int i2 = partir[1].IndexOf(op);
                                        if (i2 != -1)
                                        {

                                            foreach (string inc in incrementadores)
                                            {
                                                int i3 = partir[2].IndexOf(inc);
                                                if (i3 != -1) return 1;
                                            }
                                            return 9;
                                        }
                                    }
                                    return 9;
                                }
                                else return 9;
                            }
                            else return 9;
                        default:
                            break;
                    }
                }
                catch { }
                return 10;
            }
            else return 6;
        }
      
    }
}
