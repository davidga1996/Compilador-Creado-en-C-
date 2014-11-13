using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChumpePP.Class;
using System.Reflection;
using System.Diagnostics;
using System.Media;


namespace ChumpePP
{
    public partial class ChumpeDesktop : Form
    {
        public ChumpeDesktop()
        {
            InitializeComponent();
          
        }

        #region variables

        private bool Poblamiento = true;
        private LectorSintaxis LectorSintactico;
        private Color kCommentarioColor = Color.LightGreen;
        private List<string> Tokens = new List<string>();

        private List<string> CodigoEscrito = new List<string>();


        #endregion

        #region Estructurasintaxis
        struct WordAndPosition
        {
            public string Word;
            public int Position;
            public int Length;
            public override string ToString()
            {
                string s = "Word = " + Word + ", Position = " + Position + ", Length = " + Length + "\n";
                return s;
            }
        };

        WordAndPosition[] TheBuffer = new WordAndPosition[4000];

        private bool TestComment(string s)
        {
            string testString = s.Trim();
            if ((testString.Length >= 2) &&
                 (testString[0] == '/') &&
                 (testString[1] == '/')
                )
                return true;

            return false;
        }

        private int ParseLine(string s)
        {
            TheBuffer.Initialize();
            int count = 0;
            Regex r = new Regex(@"\w+|[^A-Za-z0-9_ \f\t\v]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match m;

            for (m = r.Match(s); m.Success; m = m.NextMatch())
            {
                TheBuffer[count].Word = m.Value;
                TheBuffer[count].Position = m.Index;
                TheBuffer[count].Length = m.Length;
                count++;
            }


            return count;
        }

        private Color MostrarColor(string s)
        {
            Color Color = Color.Black;

            if (LectorSintactico.IsFuncion(s))
            {
                Color = Color.Red;
            }

            if (LectorSintactico.IsLlave(s))
            {
                Color = Color.Blue;
            }

            if (LectorSintactico.IsSeparador(s))
            {
                Color = Color.DarkOrange;
            }


            return Color;
        }

        private void CrearSintaxisPorCadaLinea()
        {
            int Start = richTextBox1.SelectionStart;
            int Length = richTextBox1.SelectionLength;

            
            int pos = Start;
            while ((pos > 0) && (richTextBox1.Text[pos - 1] != '\n'))
                pos--;

            int pos2 = Start;
            while ((pos2 < richTextBox1.Text.Length) &&
                    (richTextBox1.Text[pos2] != '\n'))
                pos2++;

            string s = richTextBox1.Text.Substring(pos, pos2 - pos);
            if (TestComment(s) == true)
            {
                richTextBox1.Select(pos, pos2 - pos);
                richTextBox1.SelectionColor = kCommentarioColor;
            }
            else
            {
                string previousWord = "";
                int count = ParseLine(s);
                for (int i = 0; i < count; i++)
                {
                    WordAndPosition wp = TheBuffer[i];

                    if (wp.Word == "/" && previousWord == "/")
                    {
                        
                        int posCommentStart = wp.Position - 1;
                        int posCommentEnd = pos2;
                        while (wp.Word != "\n" && i < count)
                        {
                            wp = TheBuffer[i];
                            i++;
                        }

                        i--;
                        posCommentEnd = pos2;
                        richTextBox1.Select(posCommentStart + pos, posCommentEnd - (posCommentStart + pos));
                        richTextBox1.SelectionColor = this.kCommentarioColor;

                    }
                    else
                    {

                        Color c = MostrarColor(wp.Word);
                        richTextBox1.Select(wp.Position + pos, wp.Length);
                        richTextBox1.SelectionColor = c;
                    }

                    previousWord = wp.Word;

                }
            }

            if (Start >= 0)
                richTextBox1.Select(Start, Length);


        }

        private void CrearSintaxisColorAllText(string s)
        {
            Poblamiento = true;

            int CurrentSelectionStart = richTextBox1.SelectionStart;
            int CurrentSelectionLength = richTextBox1.SelectionLength;

            int count = ParseLine(s);
            string previousWord = "";
            for (int i = 0; i < count; i++)
            {
                WordAndPosition wp = TheBuffer[i];

                
                if (wp.Word == "/" && previousWord == "/")
                {
                   
                    int posCommentStart = wp.Position - 1;
                    int posCommentEnd = i;
                    while (wp.Word != "\n" && i < count)
                    {
                        wp = TheBuffer[i];
                        i++;
                    }

                    i--;

                    posCommentEnd = wp.Position;
                    richTextBox1.Select(posCommentStart, posCommentEnd - posCommentStart);
                    richTextBox1.SelectionColor = this.kCommentarioColor;

                }
                else
                {

                    Color c = MostrarColor(wp.Word);
                    richTextBox1.Select(wp.Position, wp.Length);
                    richTextBox1.SelectionColor = c;
                }

                previousWord = wp.Word;
            }

            if (CurrentSelectionStart >= 0)
                richTextBox1.Select(CurrentSelectionStart, CurrentSelectionLength);

            Poblamiento = false;

        }
        #endregion

        #region Inicializadores

        private delegate void ANALIZADOR_SEMANTICO_SINTACTICO__();

        private void GetCodigoEscrito()
        {
            try
            {
                string[] data = richTextBox1.Lines;
                CodigoEscrito = new List<string>();
                CodigoEscrito.AddRange(data);
            }
            catch { }
        }

        private void CodigoInicio()
        {
            string[] codigo = new string[]
            {
                "espacio ChumpeDesktop",
                "{",
                "     usar sistema;",
                "     usar sistema.coleccion;",
                "     usar sistema.componentes;",

                "     publico clase chumpe",
                "     {",
                "       chumpe_inicio()",
                "       {",
                "       }",
                "     }",
                "}"
            };
            richTextBox1.Lines = codigo;
        }

        #endregion

        #region eventos

        private void ChumpeDesktop_Load(object sender, EventArgs e)
        {
            LectorSintactico = new LectorSintaxis("chumpe.syntax");
            CodigoInicio();
            this.CrearSintaxisColorAllText(richTextBox1.Text);
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            this.Text = "CHUMPE++ V" + version;
            this.richTextBox1.AcceptsTab = true;
        }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {
            if (Poblamiento)
                return;

            ColorSyntaxEditor.FlickerFreeRichEditTextBox._Paint = false;
            CrearSintaxisPorCadaLinea();
            ColorSyntaxEditor.FlickerFreeRichEditTextBox._Paint = true;

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetCodigoEscrito();
            Archivos.Guardar(CodigoEscrito, true);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetCodigoEscrito();
            Archivos.Guardar(CodigoEscrito);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetCodigoEscrito();
            Archivos.Guardar(CodigoEscrito);
            Application.Exit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            if (Archivos.Direccion != null) Archivos.Direccion = null;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string[] datos = Archivos.AbrirArchivo();
                if (datos != null)
                {
                    richTextBox1.Lines = datos;
                }
            }
            catch { }
        }

        private void cmd_compilar_Click(object sender, EventArgs e)
        {
            toolnotificaciones.Text = "Compilando espere..";
            toolProgreso.Style = ProgressBarStyle.Continuous;
            toolProgreso.Overflow = ToolStripItemOverflow.Always;
            toolProgreso.Increment(10);
            System.Threading.Thread hilo = 
                new System.Threading.Thread(delegate()
                {
                    ANALIZADOR_SEMANTICO_SINTACTICO__ analizador = new ANALIZADOR_SEMANTICO_SINTACTICO__(ANALIZAR_COMPILAR);
                    this.Invoke(analizador);
                });
            hilo.Start();
        }

        private void ANALIZAR_COMPILAR()
        {
            Compilador compilador = new Compilador();
            AnalizadorSemantico semantico = new AnalizadorSemantico();
            List<string> CodigoComputado = new List<string>();
            System.IO.Stream str = Properties.Resources.chumsound;
            SoundPlayer ChumpeSound = new SoundPlayer(str);
            ChumpeSound.Load();
            toolErrorSintaxis.Text = "";
            try
            {
                toolProgreso.Increment(20);
                toolnotificaciones.Text = "Analizando codigo... (20%)";
                List<string> Codigo = new List<string>();
                Codigo.AddRange(richTextBox1.Lines);
                semantico.SetCodigoAnalizar(Codigo);
                semantico.Computar(out CodigoComputado);
                toolProgreso.Increment(30);
                List<string> Errores = semantico.MostrarErrores();
                if (Errores.Count != 0)
                {
                    ChumpeSound.Play();
                    toolErrorSintaxis.Text = "Error al compilar... ";
                    FormMostrarErrores.ListaErrores = Errores;
                    FormMostrarErrores FrmError = new FormMostrarErrores();
                    FrmError.Show();
                    toolnotificaciones.Text = "sin notificaciones...";
                    toolProgreso.Increment(100);
                    return;
                }
                string direccion = Archivos.Direccion;
                if (direccion == null || direccion == "")
                {
                    GetCodigoEscrito();
                    Archivos.Guardar(CodigoEscrito);
                    direccion = Archivos.Direccion;
                }

                string[] trozo_direccion = direccion.Split(new string[] { "\\" , ".chumpe" } , StringSplitOptions.RemoveEmptyEntries);
                string nombre = trozo_direccion[trozo_direccion.Length - 1];
                if (nombre == "" || string.IsNullOrEmpty(nombre))
                    nombre = "Chompipe";
                toolnotificaciones.Text = "Compilando... (60%)";
                toolProgreso.Increment(60);

                var d = compilador.CheckCodigoAcompilar(CodigoComputado);
                var k = compilador.GenerarCodigoCsharp(d, "__IL_SISTEMA_INT");
                List<string> ILerr = new List<string>();
                bool compilado = compilador.CompilarCodigo(k, nombre + ".exe" , out ILerr);

                if (compilado)
                {
                    Process p = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo(System.IO.Directory.GetCurrentDirectory() + @"\" + nombre + ".exe");
                    p.StartInfo = psi;
                    p.Start();
                    toolErrorSintaxis.Text = "UN CHUMPE SE HA COMPILADO... ";
                    toolnotificaciones.Text = "sin notificaciones...";
                }
                else
                {
                    if (ILerr.Count >= 1)
                    {
                        FormMostrarErrores.ListaErrores = ILerr;
                        FormMostrarErrores FrmError = new FormMostrarErrores();
                        FrmError.Show();
                    }
                    ChumpeSound.Play();
                    toolErrorSintaxis.Text = "Compilacion exitosa pero con error en IL";
                }
                toolProgreso.Increment(100);
            
            }
            catch(Exception ex)
            {
                ChumpeSound.Play();
                toolProgreso.Increment(0);
                MessageBox.Show(ex.Message);
            }
        }


        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Tab:
                    var d = richTextBox1.SelectionTabs;
                    break;
            }
        }

        private void toolguardar_Click(object sender, EventArgs e)
        {
            GetCodigoEscrito();
            Archivos.Guardar(CodigoEscrito);
        }

        private void toolopen_Click(object sender, EventArgs e)
        {
            try
            {
                string[] datos = Archivos.AbrirArchivo();
                if (datos != null)
                {
                    richTextBox1.Lines = datos;
                    LectorSintactico = new LectorSintaxis("chumpe.syntax");
                    this.CrearSintaxisColorAllText(richTextBox1.Text);
                }
            }
            catch { }
        }

        private void toolnuevo_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            if (Archivos.Direccion != null) Archivos.Direccion = null;
        }

        private void toolcopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
            toolnotificaciones.Text = "TEXTO COPIADO ...";
        }

        private void toolpegar_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = Clipboard.GetText();
            toolnotificaciones.Text = "TEXTO PEGADO..";
        }

        private void toolStripTextBox2_Click(object sender, EventArgs e)
        {
            //........................................
        }

        private void menuStrip1_TextChanged(object sender, EventArgs e)
        {
            //.........................................
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //...................................
        }

        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtbuscar.Text.Length <= 0)
                {
                    richTextBox1.SelectAll();
                    richTextBox1.SelectionBackColor = Color.White;
                    return;
                }
                else
                {
                    int final = richTextBox1.TextLength;
                    int longitud_cadena = txtbuscar.Text.Length;
                    int posicion_caracter = richTextBox1.Find(txtbuscar.Text, 0, final, RichTextBoxFinds.MatchCase);
                    int error_paro = 0;
                    while (posicion_caracter >= 1)
                    {
                        error_paro++;
                        posicion_caracter = richTextBox1.Find(txtbuscar.Text, (posicion_caracter + 1), final, RichTextBoxFinds.MatchCase);
                        richTextBox1.Select(posicion_caracter, longitud_cadena);
                        richTextBox1.SelectionBackColor = Color.LightPink;
                        if (error_paro >= 2000) break;
                    }
                }

            }
            catch { }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string Autores = "CREADO EN LA UNIVERSIDAD DON BOSCO EL SALVADOR";
            Autores += "\n\nPor:Rolando Antonio Arriaza ";
            Autores += "\n\nLicencia: GPL";
            MessageBox.Show(Autores, "Creado por ... ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comoHacerUnChumpeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        #endregion

       
       
      

      
     

     
      
        

    }
}
