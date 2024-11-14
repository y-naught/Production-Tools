using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.UI;
using System.ComponentModel;


namespace Production_Tools.Views
{
    class ProductionToolsLayoutSetupWizardDialog : Dialog<DialogResult>
    {

        public ProductionToolsLayoutSetupWizardDialog()
        {
            // Setup goes here
            Padding = new Padding(5);
            Resizable = false;
            Result = DialogResult.Cancel;
            Title = GetType().Name;
            WindowStyle = WindowStyle.Default;

            var add_layout_button = new Button {Text="Add New Layout"};
            add_layout_button.Click += (sender, e) => OnAddClick(e);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(DialogResult.Ok);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close(DialogResult.Cancel);

            var button_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, add_layout_button, null) }
            };

            var defaults_layout = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(5, 5),
                Rows = { new TableRow(null, DefaultButton, AbortButton, null) }
            };

            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(button_layout),
                    new TableRow(defaults_layout)
                }
            };
        }
    

    protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

    protected override void OnClosing(CancelEventArgs e)
        {
            this.SavePosition();
            base.OnClosing(e);
        }
    protected void OnAddClick(EventArgs e)
        {
            // run some code here that starts the add
            RhinoApp.WriteLine("Adding A Layout To System");
        }
    }
}