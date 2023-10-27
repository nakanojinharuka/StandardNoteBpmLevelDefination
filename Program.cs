// This Code is created by Nakano. Copying requires permission. 
using Core_Advanced_13_1;
using MathNet.Numerics.LinearAlgebra;
using OfficeOpenXml;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
ExcelPackage package = new("F:/Bandori stats 3/Transformation and Regression.xlsx");
ExcelWorksheet Hit_Transform = package.Workbook.Worksheets["Hit Transform"];
ExcelWorksheet Noe_Transform = package.Workbook.Worksheets["Note Transform"];
ExcelWorksheet REG_Constants = package.Workbook.Worksheets["Regression"];
ExcelRange hit_t = Hit_Transform.Cells[1, 1, 18, 5];
ExcelRange nte_t = Noe_Transform.Cells[1, 1, 18, 4];
ExcelRange reg_c = REG_Constants.Cells[1, 1, 7, 1];
double[,] pca_transform_hit = new double[18, 5];
double[,] pca_transform_nte = new double[18, 4];
double[] reg = new double[7];
for (int i = 0; i < 18; i++)
{
    for (int j = 0; j <= 6; j++)
    {
        switch (j)
        {
            case < 4:
                pca_transform_hit[i, j] = hit_t.GetCellValue<double>(i, j);
                pca_transform_nte[i, j] = nte_t.GetCellValue<double>(i, j);
                reg[j] = reg_c.GetCellValue<double>(j, 0);
                break;
            case 4:
                pca_transform_hit[i, j] = hit_t.GetCellValue<double>(i, j);
                reg[j] = reg_c.GetCellValue<double>(j, 0);
                break;
            case > 4:
                reg[j] = reg_c.GetCellValue<double>(j, 0);
                break;
            default:
        }
    }
}
Matrix<double> pca_hit_transform = Matrix<double>.Build.DenseOfArray(pca_transform_hit);
Matrix<double> pca_nte_transform = Matrix<double>.Build.DenseOfArray(pca_transform_nte);
Vector<double> regression = Vector<double>.Build.DenseOfArray(reg);
ArtificialStaff staff = new(128615, "expert");
Task<double[]> task1 = Task.Run(() => staff.Properties_Integration());
Task<Matrix<double>> task2 = Task.Run(() => staff.Distribution_Allocation("hit"));
Task<Matrix<double>> task3 = Task.Run(() => staff.Distribution_Allocation("note"));
Task[] task_basic_data = new Task[3] { task1, task2, task3 };
Task.WaitAll(task_basic_data);
Task<Vector<double>> task4 = Task.Run(() => ArtificialStaff.Decomposition_DimensionExpansion(task2.Result, 18));
Task<Vector<double>> task5 = Task.Run(() => ArtificialStaff.Decomposition_DimensionExpansion(task3.Result, 18));
Task[] task_singluar_value = new Task[2] { task4, task5 };
Task.WaitAll(task_singluar_value);
Vector<double> pca_transformed_hit = pca_hit_transform.Transpose() * task4.Result;
Vector<double> pca_transformed_nte = pca_nte_transform.Transpose() * task5.Result;
double[] regression_variable = new double[7] { 1, task1.Result[0], task1.Result[2], task1.Result[3], pca_transformed_hit[0], pca_transformed_hit[2], pca_transformed_nte[3] };
Regression regr = new();
double result = regr.Sex_Regression(reg, regression_variable);
Console.WriteLine(result);