using MathNet.Numerics.Integration;
using MathNet.Numerics.LinearAlgebra;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OfficeOpenXml;
Dictionary<string, int> Difficulty = new() { { "expert", 3 }, { "special", 4 } };
double[,] pca_transform_hit = new double[18, 5], pca_transform_nte = new double[18, 5];
double[] reg_hit = new double[5], reg_nte = new double[5], reg_cor = new double[4], reg_cpc = new double[4];
Matrix<double> pca_hit_transform, pca_nte_transform;
Vector<double> reg_hit_const, reg_nte_const, reg_cor_const, reg_cpc_const;
void XlsxFileImport()
{
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    ExcelPackage package = new("F:/Bandori stats/Transformation and Regression.xlsx");
    ExcelWorksheet PCA_Hit_Transform = package.Workbook.Worksheets["PCA Transform Matrix for Hit's"];
    ExcelWorksheet PCA_Noe_Transform = package.Workbook.Worksheets["PCA Transform Matrix for Note's"];
    ExcelWorksheet REG_Hit_Constants = package.Workbook.Worksheets["Regression constant on PCA Hit"];
    ExcelWorksheet REG_Nte_Constants = package.Workbook.Worksheets["Regression constant on PCA Note"];
    ExcelWorksheet REG_Correction_02 = package.Workbook.Worksheets["Positive Negative Correction"];
    ExcelWorksheet REG_Correction_01 = package.Workbook.Worksheets["Curve Pleatu Correction"];
    ExcelRange hit_range = PCA_Hit_Transform.Cells[1, 1, 18, 5];
    ExcelRange nte_range = PCA_Noe_Transform.Cells[1, 1, 18, 5];
    ExcelRange hit_const = REG_Hit_Constants.Cells[1, 1, 5, 1];
    ExcelRange nte_const = REG_Nte_Constants.Cells[1, 1, 5, 1];
    ExcelRange cor_const = REG_Correction_02.Cells[1, 1, 4, 1];
    ExcelRange cpc_const = REG_Correction_01.Cells[1, 1, 4, 1];
    for (int i = 0; i < 5; i++)
    {
        reg_hit[i] = hit_const.GetCellValue<double>(i, 0);
        reg_nte[i] = nte_const.GetCellValue<double>(i, 0);
        if (i != 4)
        {
            reg_cor[i] = cor_const.GetCellValue<double>(i, 0);
            reg_cpc[i] = cpc_const.GetCellValue<double>(i, 0);
        }
    }
    for (int i = 0; i < 18; i++)
    {
        for (int j = 0; j < 5; j++)
        {
            pca_transform_hit[i, j] = hit_range.GetCellValue<double>(i, j);
            pca_transform_nte[i, j] = nte_range.GetCellValue<double>(i, j);
        }
    }
    pca_hit_transform = Matrix<double>.Build.DenseOfArray(pca_transform_hit);
    pca_nte_transform = Matrix<double>.Build.DenseOfArray(pca_transform_nte);
    reg_hit_const = Vector<double>.Build.DenseOfArray(reg_hit);
    reg_nte_const = Vector<double>.Build.DenseOfArray(reg_nte);
    reg_cor_const = Vector<double>.Build.DenseOfArray(reg_cor);
    reg_cpc_const = Vector<double>.Build.DenseOfArray(reg_cpc);
    return;
}
Matrix<double> PropertiesO(int id, string diff)
{
    int slide = 0, slideflick = 0, slidenode = 0, slidenode_true = 0;
    double lane_change = 0, beat_change = 0;
    int flick = 0, single = 0, bpm = 0, directional = 0, directional_widthsum = 0;
    string URL1 = $"https://bestdori.com/api/charts/{id}/{diff}.json";
    string URL2 = $"https://api.ayachan.fun/DiffAnalysis?id={id}&diff={Difficulty[diff]}&speed=1.0";
    WebClient Client = new();
    try
    {
        byte[] myDataBuffer1 = Client.DownloadData(URL1);
    }
    catch (Exception)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,] { });
    }
    byte[] myDataBuffer = Client.DownloadData(URL1);
    string SourceCode = Encoding.GetEncoding("UTF-8").GetString(myDataBuffer);
    var chart = (JArray)JsonConvert.DeserializeObject(SourceCode);
    foreach (var item in chart)
    {
        switch (item["type"].ToString())
        {
            case "Slide":
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
                        lanes.Clear();
                        beats.Clear();
                        lanes_changes.Clear();
                        beats_changes.Clear();
                    }
                    break;
                }
            case "Single":
            case "Long":
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
                break;
            case "Directional":
                directional++;
                directional_widthsum += (int)item["width"];
                break;
            case "BPM":
                if (bpm == 0)
                {
                    byte[] myDataBuffer1 = Client.DownloadData(URL2);
                    string SourceCode1 = Encoding.GetEncoding("UTF-8").GetString(myDataBuffer1);
                    JObject? sourcechart1 = (JObject)JsonConvert.DeserializeObject(SourceCode1);
                    bpm = (int)sourcechart1["detail"]["MainBPM"];
                }
                break;
        }
    }

    double x1 = single; double x2 = slide; double x3 = flick + slideflick;
    double x1x2 = slidenode_true + (int)(lane_change / 100);
    double x1x3 = directional + directional_widthsum;
    double x2x3 = bpm + (int)(lane_change / 100);
    double[,] items = new double[3, 3] { { x1, x1x2, x1x3 }, { x1x2, x2, x2x3 }, { x1x3, x2x3, x3 } };
    Matrix<double> Properties = Matrix<double>.Build.DenseOfArray(items);
    return Properties;
}
Matrix<double> Properties(int id, string diff)
{
    int slide = 0, slideflick = 0, slidenode = 0, slidenode_true = 0;
    double lane_change = 0, beat_change = 0;
    int flick = 0, single = 0, bpm = 0, directional = 0, directional_widthsum = 0;
    string URL1 = $"https://bestdori.com/api/post/details?id={id}";
    string URL2 = $"https://api.ayachan.fun/DiffAnalysis?id={id}&diff={Difficulty[diff]}&speed=1.0";
    WebClient Client = new();
    try
    {
        byte[] myDataBuffer1 = Client.DownloadData(URL1);
    }
    catch (Exception)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,] { });
    }
    byte[] myDataBuffer = Client.DownloadData(URL1);
    string SourceCode = Encoding.GetEncoding("UTF-8").GetString(myDataBuffer);
    var sourcechart = (JObject)JsonConvert.DeserializeObject(SourceCode);
    var chart = sourcechart["post"]["chart"];
    foreach (var item in chart)
    {
        switch (item["type"].ToString())
        {
            case "Slide":
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
                        lanes.Clear();
                        beats.Clear();
                        lanes_changes.Clear();
                        beats_changes.Clear();
                    }
                    break;
                }
            case "Single":
            case "Long":
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
                break;
            case "Directional":
                directional++;
                directional_widthsum += (int)item["width"];
                break;
            case "BPM":
                if (bpm == 0)
                    {
                        byte[] myDataBuffer1 = Client.DownloadData(URL2);
                        string SourceCode1 = Encoding.GetEncoding("UTF-8").GetString(myDataBuffer1);
                        JObject? sourcechart1 = (JObject)JsonConvert.DeserializeObject(SourceCode1);
                        bpm = (int)sourcechart1["detail"]["MainBPM"];
                    }
                break;
        }
    }

    double x1 = single; double x2 = slide; double x3 = flick + slideflick;
    double x1x2 = slidenode_true + (int)(lane_change / 100);
    double x1x3 = directional + directional_widthsum;
    double x2x3 = bpm + (int)(lane_change / 100);
    double[,] items = new double[3, 3] { { x1, x1x2, x1x3 }, { x1x2, x2, x2x3 }, { x1x3, x2x3, x3 } };
    Matrix<double> Properties = Matrix<double>.Build.DenseOfArray(items);
    return Properties;
}
Matrix<double> Hit_Allocation(int id, string diff)
{
    string URL = $"https://api.ayachan.fun/distribution?id={id}&diff={Difficulty[diff]}";
    WebClient Client = new();
    byte[] Buffer = Client.DownloadData(URL);
    string Content = Encoding.GetEncoding("UTF-8").GetString(Buffer);
    JObject? distribution = (JObject)JsonConvert.DeserializeObject(Content);
    try
    {
        var a = distribution["error"];
        if (a == null)
        {
            
        }
        else
        {
            if (distribution["error"].ToString() == "这不是一张谱面")
            {
                return Matrix<double>.Build.DenseOfArray(new double[,] { });
            }
        }
    }
    catch (NullReferenceException)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,] { });
    }
    JToken? Distribute = distribution["hitDistribution"];
    string[] Distribution = JsonConvert.SerializeObject(Distribute).Split(new char[] { ',', '[', ']' });
    List<int> dis = new();
    for (int i = 0; i < Distribution.Length; i++)
    {
        try
        {
            dis.Add(Convert.ToInt32(Distribution[i]));
        }
        catch (Exception)
        {
            continue;
        }
    }
    int square_initial_constant = (int)Math.Sqrt(dis.Count), square_final_constant = (int)Math.Sqrt(dis.Count);
    if (Math.Sqrt(dis.Count) - square_initial_constant is > 0 and < 1) square_final_constant++;
    Matrix<double> hit_matrix = Matrix<double>.Build.Dense(square_final_constant, square_final_constant);
    //right triangle part
    List<int> intso = new();
    //left triangle part
    List<int> intsb = new();
    //diagnoal part
    List<int> ints = new();
    List<int> diag = new();
    for (int i = 0; i <= (int)Math.Sqrt(dis.Count); i++)
    {
        if (i == 0)
        {
            for (int n = 1; n <= (int)Math.Sqrt(dis.Count) - i; n++)
            {
                ints.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
            }
            if (ints.Count - square_final_constant == -1)
            {
                ints.Add(1);
            }
            else
            {

            }
            ints[0]++;
            diag = ints;
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
                    try
                    {
                        intso.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
                    }
                    catch (Exception)
                    {
                        intso.Add(1);
                    }
                    try
                    {
                        intsb.Add(dis[(n + i) * (n + i) - 2 * i]);
                    }
                    catch (Exception)
                    {
                        intsb.Add(1);
                    }
                }
            }
            for (int n = 1; n < intso.Count; n++)
            {
                try
                {
                    double ele = (intso[n - 1] + intsb[n - 1]) / 2;
                    hit_matrix[n - 1, i + n - 1] = ele;
                    hit_matrix[i + n - 1, n - 1] = ele;
                }
                catch (Exception)
                {

                }
            }
            intso.Clear();
            intsb.Clear();
        }
    }
    for (int j = 0; j < diag.Count; j++)
    {
        int hit = diag[j];
        hit_matrix[j, j] = hit;
    }
    ints.Clear();
    return hit_matrix;
}
Matrix<double> Note_Allocation(int id, string diff)
{
    string URL = $"https://api.ayachan.fun/distribution?id={id}&diff={Difficulty[diff]}";
    WebClient Client = new();
    byte[] Buffer = Client.DownloadData(URL);
    string Content = Encoding.GetEncoding("UTF-8").GetString(Buffer);
    JObject? distribution = (JObject)JsonConvert.DeserializeObject(Content);
    try
    {
        var a = distribution["error"];
        if (a == null)
        {
            
        }
        else
        {
            if (distribution["error"].ToString() == "这不是一张谱面")
            {
                return Matrix<double>.Build.DenseOfArray(new double[,] { });
            }
        }
    }
    catch (NullReferenceException)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,] { });
    }
    JToken? Distribute = distribution["noteDistribution"];
    string[] Distribution = JsonConvert.SerializeObject(Distribute).Split(new char[] { ',', '[', ']' });
    List<int> dis = new();
    for (int i = 0; i < Distribution.Length; i++)
    {
        try
        {
            dis.Add(Convert.ToInt32(Distribution[i]));
        }
        catch (Exception)
        {
            continue;
        }
    }
    int square_initial_constant = (int)Math.Sqrt(dis.Count), square_final_constant = (int)Math.Sqrt(dis.Count);
    if (Math.Sqrt(dis.Count) - square_initial_constant is > 0 and < 1) square_final_constant++;
    Matrix<double> note_matrix = Matrix<double>.Build.Dense(square_final_constant, square_final_constant);
    //right triangle part
    List<int> intso = new();
    //left triangle part
    List<int> intsb = new();
    //diagnoal part
    List<int> ints = new();
    List<int> diag = new();
    for (int i = 0; i <= (int)Math.Sqrt(dis.Count); i++)
    {
        if (i == 0)
        {
            for (int n = 1; n <= (int)Math.Sqrt(dis.Count) - i; n++)
            {
                ints.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
            }
            if (ints.Count - square_final_constant == -1)
            {
                ints.Add(1);
            }
            else
            {

            }
            ints[0]++;
            diag = ints;
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
                    try
                    {
                        intso.Add(dis[(n + i) * (n + i) - 2 * i - 1]);
                    }
                    catch (Exception)
                    {
                        intso.Add(1);
                    }
                    try
                    {
                        intsb.Add(dis[(n + i) * (n + i) - 2 * i]);
                    }
                    catch (Exception)
                    {
                        intsb.Add(1);
                    }
                }
            }
            for (int n = 1; n < intso.Count; n++)
            {
                try
                {
                    double ele = (intso[n - 1] + intsb[n - 1]) / 2;
                    note_matrix[n - 1, i + n - 1] = ele;
                    note_matrix[i + n - 1, n - 1] = ele;
                }
                catch (Exception)
                {

                }
            }
            intso.Clear();
            intsb.Clear();
        }
    }
    for (int j = 0; j < diag.Count; j++)
    {
        int note = diag[j];
        note_matrix[j, j] = note;
    }
    ints.Clear();
    return note_matrix;
}
(Vector<double>, Vector<double>) Decomposition(Matrix<double> decomposit_to)
{
    if (decomposit_to.ToArray().Length == 0)
    {
        return (Vector<double>.Build.DenseOfArray(Array.Empty<double>()), Vector<double>.Build.DenseOfArray(Array.Empty<double>()));
    }
    var evd = decomposit_to.Evd(Symmetricity.Symmetric);
    Vector<double> Value_EVD = evd.EigenValues.Real();
    Matrix<double> Ortho_EVD = evd.EigenVectors;
    var svd = decomposit_to.Svd(true);
    Matrix<double> Ortho_SVD_1 = svd.U;
    Matrix<double> Ortho_SVD_2 = svd.VT;
    Vector<double> Singluar_SVD = svd.S;
    return (Value_EVD, Singluar_SVD);
}
Vector<double> DimensionExpansion(Vector<double> source, int expect_size)
{
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
double[] Positive_Negative_Judgement(Matrix<double> to_judge)
{
    if (to_judge.ColumnCount == 0) return Array.Empty<double>();
    var evd_judge = to_judge.Evd(Symmetricity.Symmetric);
    Vector<double> eig_judge = evd_judge.EigenValues.Real();
    int positive = 0; int negative = 0; int zero = 0;
    int maximum = 0; int minimum = 0;
    foreach (double i in eig_judge)
    {
        switch (i)
        {
            case > 0.0:
                positive++;
                break;
            case < 0.0:
                negative++;
                break;
            case 0.0:
                zero++;
                break;
            default:
                break;
        }
    }
    switch (zero)
    {
        case 0:
            maximum = positive;
            minimum = negative;
            break;
        case not 0:
            break;
        default:
    }
    return new double[3] { maximum, minimum, zero };
}
double CurveIntegration(Matrix<double> to_extract)
{
    if (to_extract.ColumnCount == 0) return 0;
    double x1 = to_extract[0, 0];
    double x2 = to_extract[1, 1];
    double x3 = to_extract[2, 2];
    double x1x2 = to_extract[0, 1];
    double x1x3 = to_extract[0, 2];
    double x2x3 = to_extract[1, 2];
    double k11 = x1;
    double k22 = x2;
    double k33 = x3;
    double k12 = x1x2, k21 = x1x2;
    double k13 = x1x3, k31 = x1x3;
    double k23 = x2x3, k32 = x2x3;
    double integration = NewtonCotesTrapeziumRule.IntegrateTwoPoint(t =>
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
        0, 5);
    return integration;
}
double PleatuIntegration(Matrix<double> to_extract)
{
    if (to_extract.ColumnCount == 0) return 0;
    double x1 = to_extract[0, 0];
    double x2 = to_extract[1, 1];
    double x3 = to_extract[2, 2];
    double x1x2 = to_extract[0, 1];
    double x1x3 = to_extract[0, 2];
    double x2x3 = to_extract[1, 2];
    double integration = GaussLegendreRule.Integrate((u, v) =>
    ((u * u - u * v + v * v) * (u * u - u * v + v * v) * x1 +
     (u * u + u * v - v * v) * (u * u + u * v - v * v) * x2 +
     (v * v + u * v - u * u) * (v * v + u * v - u * u) * x3 +
     (u * u - u * v + v * v) * (u * u + u * v - v * v) * x1x2 +
     (v * v + u * v - u * u) * (u * u + u * v - v * v) * x2x3 +
     (u * u - u * v + v * v) * (v * v + u * v - u * u) * x1x3) *
    Math.Sqrt(
        ((2 * u - v) * (2 * u - v) + (2 * u + v) * (2 * u + v) + (-2 * u + v) * (-2 * u + v)) *
        ((2 * v - u) * (2 * v - u) + (-2 * v + u) * (-2 * v + u) + (2 * v + u) * (2 * v + u)) -
        ((2 * u - v) * (2 * v - u) + (2 * u + v) * (-2 * v + u) + (-2 * u + v) * (2 * v + u)) *
        ((2 * u - v) * (2 * v - u) + (2 * u + v) * (-2 * v + u) + (-2 * u + v) * (2 * v + u))),
    0, 10, 0, 10, 8);
    return integration;
}
double MainProcess(int id, string diff = "expert")
{
    XlsxFileImport();
    Task<Matrix<double>> task1 = Task.Run(() => Properties(id, diff));
    Task<Matrix<double>> task2 = Task.Run(() => Hit_Allocation(id, diff));
    Task<Matrix<double>> task3 = Task.Run(() => Note_Allocation(id, diff));
    Task[] task_basic_data = new Task[3] { task1, task2, task3 };
    Task.WaitAll(task_basic_data);
    Task<double[]> task4 = Task.Run(() => Positive_Negative_Judgement(task1.Result));
    Task<(Vector<double>, Vector<double>)> task5 = Task.Run(() => Decomposition(task2.Result));
    Task<(Vector<double>, Vector<double>)> task6 = Task.Run(() => Decomposition(task3.Result));
    Task[] task_extract_information = new Task[3] { task4, task5, task6 };
    Task.WaitAll(task_extract_information);
    double[] precise_correction = task4.Result;
    Task<Vector<double>> task7 = Task.Run(() => DimensionExpansion(task5.Result.Item2, 18));
    Task<Vector<double>> task8 = Task.Run(() => DimensionExpansion(task6.Result.Item2, 18));
    Task<double> task9 = Task.Run(() => CurveIntegration(task1.Result));
    Task<double> taskA = Task.Run(() => PleatuIntegration(task1.Result));
    Task[] task_formula_production = new Task[4] { task7, task8, task9, taskA };
    Task.WaitAll(task_formula_production);
    Vector<double> dh_evaluation = Vector<double>.Build.DenseOfVector(task7.Result) * pca_hit_transform;
    Vector<double> dn_evaluation = Vector<double>.Build.DenseOfVector(task8.Result) * pca_nte_transform;
    double diff1 = 0, diff2 = 0, diff3 = 0, diff4 = 0;
    for (int i = 0; i <= 4; i++)
    {
        switch (i)
        {
            case < 4:
                diff1 += dh_evaluation[i] * reg_hit[i];
                diff2 += dn_evaluation[i] * reg_nte[i];
                break;
            case 4:
                diff1 += reg_hit[i];
                diff2 += reg_nte[i];
                break;
            default:
                break;
        }
    }
    double initial_level = diff1 * 0.3 + diff2 * 0.7;
    double[] precise_correction_constants = new double[3] { initial_level, precise_correction[0], precise_correction[1] };
    for (int j = 0; j < 4; j++)
    {
        switch (j)
        {
            case < 3:
                diff3 += precise_correction_constants[j] * reg_cor_const[j];
                break;
            case 3:
                diff3 += reg_cor_const[j];
                break;
            default:
                break;
        }
    }
    double secondary_level = diff3;
    double integration1 = task9.Result;
    double integration2 = taskA.Result;
    double[] curve_pleatu_corrections = new double[3] { Math.Log(Math.Abs(integration1)), Math.Log(Math.Abs(integration2)), secondary_level };
    for (int j = 0; j < 4; j++)
    {
        switch (j)
        {
            case < 3:
                diff4 += curve_pleatu_corrections[j] * reg_cpc_const[j];
                break;
            case 3:
                diff4 += reg_cpc_const[j];
                break;
            default:
                break;
        }
    }
    double third_level = diff4;
    return third_level;
}
double MainProcessO(int id, string diff = "expert")
{
    XlsxFileImport();
    Task<Matrix<double>> task1 = Task.Run(() => PropertiesO(id, diff));
    Task<Matrix<double>> task2 = Task.Run(() => Hit_Allocation(id, diff));
    Task<Matrix<double>> task3 = Task.Run(() => Note_Allocation(id, diff));
    Task[] task_basic_data = new Task[3] { task1, task2, task3 };
    Task.WaitAll(task_basic_data);
    Task<double[]> task4 = Task.Run(() => Positive_Negative_Judgement(task1.Result));
    Task<(Vector<double>, Vector<double>)> task5 = Task.Run(() => Decomposition(task2.Result));
    Task<(Vector<double>, Vector<double>)> task6 = Task.Run(() => Decomposition(task3.Result));
    Task[] task_extract_information = new Task[3] { task4, task5, task6 };
    Task.WaitAll(task_extract_information);
    double[] precise_correction = task4.Result;
    Task<Vector<double>> task7 = Task.Run(() => DimensionExpansion(task5.Result.Item2, 18));
    Task<Vector<double>> task8 = Task.Run(() => DimensionExpansion(task6.Result.Item2, 18));
    Task<double> task9 = Task.Run(() => CurveIntegration(task1.Result));
    Task<double> taskA = Task.Run(() => PleatuIntegration(task1.Result));
    Task[] task_formula_production = new Task[4] { task7, task8, task9, taskA };
    Task.WaitAll(task_formula_production);
    Vector<double> dh_evaluation = Vector<double>.Build.DenseOfVector(task7.Result) * pca_hit_transform;
    Vector<double> dn_evaluation = Vector<double>.Build.DenseOfVector(task8.Result) * pca_nte_transform;
    double diff1 = 0, diff2 = 0, diff3 = 0, diff4 = 0;
    for (int i = 0; i <= 4; i++)
    {
        switch (i)
        {
            case < 4:
                diff1 += dh_evaluation[i] * reg_hit[i];
                diff2 += dn_evaluation[i] * reg_nte[i];
                break;
            case 4:
                diff1 += reg_hit[i];
                diff2 += reg_nte[i];
                break;
            default:
                break;
        }
    }
    double initial_level = diff1 * 0.3 + diff2 * 0.7;
    double[] precise_correction_constants = new double[3] { initial_level, precise_correction[0], precise_correction[1] };
    for (int j = 0; j < 4; j++)
    {
        switch (j)
        {
            case < 3:
                diff3 += precise_correction_constants[j] * reg_cor_const[j];
                break;
            case 3:
                diff3 += reg_cor_const[j];
                break;
            default:
                break;
        }
    }
    double integration1 = task9.Result;
    double integration2 = taskA.Result;
    double secondary_level = diff3;
    double[] curve_pleatu_corrections = new double[3] { Math.Log(Math.Abs(integration1)), Math.Log(Math.Abs(integration2)), secondary_level };
    for (int j = 0; j < 4; j++)
    {
        switch (j)
        {
            case < 3:
                diff4 += curve_pleatu_corrections[j] * reg_cpc_const[j];
                break;
            case 3:
                diff4 += reg_cpc_const[j];
                break;
            default:
                break;
        }
    }
    double third_level = diff4;
    return third_level;
}
//107589 361<381<400, 20 rank matrix
//115375 324<337<361, 19 rank matrix
int[] samples = new int[] { 118640, 117974, 117590, 52890, 109547, 99692, 93358, 88473, 87270, 86956, 125851, 125819, 125820, 125356, 118308, 116208, 115375, 115094, 62907, 103637 };
await Parallel.ForEachAsync(
    samples,
    new ParallelOptions() { CancellationToken = CancellationToken.None, MaxDegreeOfParallelism = 6000 },
    async (item, _) =>
    {
        await Task.Run(() =>
        {
            try
            {
                var a = new WebClient().DownloadData($"https://api.ayachan.fun/DiffAnalysis?id={item}&diff=3&speed=1.0");
                var b = new WebClient().DownloadData($"https://api.ayachan.fun/distribution?id={item}&diff=3");
                var c = new WebClient().DownloadData($"https://bestdori.com/api/post/details?id={item}");
            }
            catch (WebException ex)
            {
                throw ex;
            }
            Console.WriteLine($"{item},{MainProcess(item)}");
        }, _);
    });