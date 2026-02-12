using log4net;
using MathNet.Numerics.LinearAlgebra.Double;
using OcrConfig.Forms;
using System;
using System.IO;
using Ocr.Helper;
using TcT.ActivityRecorderClient;

namespace Ocr.Recognition
{
    public class TransformConfigurationExt
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TrainingOptimizationForm));
        private static TransformConfiguration config;
        private const string fileName = "results.json";
        public static bool HasResults { get { return File.Exists(fileName); } }

        public static bool Save(double[] v)
        {
            try
            {
                config = new TransformConfiguration(v);
                string json = JsonHelper.SerializeData(config);
                File.WriteAllText(fileName, json);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Saving TransformConfiguration", ex);
                return false;
            }
        }
        public static DenseVector Load()
        {
            try
            {
                string json = File.ReadAllText(fileName);
                TransformConfiguration config;
                JsonHelper.DeserializeData(json, out config);
                DenseVector v = config.DenseVector;
                return v;
            }
            catch (Exception ex)
            {
                log.Error("Loading TransformConfiguration", ex);
                return null;
            }
        }

        public static void Swipe()
        {
            if(HasResults) File.Delete(fileName);
        }
    }
}
