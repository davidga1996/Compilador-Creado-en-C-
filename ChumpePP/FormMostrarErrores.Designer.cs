namespace ChumpePP
{
    partial class FormMostrarErrores
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grilla_error = new System.Windows.Forms.DataGridView();
            this.numero = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.err = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grilla_error)).BeginInit();
            this.SuspendLayout();
            // 
            // grilla_error
            // 
            this.grilla_error.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilla_error.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numero,
            this.err});
            this.grilla_error.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilla_error.Location = new System.Drawing.Point(0, 0);
            this.grilla_error.Name = "grilla_error";
            this.grilla_error.Size = new System.Drawing.Size(559, 221);
            this.grilla_error.TabIndex = 0;
            // 
            // numero
            // 
            this.numero.HeaderText = "";
            this.numero.Name = "numero";
            // 
            // err
            // 
            this.err.HeaderText = "El Chumpe ha encontrado los siguientes errores";
            this.err.MinimumWidth = 10;
            this.err.Name = "err";
            this.err.Width = 600;
            // 
            // FormMostrarErrores
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 221);
            this.Controls.Add(this.grilla_error);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMostrarErrores";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormMostrarErrores";
            this.Load += new System.EventHandler(this.FormMostrarErrores_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grilla_error)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grilla_error;
        private System.Windows.Forms.DataGridViewTextBoxColumn numero;
        private System.Windows.Forms.DataGridViewTextBoxColumn err;
    }
}