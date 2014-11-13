using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChumpePP
{
    public partial class FormMostrarErrores : Form
    {

        public static List<string> ListaErrores = new List<string>();

        public FormMostrarErrores()
        {
            InitializeComponent();
        }

        private void FormMostrarErrores_Load(object sender, EventArgs e)
        {
            this.Text = "Error al compilar ";
            if (ListaErrores.Count != 0)
            {
                for (int i = 0; i < ListaErrores.Count; i++)
                {
                    grilla_error.Rows.Add();
                    grilla_error[0, i].Value = (i + 1);
                    grilla_error[1, i].Value = ListaErrores[i].ToString();
                }
            }
        }
    }
}
