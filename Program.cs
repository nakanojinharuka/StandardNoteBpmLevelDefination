using Core_Advanced_13_1;
using OfficeOpenXml;
//ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//ExcelPackage package = new("F:/Bandori stats 3/Transformation and Regression.xlsx");
//ExcelWorksheet Hit_Transform = package.Workbook.Worksheets["Hit Transform"];
//ExcelWorksheet Noe_Transform = package.Workbook.Worksheets["Note Transform"];
//ExcelWorksheet REG_Constants = package.Workbook.Worksheets["Regression"];
for (int i = 487; i < 545; i++)
{
    ArtificialStaff staff = new(i, "expert");
    if (staff.Exists == -1) continue;
    double[] integral = staff.Properties_Integration();
    Console.WriteLine($"{i},expert,{staff.LevelNum},{integral[0]},{integral[1]},{integral[2]},{integral[3]}");
    ArtificialStaff staff2 = new(i, "special");
    if (staff2.Exists == -1) continue;
    integral = staff2.Properties_Integration();
    Console.WriteLine($"{i},special,{staff2.LevelNum},{integral[0]},{integral[1]},{integral[2]},{integral[3]}");
}