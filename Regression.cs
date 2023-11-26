namespace Core_Advanced_13_1
{
    internal interface IRegression
    {
        double Uni_Regression(double[] coefficient, double[] variables);
        double Bi_Regression(double[] coefficient, double[] variables);
        double Tri_Regression(double[] coefficient, double[] variables);
        double Sex_Regression(double[] coefficient, double[] variables);
    }
    class Regression : IRegression
    {
        public double Uni_Regression(double[] coefficient, double[] variables)
        {
            if (coefficient.Length != variables.Length || variables.Length != 2) return 0;
            double result = 0;
            for (int i = 0; i < coefficient.Length; i++)
            {
                result += coefficient[i] * variables[i];
            }
            return result;
        }
        public double Bi_Regression(double[] coefficient, double[] variables)
        {
            if (coefficient.Length != variables.Length || variables.Length != 3) return 0;
            double result = 0;
            for (int i = 0; i < coefficient.Length; i++)
            {
                result += coefficient[i] * variables[i];
            }
            return result;
        }
        public double Tri_Regression(double[] coefficient, double[] variables)
        {
            if (coefficient.Length != variables.Length || variables.Length != 4) return 0;
            double result = 0;
            for (int i = 0; i < coefficient.Length; i++)
            {
                result += coefficient[i] * variables[i];
            }
            return result;
        }
        public double Sex_Regression(double[] coefficient, double[] variables)
        {
            if (coefficient.Length != variables.Length || variables.Length != 7) return 0;
            double result = 0;
            for (int i = 0; i < coefficient.Length; i++)
            {
                result += coefficient[i] * variables[i];
            }
            return result;
        }
    }
}