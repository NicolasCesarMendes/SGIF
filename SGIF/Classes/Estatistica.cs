namespace SGIF.Classes
{
    // Cálculos estatísticos simples, sem acesso ao banco.
    public static class Estatistica
    {
        // Reta y = intercepto + inclinacao * x pelo método dos mínimos quadrados.
        // r2 (0 a 1) mostra o quanto a reta explica os dados.
        public static void RegressaoLinear(double[] x, double[] y, out double inclinacao, out double intercepto, out double r2)
        {
            int n = x.Length;
            double mediaX = x.Average();
            double mediaY = y.Average();

            double sxx = 0, sxy = 0, syy = 0;

            for (int i = 0; i < n; i++)
            {
                double dx = x[i] - mediaX;
                double dy = y[i] - mediaY;

                sxx += dx * dx;
                sxy += dx * dy;
                syy += dy * dy;
            }

            if (sxx == 0)
            {
                inclinacao = 0;
                intercepto = mediaY;
                r2 = 0;
                return;
            }

            inclinacao = sxy / sxx;
            intercepto = mediaY - inclinacao * mediaX;
            r2 = syy == 0 ? 0 : (sxy * sxy) / (sxx * syy);
        }
    }
}
