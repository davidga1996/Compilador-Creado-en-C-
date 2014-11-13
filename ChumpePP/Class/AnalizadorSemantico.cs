using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Linq.Expressions;

namespace ChumpePP.Class
{
    class AnalizadorSemantico
    {

        

        private List<string> CodigoAnalizar = null;

        private List<string> OPERADORES_ = new List<string>();
        private List<string> PALABRAS_RESERVADAS_ = new List<string>();
        private List<string> FUNCIONES_ = new List<string>();
        private List<string> EXPRESIONES_ = new List<string>();
        private List<string> DELIMITADORES = new List<string>();
        private List<string> SEPARADORES_ = new List<string>();
        private List<string> ERRORES_ = new List<string>();


        private Queue<string> Dimensionales = new Queue<string>();

        /// <summary>
        /// 
        /// </summary>
        private enum ERR_SECUENCIA:int
        {
            DELIMITADOR_COMA = 2,
            EXPRESION_CIERRE_SI = 3,
            EXPRESION_CIERRE_PARA = 4,
            EXPRESION_CIERRE_MIENTRAS = 5,
            ESPRESION_PARENTESIS = 6,
            EXPRESION_SI = 7,
            EXPRESION_MIENTRAS =8,
            EXPRESION_PARA =9,
            EXPRESION_NOIDENTIFICADA = 10,
            DIMENSION_INCORRECTA = 12,


        }

        /// <summary>
        /// 
        /// </summary>
        private enum ERR_DIMENSIONES : int
        {
            DIM_NOES_CADENA = 1,
            DIM_NOES_ENTERO = 2,
            DIM_NOES_FLOTANTE =3,
            DIM_NOES_DECIMAL =4,
            DIM_NOES_OBJECT = 5,
            DIM_SIN_DELIMITADOR_PUNTOCOMA
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="CodigoEscrito">Lista cadena </param>
        public AnalizadorSemantico( List<string> CodigoEscrito = null)
        {
         
            if (CodigoEscrito != null) this.CodigoAnalizar = CodigoEscrito;

            OPERADORES_.Add("+");
            OPERADORES_.Add("=");
            OPERADORES_.Add("-");
            OPERADORES_.Add("*");
            OPERADORES_.Add("/");
            OPERADORES_.Add(">");
            OPERADORES_.Add("<");
            OPERADORES_.Add(">=");
            OPERADORES_.Add("<=");
            OPERADORES_.Add("!");
            OPERADORES_.Add("&");
            OPERADORES_.Add("%");
            OPERADORES_.Add("||");
            OPERADORES_.Add("or");
            OPERADORES_.Add("not");
            OPERADORES_.Add("and");

            PALABRAS_RESERVADAS_.Add("privado");
            PALABRAS_RESERVADAS_.Add("publico");
            PALABRAS_RESERVADAS_.Add("protegido");
            PALABRAS_RESERVADAS_.Add("virtual");
            PALABRAS_RESERVADAS_.Add("entero");
            PALABRAS_RESERVADAS_.Add("decimal");
            PALABRAS_RESERVADAS_.Add("flotante");
            PALABRAS_RESERVADAS_.Add("cadena");
            PALABRAS_RESERVADAS_.Add("arreglo");
            PALABRAS_RESERVADAS_.Add("objeto");
            PALABRAS_RESERVADAS_.Add("usar");
            PALABRAS_RESERVADAS_.Add("clase");
            PALABRAS_RESERVADAS_.Add("sistema");
            PALABRAS_RESERVADAS_.Add("romper");
            PALABRAS_RESERVADAS_.Add("continuar");
            PALABRAS_RESERVADAS_.Add("retornar");

            EXPRESIONES_.Add("si");
            EXPRESIONES_.Add("entonces");
            EXPRESIONES_.Add("mientras");
            EXPRESIONES_.Add("para");

            DELIMITADORES.Add("finsi");
            DELIMITADORES.Add("finpara");
            DELIMITADORES.Add("finmientras");

            FUNCIONES_.Add("pi");
            FUNCIONES_.Add("e");
            FUNCIONES_.Add("chumpe");
            FUNCIONES_.Add("espacio");
            FUNCIONES_.Add("sistema");
            FUNCIONES_.Add("imprimir");
            FUNCIONES_.Add("leer");

            SEPARADORES_.Add("{");
            SEPARADORES_.Add("}");
            SEPARADORES_.Add("(");
            SEPARADORES_.Add(")");

            AnalizadorLexico.OPERADORES_ACEPTADOS = this.OPERADORES_;
            AnalizadorLexico.PALABRAS_RESERVADAS = this.PALABRAS_RESERVADAS_;
            AnalizadorLexico.EXPRESIONES = this.EXPRESIONES_;
            AnalizadorLexico.DELIMITADORES_EXPRESIONES = this.DELIMITADORES;

        }

        /// <summary>
        /// Establece el codigo a analizar
        /// </summary>
        /// <param name="CodigoEscrito">lista cadena</param>
        public void SetCodigoAnalizar(List<string> CodigoEscrito)
        {
            this.CodigoAnalizar = CodigoEscrito;
        }

        /// <summary>
        ///   Computa el codigo verificando el analizador lexico y semantico
        /// </summary>
        /// <param name="CodigoComputado"></param>
        public void Computar(out List<string> CodigoComputado)
        {
            List<string> Codigo = new List<string>();
            foreach (string c in this.CodigoAnalizar)
                Codigo.Add(c.Trim());
            CodigoComputado = Codigo;
            try
            {
                this.ANALIZAR_SEPARADORES(Codigo);
                this.ANALIZAR_NOMBRE_ESPACIO(Codigo);
                this.ANALIZAR_PALABRAS_RESERVADAS(Codigo);
                this.ANALIZAR_EXPRESIONES(Codigo);
                this.ANALIZAR_DECLARACIONES(Codigo);
            }
            catch { }
        }

        /// <summary>
        /// Lista de errores en el momento de computar
        /// </summary>
        /// <returns>Lista (string)</returns>
        public List<string> MostrarErrores()
        {
            return this.ERRORES_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        private bool ANALIZAR_SEPARADORES(List<string> Codigo)
        {
            int separador_llave_izquierda = 0;
            int separador_llave_derecha = 0;
            int separador_parentesis_izquierda = 0;
            int separador_parentesis_derecha = 0;
            bool error = false;
            try
            {
                foreach (string c in Codigo)
                {
                    foreach (string separador in SEPARADORES_)
                    {
                        int index = c.IndexOf(separador);
                        if (index != -1)
                        {
                            switch (separador)
                            {
                                case "{":
                                    separador_llave_izquierda++;
                                    break;
                                case "}":
                                    separador_llave_derecha++;
                                    break;
                                case "(":
                                    separador_parentesis_izquierda++;
                                    break;
                                case ")":
                                    separador_parentesis_derecha++;
                                    break;
                            }
                        }
                    }
                }
                if (separador_llave_derecha != separador_llave_izquierda)
                {
                    this.ERRORES_.Add("Error: Ha perdido una o mas llave(s) dentro del archivo {...}");
                    error = true;
                }
                if (separador_parentesis_izquierda != separador_parentesis_derecha)
                {
                    this.ERRORES_.Add("Error: Ha perdido uno o mas parentesis dentro del archivo (...)");
                    error = true;
                }
            }

            catch { return true; }

            return error;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        private bool ANALIZAR_NOMBRE_ESPACIO(List<string> Codigo)
        {
            int espacio = 0;
            foreach (string c in Codigo)
            {
                int index = c.IndexOf("espacio");
                if (index != -1)
                {
                    espacio++;
                }
            }
            if (espacio >= 2)
            {
                this.ERRORES_.Add("Error: No se puede nombrar dos espacios en el mismo contexto { un chumpe } ");
                return true;
            }
            else if (espacio == 0)
            {
                this.ERRORES_.Add("Error: El Espacio de trabajo no ha sido nombrado { espacio chumpe } ");
                return true;
            }
            else return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        private bool ANALIZAR_PALABRAS_RESERVADAS(List<string> Codigo)
        {
            try
            {
                foreach (string C in Codigo)
                {
                    foreach (string Palabra in PALABRAS_RESERVADAS_)
                    {
                        int existe = C.IndexOf(Palabra);
                        int analisis_sentencia_correcta = -1;
                        if (existe != -1)
                        {
                            switch (Palabra)
                            {
                                case "privado":
                                case "publico":
                                case "protegido":
                                case "virtual":
                                    analisis_sentencia_correcta = AnalizadorLexico.ANALIZAR_SINTAXIS_DENIVEL(C);
                                    break;
                                case "entero":
                                case "decimal":
                                case "cadena":
                                case "flotante":
                                    analisis_sentencia_correcta = AnalizadorLexico.ANALIZAR_SINTAXIS_DIMENSIONALES(C);
                                    if(analisis_sentencia_correcta ==1)
                                        this.Dimensionales.Enqueue(C);
                                    break;
                                case "clase":
                                    //analisis_sentencia_correcta = AnalizadorLexico.ANALIZAR_SINTAXIS_CLASE(C);
                                    break;
                            }
                            if (analisis_sentencia_correcta == 0)
                            {
                                this.ERRORES_.Add("Error: cerca de la linea " + C + ", Revisa tu chumpe");
                                return true;
                            }
                            else if (analisis_sentencia_correcta >= 2)
                            {
                                switch (analisis_sentencia_correcta)
                                {
                                    case (int)ERR_SECUENCIA.DELIMITADOR_COMA:
                                        this.ERRORES_.Add("Error: No se encuentra el delimitador (;) cerca de la linea " + C);
                                        return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch { return true; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        private bool ANALIZAR_EXPRESIONES(List<string> Codigo)
        {
            try
            {
                Stack<string> PilaExpresion = new Stack<string>();
                int i = 0;
                for (i = 0; i < Codigo.Count; i++)
                {
                    foreach (string exp  in EXPRESIONES_)
                    {
                        int index = Codigo[i].IndexOf(exp);
                        int d1 = Codigo[i].IndexOf("(");
                        int d2 = Codigo[i].IndexOf(")");
                        if (index != -1 && d1 != -1 && d2 != -1)
                        {
                             PilaExpresion.Push(Codigo[i]);
                             break;
                        }
                        else if (index != -1) continue;
                        else
                        {
                            bool bandera = false;
                            foreach (string delim in DELIMITADORES)
                            {
                                int index_de = Codigo[i].IndexOf(delim);
                                if (index_de != -1)
                                {
                                    PilaExpresion.Push(Codigo[i]);
                                    bandera = true;
                                    break;
                                }
                                else bandera = false;
                            }
                            if (bandera == true) break;
                        }
                    }

                }
                string err_expresion = null;
                int valor_err = AnalizadorLexico.ANALIZADOREXPRESIONES_RESERVADAS(PilaExpresion , out err_expresion);
                switch (valor_err)
                {
                    case (int) ERR_SECUENCIA.ESPRESION_PARENTESIS:
                        this.ERRORES_.Add("Error en expresiones de parentesis cerca de " + err_expresion);
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_CIERRE_MIENTRAS:
                        this.ERRORES_.Add("Error se esperaba un cierre en el ciclo mientras");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_CIERRE_PARA:
                        this.ERRORES_.Add("Error se esperaba un cierre en el ciclo para ");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_CIERRE_SI:
                        this.ERRORES_.Add("Error se esperaba un cierre en la condicion si ");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_MIENTRAS:
                        this.ERRORES_.Add("Error en ciclo mientras se esperaba una expresion aceptada cerca de  [" + err_expresion + "]");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_PARA:
                        this.ERRORES_.Add("Error en ciclo para se esperaba una expresion aceptada cerca de  [" + err_expresion + "]");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_SI:
                        this.ERRORES_.Add("Error en la condicion si se esperaba una expresion aceptada cerca de  [" + err_expresion + "]");
                        break;
                    case (int)ERR_SECUENCIA.EXPRESION_NOIDENTIFICADA:
                        this.ERRORES_.Add("Error cerca de la expresion  [" + err_expresion + "] no se ha identificado (¿ha perdido algun chumpe?)");
                        break;
                }
                if (valor_err != 1) return true;
                else return false;
            }
            catch { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        private int ANALIZAR_DECLARACIONES(List<string> Codigo)
        {
            if (Dimensionales.Count == 0) return 1;

            Dictionary<string, string[]> tokens_analizar = new Dictionary<string, string[]>();

            string[] lista_tokens = new string[] {
                   "entero",
                   "decimal",
                   "objeto",
                   "flotante",
                   "cadena"
               };

            while (Dimensionales.Count >= 1)
            {
                string dim = Dimensionales.Dequeue();
                foreach (string tokens in lista_tokens)
                {
                    bool c = dim.Contains(tokens);
                    if (c)
                    {
                        string[] trozo = dim.Replace(tokens, "").Replace("publico", "").Replace("privado", "").Replace("protegido", "").Replace("virtual", "").Replace(" ", "").Replace(";", "").Split('=');
                        try
                        {
                            try
                            {
                                tokens_analizar.Add(trozo[0], new string[] { tokens, trozo[1] });
                            }
                            catch { tokens_analizar.Add(trozo[0], new string[] { tokens, null }); }
                        }
                        catch
                        {
                            this.ERRORES_.Add("Existe un error:  ya se ha declarado una variable con el nombre de "
                                + trozo[0] + " y de tipo " + tokens + " , Cambia el nombre  para que compile tu chumpe");
                        }
                    }
                }
            }

            List<string> expresion_error = new List<string>();
            List<int> dim_err = new List<int>();
            int err_salida = AnalizadorLexico.ANALIZADOR__DIMENSIONES_EXACTAS(tokens_analizar, Codigo ,
                out expresion_error , out dim_err);
            switch (err_salida)
            {
                case (int) ERR_SECUENCIA.DIMENSION_INCORRECTA:
                    for (int i = 0; i < dim_err.Count; i++)
                    {
                        switch (dim_err[i])
                        {
                            case (int)ERR_DIMENSIONES.DIM_NOES_CADENA:
                                this.ERRORES_.Add("el nombre [" + expresion_error[i] +
                                    "] declarado como cadena [no es de ese tipo en algun lugar del contexto] [ERR:" + dim_err[i] + "]");
                                break;
                            case (int)ERR_DIMENSIONES.DIM_NOES_DECIMAL:
                                this.ERRORES_.Add("el nombre [" + expresion_error[i] +
                                   "] declarado como decimal [no es de ese tipo en algun lugar del contexto] [ERR:" + dim_err[i] + "]");
                                break;
                            case (int)ERR_DIMENSIONES.DIM_NOES_ENTERO:
                                this.ERRORES_.Add("el nombre [" + expresion_error[i] +
                                   "] declarado como entero [no es de ese tipo en algun lugar del contexto] [ERR:" + dim_err[i] + "]");
                                break;
                            case (int)ERR_DIMENSIONES.DIM_NOES_FLOTANTE:
                                this.ERRORES_.Add("el nombre [" + expresion_error[i] +
                                   "] declarado como flotante [no es de ese tipo en algun lugar del contexto] [ERR:" + dim_err[i] + "]");
                                break;
                            case (int)ERR_DIMENSIONES.DIM_SIN_DELIMITADOR_PUNTOCOMA:
                                this.ERRORES_.Add("el nombre [" + expresion_error[i] +
                                   "] le falta el delimitador [;] en algun contexto [ERR:" + dim_err[i] + "]");
                                break;
                        }
                    }
                    break;
                case (int) ERR_SECUENCIA.EXPRESION_NOIDENTIFICADA:
                    this.ERRORES_.Add("La expresion " + expresion_error + " No ha sido identificada");
                    break;
            }
        
            return 0;
        }

    }
}
