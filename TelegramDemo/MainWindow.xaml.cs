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
using TelegramDemo.Common;
using TelegramDemo.Common;
using TelegramDemo.Util;
using System.Data;
using TelegramDemo.Core;

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

            InitializeSeuqenceComboList();

            InitializeSequenceStepTitle();
        }

        #endregion

        #region Fields

        const int DISTANCE_FU_BETWEEN = 150;
        const int DISTANCE_FU_AND_BOTTOM_LINE_BETWEEN = 150;
        const int DISTANCE_FU_STATE_TEXT_OFFSET = 10;
        const int WIDTH_FU_GROUP_CANVAS = 838;
        const int TOP_OFFSET = 10;

        const int SEQUENCE_STEP_X = 5;
        const int SEQUENCE_STEP_Y_OFFSET = 50;

        const int MULTI_TELEGRAMS_ANIMATION_SPEED_FACTOR = 2;
        const double DEFAULT_ANIMATION_SPEED = 0.2;

        const double HEIGHT_SEQUENCE_STEP_GROUP_CANVAS = 446;

        //TODO:
        //private int distanceBetweenFU = DISTANCE_FU_BETWEEN;
        private double AnimationSpeed = DEFAULT_ANIMATION_SPEED;

        private Label seqStepTitle = new Label();

        private TelegramGroup tg;
        private SequenceStepGroup ssg;
        private FunctionUnitGroup fug;

        private Dictionary<string, LineGeometry> arcnetDic = new Dictionary<string, LineGeometry>();

        //TODO:
        private int sequenceStepCounter = 1;
        private List<Telegram> dequeuedTelegrams = new List<Telegram>();

        #endregion

        #region Events

        private void btnLoadSequence_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSequences.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a sequence");
                return;
            }

            string sequenceFileFullName = string.Format(@".\Sequences\{0}.xml", cmbSequences.SelectedValue.ToString());
            RefreshSequenceStepGroup(sequenceFileFullName);
            RefreshFunctionUnitAndTelegramGroup(sequenceFileFullName);

            ResetUIForSequenceLoad();

            LoadAllSequenceStepsOnUI();

            DrawFUandArcnetLine();

            DrawArcnetMainLine();
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

            fug.ResetAllFUState();

            ClearAllSentOutTelegramsOnUI();

            dequeuedTelegrams = tg.PopTelegrams();

            //TODO:
            ssg.SetSequnceStepState(dequeuedTelegrams[0].SequenceStepCategory, StepState.Started);

            ProcessMultiTelegrams(dequeuedTelegrams);

            UpdateFUStateAfterTelegramSentOut(dequeuedTelegrams);

            ShowTelegramDetail(dequeuedTelegrams[0].ContentDS);

        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (tg == null)
            {
                MessageBox.Show("No Sequence loaded yet");
                return;
            }

            tg.BackToPreviousTelegram();
            fug.ResetAllFUState();

            //reset ui
            HideTelegramContent();
            ClearAllSentOutTelegramsOnUI();

            RollBackSequenceSteps();
        }

        private void step_StepSelected(object sender, string e)
        {
            string stepName = e;

            btnLoadSequence_Click(this, null);

            List<Telegram> ts = tg.PopTelegrams();
            while (!tg.NoMoreTelegrams && ts[0].SequenceStepCategory != stepName)
            {
                ts = tg.PopTelegrams();
                ssg.SetSequnceStepState(ts[0].SequenceStepCategory, StepState.Started);
            }

            //we need to roll back to previous telegrams, since we pop out the first telegram that matching the sequence step name
            tg.BackToPreviousTelegram();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnimationSpeed = Math.Round(DEFAULT_ANIMATION_SPEED * (int)e.NewValue, 1);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
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

        #endregion

        #region Function Unit, Sequence Step Group, Telegram Group related logic

        private void RollBackSequenceSteps()
        {
            //First: reset all to Not Started
            foreach (SequenceStep ss in ssg.GetSequenceSteps())
            {
                ss.SetState(StepState.NotStarted);
            }

            //Second: update state based on sent out telegrams
            foreach (List<Telegram> tgs in tg.GetSentOutTelegrams())
            {
                ssg.SetSequnceStepState(tgs[0].SequenceStepCategory, StepState.Started);
            }
        }

        private void RefreshSequenceStepGroup(string sequenceFileFullName)
        {
            sequenceStepCounter = 1;

            if (ssg != null)
            {
                foreach (SequenceStep ss in ssg.GetSequenceSteps())
                {
                    ss.StepSelected -= step_StepSelected;
                }
            }

            ssg = SequenceStepGroupBuilder.CreateSequenceStepGroup(sequenceFileFullName);
        }

        private void RefreshFunctionUnitAndTelegramGroup(string sequenceFileFullName)
        {
            fug = FunctionUnitGroupBuilder.CreateFunctionUnits(sequenceFileFullName);
            tg = TelegramGroupBuilder.CreateTelegramGroup(sequenceFileFullName, fug.FunctionUnitDic);
        }

        private void UpdateFUStateAfterTelegramSentOut(List<Telegram> sentOutTelegrams)
        {
            foreach (Telegram t in sentOutTelegrams)
            {
                t.SenderFU.UpdateSenderFUState();
                t.ReceiverFU.UpdateReceiverFUState(t.TelegramName);
            }
        }

        #endregion

        #region UI Initialization

        private void InitializeSeuqenceComboList()
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
            Canvas.SetTop(seqStepTitle, TOP_OFFSET);
        }

        private void LoadAllSequenceStepsOnUI()
        {
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

        #endregion

        #region UI Reset

        private void ClearCanvas(Canvas c)
        {
            List<UIElement> tobeCleared = new List<UIElement>();
            foreach (UIElement uie in c.Children)
            {
                if (uie is Label || uie is Path || uie is TextBlock)
                    tobeCleared.Add(uie);
            }
            foreach (UIElement uie in tobeCleared)
            {
                c.Children.Remove(uie);
            }
        }

        private void ResetUIForSequenceLoad()
        {
            //cleanup all FU and telegram related UI
            ClearCanvas(c1);

            //cleanup all sequence step related UI
            ClearCanvas(c3);

            //cleanup grid showing current telegram content in detail
            HideTelegramContent();

            ResetSequenceStepCanvas();
        }

        private void ResetSequenceStepCanvas()
        {
            this.c3.Height = HEIGHT_SEQUENCE_STEP_GROUP_CANVAS;
            this.c3.Children.Add(seqStepTitle);
            Canvas.SetLeft(seqStepTitle, SEQUENCE_STEP_X);
            Canvas.SetTop(seqStepTitle, TOP_OFFSET);
        }

        private void ClearAllSentOutTelegramsOnUI()
        {
            foreach (Telegram t in dequeuedTelegrams)
            {
                this.c1.Children.Remove(t.Shape);
            }
        }

        #endregion

        #region UI Telegram Content Detail

        private void ShowTelegramDetail(DataSet dsContent)
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

        private void HideTelegramContent()
        {
            dgTelegramContent.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region UI Drawing FU and Lines

        private void DrawFUandArcnetLine()
        {
            arcnetDic.Clear();

            int distanceBetweenFU = WIDTH_FU_GROUP_CANVAS / fug.FUCount;
            int fuLocationStep = 0;

            foreach (FunctionUnit fu in fug.AllFUs)
            {
                this.c1.Children.Add(fu.FULabel);
                this.c1.Children.Add(fu.StateText);
                Canvas.SetLeft(fu.FULabel, fuLocationStep * distanceBetweenFU + DISTANCE_FU_STATE_TEXT_OFFSET);
                Canvas.SetTop(fu.FULabel, TOP_OFFSET);
                Canvas.SetLeft(fu.StateText, fuLocationStep * distanceBetweenFU + DISTANCE_FU_STATE_TEXT_OFFSET + fu.FULabel.Width);
                Canvas.SetTop(fu.StateText, TOP_OFFSET);
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
                arcnetLine.EndPoint = new Point(left, top + DISTANCE_FU_AND_BOTTOM_LINE_BETWEEN);
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

        #region UI Telegram Animation

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
