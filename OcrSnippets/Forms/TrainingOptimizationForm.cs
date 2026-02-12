using log4net;
using MathNet.Numerics.LinearAlgebra.Double;
using Ocr.Engine;
using Ocr.Learning;
using Ocr.Optimization;
using Ocr.Recognition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Ocr.Helper;
using Ocr.Model;
using Tct.ActivityRecorderService.Ocr;

namespace OcrConfig.Forms
{
    public partial class TrainingOptimizationForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TrainingOptimizationForm));
        private static readonly DataContractSerializer serializer = new DataContractSerializer(typeof(SwarmState));
        private readonly OcrConfiguration config;
        private readonly SynchronizationContext context = SynchronizationContext.Current;
        private readonly double referenceFitness;
        private readonly Dictionary<Bitmap, string> samples;
        private readonly BackgroundWorker worker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        private DenseVector best;
        private DisplayMode displayMode;
        private string saveTo;
	    private RecognitionProcessor contentProcessor;

        public static event EventHandler<TrainingResultEventArgs> OnFinished;

        public TrainingOptimizationForm()
        {
            InitializeComponent();
        }

        public static double RefineMetric(DenseVector dv, SampleStorage samples, OcrConfiguration config)
        {
            string learningText = LearningHelper.GetLearningText(config.CharSet);
            var c = OcrLearnMetric.Create(dv, config, learningText, samples.GetSamples(),
                LearningHelper.GetFont((LearningHelper.OcrFontFamily)(int)dv[13], (float)dv[12]));
            var res = OcrLearnMetric.Evaluate(c);
            return (res);
        }

        public TrainingOptimizationForm(SampleStorage samples, OcrConfiguration config, DisplayMode displayMode = DisplayMode.DetailedMode, DenseVector denseVector = null)
            : this()
        {
            this.config = config;
            this.samples = samples.GetSamples();

            referenceFitness = this.samples.Select(x => Math.Sqrt(x.Value.Length)).Sum();
            lblSamples.Text = samples.Count.ToString();
            btnChangeDisplayMode.Text = displayMode == DisplayMode.ShowProgressOnly
                ? "FS"
                : "PB";
            this.displayMode = displayMode;
			contentProcessor = new RecognitionProcessor(config.ContentRegex, config.IgnoreCase);

            Load += (sender, args) =>
            {
                pnlDetails.Visible = this.displayMode == DisplayMode.DetailedMode;
                btnSave.Visible = this.displayMode == DisplayMode.DetailedMode;
                var learningText = LearningHelper.GetLearningText(config.CharSet);
				//check CharSet validity
				var invalidSamples = samples.GetSamples().Values.Where(content => LearningHelper.IsInputInvalid(config.CharSet, content));
				if (invalidSamples.Any())
				{
					MessageBox.Show("Selected CharSet is not compatible with the given contents!", "Error");
					Close();
					return;
				}
                var channelMax = Enum.GetValues(typeof(ImageHelper.DesaturateMode)).Cast<int>().Max() + 1;
                var fontFamilyMax = Enum.GetValues(typeof(LearningHelper.OcrFontFamily)).Cast<int>().Max() + 1;
                var lowerLimits = new DenseVector(new[] { 0.7, 0.7, 2.0, 0.0, 0.0, 0.0, 0.7, 0.7, 0.0, 0.0, 1.0, 0.0, 8.0, 0.0 });
                var upperLimits = new DenseVector(new[] { 1.3, 1.3, 4.0, 256.0, channelMax, 5.0, 1.3, 1.3, channelMax, 256.0, 4.0, 5.0, 48.0, fontFamilyMax });
                Func<DenseVector, OcrLearnMetric> p;
                if (denseVector == null)
                    p = (x) => OcrLearnMetric.Create(x, config, learningText, samples.GetSamples(),
                        LearningHelper.GetFont((LearningHelper.OcrFontFamily)(int)x[13], (float)x[12]));
                else
                    p = (x) => OcrLearnMetric.Create(denseVector, config, learningText, samples.GetSamples(),
                        LearningHelper.GetFont((LearningHelper.OcrFontFamily)(int)denseVector[13], (float)denseVector[12]));
                var swarm = ParticleSwarmFactory.Minimize(
                        (x) => p(x),
                        OcrLearnMetric.Evaluate,
                        lowerLimits,
                        upperLimits);
                StartSwarm(swarm);
            };
            DialogResult = DialogResult.No;
        }
        // directly called for continuing optimization
        public TrainingOptimizationForm(SampleStorage samples, OcrConfiguration config, string fileName)
            : this(samples, config, DisplayMode.DetailedMode)
        {
            Load += (sender, args) =>
            {
                using (var reader = XmlReader.Create(fileName))
                {
                    if (serializer.ReadObject(reader) is SwarmState) return;
                    MessageBox.Show("Error reading file");
                    DialogResult = DialogResult.Abort;
                    Close();
                }
            };
        }

        private void StartSwarm(ParticleSwarm<OcrLearnMetric> swarm)
        {
            worker.DoWork += (s, e) =>
            {
                var senderWorker = (BackgroundWorker)s;
                while (!worker.CancellationPending)
                {
                    swarm.Iterate();
                    if (!worker.CancellationPending)
                        senderWorker.ReportProgress(0, swarm);
                }
            };

            worker.ProgressChanged += (s, e) =>
            {
                var state = (ParticleSwarm<OcrLearnMetric>)e.UserState;
                var res = EvaluateAndUpdateInfo(state);
                if (res == 0)
                    worker.CancelAsync();
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                var swarmState = swarm.GetState();
                saveTo = Path.Combine(@".\\", Path.GetFileNameWithoutExtension(config.DestinationLanguageFile) + ".traineddata");
                using (var writer = XmlWriter.Create(saveTo))
                {
                    serializer.WriteObject(writer, swarmState);
                }
                saveTo = null;
                TransformConfigurationExt.Save(swarmState.BestPosition);

                var handler = OnFinished;
                if (handler != null)
                    handler(this, new TrainingResultEventArgs(config));
                else
                {
                    var lf = config.Language;       // swap Language during serialization
                    config.Language = config.DestinationLanguageFile;
                    Clipboard.SetText(config.ToString());
                    config.Language = lf;
                    context.Post(_ =>
                    {
                        if (displayMode == DisplayMode.DetailedMode)
                        {
                            // iteration finished normally
                            MessageBox.Show("Save trained data for further processing.\r\nResult is available in clipboard.\r\nYou can paste (Ctrl-V) it or you can click on Display Result button.", "Optimization Finished", MessageBoxButtons.OK);
                            DialogResult = DialogResult.None;
                        }
                        else
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }, null);
                }
            };
            worker.RunWorkerAsync();
        }
        protected override void OnClosed(EventArgs e)
        {
            worker.CancelAsync();
            base.OnClosed(e);
        }
        private double EvaluateAndUpdateInfo(ParticleSwarm<OcrLearnMetric> swarm)
        {
            dgResults.Rows.Clear();
            var errorSum = 0.0;
            var looseSum = 0.0;
            var totalCount = 0;
            var looseCount = 0;
            best = swarm.BestPosition;
            if (best == null || swarm.Best == null) return 0d;
            foreach (var result in swarm.Best.Results)
            {
                var expected = contentProcessor.Process(result.Key);
                var evaluated = contentProcessor.Process(result.Value);
                var error = RecognitionService.EvaluateResult(evaluated, expected);
                var looseError = RecognitionService.EvaluateResult(evaluated.Replace(" ", "").ToLower(),
                    expected.Replace(" ", "").ToLower());
                if (looseError <= 0.0)
                    looseCount++;

                if (error <= 0.0)
                    totalCount++;

                errorSum += error;
                looseSum += looseError;
                dgResults.Rows.Add(evaluated, expected, error, (1 - error / expected.Length).ToString("P1"),
                    (1 - looseError / expected.Length).ToString("P1"));

                lblCharAccuracy.Text = string.Format("{0:P} ({1:P})", 1 - errorSum / samples.Select(x => x.Value.Length).Sum(),
                    1 - looseSum / samples.Select(x => x.Value.Length).Sum());
                lblSampleAccuracy.Text = string.Format("{0:P} ({1:P})", totalCount / (double)samples.Count,
                    looseCount / (double)samples.Count);
            }

            lblLearnP1.Text = "Brightness: " + (float)swarm.BestPosition[6];
            lblLearnP2.Text = "Contrast: " + (float)swarm.BestPosition[7];
            lblLearnP4.Text = "Treshold mode: " + (ImageHelper.DesaturateMode)(int)swarm.BestPosition[8];
            lblLearnP5.Text = "TresholdLimit limit: " + (byte)swarm.BestPosition[9];
            lblLearnP3.Text = "Scale: " + swarm.BestPosition[10];
            lblLearnP6.Text = "Interpolation: " + ImageHelper.ToInterpolationMode(swarm.BestPosition[11]);
            lblLearnP8.Text = "Font size: " + swarm.BestPosition[12];
            lblLearnP7.Text = "Font: " + (LearningHelper.OcrFontFamily)(int)swarm.BestPosition[13];
            lblLearnP9.Text = "Characters: " + swarm.Best.LearningString;
            lblRecogP1.Text = "Brightness: " + (float)swarm.BestPosition[0];
            lblRecogP2.Text = "Contrast: " + (float)swarm.BestPosition[1];
            lblRecogP3.Text = "Scale: " + swarm.BestPosition[2];
            lblRecogP5.Text = "TresholdLimit value: " + (byte)swarm.BestPosition[3];
            lblRecogP4.Text = "Treshold mode: " + (ImageHelper.DesaturateMode)(int)swarm.BestPosition[4];
            lblRecogP6.Text = "Interpolation: " + ImageHelper.ToInterpolationMode(swarm.BestPosition[5]);
            config.EngineParameters = swarm.BestPosition.ToArray();

            pAccuracy.Value = Math.Max(Math.Min((int)(100 * (1 + swarm.BestFitness / referenceFitness)), 100), 0);
            lblIterations.Text = swarm.Iterations.ToString();
            lblSpeed.Text = (swarm.Best.EvaluationTime / swarm.Best.Results.Count).ToString("F1") + " ms";
            lblTrainSpeed.Text = swarm.Best.TrainTime.ToString("F1") + " ms";
            log.InfoFormat("Best match: {0}", swarm.BestPosition);
            return errorSum;
        }

        private void HandleExportClicked(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Trained data|*.traineddata";
                dialog.FileName = config.DestinationLanguageFile;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var learningText = LearningHelper.GetLearningText(config.CharSet);
                    var transConfig = new TransformConfiguration(best)
                    {
                        BrightnessCorrection = (float)best[6],
                        ContrastCorrection = (float)best[7],
                        TresholdChannel = (ImageHelper.DesaturateMode)(int)best[8],
                        TresholdLimit = best[9] > 50 ? (byte?)best[9] : null,
                        Scale = best[10],
                        InterpolationMode = ImageHelper.ToInterpolationMode(best[11]),
                        Language = Path.GetFileNameWithoutExtension(dialog.FileName)
                    };
					try
					{
						TesseractEngineEx.CreateTrainData(learningText, LearningHelper.GetFont((LearningHelper.OcrFontFamily)(int)best[13], (float)best[12]), transConfig, dialog.FileName);
					}
					catch (IOException ex)
					{
						MessageBox.Show(ex.Message, "Exception occured", MessageBoxButtons.OK);
					}
                    if (!worker.IsBusy)
                        Close();
                }
            }
        }

	    private void btnExportAll_Click(object sender, EventArgs e)
	    {
		    using (var dialog = new SaveFileDialog())
		    {
			    dialog.Filter = "Trained data|*.traineddata";
			    dialog.FileName = config.DestinationLanguageFile;
			    if (dialog.ShowDialog(this) == DialogResult.OK)
			    {
				    var learningText = LearningHelper.GetLearningText(config.CharSet);
				    var transConfig = new TransformConfiguration(best)
				    {
					    BrightnessCorrection = (float) best[6],
					    ContrastCorrection = (float) best[7],
					    TresholdChannel = (ImageHelper.DesaturateMode) (int) best[8],
					    TresholdLimit = best[9] > 50 ? (byte?) best[9] : null,
					    Scale = best[10],
					    InterpolationMode = ImageHelper.ToInterpolationMode(best[11]),
					    Language = config.Language
				    };
				    var fontList = new List<Font>();
				    for (int i = 0; i <= 7; i++)
				    {
					    fontList.Add(LearningHelper.GetFont((LearningHelper.OcrFontFamily) i, (float) best[12]));
				    }
				    try
				    {
					    TesseractEngineEx.CreateCombinedTrainData(learningText, fontList, transConfig,
						    dialog.FileName.Replace(".traineddata", (int)config.CharSet + ".traineddata"));
				    }
				    catch (IOException ex)
				    {
					    MessageBox.Show(ex.Message, "Exception occured", MessageBoxButtons.OK);
				    }
			    }
		    }
	    }

        private void HandleSaveClicked(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Optimization data|*.sas";
                dialog.Title = "Save swarm state";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    saveTo = dialog.FileName;
            }
        }

        private void ChangeDisplayMode_Click(object sender, EventArgs e)
        {
            if (displayMode == DisplayMode.ShowProgressOnly)
            {
                displayMode = DisplayMode.DetailedMode;
                pnlDetails.Visible = true;
                btnChangeDisplayMode.Text = "PB";
            }
            else
            {
                displayMode = DisplayMode.ShowProgressOnly;
                pnlDetails.Visible = false;
                btnChangeDisplayMode.Text = "FS";
            }
        }
        private void pnlDetails_VisibleChanged(object sender, EventArgs e)
        {
            var s = ClientSize;
            SuspendLayout();
            ClientSize = pnlDetails.Visible
                ? new Size(s.Width, Math.Min(s.Height + pnlDetails.MaximumSize.Height, pnlDetails.MaximumSize.Height))
                : new Size(s.Width, pnlProgress.MinimumSize.Height + pnlProgress.Margin.Top + pnlProgress.Margin.Bottom);
            ResumeLayout(true);
        }
    }

    public class TrainingResultEventArgs : EventArgs
    {
        public OcrConfiguration Config { private set; get; }

        public TrainingResultEventArgs(OcrConfiguration config)
        {
            Config = config;
        }
    }
}