using System;
using System.Windows.Input;
using Eto.Forms;
using Rhino;

namespace Production_Tools.Views
{
    internal class ProductionToolsSetupPanelView : Rhino.UI.ViewModel
    {
        public ProductionToolsSetupPanelView(uint documentRuntimeSerialNumber)
        {
                  // Button commands
            NextCommand = new RelayCommand<object>(obj => { NextButtonCommand(); });
            BackCommand = new RelayCommand<object>(obj => { BackButtonCommand(); });
            FinishCommand = new RelayCommand<object>(obj => { FinishButtonCommand(); }); 
            
            DocumentRuntimeSerialNumber = documentRuntimeSerialNumber;

            m_panels = new ProductionToolsWizardEtoPanel[]
            {
                new WizardEtoPanel0(this),
                new WizardEtoPanel1(this)
            };

            Content = m_panels[0];
        }

            #region Document access
            public uint DocumentRuntimeSerialNumber { get; }
            public RhinoDoc Document => RhinoDoc.FromRuntimeSerialNumber(DocumentRuntimeSerialNumber);
            #endregion Document access

            public Control Content
            {
                get => m_content;
                set{
                    if (m_content == value)
                        return;
                    m_content = value;
                    RaisePropertyChanged(nameof(Content));
                }
            }


            private Control m_content;
            private int m_current_panel;

            private readonly ProductionToolsWizardEtoPanel[] m_panels;

            #region Next, Back and Finish commands
            private void NextButtonCommand()
            {
            if (m_current_panel >= m_panels.Length)
                return;
            Content = m_panels[++m_current_panel];
            }

            private void BackButtonCommand()
            {
            if (m_current_panel <= 0)
                return;
            Content = m_panels[--m_current_panel];
            }

            private void FinishButtonCommand()
            {
            MessageBox.Show("Finish...");
            }

            #endregion Next, Back and Finish commands

            #region Button ICommand handlers
            public ICommand NextCommand { get; }
            public ICommand BackCommand { get; }
            public ICommand FinishCommand { get; }

            #endregion Button ICommand handlers
    }
}
