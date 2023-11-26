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
        public int LevelNum { get; set; } = 0;
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
        public double Exists { get; set; } = -1;
        public ArtificialStaff(int id, string diff = "expert")
        {
            ID = id;
            Level = diff;
            int slide = 0, slideflick = 0, slidenode = 0, slidenode_true = 0;
            double lane_change = 0, beat_change = 0, left_per = 0, right_per = 0;
            int flick = 0, single = 0, bpm = 0, directional = 0, directional_widthsum = 0;
            double active = 0, silence = 0, flick_note_interval = 0, note_flick_interval = 0, interval_average = 0;
            double max_screen_nps = 0, max_speed = 0, finger_max_nps = 0;
            int level = 0;
            //string URL1 = $"https://bestdori.com/api/post/details?id={id}";
            string URL1 = $"https://bestdori.com/api/charts/{id}/{diff}.json";
            string URL2 = $"https://api.ayachan.fun/v2/map-info/bestdori/{id}?diff={Difficulty[diff]}";
            string URL3 = $"https://bestdori.com/api/songs/{id}.json";
            HttpClient Client = new();
            var Status = Client.GetAsync(URL1).Result;
            if (Status.StatusCode == System.Net.HttpStatusCode.NotFound) return;
            string Content = Client.GetStringAsync(URL1).Result;
            if (string.IsNullOrEmpty(Content) == true) return;
            //JObject? Json_Content = (JObject?)JsonConvert.DeserializeObject(Content);
            JArray? Json_Content = (JArray?)JsonConvert.DeserializeObject(Content);
            //JToken? Post = Json_Content.GetValue("post");
            JToken? Chart = Json_Content;
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
                            bpm = (int)Json_Content2["map_metrics"]["main_bpm"];
                            left_per = (double)Json_Content2["map_metrics_extend"]["left_percent"];
                            right_per = 1 - left_per;
                            left_per = (int)(left_per * 100);
                            right_per = (int)(right_per * 100);
                            flick_note_interval = (double)Json_Content2["map_metrics_extend"]["flick_note_interval"];
                            note_flick_interval = (double)Json_Content2["map_metrics_extend"]["note_flick_interval"];
                            interval_average = (int)(Math.Abs(flick_note_interval - note_flick_interval) * 100);
                            max_screen_nps = (double)Json_Content2["map_metrics"]["max_screen_nps"];
                            max_speed = (double)Json_Content2["map_metrics_extend"]["max_speed"];
                            finger_max_nps = (double)Json_Content2["map_metrics_extend"]["finger_max_hps"];
                            string Content3 = Client.GetStringAsync(URL3).Result;
                            JObject? Json_Content3 = (JObject)JsonConvert.DeserializeObject(Content3);
                            level = (int)Json_Content3["difficulty"][$"{Difficulty[diff]}"]["playLevel"];
                            active = (double)Json_Content3["length"];
                            silence = 1 - active;
                            active = (int)(active * 100);
                            silence = (int)(silence * 100);
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
            Exists = 1;
            LevelNum = level;
        }
        public double[] Properties_Integration()
        {
            double x1x1 = Single; double x2x2 = Slide; double x3x3 = Flick + Slideflick; double x4x4 = Interval_average;
            double x1x2 = Slidenode_true * (Lane_change + 1); double x1x3 = (Directional + 1) * (Directional_widthsum + 1);
            double x1x4 = Bpm * (Beat_change + 1); double x2x3 = Left_per * Max_Screen_NPS * Max_Speed;
            double x2x4 = Right_per * Max_Screen_NPS * Max_Speed; double x3x4 = Math.Abs(Active - Silence) * Finger_MaxNPS;
            double[,] Properties = new double[4, 4] { { x1x1, x1x2, x1x3, x1x4 }, { x1x2, x2x2, x2x3, x2x4 }, { x1x3, x2x3, x3x3, x3x4 }, { x1x4, x2x4, x3x4, x4x4 } };
            Matrix<double> PropertiesMatrix = Matrix<double>.Build.DenseOfArray(Properties);
            Vector<double> eig_judge = PropertiesMatrix.Evd(Symmetricity.Symmetric).EigenValues.Real();
            double l4 = eig_judge[2], l3 = eig_judge[1], l2 = eig_judge[3], l1 = eig_judge[0];
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
                    Func<double, double> special_solution_p = (x) => 1 / (r1 * r1) * Math.Exp(r1 * x) + 1 / (r2 * r2) * Math.Exp(r2 * x) + A * x * x + B * x + C;
                    Func<double, double> dVp1 = (x) => special_solution_p(x) * special_solution_p(x) * Math.PI;
                    double Integral_P = NewtonCotesTrapeziumRule.IntegrateAdaptive(dVp1, -10, 10, 1e-6);
                    status_array[0] = Integral_P;
                    status_array[1] = 1;
                    break;
                case 0:
                    double r = -l1 / (2 * l2);
                    Func<double, double> special_solution_z = (x) => (1 / r + 1 / r * x) * Math.Exp(r * x) + A * x * x + B * x + C;
                    Func<double, double> dVz1 = (x) => special_solution_z(x) * special_solution_z(x) * Math.PI;
                    double Integral_Z = NewtonCotesTrapeziumRule.IntegrateAdaptive(dVz1, -10, 10, 1e-6);
                    status_array[0] = Integral_Z;
                    status_array[1] = 2;
                    break;
                case < 0:
                    double alpha = -l1 / (2 * l2);
                    double beta = Math.Sqrt(Math.Abs(Delta)) / (2 * l2);
                    Func<double, double> special_solution_n = (x) => 1 / (alpha + beta) * Math.Exp(alpha * x) * (Math.Cos(beta * x) + Math.Sqrt(3) * Math.Sin(beta * x));
                    Func<double, double> dVn1 = (x) => special_solution_n(x) * special_solution_n(x) * Math.PI;
                    double Integral_N = NewtonCotesTrapeziumRule.IntegrateAdaptive(dVn1, -10, 10, 1e-6);
                    status_array[0] = Integral_N;
                    status_array[1] = 3;
                    break;
                default:
                    status_array[0] = 0;
                    status_array[1] = -1;
                    break;
            }
            double a1 = PropertiesMatrix[0, 0];
            double a2 = PropertiesMatrix[1, 1] + PropertiesMatrix[2, 2];
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
            double curve_integration = NewtonCotesTrapeziumRule.IntegrateAdaptive(t =>
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
                -10, 10, 1e-14);
            status_array[2] = curve_integration;
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
            -10, 10, -10, 10, 1024);
            status_array[3] = surface_integration;
            return status_array;
        }
        public Matrix<double> Distribution_Allocation(string type)
        {
            string URL = $"https://api.ayachan.fun/v2/map-info/bestdori/{ID}?diff={Difficulty[Level]}";
            HttpClient Client = new();
            string Content = Client.GetStringAsync(URL).Result;
            JObject? distribution = (JObject)JsonConvert.DeserializeObject(Content);
            try
            {
                if (distribution["map_metrics"] == null)
                {
                    return Matrix<double>.Build.DenseOfArray(new double[,] { });
                }
            }
            catch (NullReferenceException){ return Matrix<double>.Build.DenseOfArray(new double[,] { }); }
            JToken? Distribute = distribution["map_metrics"]["Distribution"][type];
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
        public static (double, double) Decomposition_Integral(Matrix<double> decomposit_to)
        {
            if (decomposit_to.ColumnCount == 0) return (0, 0);
            var SVD = decomposit_to.Svd(true);
            var EVD = decomposit_to.Evd(Symmetricity.Symmetric);
            Matrix<double> U = SVD.U;
            Matrix<double> V = SVD.VT;
            Vector<double> S = SVD.S;
            Matrix<double> EV = EVD.EigenVectors;
            Vector<System.Numerics.Complex> EC = EVD.EigenValues;
            Vector<double> E;
            double[] ecr = new double[EC.Count];
            int i = 0;
            foreach (var item in EC)
            {
                ecr[i] = item.Real;
                ++i;
            }
            E = Vector<double>.Build.DenseOfArray(ecr);
            if (E.Count < 10)
            {
                E.Add(0);
                S.Add(0);
            }
            double s1 = S[0], s2 = S[1], s3 = S[2], s4 = S[3], s5 = S[4], s6 = S[5], s7 = S[6], s8 = S[7], s9 = S[8], s10 = S[9];
            double e1 = E[0], e2 = E[1], e3 = E[2], e4 = E[3], e5 = E[4], e6 = E[5], e7 = E[6], e8 = E[7], e9 = E[8], e10 = E[9];
            double SD3 = (s6 - s5) * s7 * s7 + (s5 - s7) * s6 * s6 + (s7 - s6) * s5 * s5;
            double su = s7 - s6;
            double sv = s5 - s7;
            double sw = s6 - s5;
            Func<double, double> su5 = (x) =>
            Math.PI / 6 * Math.Exp(s5 * x) -
            ((s1 * x * x * x + s2 * x * x + s3 * x + s4) / s5 +
            (3 * s1 * x * x + 2 * s2 * x + s3) / (s5 * s5) +
            (6 * s1 * x + 2 * s2) / (s5 * s5 * s5) +
            (6 * s1) / (s5 * s5 * s5 * s5)) *
            su / SD3;
            Func<double, double> sv6 = (x) =>
            Math.PI / 3 * Math.Exp(s6 * x) -
            ((s1 * x * x * x + s2 * x * x + s3 * x + s4) / s6 +
            (3 * s1 * x * x + 2 * s2 * x + s3) / (s6 * s6) +
            (6 * s1 * x + 2 * s2) / (s6 * s6 * s6) +
            (6 * s1) / (s6 * s6 * s6 * s6)) *
            sv / SD3;
            Func<double, double> sw7 = (x) =>
            Math.PI / 2 * Math.Exp(s7 * x) -
            ((s1 * x * x * x + s2 * x * x + s3 * x + s4) / s7 +
            (3 * s1 * x * x + 2 * s2 * x + s3) / (s7 * s7) +
            (6 * s1 * x + 2 * s2) / (s7 * s7 * s7) +
            (6 * s1) / (s7 * s7 * s7 * s7)) *
            sw / SD3;
            double result_s = NewtonCotesTrapeziumRule.IntegrateAdaptive(su5 + sv6 + sw7, 0, (s8 * EV.Determinant() + (s9 + 1) * V.Determinant() + (s10 + 2) * U.Determinant()) / 3, 1E-15);
            double ED3 = (e6 - e5) * e7 * e7 + (e5 - e7) * e6 * e6 + (e7 - e6) * e5 * e5;
            double eu = e7 - e6;
            double ev = e5 - e7;
            double ew = e6 - e5;
            Func<double, double> eu5 = (x) =>
            Math.PI / 6 * Math.Exp(e5 * x) -
            ((e1 * x * x * x + e2 * x * x + e3 * x + e4) / e5 +
            (3 * e1 * x * x + 2 * e2 * x + e3) / (e5 * e5) +
            (6 * e1 * x + 2 * e2) / (e5 * e5 * e5) +
            (6 * e1) / (e5 * e5 * e5 * e5)) *
            eu / ED3;
            Func<double, double> ev6 = (x) =>
            Math.PI / 3 * Math.Exp(e6 * x) -
            ((e1 * x * x * x + e2 * x * x + e3 * x + e4) / e6 +
            (3 * e1 * x * x + 2 * e2 * x + e3) / (e6 * e6) +
            (6 * e1 * x + 2 * e2) / (e6 * e6 * e6) +
            (6 * e1) / (e6 * e6 * e6 * e6)) *
            ev / ED3;
            Func<double, double> ew7 = (x) =>
            Math.PI / 2 * Math.Exp(e7 * x) -
            ((e1 * x * x * x + e2 * x * x + e3 * x + e4) / e7 +
            (3 * e1 * x * x + 2 * e2 * x + e3) / (e7 * e7) +
            (6 * e1 * x + 2 * e2) / (e7 * e7 * e7) +
            (6 * e1) / (e7 * e7 * e7 * e7)) *
            ew / ED3;
            double result_e = NewtonCotesTrapeziumRule.IntegrateAdaptive(eu5 + ev6 + ew7, 0, (e8 * EV.Determinant() + (e9 + 1) * V.Determinant() + (e10 + 2) * U.Determinant()) / 3, 1E-15);
            return (result_s, result_e);
        }
    }
}