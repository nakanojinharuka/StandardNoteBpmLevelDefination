using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace Core_Advanced_13_1
{
    internal class ArtificialStaff
    {
        readonly Dictionary<string, int> Difficulty = new() { { "expert", 3 }, { "special", 4 } };
        readonly Dictionary<string, string> Distribution2 = new() { { "hit", "hitDistribution" }, { "note", "noteDistribution" } };
        public int ID { get; set; } = 0;
        public string Level { get; set; } = "";
        public int Slide { get; set; } = 0;
        public int Slideflick { get; set; } = 0;
        public int Slidenode { get; set; } = 0;
        public int Slidenode_true { get; set; } = 0;
        public double Lane_change { get; set; } = 0;
        public double Beat_change { get; set; } = 0;
        public double Left_per { get; set; } = 0;
        public double Right_per { get; set; } = 0;
        public int Flick { get; set; } = 0;
        public int Single { get; set; } = 0;
        public int Bpm { get; set; } = 0;
        public int Directional { get; set; } = 0;
        public int Directional_widthsum { get; set; } = 0;
        public double Active { get; set; } = 0;
        public double Silence { get; set; } = 0;
        public double Flick_note_interval { get; set; } = 0;
        public double Note_flick_interval { get; set; } = 0;
        public double Interval_average { get; set; } = 0;
        public double Max_Screen_NPS { get; set; } = 0;
        public double Max_Speed { get; set; } = 0;
        public double Finger_MaxNPS { get; set; } = 0;
        public ArtificialStaff(int id, string diff = "expert")
        {
            ID = id;
            Level = diff;
            int slide = 0, slideflick = 0, slidenode = 0, slidenode_true = 0;
            double lane_change = 0, beat_change = 0, left_per = 0, right_per = 0;
            int flick = 0, single = 0, bpm = 0, directional = 0, directional_widthsum = 0;
            double active = 0, silence = 0, flick_note_interval = 0, note_flick_interval = 0, interval_average = 0;
            double max_screen_nps = 0, max_speed = 0, finger_max_nps = 0;
            string URL1 = $"https://bestdori.com/api/post/details?id={id}";
            string URL2 = $"https://api.ayachan.fun/DiffAnalysis?id={id}&diff={Difficulty[diff]}&speed=1.0";
            HttpClient Client = new();
            string Content = Client.GetStringAsync(URL1).Result;
            JObject? Json_Content = (JObject?)JsonConvert.DeserializeObject(Content);
            if (Json_Content.GetValue("error") != null) return;
            JToken? Post = Json_Content.GetValue("post");
            JToken? Chart = Post["chart"];
            foreach (var item in Chart)
            {
                switch (item["type"].ToString())
                {
                    case "Slide":
                        {
                            slide++;
                            try { if ((bool)item["connections"].Last["flick"] == true) slideflick++; }
                            catch (Exception) { continue; }
                            finally
                            {
                                List<double> lanes = new(), beats = new(), lanes_changes = new(), beats_changes = new();
                                foreach (var i in item["connections"])
                                {
                                    try
                                    {
                                        lanes.Add((double)i["lane"]); beats.Add((double)i["beat"]);
                                        if ((bool)i["hidden"] == true) { slidenode_true++; continue; }
                                    }
                                    catch (Exception) { slidenode++; slidenode_true++; continue; }
                                }
                                for (int k = 0, j = 0; k < beats.Count - 1 || j < lanes.Count - 1; k++, j++)
                                {
                                    lanes_changes.Add(lanes[j + 1] - lanes[j]);
                                    beats_changes.Add(beats[k + 1] - beats[k]);
                                }
                                lane_change = lanes_changes.Sum();
                                beat_change = beats_changes.Sum();
                            }
                            break;
                        }
                    case "Single":
                    case "Long":
                        try { if ((bool)item["flick"] == true) flick++; }
                        catch (Exception) { single++; continue; }
                        break;
                    case "Directional":
                        directional++;
                        directional_widthsum += (int)item["width"];
                        break;
                    case "BPM":
                        if (bpm == 0)
                        {
                            string Content2 = Client.GetStringAsync(URL2).Result;
                            JObject? Json_Content2 = (JObject)JsonConvert.DeserializeObject(Content2);
                            bpm = (int)Json_Content2["detail"]["MainBPM"];
                            left_per = (double)Json_Content2["detail"]["LeftPercent"];
                            right_per = 1 - left_per;
                            left_per = (int)(left_per * 100);
                            right_per = (int)(right_per * 100);
                            active = (double)Json_Content2["detail"]["ActivePercent"];
                            silence = 1 - active;
                            active = (int)(active * 100);
                            silence = (int)(silence * 100);
                            flick_note_interval = (double)Json_Content2["detail"]["FlickNoteInterval"];
                            note_flick_interval = (double)Json_Content2["detail"]["NoteFlickInterval"];
                            interval_average = (int)(Math.Abs(flick_note_interval - note_flick_interval) * 100);
                            max_screen_nps = (double)Json_Content2["detail"]["MaxScreenNPS"];
                            max_speed = (double)Json_Content2["detail"]["MaxSpeed"];
                            finger_max_nps = (double)Json_Content2["detail"]["FingerMaxHPS"];
                        }
                        break;
                }
            }
            Slide = slide;
            Slideflick = slideflick;
            Slidenode = slidenode;
            Slidenode_true = slidenode_true;
            Lane_change = lane_change;
            Beat_change = beat_change;
            Left_per = left_per;
            Right_per = right_per;
            Flick = flick;
            Single = single;
            Bpm = bpm;
            Directional = directional;
            Directional_widthsum = directional_widthsum;
            Active = active;
            Silence = silence;
            Flick_note_interval = flick_note_interval;
            Note_flick_interval = note_flick_interval;
            Interval_average = interval_average;
            Max_Screen_NPS = max_screen_nps;
            Max_Speed = max_speed;
            Finger_MaxNPS = finger_max_nps;
        }
        public double[] Properties_Integration()
        {
            double x1x1 = Single; double x2x2 = Slide; double x3x3 = Flick + Slideflick; double x4x4 = Interval_average;
            double x1x2 = Slidenode_true * Lane_change; double x1x3 = Directional * Directional_widthsum;
            double x1x4 = Bpm * Beat_change; double x2x3 = Left_per * Max_Screen_NPS * Max_Speed;
            double x2x4 = Right_per * Max_Screen_NPS * Max_Speed; double x3x4 = Active * Silence * Finger_MaxNPS;
            double[,] Properties = new double[4, 4] { { x1x1, x1x2, x1x3, x1x4 }, { x1x2, x2x2, x2x3, x2x4 }, { x1x3, x2x3, x3x3, x3x4 }, { x1x4, x2x4, x3x4, x4x4 } };
            Matrix<double> PropertiesMatrix = Matrix<double>.Build.DenseOfArray(Properties);
            Vector<double> eig_judge = PropertiesMatrix.Evd(Symmetricity.Symmetric).EigenValues.Real();
            double l4 = Math.Abs(eig_judge[0]) / 100, l3 = Math.Abs(eig_judge[1]) / 10, l2 = Math.Abs(eig_judge[2]) / 10, l1 = Math.Abs(eig_judge[3]) / 100;
            //solve differentational equation l2y''+l1y'+l4y=l3(x^2).
            double Delta = l1 * l1 - 4 * l4 * l2;
            double A = l3 / l4;
            double B = -(2 * l1 * l3) / (l4 * l4);
            double C = (2 * l1 * l1 * l3 - 2 * l2 * l3 * l4) / (l4 * l4 * l4);
            double[] status_array = new double[4];
            switch (Delta)
            {
                case > 0:
                    double r1 = (-l1 + Math.Sqrt(Delta)) / (2 * l2);
                    double r2 = (-l1 - Math.Sqrt(Delta)) / (2 * l2);
                    Func<double, double> special_solution_p = (x) => 1 / 450 * Math.Exp(r1 * x) + 1 / 150 * Math.Exp(r2 * x) + A * x * x + B * x + C;
                    double Integral_P = NewtonCotesTrapeziumRule.IntegrateAdaptive(special_solution_p, 0, 0.01, 1e-14);
                    status_array[0] = Math.Exp(Integral_P);
                    status_array[1] = 1;
                    break;
                case 0:
                    double r = -l1 / (2 * l2);
                    Func<double, double> special_solution_z = (x) => (1 / 450 + 1 / 150 * x) * Math.Exp(r * x) + A * x * x + B * x + C;
                    double Integral_Z = NewtonCotesTrapeziumRule.IntegrateAdaptive(special_solution_z, 0, 0.01, 1e-14);
                    status_array[0] = Math.Exp(Integral_Z);
                    status_array[1] = 2;
                    break;
                case < 0:
                    double alpha = -l1 / (2 * l2);
                    double beta = Math.Sqrt(Math.Abs(Delta)) / (2 * l2);
                    Func<double, double> special_solution_n = (x) => 1 / 300 * Math.Exp(alpha * x) * (Math.Cos(beta * x) + Math.Sqrt(3) * Math.Sin(beta * x));
                    double Integral_N = NewtonCotesTrapeziumRule.IntegrateAdaptive(special_solution_n, 0, 0.01, 1e-14);
                    status_array[0] = Math.Exp(Integral_N);
                    status_array[1] = 3;
                    break;
                default:
                    status_array[0] = 0;
                    status_array[1] = -1;
                    break;
            }
            double a1 = PropertiesMatrix[0, 0];
            double a2 = (PropertiesMatrix[1, 1] + PropertiesMatrix[2, 2]) / 2;
            double a3 = PropertiesMatrix[3, 3];
            double a1a2 = PropertiesMatrix[0, 1] + PropertiesMatrix[0, 2];
            double a1a3 = PropertiesMatrix[1, 2] + PropertiesMatrix[0, 3];
            double a2a3 = PropertiesMatrix[1, 3] + PropertiesMatrix[2, 3];
            double k11 = a1;
            double k22 = a2;
            double k33 = a3;
            double k12 = a1a2, k21 = a1a2;
            double k13 = a1a3, k31 = a1a3;
            double k23 = a2a3, k32 = a2a3;
            double curve_integration = NewtonCotesTrapeziumRule.IntegrateTwoPoint(t =>
                (+(k11 * k11 * t * t * t * t * t * t + k12 * k12 * t * t * t * t + k13 * k13 * t * t + 2 * k11 * k12 * t * t * t * t * t + 2 * k11 * k13 * t * t * t * t + 2 * k12 * k13 * t * t * t) +
                 (k21 * k21 * t * t * t * t * t * t + k22 * k22 * t * t * t * t + k23 * k23 * t * t + 2 * k21 * k22 * t * t * t * t * t + 2 * k21 * k23 * t * t * t * t + 2 * k22 * k23 * t * t * t) -
                 (k31 * k31 * t * t * t * t * t * t + k32 * k32 * t * t * t * t + k33 * k33 * t * t + 2 * k31 * k32 * t * t * t * t * t + 2 * k31 * k33 * t * t * t * t + 2 * k32 * k33 * t * t * t)) *
                 (3 * k11 * t * t + 2 * k12 * t + k13) +
                (+(k11 * k11 * t * t * t * t * t * t + k12 * k12 * t * t * t * t + k13 * k13 * t * t + 2 * k11 * k12 * t * t * t * t * t + 2 * k11 * k13 * t * t * t * t + 2 * k12 * k13 * t * t * t) -
                 (k21 * k21 * t * t * t * t * t * t + k22 * k22 * t * t * t * t + k23 * k23 * t * t + 2 * k21 * k22 * t * t * t * t * t + 2 * k21 * k23 * t * t * t * t + 2 * k22 * k23 * t * t * t) +
                 (k31 * k31 * t * t * t * t * t * t + k32 * k32 * t * t * t * t + k33 * k33 * t * t + 2 * k31 * k32 * t * t * t * t * t + 2 * k31 * k33 * t * t * t * t + 2 * k32 * k33 * t * t * t)) *
                 (3 * k21 * t * t + 2 * k22 * t + k23) +
                (-(k11 * k11 * t * t * t * t * t * t + k12 * k12 * t * t * t * t + k13 * k13 * t * t + 2 * k11 * k12 * t * t * t * t * t + 2 * k11 * k13 * t * t * t * t + 2 * k12 * k13 * t * t * t) +
                 (k21 * k21 * t * t * t * t * t * t + k22 * k22 * t * t * t * t + k23 * k23 * t * t + 2 * k21 * k22 * t * t * t * t * t + 2 * k21 * k23 * t * t * t * t + 2 * k22 * k23 * t * t * t) +
                 (k31 * k31 * t * t * t * t * t * t + k32 * k32 * t * t * t * t + k33 * k33 * t * t + 2 * k31 * k32 * t * t * t * t * t + 2 * k31 * k33 * t * t * t * t + 2 * k32 * k33 * t * t * t)) *
                 (3 * k31 * t * t + 2 * k32 * t + k33),
                0, 6);
            status_array[2] = Math.Log(curve_integration);
            double surface_integration = GaussLegendreRule.Integrate((u, v) =>
            ((u * u - u * v + v * v) * (u * u - u * v + v * v) * a1 +
            (u * u + u * v - v * v) * (u * u + u * v - v * v) * a2 +
            (v * v + u * v - u * u) * (v * v + u * v - u * u) * a3 +
            (u * u - u * v + v * v) * (u * u + u * v - v * v) * a1a2 +
            (v * v + u * v - u * u) * (u * u + u * v - v * v) * a2a3 +
            (u * u - u * v + v * v) * (v * v + u * v - u * u) * a1a3) *
            Math.Sqrt(
                ((2 * u - v) * (2 * u - v) + (2 * u + v) * (2 * u + v) + (-2 * u + v) * (-2 * u + v)) *
                ((2 * v - u) * (2 * v - u) + (-2 * v + u) * (-2 * v + u) + (2 * v + u) * (2 * v + u)) -
                ((2 * u - v) * (2 * v - u) + (2 * u + v) * (-2 * v + u) + (-2 * u + v) * (2 * v + u)) *
                ((2 * u - v) * (2 * v - u) + (2 * u + v) * (-2 * v + u) + (-2 * u + v) * (2 * v + u))
                ),
            0, 12, 0, 12, 8);
            status_array[3] = Math.Log(surface_integration);
            return status_array;
        }
        public Matrix<double> Distribution_Allocation(string type)
        {
            string URL = $"https://api.ayachan.fun/distribution?id={ID}&diff={Difficulty[Level]}";
            HttpClient Client = new();
            string Content = Client.GetStringAsync(URL).Result;
            JObject? distribution = (JObject)JsonConvert.DeserializeObject(Content);
            try
            {
                if (distribution["error"] != null)
                {
                    if (distribution["error"].ToString() == "这不是一张谱面")
                    {
                        return Matrix<double>.Build.DenseOfArray(new double[,] { });
                    }
                }
            }
            catch (NullReferenceException){ return Matrix<double>.Build.DenseOfArray(new double[,] { }); }
            JToken? Distribute = distribution[Distribution2[type]];
            string[] Distribution = JsonConvert.SerializeObject(Distribute).Split(new char[] { ',', '[', ']' });
            List<int> dis = new();
            for (int i = 0; i < Distribution.Length; i++)
            {
                try{ dis.Add(Convert.ToInt32(Distribution[i])); }
                catch (Exception){ continue; }
            }
            int square_initial_constant = (int)Math.Sqrt(dis.Count), square_final_constant = (int)Math.Sqrt(dis.Count);
            if (Math.Sqrt(dis.Count) - square_initial_constant is > 0 and < 1) square_final_constant++;
            Matrix<double> matrix = Matrix<double>.Build.Dense(square_final_constant, square_final_constant);
            //right triangle part | left triangle part | diagnoal part
            List<int> intso = new(), intsb = new(), ints = new(), diag = new();
            for (int i = 0; i <= (int)Math.Sqrt(dis.Count); i++)
            {
                if (i == 0)
                {
                    for (int n = 1; n <= (int)Math.Sqrt(dis.Count) - i; n++) ints.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
                    if (ints.Count - square_final_constant == -1) ints.Add(1);
                    ints[0]++; diag = ints;
                }
                else
                {
                    for (int n = 1; n <= diag.Count; n++)
                    {
                        if (((n + i) * (n + i) - 2 * i) < dis.Count)
                        {
                            intsb.Add(dis[(n + i) * (n + i) - 2 * i]);
                            intso.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
                        }
                        else if (((n + i) * (n + i) - 2 * i) >= dis.Count)
                        {
                            try{ intso.Add(dis[(n + i) * (n + i) - 2 * i - 1]); }
                            catch (Exception){ intso.Add(1); }
                            try{ intsb.Add(dis[(n + i) * (n + i) - 2 * i]); }
                            catch (Exception){ intsb.Add(1); }
                        }
                    }
                    for (int n = 1; n < intso.Count; n++)
                    {
                        try
                        {
                            double ele = (intso[n - 1] + intsb[n - 1]) / 2;
                            matrix[n - 1, i + n - 1] = ele; matrix[i + n - 1, n - 1] = ele;
                        }
                        catch (Exception){ continue; }
                    }
                }
            }
            for (int j = 0; j < diag.Count; j++) matrix[j, j] = diag[j];
            return matrix;
        }
        public static Vector<double> Decomposition_DimensionExpansion(Matrix<double> decomposit_to, int expect_size)
        {
            if (decomposit_to.ToArray().Length == 0) return Vector<double>.Build.DenseOfArray(Array.Empty<double>());
            Vector<double> source = decomposit_to.Svd(true).S;
            double[] source_array = source.ToArray();
            double[] result_array = new double[expect_size];
            if (source_array.Length < expect_size)
            {
                for (int i = 0; i < expect_size; i++)
                {
                    if (i < source.Count)
                    {
                        result_array[i] = source_array[i];
                    }
                    else
                    {
                        result_array[i] = 0;
                    }
                }
            }
            else if (source_array.Length == expect_size)
            {
                return Vector<double>.Build.DenseOfArray(source_array);
            }
            else
            {
                return Vector<double>.Build.Dense(Array.Empty<double>());
            }
            return Vector<double>.Build.DenseOfArray(result_array);
        }
    }
}