using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TelegramDemo.Common;

namespace TelegramDemo.Core
{
    public class SequenceStep
    {
        private string stepName;

        private TextBlock stepLabel = new TextBlock();

        public event EventHandler<string> StepSelected;

        public SequenceStep(string name)
        {
            stepName = name;
            stepLabel.Text = name;
            stepLabel.Background = Brushes.SkyBlue;
            stepLabel.Width = 100;
            stepLabel.TextWrapping = System.Windows.TextWrapping.Wrap;
            SetState(StepState.NotStarted);

            stepLabel.MouseLeftButtonDown += stepLabel_MouseLeftButtonDown;
        }

        void stepLabel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(StepSelected != null)
            {
                StepSelected.Invoke(this, StepName);
            }
        }

        public void SetState(StepState state)
        {
            switch(state)
            {
                case StepState.NotStarted:
                    stepLabel.Opacity = 0.3;
                    break;
                case StepState.Started:
                    stepLabel.Opacity = 1;
                    break;
                default:
                    break;
            }
        }

        public TextBlock Shape
        { get { return stepLabel; } }

        public string StepName
        { get { return stepName; } }
        
    }
}
