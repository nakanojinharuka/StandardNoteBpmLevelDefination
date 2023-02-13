using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Symbolics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
internal class Chart
    {
        public Dictionary<string, int> DifficultyMap = new() { { "expert", 3 }, { "special", 4 } };
        public double Integration { get; set; } = 0;
        public double Level { get; set; } = 0;
        public SymbolicExpression CommonSolution { get; set; } = 0;
        public double[] EigenValue { get; set; } = new double[3];
        public Matrix<double> EigenMatrix { get; set; } = Matrix<double>.Build.Dense(3, 3);
        public double Time { get; set; } = 0;
        public Chart(int id)
        {
            string url = $"https://bestdori.com/api/post/details?id={id}";
            string url2 = $"https://api.ayachan.fun/distribution?id={id}&diff=3";
            string url3 = $"https://api.ayachan.fun/DiffAnalysis?id={id}&diff=3&speed=1.0";
            string encoder = "UTF-8";
            double level = 0;
            double slide = 0;
            double single = 0;
            double flick = 0;
            double slideflick = 0;
            double slidenode = 0;
            double slidenode_true = 0;
            double directional = 0;
            double directional_widthsum = 0;
            double bpm = 0;
            double lane_change = 0;
            double beat_change = 0;
            WebClient myWebClient = new();
            byte[] myDataBuffer = myWebClient.DownloadData(url);
            string SourceCode = Encoding.GetEncoding(encoder).GetString(myDataBuffer);
            JObject? sourcechart = (JObject)JsonConvert.DeserializeObject(SourceCode);
            var chart = sourcechart["post"]["chart"];
            foreach (var item in chart)
            {
                if (item["type"].ToString() == "Slide")
                {
                    slide++;
                    try
                    {
                        if ((bool)item["connections"].Last["flick"] == true)
                        {
                            slideflick++;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    finally
                    {
                        List<double> lanes = new();
                        List<double> beats = new();
                        List<double> lanes_changes = new();
                        List<double> beats_changes = new();
                        foreach (var i in item["connections"])
                        {
                            try
                            {
                                lanes.Add((double)i["lane"]);
                                beats.Add((double)i["beat"]);
                                if ((bool)i["hidden"] == true)
                                {
                                    slidenode_true++;
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                slidenode++;
                                slidenode_true++;
                                continue;
                            }
                        }
                        for (int k = 0, j = 0; k < beats.Count - 1 || j < lanes.Count - 1; k++, j++)
                        {
                            lanes_changes.Add(lanes[j + 1] - lanes[j]);
                            beats_changes.Add(beats[k + 1] - beats[k]);
                        }
                        lane_change = lanes_changes.Sum();
                        beat_change = beats_changes.Sum();
                    }
                }
                else if (item["type"].ToString() == "Single" || item["type"].ToString() == "Long")
                {
                    try
                    {
                        if ((bool)item["flick"] == true)
                        {
                            flick++;
                        }
                    }
                    catch (Exception)
                    {
                        single++;
                        continue;
                    }
                    finally
                    {

                    }
                }
                else if (item["type"].ToString() == "Directional")
                {
                    directional++;
                    directional_widthsum += (int)item["width"];
                }
                else if (item["type"].ToString() == "BPM")
                {
                    if (bpm == 0)
                    {
                        byte[] myDataBuffer1 = myWebClient.DownloadData(url3);
                        string SourceCode1 = Encoding.GetEncoding(encoder).GetString(myDataBuffer1);
                        JObject? sourcechart1 = (JObject)JsonConvert.DeserializeObject(SourceCode1);
                        bpm = (int)sourcechart1["detail"]["MainBPM"];
                        level = (int)sourcechart1["basic"]["Level"];
                        Level = level;
                    }
                    else
                    {

                    }
                }
            }
            myDataBuffer = myWebClient.DownloadData(url2);
            SourceCode = Encoding.GetEncoding(encoder).GetString(myDataBuffer);
            JObject? sourcedistribution = (JObject)JsonConvert.DeserializeObject(SourceCode);
            var hitdis = sourcedistribution["hitDistribution"];
            var notedis = sourcedistribution["noteDistribution"];
            double[,] vandemond6 = { { 1, 1, 1, 1, 1, 1 }, { 1, 2, 4, 8, 16, 32 }, { 1, 3, 9, 27, 81, 243 }, { 1, 4, 16, 64, 256, 1024 }, { 1, 5, 25, 125, 625, 3125 }, { 1, 6, 36, 216, 1296, 7776 } };
            double[,] vandemond5 = { { 1, 1, 1, 1, 1 }, { 1, 2, 4, 8, 16 }, { 1, 3, 9, 27, 81 }, { 1, 4, 16, 64, 256 }, { 1, 5, 25, 125, 625 } };
            double[,] vandemond4 = { { 1, 1, 1, 1 }, { 1, 2, 4, 8 }, { 1, 3, 9, 27 }, { 1, 4, 16, 64 } };
            double[,] vandemond3 = { { 1, 1, 1 }, { 1, 2, 4 }, { 1, 3, 9 } };
            double[,] vandemond2 = { { 1, 1 }, { 1, 2 } };
            var vandemondmatrix6 = Matrix<double>.Build.DenseOfArray(vandemond6);
            var vandemondmatrix5 = Matrix<double>.Build.DenseOfArray(vandemond5);
            var vandemondmatrix4 = Matrix<double>.Build.DenseOfArray(vandemond4);
            var vandemondmatrix3 = Matrix<double>.Build.DenseOfArray(vandemond3);
            var vandemondmatrix2 = Matrix<double>.Build.DenseOfArray(vandemond2);
            string[] hit_str = JsonConvert.SerializeObject(hitdis).Split(new char[] { ',', '[', ']' });
            List<int> hit_list = new();
            string[] note_str = JsonConvert.SerializeObject(notedis).Split(new char[] { ',', '[', ']' });
            List<int> note_list = new();
            SymbolicExpression density_note = 0, density_hit = 0;
            for (int i = 0; i < hit_str.Length; i++)
            {
                try
                {
                    hit_list.Add(Convert.ToInt32(hit_str[i]));
                    note_list.Add(Convert.ToInt32(note_str[i]));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            int[] hit_array = hit_list.ToArray();
            int[] note_array = note_list.ToArray();
            var x = SymbolicExpression.Variable("x");
            double IH = 0;
            double IN = 0;
            for (int i = 0; i < hit_array.Length; i += 5)
            {
                if (i + 5 < hit_array.Length || i + 6 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3], hit_array[i + 4], hit_array[i + 5] });
                    var resultA = vandemondmatrix6.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3], note_array[i + 4], note_array[i + 5] });
                    var resultB = vandemondmatrix6.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 6, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 6, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 7; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 5 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3], hit_array[i + 4] });
                    var resultA = vandemondmatrix5.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3], note_array[i + 4] });
                    var resultB = vandemondmatrix5.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 5, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 5, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 6; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 4 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3] });
                    var resultA = vandemondmatrix4.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3] });
                    var resultB = vandemondmatrix4.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 4, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 4, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 5; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 3 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2] });
                    var resultA = vandemondmatrix3.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2] });
                    var resultB = vandemondmatrix3.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 3, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 3, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 4; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 2 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1] });
                    var resultA = vandemondmatrix2.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1] });
                    var resultB = vandemondmatrix2.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 2, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 2, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 3; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
            }
            Time = note_array.Length;
            SymbolicExpression metadata_expression = (bpm + (slidenode + single) * x + (flick + slideflick) * x.Pow(2) + directional * x.Pow(3)) / 180 * SymbolicExpression.E.Pow(slide / 240 * x);
            var fM = metadata_expression.Compile("x");
            double IM = NewtonCotesTrapeziumRule.IntegrateAdaptive(fM, 0, 1, 1e-12);
            double IP = bpm / 30 * (Math.Sqrt(5) - 1);
            double IE = (single + slidenode + flick + slideflick + directional) / (slide + slidenode_true / 3000 + 100);
            double ID = NewtonCotesTrapeziumRule.IntegrateAdaptive(fM, 0, (density_hit + density_note).Abs().RealNumberValue / (note_array.Length), 1e-12);
            SymbolicExpression color_expression = 0;
            if (directional != 0)
            {
                color_expression += (single / 1000 * x + slidenode / (1 + slide) * x.Pow(2) + (flick + slideflick) / 1000 * 255 * x.Pow(3) + directional_widthsum / directional * 255 * x.Pow(4)) / (slidenode_true / 3 + 1);
            }
            else
            {
                color_expression += (single / 1000 * x + slidenode / (1 + slide) * x.Pow(2) + (flick + slideflick) / 1000 * 255 * x.Pow(3)) / (slidenode_true / 3 + 1);
            }
            SymbolicExpression slide_change_expression = lane_change * x + beat_change / bpm * 60;
            var fS = slide_change_expression.Compile("x");
            double IS = NewtonCotesTrapeziumRule.IntegrateAdaptive(fS, 0, bpm / 60, 1e-12);
            var fC = color_expression.Compile("x");
            double IC = NewtonCotesTrapeziumRule.IntegrateAdaptive(fC, 0, 1, 1e-12);
            SymbolicExpression density_expression = (density_note.Abs() + density_hit.Abs()) / (x + 1) / 100;
            var fR = density_expression.Compile("x");
            double IR = NewtonCotesTrapeziumRule.IntegrateAdaptive(fR, 0, 1, 1e-12);
            // Solve IH y'' + ID y' + IN y = e^(ISx) ((IM x + IP) cos(IR x) + (IE x + IC) sin(IR x))
            SymbolicExpression CommonSolution = 0;
            SymbolicExpression C1 = 1, C2 = 1;
            double delta = ID * ID - 4 * IH * IN;
            double A = IH + ID * IS + IN * IS * IS - IN * IR * IR;
            double B = ID * IR + 2 * IR * IN * IS;
            double C = B / IR;
            double D = 2 * IR * IN;
            double E = ID - 2 * IN * IS;
            var coefficientmatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { A, 0, B, 0 }, { C, A, D, B }, { -B, 0, A, 0 }, { -D, -B, E, A } });
            var coefficientvector = Vector<double>.Build.DenseOfArray(new double[] { IM, IE, IP, IC });
            var solution = coefficientmatrix.LU().Solve(coefficientvector);
            SymbolicExpression ParticulurSolution = SymbolicExpression.E.Pow(IS * x) * ((solution[0] * x + solution[1]) * (IR * x).Cos() + (solution[2] * x + solution[3]) * (IR * x).Sin());
            switch (delta)
            {
                case > 0:
                    double r1 = (-ID + Math.Sqrt(delta)) / (2 * IH);
                    double r2 = (-ID - Math.Sqrt(delta)) / (2 * IH);
                    CommonSolution = C1 * SymbolicExpression.E.Pow(r1 * x) + C2 * SymbolicExpression.E.Pow(r2 * x) + ParticulurSolution;
                    break;
                case 0:
                    double r = (-ID + Math.Sqrt(delta)) / (2 * IH);
                    CommonSolution = (C1 + C2 * x) * SymbolicExpression.E.Pow(r * x) + ParticulurSolution;
                    break;
                case < 0:
                    double a = -ID / (2 * IH);
                    double b = Math.Sqrt(-delta) / (2 * IH);
                    CommonSolution = SymbolicExpression.E.Pow(a * x) * (C1 * (b * x).Cos() + C2 * (b * x).Sin()) + ParticulurSolution;
                    break;
                default:
                    break;
            }
            var fCS = CommonSolution.Compile("x");
            double ICS = NewtonCotesTrapeziumRule.IntegrateAdaptive(fCS, -1, 1, 1e-13);
            Integration = ICS;
            this.CommonSolution = CommonSolution;
            double[,] H = { { Math.Sqrt(IH), IS + IM, IP + IC }, { IS + IM, Math.Sqrt(IN), IE + IR }, { IP + IC, IE + IR, Math.Sqrt(ID) } };
            var H_Matrix = Matrix<double>.Build.DenseOfArray(H);
            var H_QR = H_Matrix.QR();
            var H_T_RQ = H_QR.R * H_QR.Q;
            var H_EV = H_QR.Q;
            for (int l = 1; l <= 324; l++)
            {
                if (l == 324)
                {
                    EigenValue[0] = H_T_RQ[0, 0];
                    EigenValue[1] = H_T_RQ[1, 1];
                    EigenValue[2] = H_T_RQ[2, 2];
                    EigenMatrix = H_EV;
                    break;
                }
                H_QR = H_T_RQ.QR();
                H_T_RQ = H_QR.R * H_QR.Q;
                H_EV *= H_QR.Q;
            }
        }
        public Chart(double id, string difficulty)
        {
            string url = $"https://bestdori.com/api/charts/{(int)id}/{difficulty}.json";
            string url2 = $"https://api.ayachan.fun/distribution?id={(int)id}&diff={DifficultyMap[difficulty]}";
            string url3 = $"https://api.ayachan.fun/DiffAnalysis?id={(int)id}&diff={DifficultyMap[difficulty]}&speed=1.0";
            string encoder = "UTF-8";
            double level = 0;
            double slide = 0;
            double single = 0;
            double flick = 0;
            double slideflick = 0;
            double slidenode = 0;
            double slidenode_true = 0;
            double directional = 0;
            double directional_widthsum = 0;
            double bpm = 0;
            double lane_change = 0;
            double beat_change = 0;
            WebClient myWebClient = new();
            byte[] myDataBuffer = myWebClient.DownloadData(url);
            string SourceCode = Encoding.GetEncoding(encoder).GetString(myDataBuffer);
            JArray sourcechart = (JArray)JsonConvert.DeserializeObject(SourceCode);
            var chart = (JToken)sourcechart;
            foreach (var item in chart)
            {
                if (item["type"].ToString() == "Slide")
                {
                    slide++;
                    try
                    {
                        if ((bool)item["connections"].Last["flick"] == true)
                        {
                            slideflick++;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    finally
                    {
                        List<double> lanes = new();
                        List<double> beats = new();
                        List<double> lanes_changes = new();
                        List<double> beats_changes = new();
                        foreach (var i in item["connections"])
                        {
                            try
                            {
                                lanes.Add((double)i["lane"]);
                                beats.Add((double)i["beat"]);
                                if ((bool)i["hidden"] == true)
                                {
                                    slidenode_true++;
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                slidenode++;
                                slidenode_true++;
                                continue;
                            }
                        }
                        for (int k = 0, j = 0; k < beats.Count - 1 || j < lanes.Count - 1; k++, j++)
                        {
                            lanes_changes.Add(lanes[j + 1] - lanes[j]);
                            beats_changes.Add(beats[k + 1] - beats[k]);
                        }
                        lane_change = lanes_changes.Sum();
                        beat_change = beats_changes.Sum();
                    }
                }
                else if (item["type"].ToString() == "Single" || item["type"].ToString() == "Long")
                {
                    try
                    {
                        if ((bool)item["flick"] == true)
                        {
                            flick++;
                        }
                    }
                    catch (Exception)
                    {
                        single++;
                        continue;
                    }
                    finally
                    {

                    }
                }
                else if (item["type"].ToString() == "Directional")
                {
                    directional++;
                    directional_widthsum += (int)item["width"];
                }
                else if (item["type"].ToString() == "BPM")
                {
                    if (bpm == 0)
                    {
                        byte[] myDataBuffer1 = myWebClient.DownloadData(url3);
                        string SourceCode1 = Encoding.GetEncoding(encoder).GetString(myDataBuffer1);
                        JObject? sourcechart1 = (JObject)JsonConvert.DeserializeObject(SourceCode1);
                        bpm = (int)sourcechart1["detail"]["MainBPM"];
                        level = (int)sourcechart1["basic"]["Level"];
                        Level = level;
                    }
                    else
                    {

                    }
                }
            }
            myDataBuffer = myWebClient.DownloadData(url2);
            SourceCode = Encoding.GetEncoding(encoder).GetString(myDataBuffer);
            JObject? sourcedistribution = (JObject)JsonConvert.DeserializeObject(SourceCode);
            var hitdis = sourcedistribution["hitDistribution"];
            var notedis = sourcedistribution["noteDistribution"];
            double[,] vandemond6 = { { 1, 1, 1, 1, 1, 1 }, { 1, 2, 4, 8, 16, 32 }, { 1, 3, 9, 27, 81, 243 }, { 1, 4, 16, 64, 256, 1024 }, { 1, 5, 25, 125, 625, 3125 }, { 1, 6, 36, 216, 1296, 7776 } };
            double[,] vandemond5 = { { 1, 1, 1, 1, 1 }, { 1, 2, 4, 8, 16 }, { 1, 3, 9, 27, 81 }, { 1, 4, 16, 64, 256 }, { 1, 5, 25, 125, 625 } };
            double[,] vandemond4 = { { 1, 1, 1, 1 }, { 1, 2, 4, 8 }, { 1, 3, 9, 27 }, { 1, 4, 16, 64 } };
            double[,] vandemond3 = { { 1, 1, 1 }, { 1, 2, 4 }, { 1, 3, 9 } };
            double[,] vandemond2 = { { 1, 1 }, { 1, 2 } };
            var vandemondmatrix6 = Matrix<double>.Build.DenseOfArray(vandemond6);
            var vandemondmatrix5 = Matrix<double>.Build.DenseOfArray(vandemond5);
            var vandemondmatrix4 = Matrix<double>.Build.DenseOfArray(vandemond4);
            var vandemondmatrix3 = Matrix<double>.Build.DenseOfArray(vandemond3);
            var vandemondmatrix2 = Matrix<double>.Build.DenseOfArray(vandemond2);
            string[] hit_str = JsonConvert.SerializeObject(hitdis).Split(new char[] { ',', '[', ']' });
            List<int> hit_list = new();
            string[] note_str = JsonConvert.SerializeObject(notedis).Split(new char[] { ',', '[', ']' });
            List<int> note_list = new();
            SymbolicExpression density_note = 0, density_hit = 0;
            for (int i = 0; i < hit_str.Length; i++)
            {
                try
                {
                    hit_list.Add(Convert.ToInt32(hit_str[i]));
                    note_list.Add(Convert.ToInt32(note_str[i]));
                }
                catch (Exception)
                {
                    continue;
                }
            }
            int[] hit_array = hit_list.ToArray();
            int[] note_array = note_list.ToArray();
            var x = SymbolicExpression.Variable("x");
            double IH = 0;
            double IN = 0;
            for (int i = 0; i < hit_array.Length; i += 5)
            {
                if (i + 5 < hit_array.Length || i + 6 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3], hit_array[i + 4], hit_array[i + 5] });
                    var resultA = vandemondmatrix6.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3], note_array[i + 4], note_array[i + 5] });
                    var resultB = vandemondmatrix6.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 6, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 6, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 7; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 5 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3], hit_array[i + 4] });
                    var resultA = vandemondmatrix5.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3], note_array[i + 4] });
                    var resultB = vandemondmatrix5.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 5, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 5, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 6; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 4 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2], hit_array[i + 3] });
                    var resultA = vandemondmatrix4.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2], note_array[i + 3] });
                    var resultB = vandemondmatrix4.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 4, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 4, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 5; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 3 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1], hit_array[i + 2] });
                    var resultA = vandemondmatrix3.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1], note_array[i + 2] });
                    var resultB = vandemondmatrix3.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 3, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 3, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 4; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
                else if (i + 2 == hit_array.Length)
                {
                    var targetvector1 = Vector<double>.Build.DenseOfArray(new double[] { hit_array[i], hit_array[i + 1] });
                    var resultA = vandemondmatrix2.LU().Solve(targetvector1);
                    var targetvector2 = Vector<double>.Build.DenseOfArray(new double[] { note_array[i], note_array[i + 1] });
                    var resultB = vandemondmatrix2.LU().Solve(targetvector2);
                    var resultA_array = resultA.ToArray();
                    var resultB_array = resultB.ToArray();
                    SymbolicExpression resultA_expression = 0;
                    SymbolicExpression resultB_expression = 0;
                    for (int j = 0; j < resultA_array.Length; j++)
                    {
                        resultA_expression += resultA_array[j] * x.Pow(j);
                        resultB_expression += resultB_array[j] * x.Pow(j);
                    }
                    var fA = resultA_expression.Compile("x");
                    var fB = resultB_expression.Compile("x");
                    double IA = NewtonCotesTrapeziumRule.IntegrateAdaptive(fA, 0, 2, 1e-12);
                    double IB = NewtonCotesTrapeziumRule.IntegrateAdaptive(fB, 0, 2, 1e-12);
                    IH += IA;
                    IN += IB;
                    for (int k = 1; k < 3; k++)
                    {
                        SymbolicExpression DA_k = resultA_expression.DifferentiateAt("x", k);
                        SymbolicExpression DB_k = resultB_expression.DifferentiateAt("x", k);
                        density_hit += DA_k;
                        density_note += DB_k;
                    }
                }
            }
            Time = note_array.Length;
            SymbolicExpression metadata_expression = (bpm + (slidenode + single) * x + (flick + slideflick) * x.Pow(2) + directional * x.Pow(3)) / 180 * SymbolicExpression.E.Pow(slide / 240 * x);
            var fM = metadata_expression.Compile("x");
            double IM = NewtonCotesTrapeziumRule.IntegrateAdaptive(fM, 0, 1, 1e-12);
            double IP = bpm / 30 * (Math.Sqrt(5) - 1);
            double IE = (single + slidenode + flick + slideflick + directional) / (slide + slidenode_true / 3000 + 100);
            double ID = NewtonCotesTrapeziumRule.IntegrateAdaptive(fM, 0, (density_hit + density_note).Abs().RealNumberValue / (note_array.Length), 1e-12);
            SymbolicExpression color_expression = 0;
            if (directional != 0)
            {
                color_expression += (single / 1000 * x + slidenode / (1 + slide) * x.Pow(2) + (flick + slideflick) / 1000 * 255 * x.Pow(3) + directional_widthsum / directional * 255 * x.Pow(4)) / (slidenode_true / 3 + 1);
            }
            else
            {
                color_expression += (single / 1000 * x + slidenode / (1 + slide) * x.Pow(2) + (flick + slideflick) / 1000 * 255 * x.Pow(3)) / (slidenode_true / 3 + 1);
            }
            SymbolicExpression slide_change_expression = lane_change * x + beat_change / bpm * 60;
            var fS = slide_change_expression.Compile("x");
            double IS = NewtonCotesTrapeziumRule.IntegrateAdaptive(fS, 0, bpm / 60, 1e-12);
            var fC = color_expression.Compile("x");
            double IC = NewtonCotesTrapeziumRule.IntegrateAdaptive(fC, 0, 1, 1e-12);
            SymbolicExpression density_expression = (density_note.Abs() + density_hit.Abs()) / (x + 1) / 100;
            var fR = density_expression.Compile("x");
            double IR = NewtonCotesTrapeziumRule.IntegrateAdaptive(fR, 0, 1, 1e-12);
            // Solve IH y'' + ID y' + IN y = e^(ISx) ((IM x + IP) cos(IR x) + (IE x + IC) sin(IR x))
            SymbolicExpression CommonSolution = 0;
            SymbolicExpression C1 = 1, C2 = 1;
            double delta = ID * ID - 4 * IH * IN;
            double A = IH + ID * IS + IN * IS * IS - IN * IR * IR;
            double B = ID * IR + 2 * IR * IN * IS;
            double C = B / IR;
            double D = 2 * IR * IN;
            double E = ID - 2 * IN * IS;
            var coefficientmatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { A, 0, B, 0 }, { C, A, D, B }, { -B, 0, A, 0 }, { -D, -B, E, A } });
            var coefficientvector = Vector<double>.Build.DenseOfArray(new double[] { IM, IE, IP, IC });
            var solution = coefficientmatrix.LU().Solve(coefficientvector);
            SymbolicExpression ParticulurSolution = SymbolicExpression.E.Pow(IS * x) * ((solution[0] * x + solution[1]) * (IR * x).Cos() + (solution[2] * x + solution[3]) * (IR * x).Sin());
            switch (delta)
            {
                case > 0:
                    double r1 = (-ID + Math.Sqrt(delta)) / (2 * IH);
                    double r2 = (-ID - Math.Sqrt(delta)) / (2 * IH);
                    CommonSolution = C1 * SymbolicExpression.E.Pow(r1 * x) + C2 * SymbolicExpression.E.Pow(r2 * x) + ParticulurSolution;
                    break;
                case 0:
                    double r = (-ID + Math.Sqrt(delta)) / (2 * IH);
                    CommonSolution = (C1 + C2 * x) * SymbolicExpression.E.Pow(r * x) + ParticulurSolution;
                    break;
                case < 0:
                    double a = -ID / (2 * IH);
                    double b = Math.Sqrt(-delta) / (2 * IH);
                    CommonSolution = SymbolicExpression.E.Pow(a * x) * (C1 * (b * x).Cos() + C2 * (b * x).Sin()) + ParticulurSolution;
                    break;
                default:
                    break;
            }
            var fCS = CommonSolution.Compile("x");
            double ICS = NewtonCotesTrapeziumRule.IntegrateAdaptive(fCS, -1, 1, 1e-13);
            Integration = ICS;
            this.CommonSolution = CommonSolution;
            double[,] H = { { Math.Sqrt(IH), IS + IM, IP + IC }, { IS + IM, Math.Sqrt(IN), IE + IR }, { IP + IC, IE + IR, Math.Sqrt(ID) } };
            var H_Matrix = Matrix<double>.Build.DenseOfArray(H);
            var H_QR = H_Matrix.QR();
            var H_T_RQ = H_QR.R * H_QR.Q;
            var H_EV = H_QR.Q;
            for (int l = 1; l <= 324; l++)
            {
                if (l == 324)
                {
                    EigenValue[0] = H_T_RQ[0, 0];
                    EigenValue[1] = H_T_RQ[1, 1];
                    EigenValue[2] = H_T_RQ[2, 2];
                    EigenMatrix = H_EV;
                    break;
                }
                H_QR = H_T_RQ.QR();
                H_T_RQ = H_QR.R * H_QR.Q;
                H_EV *= H_QR.Q;
            }
        }
    }
