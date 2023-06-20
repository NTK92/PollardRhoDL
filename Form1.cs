using System.Numerics;
using System.Security.Cryptography;

namespace PollardRhoDL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        BigInteger p, g, t;
        
        public BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }
        private BigInteger GenerateNBitNumber(int N)
        {
            BigInteger random = 0;
            do
            {
                random = RandomBigint(BigInteger.Pow(2, N));
            }
            while (random < BigInteger.Pow(2, N - 1));
            return random;
        }
        public BigInteger GeneratePrime(int bits)
        {
            BigInteger result = BigInteger.Pow(2, bits - 1) + 1;
            while (!Solovay_Strassen(result))
                result += 2;
            return result;
        }
        public static BigInteger RandomBigint(BigInteger N)
        {
            byte[] bytes = N.ToByteArray();
            BigInteger R;

            do
            {
                Random random = new Random();
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
                R = new BigInteger(bytes);
            } while (R >= N);

            return R;
        }
        public BigInteger Yakobi(BigInteger a, BigInteger b)
        {
            if (BigInteger.GreatestCommonDivisor(a, b) != 1) // 1 проверка взаимной простоты
                return 0;
            BigInteger r = 1; // 2 инициализация
            if (a < 0) // 3 переход к положительным числам
            {
                a *= -1;
                if (b % 4 == 3)
                    r *= -1;
            }
            while (a != 0)
            {
                BigInteger t = 0; // 4 избавление от чётности
                while (a % 2 == 0)
                {
                    t++;
                    a = a / 2;
                }
                if (t % 2 != 0)
                    if (b % 8 == 3 || BigInteger.ModPow(b, 1, 8) == 5)
                        r *= -1;
                if (a % 4 == 3 && b % 4 == 3) // 5 квадратичный закон взаимности
                    r *= -1;
                BigInteger c = a;
                a = b % c;
                b = c;
            }
            return r;
        }
        public bool Solovay_Strassen(BigInteger n) // тест простоты соловея штрассена
        {
            bool b = true; //простое
            if (n <= 2 || n % 2 == 0)
            {
                MessageBox.Show("n должно быть больше 2");
                b = false;
                return b;
            }
            int k = 5;
            for (int i = 1; i <= k; i++)
            {
                BigInteger a = RandomBigint(n - 1);
                if (BigInteger.GreatestCommonDivisor(a, n) > 1)
                {
                    b = false;
                    return b;
                }
                if (BigInteger.ModPow(a, (n - 1) / 2, n) != Yakobi(a, n) % n)
                {
                    b = false;
                    return b;
                }
            }
            return b;
        }
        public BigInteger Primitive_Root(BigInteger m) // 16 ил 8 - число бит
        {
            //BigInteger g = 2;
            BigInteger g = BigInteger.Pow(2, 4 - 1) + 1;
            while (true)
            {
                // 1
                if (BigInteger.GreatestCommonDivisor(g, m) != 1)
                {
                    g++;
                    continue;
                }
                // 2
                if (BigInteger.ModPow(g, (m - 1) / 2, m) + 1 == m)
                {
                    g++;
                    continue;
                }
                else
                {
                    if (BigInteger.ModPow(g, (m - 1) / 2, m) - 1 == m)

                        for (int l = 1; l < m - 1; l++)
                            if (BigInteger.Pow(g, l) % m == 1 % m)
                            {
                                g++;
                                continue;
                            }
                    return g;
                }
            }
        }

        public IEnumerable<BigInteger> GetDiscreteLogarithm(BigInteger p, BigInteger g, BigInteger t)
        {
            BigInteger x1 = 1, a1 = 0, b1 = 0;
            BigInteger x2 = 1, a2 = 0, b2 = 0;
            
            for (var i = 1; i < p - 1; ++i)
            {
                (x1, a1, b1) = NewXAB(x1, a1, b1, p, g, t);
                (x2, a2, b2) = NewXAB(x2, a2, b2, p, g, t);
                (x2, a2, b2) = NewXAB(x2, a2, b2, p, g, t);
                if (x1 == x2)
                    break;
            }
            
            var B = (b1 - b2) % (p - 1);
            var A = (a2 - a1) % (p - 1);
            var d = BigInteger.GreatestCommonDivisor(B, (p - 1));
            //BigInteger inv = modInverse(BigInteger.Abs(B), (p - 1));
            //var x = (inv * (A % (p - 1))) % (p - 1);
            var x = GetCoeff(B, (p - 1)*(-1), A);
            var listResults = new List<BigInteger>() { x };
            
            var j = BigInteger.One;
            
            while (j < d)
            {
                var index = listResults.Count - 1;
                var new_x = listResults[index] + ((p - 1) / d);
                listResults.Add(new_x);
                j++;
            }

            var finalResults = new List<BigInteger>();
            foreach (var res in listResults)
            {
                if (BigInteger.ModPow(g, BigInteger.Abs(res), p) == t)
                {
                    finalResults.Add(res);
                }
            }

            return finalResults;
        }
        
        private (BigInteger x, BigInteger a, BigInteger b) NewXAB(BigInteger x, BigInteger a, BigInteger b,
            BigInteger p, BigInteger g, BigInteger t)
        {
            switch ((int)(BigInteger.ModPow(x, 1, 3)))
            {
                case 0:
                    x = BigInteger.Pow(x, 2) % p;
                    a = 2 * a % (p - 1);
                    b = 2 * b % (p - 1);
                    return (x, a, b);
                case 1:
                    x = (g * x) % p;
                    a = (a + 1) % (p - 1);
                    return (x, a, b);
                case 2:
                    x = (t * x) % p;
                    b = (b + 1) % (p - 1);
                    return (x, a, b);
            }
            
            return (x, a, b);
        }

       
        private BigInteger GetCoeff(BigInteger B, BigInteger P, BigInteger A)
        {
            if (A == 0)
                return 0;

            var gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(B), BigInteger.Abs(P));
            B /= gcd;
            P /= gcd;
            A /= gcd;
            for (var i = 0; i < BigInteger.Abs(B); i++)
            {
                if ((A - P * i) % B == 0)
                {
                    var y = i;
                    var x = (A - P * y) / B;
                    return x;
                }
            }
            return -1;
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            
            p = BigInteger.Parse(textBox1.Text);
            /*if (!Solovay_Strassen(p))
            {
                MessageBox.Show("p должно быть простым");
                return;
            }*/
            if (textBox4.Text != "")
            {
                g = BigInteger.Parse(textBox4.Text);
            }
            else 
            {
                g = Primitive_Root(p);
                textBox4.Text = p.ToString();
            }
            t = BigInteger.Parse(textBox3.Text);
            var res = GetDiscreteLogarithm(p,g,t);
            textBox2.Text = "";
            if (!res.Any())
            {
                textBox2.Text = "Решений нет";
            }
            foreach (var r in res)
            {
                textBox2.Text += r.ToString()+" ";
            }
            

            /*int x = 1, a = 0, b = 0;
            int X = x, A = a, B = b;
            int res = 0;
            for (int i = 1; i < p - 1; ++i)
            {
                (x, a, b) = New_xab(x, a, b);
                (X, A, B) = New_xab(X, A, B);
                (X, A, B) = New_xab(X, A, B);
                if (x == X)
                {
                    int[] arr = new int[3];
                    arr[0] = (B - b + p - 1) % (p - 1);
                    arr[1] = (a - A + p - 1) % (p - 1);
                    arr[2] = (p - 1);
                    int divider = findGCD(arr, 3);
                    if (divider != 1)
                    {
                        int _a = (B - b + p - 1) % (p - 1) / divider;
                        int _b = (a - A + p - 1) % (p - 1) / divider;
                        int modulus = (p - 1) / divider;
                        int inv = modInverse(_a, modulus);
                        res = (_b * inv) % modulus;
                        break;
                    }
                    else
                    {
                        int inv = modInverse((B - b), (p - 1));
                        res = (inv * ((a - A) % (p - 1))) % (p - 1);
                        break;
                    }
                }
            }
            textBox2.Text = res.ToString();*/
            /*
            if (0 < x && x < (p / 3))
            {
                new_x = (g * x) % p;
                new_a = (a + 1) % (p - 1);
                new_b = b;
                return (new_x, new_a, new_b);
            }
            else if (x >= (p / 3) && x < (2 * p / 3))
            {
                new_x = BigInteger.Pow(x, 2) % p;
                new_a = 2 * a % (p - 1);
                new_b = 2 * b % (p - 1);
                return (new_x, new_a, new_b);
            }
            else
            {
                new_x = (t * x) % p;
                new_a = a;
                new_b = (b + 1) % (p - 1);
                return (new_x, new_a, new_b);
            }*/
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            BigInteger _a = GenerateNBitNumber(16); // генерируем закрытый ключ (16 бит)
            textBox5.Text = _a.ToString();
            BigInteger _p = GeneratePrime(16); // генерируем открытые параметры, р - простое (16 бит)
            textBox1.Text = _p.ToString();
            BigInteger _g = Primitive_Root(_p); // g является первообразным корнем по модулю p (также является простым числом, 4 бит)
            textBox4.Text = _g.ToString();
            BigInteger _A = BigInteger.ModPow(_g, _a, _p);
            textBox3.Text = _A.ToString();
            var _res = GetDiscreteLogarithm(_p, _g, _A);
            if (!_res.Any())
            {
                textBox2.Text = "Решений нет";
            }
            foreach (var r in _res)
            {
                textBox2.Text += r.ToString() + " ";
            }
        }

    }
}