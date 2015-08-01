using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TelegramDemo.Core;
using TelegramDemo.Common;
using TelegramDemo.Util;
using System.Data;

namespace TelegramDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Fields
        
        const int DISTANCE_BETWEEN_FU_UI_DEFAULT = 150;
        const int FU_TOP_UI = 10;
        const int DISTANCE_BETWEEN_FU_AND_BOTTOM_LINE = 150;
        const int DISTATNC_FU_STATE_OFFSET = 10;
        const int SEQUENCE_STEP_X = 5;
        const int MULTI_TELEGRAMS_ANIMATION_SPEED_FACTOR = 2;
        const int SEQUENCE_STEP_Y_OFFSET = 50;
        const int FU_GROUP_CANVAS_WIDTH = 838;

        private int distanceBetweenFU = DISTANCE_BETWEEN_FU_UI_DEFAULT;

        private double AnimationSpeed = 0.2;

        private Label seqStepTitle = new Label();

        SequenceStep step;
        private int sequenceStepCounter = 1;

        Dictionary<string, FunctionUnit> fuDic;
        Dictionary<string, LineGeometry> arcnetDic = new Dictionary<string, LineGeometry>();
        TelegramGroup tg;
        SequenceStepGroup ssg;

        List<Telegram> dequeuedTelegrams = new List<Telegram>();

        private const double OriginalSeqStepCanvasHeight = 446;

        #endregion

        #region Events

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            #region obsolete Animation code     

            //Canvas.SetLeft(t1.Shape, 10);

            //NameScope.SetNameScope(this, new NameScope());
            //this.RegisterName(t1.Shape.Name, t1.Shape);

            //DoubleAnimation ani1 = new DoubleAnimation();
            //ani1.From = 0;
            //ani1.To = 25;
            //ani1.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            //Storyboard.SetTargetName(ani1, t1.Shape.Name);
            //Storyboard.SetTargetProperty(ani1, new PropertyPath(Rectangle.WidthProperty));

            //Storyboard sb = new Storyboard();
            //sb.Children.Add(ani1);

            //sb.Begin(this);

            #endregion

            InitializeSeuqenceList();

            InitializeSequenceStepTitle();

        }

        private void btnLoadSequence_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSequences.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a sequence");
                return;
            }

            CleanupCanvas(c1);
            CleanupCanvas(c3);

            ResetTelegramContent();

            string sequenceFileName = cmbSequences.SelectedValue.ToString();

            fuDic = FunctionUnitBuilder.CreateFunctionUnits(sequenceFileName);
            tg = TelegramGroupBuilder.CreateTelegramGroup(sequenceFileName, fuDic);
            ssg = SequenceStepBuilder.CreateSequenceStepGroup(sequenceFileName);

            arcnetDic.Clear();
            distanceBetweenFU = FU_GROUP_CANVAS_WIDTH / fuDic.Keys.Count;

            DrawFUandArcnetLine();

            DrawArcnetMainLine();

            LoadSequenceSteps();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (null == tg)
            {
                MessageBox.Show("No Sequence Loaded");
                return;

            }

            if (tg.NoMoreTelegrams)
            {
                MessageBox.Show("Sequence Complete");
                return;
            }

            ResetFUTelegrams();
    
            dequeuedTelegrams = tg.PopTelegrams();

            if (dequeuedTelegrams.Count > 0)
            {
                dequeuedTelegrams[0].SenderFU.UpdateSenderFUState();

                ssg.SetSequnceStepState(dequeuedTelegrams[0].SequenceStepCategory, StepState.Started);
            }

            ProcessMultiTelegrams(dequeuedTelegrams);

            foreach (Telegram t in dequeuedTelegrams)
            {
                t.ReceiverFU.UpdateReceiverFUState(t.TelegramName, t.Paras);
                ShowTelegramContent(t.ContentDS);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnimationSpeed = Math.Round(0.2 * (int)e.NewValue, 1);
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if(tg == null)
            {
                MessageBox.Show("No Sequence loaded yet");
                return;
            }

            tg.BackToPreviousTelegram();

            //reset ui
            ResetTelegramContent();
            ResetFUTelegrams();

            //reset Sequence Steps.
            ResetSequenceStepsForRollBackTelegrams();
        }

        private void step_StepSelected(object sender, string e)
        {
            string stepName = e;

            btnLoadSequence_Click(this, null);

            List<Telegram> tgs = tg.PopTelegrams();
            while(!tg.NoMoreTelegrams && tgs[0].SequenceStepCategory != stepName)
            {
                tgs = tg.PopTelegrams();
                ssg.SetSequnceStepState(tgs[0].SequenceStepCategory, StepState.Started);
            }
        }

        #endregion

        #region Initialization Method

        private void InitializeSeuqenceList()
        {
            string[] sequenceFiles = System.IO.Directory.GetFiles(@".\Sequences\", "*.xml");

            if (sequenceFiles.Length > 0)
            {
                foreach (string file in sequenceFiles)
                {
                    cmbSequences.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                }
            }
        }

        private void InitializeSequenceStepTitle()
        {
            seqStepTitle.Content = "Sequence Steps";
            seqStepTitle.FontFamily = new FontFamily("Times New Roman");
            seqStepTitle.FontSize = 16;
            seqStepTitle.FontWeight = FontWeights.Bold;

            this.c3.Children.Add(seqStepTitle);
            Canvas.SetLeft(seqStepTitle, SEQUENCE_STEP_X);
            Canvas.SetTop(seqStepTitle, 10);
        }

        private void LoadSequenceSteps()
        {
            ResetSequenceStepsForSequenceReload();

            SequenceStep step = null;

            while (!ssg.NoMoreSteps)
            {
                step = ssg.PopSequenceStep();
                step.StepSelected += step_StepSelected;
                if (c3.Height < SEQUENCE_STEP_Y_OFFSET * sequenceStepCounter)
                {
                    c3.Height = c3.Height + SEQUENCE_STEP_Y_OFFSET;
                }

                Canvas.SetLeft(step.Shape, SEQUENCE_STEP_X);
                Canvas.SetTop(step.Shape, SEQUENCE_STEP_Y_OFFSET * sequenceStepCounter++);
                c3.Children.Add(step.Shape);
            }

        }

        private void ResetSequenceStepsForRollBackTelegrams()
        {
            foreach(SequenceStep ss in ssg.GetSequenceSteps())
            {
                ss.SetState(StepState.NotStarted);
            }

            foreach(List<Telegram> tgs in tg.GetSentOutTelegrams())
            {
                ssg.SetSequnceStepState(tgs[0].SequenceStepCategory, StepState.Started);
            }
        }

        private void ResetSequenceStepsForSequenceReload()
        {
            this.c3.Height = OriginalSeqStepCanvasHeight;

            this.c3.Children.Add(seqStepTitle);
            Canvas.SetLeft(seqStepTitle, SEQUENCE_STEP_X);
            Canvas.SetTop(seqStepTitle, 10);

            sequenceStepCounter = 1;

            if (ssg != null)
            {
                foreach (SequenceStep ss in ssg.GetSequenceSteps())
                {
                    ss.StepSelected -= step_StepSelected;
                }
            }
        }

        private void ResetFUTelegrams() 
        {
            foreach (FunctionUnit fu in fuDic.Values)
            {
                fu.ResetFUState();
            }

            if (dequeuedTelegrams.Count > 0)
            {
                foreach (Telegram t in dequeuedTelegrams)
                {
                    this.c1.Children.Remove(t.Shape);
                }
            }
        }

        #endregion

        #region UI Drawing and Setting

        private void CleanupCanvas(Canvas c1)
        {
            List<UIElement> tobeCleared = new List<UIElement>();
            foreach(UIElement uie in c1.Children)
            {
                if (uie is Label || uie is Path || uie is TextBlock)
                    tobeCleared.Add(uie);
            }
            foreach(UIElement uie in tobeCleared)
            {
                c1.Children.Remove(uie);
            }
        }

        private void DrawFUandArcnetLine()
        {
            int fuLocationStep = 0;

            foreach (FunctionUnit fu in fuDic.Values)
            {
                this.c1.Children.Add(fu.FULabel);
                this.c1.Children.Add(fu.StateText);
                Canvas.SetLeft(fu.FULabel, fuLocationStep * distanceBetweenFU + DISTATNC_FU_STATE_OFFSET);
                Canvas.SetTop(fu.FULabel, FU_TOP_UI);
                Canvas.SetLeft(fu.StateText, fuLocationStep * distanceBetweenFU + DISTATNC_FU_STATE_OFFSET + fu.FULabel.Width);
                Canvas.SetTop(fu.StateText, FU_TOP_UI);
                fuLocationStep++;

                Path p = new Path();
                p.StrokeThickness = 2;
                p.Stroke = Brushes.Black;
                LineGeometry arcnetLine = new LineGeometry();
                double tempLeft = 0;
                double tempTop = 0;
                double.TryParse(fu.FULabel.GetValue(Canvas.LeftProperty).ToString(), out tempLeft);
                double.TryParse(fu.FULabel.GetValue(Canvas.TopProperty).ToString(), out tempTop);
                double left = tempLeft + fu.FULabel.Width / 2;
                double top = tempTop + fu.FULabel.Height;
                arcnetLine.StartPoint = new Point(left, top);
                arcnetLine.EndPoint = new Point(left, top + DISTANCE_BETWEEN_FU_AND_BOTTOM_LINE);
                p.Data = arcnetLine;
                arcnetDic.Add(fu.APId, arcnetLine);
                this.c1.Children.Add(p);
            }
        }

        private void DrawArcnetMainLine()
        {
            Path p2 = new Path();
            p2.StrokeThickness = 2;
            p2.Stroke = Brushes.Black;
            LineGeometry mainArcnetLine = new LineGeometry();
            mainArcnetLine.StartPoint = arcnetDic.Values.First<LineGeometry>().EndPoint;
            mainArcnetLine.EndPoint = arcnetDic.Values.Last<LineGeometry>().EndPoint;
            p2.Data = mainArcnetLine;
            this.c1.Children.Add(p2);
        }

        #endregion

        #region Telegram Content Detail Show

        private void ShowTelegramContent(DataSet dsContent)
        {
            dgTelegramContent.Visibility = System.Windows.Visibility.Visible;

            dgTelegramContent.ItemsSource = dsContent.Tables[0].DefaultView;
            dgTelegramContent.Columns[1].Width = 500;

            DataGridTextColumn colValue = (DataGridTextColumn)dgTelegramContent.Columns[1];

            Style customStyle = new Style(typeof(TextBlock));
            Setter colSetter = new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            customStyle.Setters.Add(colSetter);
            colValue.ElementStyle = customStyle;
        }

        private void ResetTelegramContent()
        {
            dgTelegramContent.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region Telegram Operation on UI

        private void ProcessMultiTelegrams(List<Telegram> tList)
        {
            Telegram t0 = tList[0];
            Point startPoint1 = arcnetDic[t0.SenderAPId].StartPoint;
            Point endPoint1 = arcnetDic[t0.SenderAPId].EndPoint;
            double startX = startPoint1.X - t0.Shape.Width / 2;
            double startY = startPoint1.Y;

            foreach (Telegram t1 in tList)
            {
                this.c1.Children.Add(t1.Shape);
                Canvas.SetLeft(t1.Shape, startX);
                Canvas.SetTop(t1.Shape, startY);
            }

            double step1Y = endPoint1.Y - startY - t0.Shape.Height / 2;

            MultiMoveFromSenderToSenderNodeOnBus(tList, 0, step1Y);

            MultiMoveFromSenderNodeToReceiverNodeOnBus(tList, step1Y);

            MultiMoveFromReciverNodeOnBusToReceiver(tList, step1Y);
        }

        private void MultiMoveFromSenderToSenderNodeOnBus(List<Telegram> tList, double targetX, double targetY)
        {
            TranslateTransform ttf = new TranslateTransform();

            for (int i = 1; i < targetY / AnimationSpeed; i++)
            {
                ttf.X = targetX;
                ttf.Y = AnimationSpeed * i;
                foreach (Telegram t in tList)
                {
                    t.Shape.RenderTransform = ttf;
                    System.Windows.Forms.Application.DoEvents();
                }
            }
        }

        private void MultiMoveFromSenderNodeToReceiverNodeOnBus(List<Telegram> tList, double targetY)
        {
            double maxX = 0;
            foreach (Telegram t in tList)
            {
                Point startPoint2 = arcnetDic[t.SenderAPId].EndPoint;
                Point endPoint2 = arcnetDic[t.ReceiverAPId].EndPoint;
                double step2X = endPoint2.X - startPoint2.X;
                t.Step2X = step2X;

                if (Math.Abs(t.Step2X) > maxX)
                {
                    maxX = Math.Abs(t.Step2X);
                }
            }

            int animationSpeedFactor = tList.Count * MULTI_TELEGRAMS_ANIMATION_SPEED_FACTOR;

            for (int i = 1; i < maxX / AnimationSpeed * animationSpeedFactor; i++)
            {
                foreach (Telegram t in tList)
                {
                    TranslateTransform ttf = new TranslateTransform();

                    if (AnimationSpeed * animationSpeedFactor * i > Math.Abs(t.Step2X))
                        continue;

                    if (t.Step2X > 0)
                        ttf.X = AnimationSpeed * animationSpeedFactor * i;
                    else
                        ttf.X = -AnimationSpeed * animationSpeedFactor * i;
                    ttf.Y = targetY;
                    t.Shape.RenderTransform = ttf;
                    System.Windows.Forms.Application.DoEvents();
                }
            }
        }

        private void MultiMoveFromReciverNodeOnBusToReceiver(List<Telegram> tList, double targetY)
        {
            int animationSpeedFactor = tList.Count * MULTI_TELEGRAMS_ANIMATION_SPEED_FACTOR;

            for (int i = 1; i < (targetY / (AnimationSpeed * animationSpeedFactor) + 1); i++)
            {
                foreach (Telegram t1 in tList)
                {
                    //must be here, if share one Transform, there will be flashing, seems telegram will use another one's transform.....
                    TranslateTransform ttf = new TranslateTransform();

                    ttf.X = t1.Step2X;
                    ttf.Y = targetY - AnimationSpeed * animationSpeedFactor * i;

                    t1.Shape.RenderTransform = ttf;
                    System.Windows.Forms.Application.DoEvents();
                }
            }
        }

        #endregion

    }
}
