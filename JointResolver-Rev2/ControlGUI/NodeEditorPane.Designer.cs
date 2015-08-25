partial class NodeEditorPane
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.columnHeaderParent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderConvex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMulti = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderHighRes = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewNodes = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // columnHeaderParent
            // 
            this.columnHeaderParent.Text = "Parent";
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Node";
            // 
            // columnHeaderConvex
            // 
            this.columnHeaderConvex.Text = "Accurate Colliders";
            // 
            // columnHeaderMulti
            // 
            this.columnHeaderMulti.Text = "Multi-Color Part";
            // 
            // columnHeaderHighRes
            // 
            this.columnHeaderHighRes.Text = "High Resolution Part";
            // 
            // listViewNodes
            // 
            this.listViewNodes.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listViewNodes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderParent,
            this.columnHeaderName,
            this.columnHeaderConvex,
            this.columnHeaderMulti,
            this.columnHeaderHighRes});
            this.listViewNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewNodes.FullRowSelect = true;
            this.listViewNodes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewNodes.Location = new System.Drawing.Point(0, 0);
            this.listViewNodes.Name = "listViewNodes";
            this.listViewNodes.Size = new System.Drawing.Size(760, 373);
            this.listViewNodes.TabIndex = 0;
            this.listViewNodes.UseCompatibleStateImageBehavior = false;
            this.listViewNodes.View = System.Windows.Forms.View.Details;
            this.listViewNodes.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewNodes_MouseDoubleClick);
            // 
            // NodeEditorPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listViewNodes);
            this.Name = "NodeEditorPane";
            this.Size = new System.Drawing.Size(760, 373);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader columnHeaderParent;
    private System.Windows.Forms.ColumnHeader columnHeaderName;
    private System.Windows.Forms.ColumnHeader columnHeaderConvex;
    private System.Windows.Forms.ColumnHeader columnHeaderMulti;
    private System.Windows.Forms.ColumnHeader columnHeaderHighRes;
    private System.Windows.Forms.ListView listViewNodes;

}
