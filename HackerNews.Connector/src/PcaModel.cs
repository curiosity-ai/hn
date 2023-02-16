using Microsoft.ML;
using Microsoft.ML.Data;
using static Microsoft.ML.PcaCatalog;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UID;
using Curiosity.Library;

namespace HackerNews
{
    internal static class PcaModel
    {
        public static bool IsAvailable => _model is object;
        private static MLContext _context = new MLContext();
        private static ITransformer _model;
        private const int Dimensions = 128;

        internal static void Build(string modelPath, Dictionary<UID128, float[]> precomputed)
        {
            var inputData = _context.Data.LoadFromEnumerable(precomputed.Values.Select(v => new InputData() {  Input = v }));

            var analyzer = _context.Transforms.ProjectToPrincipalComponents("PCA", "Input", rank: Dimensions);
            _model = analyzer.Fit(inputData);

            using (var f = File.OpenWrite(modelPath))
            {
                _context.Model.Save(_model, inputData.Schema, f);
            }
        }

        internal static void Load(string modelPath)
        {
            using (var f = File.OpenRead(modelPath))
            {
                _model = _context.Model.Load(f, out var inputSchema);
            }
        }

        public static float[] Apply(float[] input)
        {
            var inputData = _context.Data.LoadFromEnumerable(new InputData[] { new InputData() { Input = input } });
            var outputData = _model.Transform(inputData);
            var outputCol = outputData.GetColumn<float[]>("PCA");
            return outputCol.Single();
        }

        internal static void Apply(List<NodeAndVector> vectors)
        {
            var inputData = _context.Data.LoadFromEnumerable(vectors.Select(v => new InputData() {  Input = v.V}));
            var outputData = _model.Transform(inputData);
            var outputCol = outputData.GetColumn<float[]>("PCA");
            int i = 0;
            foreach(var oc in outputCol)
            {
                vectors[i].V = oc;
            }
        }

        public class InputData
        {
            [VectorType(512)]
            public float[] Input { get; set; }
        }
    }
}