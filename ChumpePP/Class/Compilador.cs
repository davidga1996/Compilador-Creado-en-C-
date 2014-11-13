using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.CodeDom;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ChumpePP
{
    class Compilador
    {

        private enum EXPRESIONES
        {
            VARIABLE=1,
            CONDICION=2,
            CICLO_PARA=3,
            CICLO_MIENTRAS=4,
            FUNCION_IMPRIMIR=5,
            FUNCION_LEER=6,
            CHUMPE_INICIO=7
        }

        private CSharpCodeProvider provider;

        public Compilador()
        {
            this.provider = new CSharpCodeProvider();

        }

        public string GenerarCodigoCsharp(CodeCompileUnit compileunit , string Nombre_fichero)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            string sourceFile;
            if (provider.FileExtension[0] == '.')
            {
                sourceFile = Nombre_fichero + provider.FileExtension;
            }
            else
            {
                sourceFile = Nombre_fichero + "." + provider.FileExtension;
            }

            using (StreamWriter sw = new StreamWriter(sourceFile, false))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");
                provider.GenerateCodeFromCompileUnit(compileunit, tw,
                    new CodeGeneratorOptions());
                tw.Close();
            }

            return sourceFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sintactica"></param>
        /// <returns></returns>
        public CodeCompileUnit CheckCodigoAcompilar(List<string> sintactica = null)
        {


            //variables de uso frecuente
            int i = 0 , k=0;
            string patron = null;

            //eliminar sintaxis innecesaria ... 
            for (i = 0; i < sintactica.Count; i++)
            {
                sintactica.Remove("{");
                sintactica.Remove("}");
            }

            //unidad de compilacion
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace namespaces = new CodeNamespace() ;
            // declaracion del namespace
            for (i = 0; i < sintactica.Count; i++)
            {
                patron = "^(espacio)";
                if (Regex.IsMatch(sintactica[i], patron))
                {
                    string[] name = sintactica[i].Replace(";" , "").Split( new string[] {"espacio" } , StringSplitOptions.RemoveEmptyEntries);
                    namespaces = new CodeNamespace(name[name.Length-1]);
                    sintactica.RemoveAt(i);
                    break;
                }
            }
            // agregar el namespace
            compileUnit.Namespaces.Add(namespaces);
            // importar librerias que se usaran 
            for (i = 0; i < sintactica.Count; i++)//codigo para terminar , no entra el switch
            {
                patron = "^(usar)";
                if (Regex.IsMatch(sintactica[i], patron))
                {
                    string[] name = sintactica[i].Replace(";", "").ToLower().Split(new string[] { "usar" }, StringSplitOptions.RemoveEmptyEntries);
                    string data = name[name.Length - 1].Replace(" ", "");
                    switch (data)
                    {
                        case "sistema":
                            namespaces.Imports.Add(new CodeNamespaceImport("System"));
                            break;
                        case "sistema.coleccion":
                            namespaces.Imports.Add(new CodeNamespaceImport("System.Collections"));
                            break;
                        case "sistema.componentes":
                            namespaces.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
                            break;
                    }
                    sintactica.RemoveAt(i);//elimina el valor 
                    i--;//retrocede la lista ...
                }
            }
            //creamos la clase de trabajo
            CodeTypeDeclaration clase = new CodeTypeDeclaration();
            for (i = 0; i < sintactica.Count; i++)
            {
                patron = @"^(publico|privado|virtual|protegido)\s+(clase)\s+(\w+)$";
                if (Regex.IsMatch(sintactica[i].ToLower(), patron))
                {
                    string[] name = sintactica[i].Replace(";", "").ToLower().Split(
                        new string[] {"publico" , "privado" , "virtual" , "protegido" , "clase" },
                        StringSplitOptions.RemoveEmptyEntries);
                    clase = new CodeTypeDeclaration(name[name.Length - 1].Replace(" " , ""));
                    sintactica.RemoveAt(i);
                    break;
                }
            }
            //agregamos la clase de trabajo
            namespaces.Types.Add(clase);

            //se agregaran las variables globales dentro del contexto de la clase agregada
            for (i = 0; i < sintactica.Count; i++)
            {
                try
                {
                    patron = @"^(privado|publico|protegido|entero|decimal|flotante|cadena|objeto)\s+(entero|decimal|flotante|cadena|objeto|\w+)\s+(\w+)\s?(\=?)\s?(\w+?)|(['""]?)\;$";
                    bool condicion = sintactica[i].Contains("chumpe_inicio()");
                    if (Regex.IsMatch(sintactica[i].ToLower(), patron) && condicion != true)
                    {
                        List<object> variable = this.TransformarVariable(sintactica[i].ToLower(), 0);
                        //clase memebers agrega variables de tipo typeof , geenrando parametros especificos
                        clase.Members.Add( (CodeMemberField)CrearVariable(variable[2].ToString(),
                            (Type)variable[1], 0 ,
                            (MemberAttributes)variable[0] , 
                            variable[3]));
                        //utilizamos una funcion llamada crearvariable y retornamos CodeMemberField
                        sintactica.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        /*if (sintactica[i].Contains(" ") 
                            || sintactica[i].Contains("") 
                            || string.IsNullOrEmpty(sintactica[i]))
                        {
                            sintactica.RemoveAt(i);
                            i--;
                        }*/
                    }
                }
                catch { }
            }
            //aca se termina para agregar las variables globales en el contexto de datos...


            //es donde se agregara todo el codigo necesario dentro de main generado en c#
            CodeEntryPointMethod Main = new CodeEntryPointMethod();
            try
            {
                for (i = 0; i < sintactica.Count; i++)
                {
                    string sintaxis = sintactica[i];
                    switch (TipoExpresion(sintaxis))
                    {
                        case EXPRESIONES.CONDICION:
                            //SE REALIZARA UNA LISTA EN LA CUAL VEREMOS LAS CLAUSULAS QUE LE SIGUEN A LA CONDICION.
                            List<string> CondicionSintax = new List<string>();
                            int finClausula = 1 , ClausulaEncontrada=0;
                            CondicionSintax.Add(sintactica[i]);
                            //EN EL CICLO SE ENCONTRARAN LOS PATRONES QUE SEPARARA LAS CONDICIONES 
                            //DE LAS DEMAS INSTRUCCIONES QUE NO SE ANIDAN A ESTAS.
                            for (k = i+1; k < sintactica.Count; k++)
                            {
                                patron = "^(si)";
                                string patron_fin = "^(finsi)";
                                if (Regex.IsMatch(sintactica[k], patron))
                                {
                                    CondicionSintax.Add(sintactica[k]);
                                    finClausula++;
                                }
                                else if (Regex.IsMatch(sintactica[k], patron_fin))
                                {
                                    ClausulaEncontrada++;
                                    CondicionSintax.Add(sintactica[k]);
                                    if (finClausula == ClausulaEncontrada)
                                        break;
                                }
                                else
                                    CondicionSintax.Add(sintactica[k]);
                            }
                            //FINAL DEL FILTRO 
                            //ELIMINAMOS EXPRESIONES QUE SE UTILIZARON
                            for (int j = i; j < k+1; j++)
                            {
                                sintactica.RemoveAt(j);
                                j--;
                                k--;
                            }
                            //ACA EN ESTE PUNTO AGREGA LA CONDICION DE ACUERDO LAS METRICAS
                            Main.Statements.Add(CrearCondicional(CondicionSintax, clase.Members, Main.Statements));
                            i = 0;
                            break;
                        case EXPRESIONES.VARIABLE:
                            try
                            {
                                List<object> VarDum = TransformarVariable(sintaxis, 1);
                                Main.Statements.Add((CodeVariableDeclarationStatement)CrearVariable(VarDum[1].ToString(),
                                  (Type)VarDum[0], 1,
                                  MemberAttributes.Private,
                                  VarDum[2]));
                                sintactica.RemoveAt(i);
                                i--;
                               
                            }
                            catch { 
                                /*
                                 * PEQUEÑO ERROR CONTROLADO DE VARIABLES,
                                 * EN UN FUTURO SE PUEDE MEJORAR ARREGLANDO LA EXPRESION REGULAR.
                                 */
                                try
                                {
                                    List<object> VarDum = TransformarVariable(sintaxis, 2);
                                    if (VarDum.Count ==2)
                                    {
                                        
                                        Main.Statements.Add((CodeAssignStatement)CrearVariable(VarDum[0].ToString(),
                                            typeof(object), 2,
                                            MemberAttributes.Private,
                                            VarDum[1]));
                                            sintactica.RemoveAt(i);
                                            i--;
                                    }
                                }
                                catch { }
                            }
                            break;
                        case EXPRESIONES.CICLO_MIENTRAS:
                            List<string> MientrasSintax = new List<string>();
                            int fin_Mientras = 1, fin_Mientras_encontrados = 0;
                            MientrasSintax.Add(sintactica[i]);
                            for (k = i + 1; k < sintactica.Count; k++)
                            {
                                patron = "^(mientras)";
                                string patron_fin = "^(finmientras)";
                                if (Regex.IsMatch(sintactica[k], patron))
                                {
                                    MientrasSintax.Add(sintactica[k]);
                                    fin_Mientras++;
                                }
                                else if (Regex.IsMatch(sintactica[k], patron_fin))
                                {
                                    fin_Mientras_encontrados++;
                                    MientrasSintax.Add(sintactica[k]);
                                    if (fin_Mientras == fin_Mientras_encontrados)
                                        break;
                                }
                                else
                                    MientrasSintax.Add(sintactica[k]);
                            }
                            for (int j = i; j < k + 1; j++)
                            {
                                sintactica.RemoveAt(j);
                                j--;
                                k--;
                            }
                            Main.Statements.Add(CrearCicloMientras(MientrasSintax, clase.Members, Main.Statements));
                            i = 0;
                            break;
                        case EXPRESIONES.CICLO_PARA:
                            List<string> ParaSintax = new List<string>();
                            int fin_para = 1, fin_para_encontrados = 0;
                            ParaSintax.Add(sintactica[i]);
                            for (k = i + 1; k < sintactica.Count; k++)
                            {
                                patron = "^(para)";
                                string patron_fin = "^(finpara)";
                                if (Regex.IsMatch(sintactica[k], patron))
                                {
                                    ParaSintax.Add(sintactica[k]);
                                    fin_para++;
                                }
                                else if (Regex.IsMatch(sintactica[k], patron_fin))
                                {
                                    fin_para_encontrados++;
                                    ParaSintax.Add(sintactica[k]);
                                    if (fin_para == fin_para_encontrados)
                                        break;
                                }
                                else
                                    ParaSintax.Add(sintactica[k]);
                            }
                            for (int j = i; j < k + 1; j++)
                            {
                                sintactica.RemoveAt(j);
                                j--;
                                k--;
                            }
                            Main.Statements.Add(CrearCicloPara(ParaSintax, clase.Members, Main.Statements));
                            i = 0;
                            break;
                        case EXPRESIONES.FUNCION_IMPRIMIR:
                            try
                            {

                                string expresion = null;
                                int estado = AnalizarFuncionImpresion(sintaxis, clase.Members, Main.Statements,out expresion);
                                if (estado == 0)
                                    Main.Statements.Add(CrearConsoleWriteLine(sintaxis, 0));
                                else
                                    Main.Statements.Add(CrearConsoleWriteLine(expresion, 1));
                                sintactica.RemoveAt(i);
                                i--;
                            }
                            catch { }
                            break;
                        case EXPRESIONES.FUNCION_LEER:
                            try
                            {

                                string[] trozos = sintaxis.Split(new string[] { "(", ")", "leer" , "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if (trozos.Length >= 1)
                                    Main.Statements.Add((CodeAssignStatement) CrearVariable(trozos[0] , typeof(object) ,3));
                                else
                                   Main.Statements.Add(CrearConsolaReadLIne());
                            }
                            catch { }
                            break;
                        case EXPRESIONES.CHUMPE_INICIO:
                             sintactica.RemoveAt(i);
                                i--;
                                break;
                        default:
                            break;
                    }
                }
            }
            catch {
                /**ERROR INNESPERADO*/
            }

            Main.Statements.Add(CrearConsolaReadLIne());
            clase.Members.Add(Main);
            return compileUnit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sintaxis"></param>
        /// <param name="Clase"></param>
        /// <param name="Main"></param>
        /// <param name="expresion"></param>
        /// <returns></returns>
        private int AnalizarFuncionImpresion(string sintaxis ,
            CodeTypeMemberCollection Clase ,
            CodeStatementCollection Main ,
            out string expresion, CodeStatementCollection Statements = null )
        {
            
            expresion = null;
            string[] Exp = sintaxis.Replace(";", "").ToLower().Split(new string[] { "(", ")", "imprimir" }, StringSplitOptions.RemoveEmptyEntries);
            string[] DataFill;
            bool bandera = false;
            foreach (var estate in Main)
            {
                if (estate.GetType() == typeof(CodeVariableDeclarationStatement))
                {
                    CodeVariableDeclarationStatement var_exist = (CodeVariableDeclarationStatement)estate;
                    DataFill = Exp[0].ToString().Split(new string[] { "+" , "-" , "*" , "/" , "%" }, StringSplitOptions.RemoveEmptyEntries);
                    if (var_exist.Name.ToLower() == DataFill[0].ToLower())
                    {
                        bandera = true;
                        break;
                    }
                }
            }

            if (Statements != null)
            {
                foreach (var estate in Statements)
                {
                    if (estate.GetType() == typeof(CodeVariableDeclarationStatement))
                    {
                        CodeVariableDeclarationStatement var_exist = (CodeVariableDeclarationStatement)estate;
                        DataFill = Exp[0].ToString().Split(new string[] { "+", "-", "*", "/", "%" }, StringSplitOptions.RemoveEmptyEntries);
                        if (var_exist.Name.ToLower() == DataFill[0].ToLower())
                        {
                            bandera = true;
                            break;
                        }
                    }
                }
            }

            if (bandera == false)
            {
                foreach (var variable in Clase)
                {
                    if (variable.GetType() == typeof(CodeMemberField))
                    {
                        CodeMemberField var_exist = (CodeMemberField)variable;
                        DataFill = Exp[0].ToString().Split(new string[] { "+", "-", "*", "/", "%" }, StringSplitOptions.RemoveEmptyEntries);
                        if (var_exist.Name.ToLower() == DataFill[0].ToLower())
                        {
                            bandera = true;
                            break;
                        }
                    }
                }
                if (bandera == false)
                    return 0;
                else
                {
                    expresion = Exp[0];
                    return 1;
                }
            }
            else
            {
                expresion = Exp[0];
                return 1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="expresion"></param>
        /// <returns></returns>
        private EXPRESIONES TipoExpresion(string expresion)
        {
            string patron_expresion = null;
            try
            {
                patron_expresion = @"^(\w+)(\=)(leer)\(\)\;$";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.FUNCION_LEER;
                patron_expresion = @"^(imprimir)\((['""]?)(\w+)|(['""]?)\)\;";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.FUNCION_IMPRIMIR;
                patron_expresion = @"^(si)";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.CONDICION;
                patron_expresion = @"^(para)";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.CICLO_PARA;
                patron_expresion = @"^(mientras)";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.CICLO_MIENTRAS;
                patron_expresion = @"^.(entero|decimal|flotante|cadena|objeto)\s+(\w+)\s?(\=?)\s?(\w+?)|(['""]?)\;$";
                if(Regex.IsMatch(expresion , patron_expresion))
                    return EXPRESIONES.VARIABLE;
                patron_expresion = @"^(chumpe_inicio\s?\(\))";
                if (Regex.IsMatch(expresion, patron_expresion))
                    return EXPRESIONES.CHUMPE_INICIO;
         
            }
            catch { }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private List<object> TransformarVariable(string variable , int tipo)
        {
            List<object> listado_variable = new List<object>();
            string[] values;
            try
            {
                switch (tipo)
                {
                    case 0:
                        values = variable.Replace(";" , "").Split(new string[] { " " , "=" , }, StringSplitOptions.RemoveEmptyEntries);
                        switch (values[0].ToLower())
                        {
                            case "privado":
                                listado_variable.Add(MemberAttributes.Private);
                                break;
                            case "publico":
                                listado_variable.Add(MemberAttributes.Public);
                                break;
                            case "protegido":
                                listado_variable.Add(MemberAttributes.Static);
                                break;
                            case "virtual":
                                listado_variable.Add(MemberAttributes.Override);
                                break;
                        }
                        switch (values[1].ToLower())
                        {
                            case "entero":
                                listado_variable.Add(typeof(int));
                                break;
                            case "decimal":
                                listado_variable.Add(typeof(decimal));
                                break;
                            case "flotante":
                                listado_variable.Add(typeof(double));
                                break;
                            case "cadena":
                                listado_variable.Add(typeof(string));
                                break;
                            case "objeto":
                                listado_variable.Add(typeof(object));
                                break;
                        }
                        listado_variable.Add(values[2]);
                        try
                        {
                            listado_variable.Add(values[3]);
                        }
                        catch { listado_variable.Add(null); }
                        break;
                    case 1:
                        values = variable.Replace(";" , "").Split(new string[] { " " , "=" , }, StringSplitOptions.RemoveEmptyEntries);
                        switch (values[0].ToLower())
                        {
                            case "entero":
                                listado_variable.Add(typeof(int));
                                break;
                            case "decimal":
                                listado_variable.Add(typeof(decimal));
                                break;
                            case "flotante":
                                listado_variable.Add(typeof(double));
                                break;
                            case "cadena":
                                listado_variable.Add(typeof(string));
                                break;
                            case "objeto":
                                listado_variable.Add(typeof(object));
                                break;
                        }
                        listado_variable.Add(values[1]);
                        try
                        {
                            if (values.Length > 3)
                            {
                                string v = "";
                                for (int i = 2; i < values.Length; i++)
                                {
                                    v += " " + values[i];
                                }
                                listado_variable.Add(v);
                            }
                            else
                                listado_variable.Add(values[2]);
                        }
                        catch { listado_variable.Add(null); }
                        break;
                    case 2:
                        values = variable.Replace(";", "").Replace(" " , "").Split(new string[] {  "=" }, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            listado_variable.Add(values[0]);
                            listado_variable.Add(values[1]);
                        }
                        catch { listado_variable = new List<object>(); }
                        break;
                }
            }
            catch { }
            return listado_variable;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="Nombre_variable"></param>
        /// <param name="type"></param>
        /// <param name="segmento">Verifica si la variable es declarada dentro del namespace=0 o dentro del main=1</param>
        /// <param name="NivelAcceso"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Object CrearVariable(string Nombre_variable, Type type, int segmento, 
            MemberAttributes NivelAcceso = MemberAttributes.Private , object value = null)
        {

            if (segmento == 0)
            {
                CodeMemberField VarName = new CodeMemberField(type, Nombre_variable);
                VarName.Attributes = NivelAcceso;
                if (value != null)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int32:
                            VarName.InitExpression = new CodePrimitiveExpression(int.Parse(value.ToString()));
                            break;
                        case TypeCode.Decimal:
                            VarName.InitExpression = new CodePrimitiveExpression(decimal.Parse(value.ToString()));
                            break;
                        case TypeCode.Double:
                            VarName.InitExpression = new CodePrimitiveExpression(double.Parse(value.ToString()));
                            break;
                        case TypeCode.String:
                            VarName.InitExpression = new CodePrimitiveExpression(value.ToString());
                            break;
                        case TypeCode.Object:
                            VarName.InitExpression = new CodePrimitiveExpression(value);
                            break;
                    }

                }
                return VarName;
            }
            else if (segmento == 1)
            {
                CodeVariableDeclarationStatement varCode = new CodeVariableDeclarationStatement(type, Nombre_variable);
                if (value != null)
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int32:
                            varCode.InitExpression = new CodePrimitiveExpression(int.Parse(value.ToString()));
                            break;
                        case TypeCode.Decimal:
                            varCode.InitExpression = new CodePrimitiveExpression(decimal.Parse(value.ToString()));
                            break;
                        case TypeCode.Double:
                            varCode.InitExpression = new CodePrimitiveExpression(double.Parse(value.ToString()));
                            break;
                        case TypeCode.String:
                            varCode.InitExpression = new CodePrimitiveExpression(value.ToString());
                            break;
                        case TypeCode.Object:
                            varCode.InitExpression = new CodePrimitiveExpression(value);
                            break;
                    }

                }
                return varCode;

            }
            else if (segmento == 2)
            {
                int A;
                decimal B;
                double C;
                CodeAssignStatement AsigRef = new CodeAssignStatement();
                
                if(int.TryParse(value.ToString(),out A))
                 AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable),
                                new CodePrimitiveExpression(int.Parse(value.ToString())));
                else if (decimal.TryParse(value.ToString(), out B))
                    AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable),
                                   new CodePrimitiveExpression(decimal.Parse(value.ToString())));
                else if (double.TryParse(value.ToString(), out C))
                    AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable),
                                   new CodePrimitiveExpression(double.Parse(value.ToString())));
                else
                {
                    string[] operadores = new string[] { "+", "-", "*", "/", "%" };
                    for (int i = 0; i < operadores.Length; i++)
                    {
                        int exist = value.ToString().IndexOf(operadores[i]);
                        if (exist >= 1)
                        {
                            AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable),
                                 new CodeSnippetExpression(value.ToString()));
                            return AsigRef;
                        }
                    }
                    AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable),
                                   new CodePrimitiveExpression(value));
                }

                return AsigRef;
            }
            else if (segmento == 3)
            {
                CodeAssignStatement AsigRef = new CodeAssignStatement();
                AsigRef = new CodeAssignStatement(new CodeVariableReferenceExpression(Nombre_variable), CrearConsolaReadLIne());
                return AsigRef;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clausulas"></param>
        /// <returns></returns>
        private CodeConditionStatement CrearCondicional(List<string> clausulas,  CodeTypeMemberCollection Clase ,
            CodeStatementCollection Main , CodeStatementCollection SubStatement = null)
        {
           
            CodeConditionStatement condicion_inicial = new CodeConditionStatement();
            try
            {
                int k = 0 , p=0;
                string clausula_inicial = clausulas[0];
                clausulas.RemoveAt(0);
                string[] trozos_iniciales = clausula_inicial.Split(new string[] { "(", ")", "si" }, StringSplitOptions.RemoveEmptyEntries);
               
                //se le agrego este filtro de espacios en blanco que puede ocacionar error en el IL
                for (k = 0; k < trozos_iniciales.Length; k++)
                    if (!string.IsNullOrEmpty(trozos_iniciales[k]) && trozos_iniciales[k] != " ")
                        break;
                    

                condicion_inicial.Condition = new CodeSnippetExpression(trozos_iniciales[k]);
                
                bool bandera = false;
                for (int i = 0; i < clausulas.Count; i++)
                {
                    string patron = "^(entonces)";
                    if (Regex.IsMatch(clausulas[i], patron)) bandera = true;
                    if (bandera == false)
                    {
                        switch (TipoExpresion(clausulas[i]))
                        {
                            case EXPRESIONES.CONDICION:
                                List<string> CondicionSintax = new List<string>();
                                int finClausula = 1, ClausulaEncontrada = 0;
                                CondicionSintax.Add(clausulas[i]);
                                for (k = i + 1; k < clausulas.Count; k++)
                                {
                                    patron = "^(si)";
                                    string patron_fin = "^(finsi)";
                                    if (Regex.IsMatch(clausulas[k], patron))
                                    {
                                        CondicionSintax.Add(clausulas[k]);
                                        finClausula++;
                                    }
                                    else if (Regex.IsMatch(clausulas[k], patron_fin))
                                    {
                                        ClausulaEncontrada++;
                                        CondicionSintax.Add(clausulas[k]);
                                        if (finClausula == ClausulaEncontrada)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                        CondicionSintax.Add(clausulas[k]);
                                }
                                p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.TrueStatements.Add(CrearCondicional(CondicionSintax, Clase, Main, condicion_inicial.TrueStatements));
                                break;
                            case EXPRESIONES.VARIABLE:
                                try
                                {
                                    List<object> VarDum = TransformarVariable(clausulas[i], 1);
                                    condicion_inicial.TrueStatements.Add((CodeVariableDeclarationStatement)CrearVariable(VarDum[1].ToString(),
                                      (Type)VarDum[0], 1,
                                      MemberAttributes.Private,
                                      VarDum[2]));
                                }
                                catch
                                {
                                    try
                                    {
                                        List<object> VarDum = TransformarVariable(clausulas[i], 2);
                                        if (VarDum.Count == 2)
                                        {

                                            condicion_inicial.TrueStatements.Add((CodeAssignStatement)CrearVariable(VarDum[0].ToString(),
                                                typeof(object), 2,
                                                MemberAttributes.Private,
                                                VarDum[1]));
                                        }
                                    }
                                    catch { }
                                }
                                break;
                            case EXPRESIONES.CICLO_MIENTRAS:
                                 List<string> MientrasSintax = new List<string>();
                                 int fin_Mientras = 1, fin_Mientras_encontrados = 0;
                                 MientrasSintax.Add(clausulas[i]);
                                 for (k = i + 1; k < clausulas.Count; k++)
                                 {
                                     patron = "^(mientras)";
                                     string patron_fin = "^(finmientras)";
                                     if (Regex.IsMatch(clausulas[k], patron))
                                     {
                                         MientrasSintax.Add(clausulas[k]);
                                         fin_Mientras++;
                                     }
                                     else if (Regex.IsMatch(clausulas[k], patron_fin))
                                     {
                                         fin_Mientras_encontrados++;
                                         MientrasSintax.Add(clausulas[k]);
                                         if (fin_Mientras == fin_Mientras_encontrados)
                                             break;
                                     }
                                     else
                                         MientrasSintax.Add(clausulas[k]);
                                 }
                                p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.TrueStatements.Add(CrearCicloMientras(MientrasSintax, Clase, Main, condicion_inicial.TrueStatements));
                                break;
                            case EXPRESIONES.CICLO_PARA:
                                  List<string> ParaSintax = new List<string>();
                                  int fin_para = 1, fin_para_encontrados = 0;
                                  ParaSintax.Add(clausulas[i]);
                                  for (k = i + 1; k < clausulas.Count; k++)
                                  {
                                      patron = "^(para)";
                                      string patron_fin = "^(finpara)";
                                      if (Regex.IsMatch(clausulas[k], patron))
                                      {
                                          ParaSintax.Add(clausulas[k]);
                                          fin_para++;
                                      }
                                      else if (Regex.IsMatch(clausulas[k], patron_fin))
                                      {
                                          fin_para_encontrados++;
                                          ParaSintax.Add(clausulas[k]);
                                          if (fin_para == fin_para_encontrados)
                                              break;
                                      }
                                      else
                                          ParaSintax.Add(clausulas[k]);
                                  }
                                 p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.TrueStatements.Add(CrearCicloPara(ParaSintax, Clase, Main, condicion_inicial.TrueStatements));
                                break;
                            case EXPRESIONES.FUNCION_IMPRIMIR:
                                try
                                {
                                    
                                    string expresion = null;
                                    int estado = AnalizarFuncionImpresion(clausulas[i], Clase, Main, out expresion , SubStatement);
                                    if (estado == 0)
                                        condicion_inicial.TrueStatements.Add(CrearConsoleWriteLine(clausulas[i], 0));
                                    else
                                        condicion_inicial.TrueStatements.Add(CrearConsoleWriteLine(expresion, 1));
                                }
                                catch { }
                                break;
                            case EXPRESIONES.FUNCION_LEER:
                                try
                                {

                                    string[] trozos = clausulas[i].Split(new string[] { "(", ")", "leer", "=" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (trozos.Length >= 1)
                                        condicion_inicial.TrueStatements.Add((CodeAssignStatement)CrearVariable(trozos[0], typeof(object), 3));
                                    else
                                        condicion_inicial.TrueStatements.Add(CrearConsolaReadLIne());
                                }
                                catch { }
                                break;
                        }
                    }
                    else
                    {
                        switch (TipoExpresion(clausulas[i]))
                        {
                            case EXPRESIONES.CONDICION:
                                List<string> CondicionSintax = new List<string>();
                                int finClausula = 1, ClausulaEncontrada = 0;
                                CondicionSintax.Add(clausulas[i]);
                                for (k = i + 1; k < clausulas.Count; k++)
                                {
                                    patron = "^(si)";
                                    string patron_fin = "^(finsi)";
                                    if (Regex.IsMatch(clausulas[k], patron))
                                    {
                                        CondicionSintax.Add(clausulas[k]);
                                        finClausula++;
                                    }
                                    else if (Regex.IsMatch(clausulas[k], patron_fin))
                                    {
                                        ClausulaEncontrada++;
                                        CondicionSintax.Add(clausulas[k]);
                                        if (finClausula == ClausulaEncontrada)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                        CondicionSintax.Add(clausulas[k]);
                                }
                                p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.FalseStatements.Add(CrearCondicional(CondicionSintax, Clase, Main, condicion_inicial.FalseStatements));
                                break;
                            case EXPRESIONES.VARIABLE:
                                try
                                {
                                    List<object> VarDum = TransformarVariable(clausulas[i], 1);
                                    condicion_inicial.FalseStatements.Add((CodeVariableDeclarationStatement)CrearVariable(VarDum[1].ToString(),
                                      (Type)VarDum[0], 1,
                                      MemberAttributes.Private,
                                      VarDum[2]));
                                }
                                catch
                                {
                                    try
                                    {
                                        List<object> VarDum = TransformarVariable(clausulas[i], 2);
                                        if (VarDum.Count == 2)
                                        {

                                            condicion_inicial.FalseStatements.Add((CodeAssignStatement)CrearVariable(VarDum[0].ToString(),
                                                typeof(object), 2,
                                                MemberAttributes.Private,
                                                VarDum[1]));
                                        }
                                    }
                                    catch { }
                                }
                                break;
                            case EXPRESIONES.CICLO_MIENTRAS:
                                List<string> MientrasSintax = new List<string>();
                                int fin_Mientras = 1, fin_Mientras_encontrados = 0;
                                MientrasSintax.Add(clausulas[i]);
                                for (k = i + 1; k < clausulas.Count; k++)
                                {
                                    patron = "^(mientras)";
                                    string patron_fin = "^(finmientras)";
                                    if (Regex.IsMatch(clausulas[k], patron))
                                    {
                                        MientrasSintax.Add(clausulas[k]);
                                        fin_Mientras++;
                                    }
                                    else if (Regex.IsMatch(clausulas[k], patron_fin))
                                    {
                                        fin_Mientras_encontrados++;
                                        MientrasSintax.Add(clausulas[k]);
                                        if (fin_Mientras == fin_Mientras_encontrados)
                                            break;
                                    }
                                    else
                                        MientrasSintax.Add(clausulas[k]);
                                }
                                p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.FalseStatements.Add(CrearCicloMientras(MientrasSintax, Clase, Main, condicion_inicial.FalseStatements));
                                break;
                            case EXPRESIONES.CICLO_PARA:
                                List<string> ParaSintax = new List<string>();
                                int fin_para = 1, fin_para_encontrados = 0;
                                ParaSintax.Add(clausulas[i]);
                                for (k = i + 1; k < clausulas.Count; k++)
                                {
                                    patron = "^(para)";
                                    string patron_fin = "^(finpara)";
                                    if (Regex.IsMatch(clausulas[k], patron))
                                    {
                                        ParaSintax.Add(clausulas[k]);
                                        fin_para++;
                                    }
                                    else if (Regex.IsMatch(clausulas[k], patron_fin))
                                    {
                                        fin_para_encontrados++;
                                        ParaSintax.Add(clausulas[k]);
                                        if (fin_para == fin_para_encontrados)
                                            break;
                                    }
                                    else
                                        ParaSintax.Add(clausulas[k]);
                                }
                                p = k;
                                for (int j = i; j < k + 1; j++)
                                {
                                    clausulas.RemoveAt(j);
                                    j--;
                                    k--;
                                }
                                i = p;
                                condicion_inicial.FalseStatements.Add(CrearCicloPara(ParaSintax, Clase, Main, condicion_inicial.FalseStatements));
                                break;
                            case EXPRESIONES.FUNCION_IMPRIMIR:
                                try
                                {

                                    string expresion = null;
                                    int estado = AnalizarFuncionImpresion(clausulas[i], Clase, Main, out expresion, SubStatement);
                                    if (estado == 0)
                                        condicion_inicial.FalseStatements.Add(CrearConsoleWriteLine(clausulas[i], 0));
                                    else
                                        condicion_inicial.FalseStatements.Add(CrearConsoleWriteLine(expresion, 1));
                                }
                                catch { }
                                break;
                            case EXPRESIONES.FUNCION_LEER:
                                try
                                {

                                    string[] trozos = clausulas[i].Split(new string[] { "(", ")", "leer", "=" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (trozos.Length >= 1)
                                        condicion_inicial.FalseStatements.Add((CodeAssignStatement)CrearVariable(trozos[0], typeof(object), 3));
                                    else
                                        condicion_inicial.FalseStatements.Add(CrearConsolaReadLIne());
                                }
                                catch { }
                                break;
                        }
                    }
                }
            }
            catch { }
            //CodeVariableReferenceExpression ex__ = new CodeVariableReferenceExpression(expresion);
          
            return condicion_inicial;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="clausulas"></param>
        /// <param name="Clase"></param>
        /// <param name="Main"></param>
        /// <returns></returns>
        private CodeIterationStatement CrearCicloPara(List<string> clausulas, CodeTypeMemberCollection Clase,
            CodeStatementCollection Main, CodeStatementCollection SubStatement = null)
        {
            CodeIterationStatement ParaStatement = new CodeIterationStatement();

            try
            {
                int n = 0 , k=0 , p=0;
                string[] clausula_inicial = clausulas[0].Split(new string[] { ";", "(", ")", "para" }, StringSplitOptions.RemoveEmptyEntries);

                string[] StInicial = clausula_inicial[0].Split('=');
                string[] StCondicio = new string[1];
                string[] StIncremento = new string[1];

                string[] delim = new string[] { ">=", "<=", ">", "<" };
                string[] op = new string[] { "++", "--" };

                CodeBinaryOperatorType tipo_operador = new CodeBinaryOperatorType();

                ParaStatement.InitStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(StInicial[0]), 
                    new CodePrimitiveExpression(int.Parse(StInicial[1].ToString())));

                for (n = 0; n < delim.Length; n++)
                {
                    StCondicio = clausula_inicial[1].Split(new string[] { delim[n] }, StringSplitOptions.RemoveEmptyEntries);
                    if (StCondicio.Length >= 2)
                    {
                        switch (delim[n])
                        {
                            case ">":
                                tipo_operador = CodeBinaryOperatorType.GreaterThan;
                                break;
                            case "<":
                                tipo_operador = CodeBinaryOperatorType.LessThan;
                                break;
                            case ">=":
                                tipo_operador = CodeBinaryOperatorType.GreaterThanOrEqual;
                                break;
                            case "<=":
                                tipo_operador = CodeBinaryOperatorType.LessThanOrEqual;
                                break;
                        }
                        break;
                    }
                }

               
                int is_number = 0;
                bool es_numero = int.TryParse(StCondicio[1] ,out is_number);

                if(es_numero)
                    ParaStatement.TestExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(StCondicio[0]),
                    tipo_operador, new CodePrimitiveExpression(is_number));
                else
                    ParaStatement.TestExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(StCondicio[0]),
                    tipo_operador, new CodeSnippetExpression(StCondicio[1]));

                for ( n = 0; n < op.Length; n++)
                {
                    int exist = clausula_inicial[2].IndexOf(op[n]);
                    if (exist >= 1)
                    {
                        switch (op[n])
                        {
                            case "++":
                                tipo_operador = CodeBinaryOperatorType.Add;
                                break;
                            case "--":
                                tipo_operador = CodeBinaryOperatorType.Subtract;
                                break;
                        }
                        break;
                    }
                }
                ParaStatement.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(StCondicio[0]), 
                    new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(StCondicio[0]), tipo_operador, new CodePrimitiveExpression(1)));


                clausulas.RemoveAt(0);
                for (int i = 0; i < clausulas.Count; i++)
                {
                    string patron = "";
                    switch (TipoExpresion(clausulas[i]))
                    {
                        case EXPRESIONES.CONDICION:
                            List<string> CondicionSintax = new List<string>();
                            int finClausula = 1, ClausulaEncontrada = 0;
                            CondicionSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(si)";
                                string patron_fin = "^(finsi)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    CondicionSintax.Add(clausulas[k]);
                                    finClausula++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    ClausulaEncontrada++;
                                    CondicionSintax.Add(clausulas[k]);
                                    if (finClausula == ClausulaEncontrada)
                                    {
                                        break;
                                    }
                                }
                                else
                                    CondicionSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            ParaStatement.Statements.Add(CrearCondicional(CondicionSintax, Clase, Main, ParaStatement.Statements));
                            break;
                        case EXPRESIONES.VARIABLE:
                            try
                            {
                                List<object> VarDum = TransformarVariable(clausulas[i], 1);
                                ParaStatement.Statements.Add((CodeVariableDeclarationStatement)CrearVariable(VarDum[1].ToString(),
                                  (Type)VarDum[0], 1,
                                  MemberAttributes.Private,
                                  VarDum[2]));
                            }
                            catch
                            {
                                try
                                {
                                    List<object> VarDum = TransformarVariable(clausulas[i], 2);
                                    if (VarDum.Count == 2)
                                    {

                                        ParaStatement.Statements.Add((CodeAssignStatement)CrearVariable(VarDum[0].ToString(),
                                            typeof(object), 2,
                                            MemberAttributes.Private,
                                            VarDum[1]));
                                    }
                                }
                                catch { }
                            }
                            break;
                        case EXPRESIONES.CICLO_MIENTRAS:
                            List<string> MientrasSintax = new List<string>();
                            int fin_Mientras = 1, fin_Mientras_encontrados = 0;
                            MientrasSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(mientras)";
                                string patron_fin = "^(finmientras)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    MientrasSintax.Add(clausulas[k]);
                                    fin_Mientras++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    fin_Mientras_encontrados++;
                                    MientrasSintax.Add(clausulas[k]);
                                    if (fin_Mientras == fin_Mientras_encontrados)
                                        break;
                                }
                                else
                                    MientrasSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            ParaStatement.Statements.Add(CrearCicloMientras(MientrasSintax, Clase, Main, ParaStatement.Statements));
                            break;
                        case EXPRESIONES.CICLO_PARA:
                            List<string> ParaSintax = new List<string>();
                            int fin_para = 1, fin_para_encontrados = 0;
                            ParaSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(para)";
                                string patron_fin = "^(finpara)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    ParaSintax.Add(clausulas[k]);
                                    fin_para++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    fin_para_encontrados++;
                                    ParaSintax.Add(clausulas[k]);
                                    if (fin_para == fin_para_encontrados)
                                        break;
                                }
                                else
                                    ParaSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            ParaStatement.Statements.Add(CrearCicloPara(ParaSintax, Clase, Main, ParaStatement.Statements));
                            break;
                        case EXPRESIONES.FUNCION_IMPRIMIR:
                            try
                            {

                                string expresion = null;
                                int estado = AnalizarFuncionImpresion(clausulas[i], Clase, Main, out expresion, SubStatement);
                                if (estado == 0)
                                    ParaStatement.Statements.Add(CrearConsoleWriteLine(clausulas[i], 0));
                                else
                                    ParaStatement.Statements.Add(CrearConsoleWriteLine(expresion, 1));
                            }
                            catch { }
                            break;
                        case EXPRESIONES.FUNCION_LEER:
                            try
                            {

                                string[] trozos = clausulas[i].Split(new string[] { "(", ")", "leer", "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if (trozos.Length >= 1)
                                    ParaStatement.Statements.Add((CodeAssignStatement)CrearVariable(trozos[0], typeof(object), 3));
                                else
                                    ParaStatement.Statements.Add(CrearConsolaReadLIne());
                            }
                            catch { }
                            break;
                    }
                }
            }
            catch { }

            return ParaStatement;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="clausulas"></param>
        /// <param name="Clase"></param>
        /// <param name="Main"></param>
        /// <returns></returns>
        private CodeIterationStatement CrearCicloMientras(List<string> clausulas, CodeTypeMemberCollection Clase,
            CodeStatementCollection Main, CodeStatementCollection SubStatement = null)
        {
            CodeIterationStatement MientrasStatement = new CodeIterationStatement();

            try
            {
                int k = 0, p = 0;
                string[] clausula_inicial = clausulas[0].Split(new string[] { "(", ")", "mientras" }, StringSplitOptions.RemoveEmptyEntries);
                
                MientrasStatement.InitStatement = new CodeSnippetStatement();

                MientrasStatement.TestExpression = new CodeSnippetExpression(clausula_inicial[0]);

                MientrasStatement.IncrementStatement = new CodeSnippetStatement();

                clausulas.RemoveAt(0);
                for (int i = 0; i < clausulas.Count; i++)
                {
                    string patron = "";
                    switch (TipoExpresion(clausulas[i]))
                    {
                        case EXPRESIONES.CONDICION:
                            List<string> CondicionSintax = new List<string>();
                            int finClausula = 1, ClausulaEncontrada = 0;
                            CondicionSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(si)";
                                string patron_fin = "^(finsi)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    CondicionSintax.Add(clausulas[k]);
                                    finClausula++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    ClausulaEncontrada++;
                                    CondicionSintax.Add(clausulas[k]);
                                    if (finClausula == ClausulaEncontrada)
                                    {
                                        break;
                                    }
                                }
                                else
                                    CondicionSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            MientrasStatement.Statements.Add(CrearCondicional(CondicionSintax, Clase, Main, MientrasStatement.Statements));
                            break;
                        case EXPRESIONES.VARIABLE:
                            try
                            {
                                List<object> VarDum = TransformarVariable(clausulas[i], 1);
                                MientrasStatement.Statements.Add((CodeVariableDeclarationStatement)CrearVariable(VarDum[1].ToString(),
                                  (Type)VarDum[0], 1,
                                  MemberAttributes.Private,
                                  VarDum[2]));
                            }
                            catch
                            {
                                try
                                {
                                    List<object> VarDum = TransformarVariable(clausulas[i], 2);
                                    if (VarDum.Count == 2)
                                    {

                                        MientrasStatement.Statements.Add((CodeAssignStatement)CrearVariable(VarDum[0].ToString(),
                                            typeof(object), 2,
                                            MemberAttributes.Private,
                                            VarDum[1]));
                                    }
                                }
                                catch { }
                            }
                            break;
                        case EXPRESIONES.CICLO_MIENTRAS:
                            List<string> MientrasSintax = new List<string>();
                            int fin_Mientras = 1, fin_Mientras_encontrados = 0;
                            MientrasSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(mientras)";
                                string patron_fin = "^(finmientras)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    MientrasSintax.Add(clausulas[k]);
                                    fin_Mientras++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    fin_Mientras_encontrados++;
                                    MientrasSintax.Add(clausulas[k]);
                                    if (fin_Mientras == fin_Mientras_encontrados)
                                        break;
                                }
                                else
                                    MientrasSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            MientrasStatement.Statements.Add(CrearCicloMientras(MientrasSintax, Clase, Main, MientrasStatement.Statements));
                            break;
                        case EXPRESIONES.CICLO_PARA:
                            List<string> ParaSintax = new List<string>();
                            int fin_para = 1, fin_para_encontrados = 0;
                            ParaSintax.Add(clausulas[i]);
                            for (k = i + 1; k < clausulas.Count; k++)
                            {
                                patron = "^(para)";
                                string patron_fin = "^(finpara)";
                                if (Regex.IsMatch(clausulas[k], patron))
                                {
                                    ParaSintax.Add(clausulas[k]);
                                    fin_para++;
                                }
                                else if (Regex.IsMatch(clausulas[k], patron_fin))
                                {
                                    fin_para_encontrados++;
                                    ParaSintax.Add(clausulas[k]);
                                    if (fin_para == fin_para_encontrados)
                                        break;
                                }
                                else
                                    ParaSintax.Add(clausulas[k]);
                            }
                            p = k;
                            for (int j = i; j < k + 1; j++)
                            {
                                clausulas.RemoveAt(j);
                                j--;
                                k--;
                            }
                            i = p;
                            MientrasStatement.Statements.Add(CrearCicloPara(ParaSintax, Clase, Main, MientrasStatement.Statements));
                            break;
                        case EXPRESIONES.FUNCION_IMPRIMIR:
                            try
                            {

                                string expresion = null;
                                int estado = AnalizarFuncionImpresion(clausulas[i], Clase, Main, out expresion, SubStatement);
                                if (estado == 0)
                                    MientrasStatement.Statements.Add(CrearConsoleWriteLine(clausulas[i], 0));
                                else
                                    MientrasStatement.Statements.Add(CrearConsoleWriteLine(expresion, 1));
                            }
                            catch { }
                            break;
                        case EXPRESIONES.FUNCION_LEER:
                            try
                            {

                                string[] trozos = clausulas[i].Split(new string[] { "(", ")", "leer", "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if (trozos.Length >= 1)
                                    MientrasStatement.Statements.Add((CodeAssignStatement)CrearVariable(trozos[0], typeof(object), 3));
                                else
                                    MientrasStatement.Statements.Add(CrearConsolaReadLIne());
                            }
                            catch { }
                            break;
                    }
                }
            }
            catch { }

            return MientrasStatement;

        }

        /// <summary>
        ///  Crea la linea de consola en forma de escritura 
        /// </summary>
        /// <param name="expresion"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        private CodeMethodInvokeExpression CrearConsoleWriteLine(string expresion , int estado=0)
        {
            if (estado == 0)
            {
                string[] Exp = expresion.Replace(";", "").ToLower().Split(new string[] { "(", ")", "imprimir" }, StringSplitOptions.RemoveEmptyEntries);
                CodeTypeReferenceExpression csSystemConsoleType = new CodeTypeReferenceExpression("System.Console");
                CodeMethodInvokeExpression Consola = new CodeMethodInvokeExpression(
                    csSystemConsoleType, "WriteLine",
                    new CodePrimitiveExpression(Convert.ToString(Exp[0])));
                return Consola;
            }
            else if (estado == 1)
            {
                CodeTypeReferenceExpression csSystemConsoleType = new CodeTypeReferenceExpression("System.Console");
                CodeMethodInvokeExpression Consola = new CodeMethodInvokeExpression(
                    csSystemConsoleType, "WriteLine",
                    new CodeVariableReferenceExpression(expresion));
                return Consola;
            }
            return new CodeMethodInvokeExpression();
        }

  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="VariableReceptora"></param>
        /// <returns></returns>
        private CodeMethodInvokeExpression CrearConsolaReadLIne()
        {
            CodeTypeReferenceExpression csSystemConsoleType = new CodeTypeReferenceExpression("System.Console");
            CodeMethodInvokeExpression csReadLine = new CodeMethodInvokeExpression(
              csSystemConsoleType, "ReadLine" );
            return csReadLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="exeFile"></param>
        /// <param name="algun_error"></param>
        /// <returns></returns>
        public bool CompilarCodigo(string sourceFile, string exeFile , out List<string> algun_error)
        {
            algun_error = new List<string>();
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.GenerateExecutable = true;
            cp.OutputAssembly = exeFile;
            cp.GenerateInMemory = false;
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceFile);

            if (cr.Errors.Count > 0)
            {
                
                algun_error.Add("Error " + sourceFile + " dentro de [ " + cr.PathToAssembly + " ] IL chumpe ");
                foreach (CompilerError ce in cr.Errors)
                {

                    algun_error.Add(ce.ToString());
                }
            }

            if (cr.Errors.Count > 0)
                return false;
            else
            {
                return true;
            }
        }
    }
}
