using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Inventor;

namespace ClientGraphicsDemoAU
{
    partial class SamplesDockWnd
    {
        #region Constructors
        /// <summary>Initialize the form in <see cref="DesignMode"/>. 
        /// Do not use this constructor from code.</summary>
        public SamplesDockWnd()
        {
            System.Diagnostics.Debug.Assert(DesignMode, "Default constructor should only be used in Design Mode.");
            InitializeComponent();
        }

        /// <summary>Initialize the dockable window.</summary>
        /// <param name="addInSite">The ApplicationAddInSite object supplied by Inventor.</param>
        public SamplesDockWnd(Inventor.ApplicationAddInSite addInSite)
            : this(addInSite, default(Inventor.DockingStateEnum))
        {
        }

        #endregion

        #region Generated Code for initializing the Dockable Window
        /// <summary>Gets the <see cref="DockableWindowWrapper"/> object.</summary>
        /// <exception cref="InvalidOperationException">components is null or the wrapper cannot be found.</exception>
        /// <exception cref="ObjectDisposedException">The object is already disposed.</exception>
        private DockableWindowWrapper GetWrapper()
        {
            DockableWindowWrapper wrapper = null;

            if (IsDisposed) // Check for disposed.
                throw new ObjectDisposedException(base.GetType().Name);

            // Try to find the wrapper in the components collection by name.
            if (components == null || null == (wrapper = components.Components[typeof(DockableWindowWrapper).Name]
                as DockableWindowWrapper))
                throw new InvalidOperationException(); // Wrapper not found.

            return wrapper;
        }

        /// <summary>Sets the visibility of the form.</summary>
        /// <param name="value">true to make the control visible; otherwise false.</param>
        /// This hook ensures the form gets attached to the DockableWindow properly.
        protected override sealed void SetVisibleCore(bool value)
        {
            // A handle is created the first time the form is shown; initialize the DockableWindow.
            // First time is when the handle is not created yet.
            if (value && !IsHandleCreated && !DesignMode)
            {
                // Show the form making the Inventor DockableWindow its owner.
                // This method will be called again inside Show(IWin32Window) but the handle will be created first.
                base.Show(GetWrapper());
            }
            else
            {
                // The Handle is already created so just pass through to the base implementation.
                base.SetVisibleCore(value);
            }
        }

        public new bool Visible
        {
            get { return GetWrapper().Visible; }
            set { GetWrapper().Visible = value; }
        }

        /// <summary>Gets or sets the docking state of the DockingWindow.</summary>
        public Inventor.DockingStateEnum DockingState
        {
            get
            {
                if (DesignMode)
                    return Inventor.DockingStateEnum.kFloat;
                return GetWrapper().DockingState;
            }
            set
            {
                if (!DesignMode)
                    GetWrapper().DockingState = value;
            }
        }

        #region DockableWindowWrapper
        /// <summary>Wrapper for Inventor.DockableWindow.</summary>
        private sealed class DockableWindowWrapper : Component, IWin32Window
        {
            /// <summary>The wrapped Inventor.DockableWindow object.</summary>
            private Inventor.DockableWindow dockableWindow;

            private DockableWindowsEvents _dockableWindowsEvents;

            private Form _form;

            /// <summary>Initializes the <see cref="DockableWindowWrapper"/>.</summary>
            /// <param name="addInSite">The ApplicationAddInSite object supplied by Inventor.</param>
            /// <param name="form">The managed form to add to the DockableWindow.</param>
            /// <param name="initialDockingState">The initial docking state of the DockableWindow 
            /// if it is created for the first time.</param>
            internal DockableWindowWrapper(
                Inventor.ApplicationAddInSite addInSite,
                Form form,
                Inventor.DockingStateEnum initialDockingState)
            {
                System.Diagnostics.Debug.Assert(addInSite != null && form != null);

                _form = form;

                // Set up the parameters.
                string clientId = addInSite.Parent.ClientId;
                string internalName = _form.GetType().FullName + "." + form.Name;
                string title = _form.Text;

                // We don't want the border to show since the form will be docked inside the DockableWindow.
                _form.FormBorderStyle = FormBorderStyle.None;

                // Retrieve or create the dockable window.
                try
                {
                    dockableWindow = addInSite.Application.UserInterfaceManager.DockableWindows[internalName];
                }
                catch
                {
                    dockableWindow = addInSite.Application.UserInterfaceManager.DockableWindows.Add(
                        clientId,
                        internalName,
                        title);
                }

                // Set the minimum size of the dockable window.
                System.Drawing.Size minimumSize = form.MinimumSize;
                if (!minimumSize.IsEmpty)
                    dockableWindow.SetMinimumSize(minimumSize.Height, minimumSize.Width);

                // Set the initial docking state of the DockableWindow if it is not remembered by Inventor.
                if (initialDockingState != default(Inventor.DockingStateEnum)) // && !dockableWindow.IsCustomized)
                    dockableWindow.DockingState = initialDockingState;

                // Set initial state to invisible.
                dockableWindow.Visible = false;

                // Listen for events.
                _form.HandleCreated += new EventHandler(OnHandleCreated);
                _form.VisibleChanged += new EventHandler(OnVisibleChanged);

                _dockableWindowsEvents =
                    addInSite.Application.UserInterfaceManager.DockableWindows.Events;

                _dockableWindowsEvents.OnHide +=
                    new DockableWindowsEventsSink_OnHideEventHandler(DockableWindowsEvents_OnHide);
            }

            void DockableWindowsEvents_OnHide(DockableWindow DockableWindow,
                EventTimingEnum BeforeOrAfter,
                NameValueMap Context,
                out HandlingCodeEnum HandlingCode)
            {
                HandlingCode = HandlingCodeEnum.kEventNotHandled;

                if (BeforeOrAfter == EventTimingEnum.kBefore && DockableWindow == dockableWindow)
                {
                   
                }
            }

            /// <summary>Gets or sets the docking state of the DockableWindow.</summary>
            internal Inventor.DockingStateEnum DockingState
            {
                get { return dockableWindow.DockingState; }
                set { dockableWindow.DockingState = value; }
            }

            internal bool Visible
            {
                get { return dockableWindow.Visible; }
                set { dockableWindow.Visible = value; }
            }

            // This event handler will be called when a handle is create for the managed form.
            void OnHandleCreated(object sender, EventArgs e)
            {
                // The form's handle was created so add it to the dockable window.
                dockableWindow.AddChild(((Form)sender).Handle);
            }

            // This event handler will be called when the form is shown or hidden.
            void OnVisibleChanged(object sender, EventArgs e)
            {
                // Set the Visible state of the Dockable Window to match the form.
                dockableWindow.Visible = ((Form)sender).Visible;
            }

            /// <summary>
            /// Releases unmanaged resources used by the <see cref="Component"/> and optionally
            /// releases the Inventor.DockableWindow.
            /// </summary>
            /// <param name="disposing">true to release the Dockable Window.</param>
            protected override void Dispose(bool disposing)
            {
                // Let the base do its clean-up.
                base.Dispose(disposing);

                if (disposing)
                {
                    // If disposing, we should release the dockable window.
                    Inventor.DockableWindow wnd = dockableWindow;
                    dockableWindow = null; // Clear the field so we can't release it twice.
                    if (wnd != null)
                    {
                        wnd.Visible = false;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(wnd);
                    }
                }
            }

            #region IWin32Window Members

            /// <summary>Gets a handle for the dockable window.</summary>
            IntPtr IWin32Window.Handle
            {
                get { return new IntPtr(dockableWindow.HWND); }
            }

            #endregion
        }
        #endregion


        #endregion

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SamplesDockWnd));
            this.panelMain = new System.Windows.Forms.Panel();
            this._tvSamples = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this._picBox = new System.Windows.Forms.PictureBox();
            this._bExecute = new System.Windows.Forms.Button();
            this._tbDesc = new System.Windows.Forms.TextBox();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._picBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this._tvSamples);
            this.panelMain.Controls.Add(this._picBox);
            this.panelMain.Controls.Add(this._bExecute);
            this.panelMain.Controls.Add(this._tbDesc);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(328, 647);
            this.panelMain.TabIndex = 1;
            // 
            // _tvSamples
            // 
            this._tvSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tvSamples.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._tvSamples.ImageIndex = 0;
            this._tvSamples.ImageList = this.imageList1;
            this._tvSamples.ItemHeight = 36;
            this._tvSamples.Location = new System.Drawing.Point(0, 101);
            this._tvSamples.Name = "_tvSamples";
            this._tvSamples.SelectedImageIndex = 0;
            this._tvSamples.Size = new System.Drawing.Size(328, 414);
            this._tvSamples.TabIndex = 6;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FolderClosed.png");
            this.imageList1.Images.SetKeyName(1, "FolderOpen.png");
            this.imageList1.Images.SetKeyName(2, "FeatDef1.ico");
            // 
            // _picBox
            // 
            this._picBox.Dock = System.Windows.Forms.DockStyle.Top;
            this._picBox.Location = new System.Drawing.Point(0, 0);
            this._picBox.Name = "_picBox";
            this._picBox.Size = new System.Drawing.Size(328, 101);
            this._picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this._picBox.TabIndex = 5;
            this._picBox.TabStop = false;
            // 
            // _bExecute
            // 
            this._bExecute.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bExecute.Enabled = false;
            this._bExecute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._bExecute.Location = new System.Drawing.Point(0, 515);
            this._bExecute.Name = "_bExecute";
            this._bExecute.Size = new System.Drawing.Size(328, 33);
            this._bExecute.TabIndex = 3;
            this._bExecute.Text = "Execute ...";
            this._bExecute.UseVisualStyleBackColor = true;
            this._bExecute.Click += new System.EventHandler(this.bExecute_Click);
            // 
            // _tbDesc
            // 
            this._tbDesc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._tbDesc.Cursor = System.Windows.Forms.Cursors.Default;
            this._tbDesc.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._tbDesc.Location = new System.Drawing.Point(0, 548);
            this._tbDesc.Multiline = true;
            this._tbDesc.Name = "_tbDesc";
            this._tbDesc.ReadOnly = true;
            this._tbDesc.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._tbDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._tbDesc.Size = new System.Drawing.Size(328, 99);
            this._tbDesc.TabIndex = 2;
            this._tbDesc.Text = "\r\nDescription:";
            // 
            // SamplesDockWnd
            // 
            this.ClientSize = new System.Drawing.Size(328, 647);
            this.ControlBox = false;
            this.Controls.Add(this.panelMain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SamplesDockWnd";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ClientGraphics Samples";
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._picBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.TreeView _tvSamples;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox _picBox;
        private System.Windows.Forms.Button _bExecute;
        private System.Windows.Forms.TextBox _tbDesc;
    }
}