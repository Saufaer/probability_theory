using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TV_Lab
{
    public struct Y
    {
        //public List<double> p; // вектор вероятностей
        public int size; // размер вектора вероятностей 
        public int res; // случайная величина
        public int ni; // сколько раз встретили случайную величину
        public int number; // номер испытания
        public bool original; // уникальность
        public double prob_disc; // невязка вероятности
        public double pi; // вероятность найденная
    }
    public struct Fn // хранение кусков функции выборочной функции распределения
    {
        public double left; // левая граница
        public double right; // правая граница
        public double p; // вероятность
    }
    public class SV
    {
        public double intensity; // интенсивность
        public double time; // время
        public int count; // количество экспериментов
        public double param; // intencity*time // параметр 
        public int real_count;  // количество найденных значений СВ
        private bool IsSorted;
        //public double[] y;
        public Y[] array_yi; // вычислительная структура СВ (несорт, нефильтр)
        public Y[] work_yi; // рабочая структура полученных и сортированных СВ
        public Fn[] real_Fn; // хранение кусков функции распределения
        public Fn[] select_Fn; // хранение кусков выборочной функции распределения
        // характеристики СВ
        public int ni_all; // сколько получили ИТОГО СВ
        public double x_medium; // среднее значение СВ
        public double prob_disc_max; // максимальное значение невязки вероятности
        public double En; // матожидание
        public double En_calc; // матожидание посчитанное экспериментально
        public double Dn; // дисперсия
        public double Dn_calc; // дисперсия посчитанная экспериментально
        public double Dn_medium; // выборочная дисперсия
        public double Me; // медиана
        public double R; // размах выборки
        public double D_outrun; // мера расхождения // max(abs(Fn_real - Fn_select))

        private long factorial(long x)
        {
            return (x == 0) ? 1 : x * factorial(x - 1);
        }

        public SV() { }

        public SV(double _intensity, double _time, int _count) // конструктор по входным параметрам
        {
            intensity = _intensity; // интенсивность а/м
            time = _time; // время
            count = _count; // количество испытаний

            IsSorted = false;

            array_yi = new Y[count];
        }

        public SV(SV enter)
        {
            intensity = enter.intensity;
            time = enter.time;
            count = enter.count;
            array_yi = new Y[count];
            IsSorted = enter.IsSorted;
            for (int i = 0; i < count; i++)
            {
                array_yi[i].pi = enter.array_yi[i].pi;
                array_yi[i].res = enter.array_yi[i].res;
                array_yi[i].size = enter.array_yi[i].size;
                array_yi[i].number = enter.array_yi[i].number;
                array_yi[i].original = enter.array_yi[i].original;
            }
        }

        public void Set(double _intensity, double _time, int _count) 
        {
            if(count!=0)
            {
                for (int i = 0; i < count; i++)
                {
                    array_yi[i].original = true;
                }
                array_yi = null;
            }
            IsSorted = false;
            intensity = _intensity; // интенсивность а/м
            time = _time; // время
            count = _count; // количество испытаний

            param = time * intensity;



          
            array_yi = new Y[count];// массив для y i-ых.
        }

        public void finalize()
        {
            for (int i = 0; i < count; i++)
            {
                array_yi[i].res = 0;
                array_yi[i].pi = 0;
                array_yi[i].size = 0;
                array_yi[i].number = 0;
                array_yi[i].original = true;
            }
            array_yi = null;
            work_yi = null;
            select_Fn = null;
            real_Fn = null;
            intensity = 0;
            time = 0;
            count = 0;
        }

        public void Generate()
        {
            Random rand = new Random();
            double param = intensity * time;
            for (int i = 0; i < count; i++)
            {
                double u = rand.NextDouble(); // "случайно" сгенерированное число "u"
                double p1, p0 = Math.Exp(-1 * param);
                double tmp = p0;
                array_yi[i].pi = tmp;
                int j = 0;
                array_yi[i].size = j;
                while (u > tmp) 
                {
                    j++;
                    p1 = (param / j) * p0;
                    array_yi[i].pi = p1;
                    p0 = p1;
                    tmp += p1;
                }
                
                {
                    array_yi[i].size = j;
                    array_yi[i].res = j;
                    array_yi[i].ni = 1;
                    array_yi[i].number = i;
                    array_yi[i].original = true;
                }
            }
            Check();
        }

        private void SortOriginal()
        {
            if (IsSorted) return;
            int tmp_res;
            int tmp_size;
            int tmp_ni;
            int tmp_number;
            double tmpi;
            double tmp_prob;
            for (int i = 0; i < real_count; i++)
            {
                for (int j = 0; j < real_count; j++) 
                {
                    if (work_yi[i].res < work_yi[j].res)
                    {
                        tmp_res = work_yi[i].res;
                        tmp_size = work_yi[i].size;
                        tmp_ni = work_yi[i].ni;
                        tmp_number = work_yi[i].number;
                        tmpi = work_yi[i].pi;
                        tmp_prob = work_yi[i].prob_disc;

                        work_yi[i].res = work_yi[j].res;
                        work_yi[i].size = work_yi[j].size;
                        work_yi[i].ni = work_yi[j].ni;
                        work_yi[i].number = work_yi[j].number;
                        work_yi[i].pi = work_yi[j].pi;
                        work_yi[i].prob_disc = work_yi[j].prob_disc;

                        work_yi[j].res = tmp_res;
                        work_yi[j].size = tmp_size;
                        work_yi[j].ni = tmp_ni;
                        work_yi[j].number = tmp_number;
                        work_yi[j].pi = tmpi;
                        work_yi[j].prob_disc = tmp_prob;
                    }
                }
            }
            IsSorted = true;
        }

        private void SortFullArray()
        {
            int tmp_res;
            int tmp_size;
            int tmp_ni;
            int tmp_number;
            double tmpi;
            double tmp_prob;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (array_yi[i].res < array_yi[j].res)
                    {
                        tmp_res = array_yi[i].res;
                        tmp_size = array_yi[i].size;
                        tmp_ni = array_yi[i].ni;
                        tmp_number = array_yi[i].number;
                        tmpi = array_yi[i].pi;
                        tmp_prob = array_yi[i].prob_disc;

                        array_yi[i].res = array_yi[j].res;
                        array_yi[i].size = array_yi[j].size;
                        array_yi[i].ni = array_yi[j].ni;
                        array_yi[i].number = array_yi[j].number;
                        array_yi[i].pi = array_yi[j].pi;
                        array_yi[i].prob_disc = array_yi[j].prob_disc;

                        array_yi[j].res = tmp_res;
                        array_yi[j].size = tmp_size;
                        array_yi[j].ni = tmp_ni;
                        array_yi[j].number = tmp_number;
                        array_yi[j].pi = tmpi;
                        array_yi[j].prob_disc = tmp_prob;
                    }
                }
            }
        }

        public void FilterOriginal()
        {
            // Ищем оригинальные СВ
            SortFullArray();
            int new_size = 0;
            for (int i = 0; i < count; i++) 
            {
                if (!array_yi[i].original) continue;
                for (int j = 0; j < count; j++)
                {
                    if ((array_yi[i].res == array_yi[j].res) && i != j)
                    {
                        array_yi[j].original = false;
                        ++new_size;
                    }
                       
                }    
            }
            real_count = count - new_size;

            work_yi = new Y[real_count];

            ni_all = 0;

            // создание новой структуры для оригинальных СВ
            for (int i = 0, j = 0; i < count; i++) 
            {
                if (!array_yi[i].original) continue;
                work_yi[j].ni = array_yi[i].ni;
                ni_all += work_yi[j].ni;
                work_yi[j].res = array_yi[i].res;
                work_yi[j].number = array_yi[i].number;
                work_yi[j].original = true;
                work_yi[j].pi = array_yi[i].pi;
                work_yi[j].size = array_yi[i].size;
                ++j;
            }
            // Сортируем
            
            //SortOriginal();
            // ищем невязку вероятности
            for (int i = 0; i < real_count; i++)
            {
                work_yi[i].prob_disc = Math.Abs(((double)work_yi[i].ni / (double)count) - work_yi[i].pi);
            }
            // ищем максимум
            prob_disc_max = work_yi[0].prob_disc;
            for (int i = 1; i < real_count; i++)
            {
                if (prob_disc_max < work_yi[i].prob_disc) prob_disc_max = work_yi[i].prob_disc;
            }
        }

        public int FindMaxres()
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                if (value < array_yi[i].res)
                    value = array_yi[i].res;
            }
            return value;
        }

        public void FindValueSettings()
        {
            FindXMedium();
            CalcEn();
            CalcDn();
            CalcMediumDn();
            CalcMe();
            CalcR();
            CalcFnReal(0.000001);
            CalcFnSelect();
            CalcD_outrun();
            CalcProb_disc_max();
        }
        private void CalcDn()
        {
            Dn_calc = 0;
            double first = 0, second = 0;

            for (int i = 0; i< real_count; i++)
            {
                first += Math.Pow(work_yi[i].res, 2) * work_yi[i].pi;
                second += work_yi[i].res * work_yi[i].pi;
            }
            second *= second;
            Dn_calc = first - second;
            Dn = intensity * time;
        }
        private void CalcMe()
        {
            int k;
            if (count % 2 > 0)
            {           // n = 2k + 1
                k = count / 2;
                Me = array_yi[k].res;
            }
            else
            {           // n = 2k
                k = count / 2;
                Me = (array_yi[k - 1].res + array_yi[k].res) / 2;
            }
        }
        private void CalcEn()
        {
            En_calc = 0;
            for (int i = 0; i < real_count; i++)
            {
                En_calc += work_yi[i].res * work_yi[i].pi;
            }
            En = intensity * time;
        }

        private void CalcR()
        {
            R = work_yi[real_count-1].res - work_yi[0].res;
        }

        private void CalcMediumDn() //S*
        {
            double sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += Math.Pow(array_yi[i].res - x_medium, 2);
            }
            Dn_medium = sum / count;
        }
        private void FindXMedium()
        {
            double sum = 0;
            for (int i = 0; i < count; i++) 
            {
                sum += array_yi[i].res;
            }
            x_medium = sum / count;
        }
        private void Check()
        {
            for (int i = 0; i < count; i++) 
            {
                for (int j = 0; j < count; j++) 
                {
                    if ((array_yi[i].res == array_yi[j].res) && (i!=j))
                    {
                        array_yi[i].ni++;
                    }
                }
            }
        }

        private void CalcFnSelect() // считаем выборочную функцию распределения
        {
            // всё таки надо с нуля
            select_Fn = new Fn[real_count];
            double summ = 0;
            for (int i = 0; i < real_count; i++)
            {
                summ += work_yi[i].ni;
                select_Fn[i].left = work_yi[i].res;
                if (i < real_count - 1) select_Fn[i].right = work_yi[i + 1].res;
                else select_Fn[i].right = work_yi[i].res + 3;
                select_Fn[i].p = summ /ni_all;
            }
        }

        private void CalcFnReal(double decision) // считаем функцию распределения
        {
            List<Fn> temp_list = new List<Fn>();
            Fn tmp_kek = new Fn();
            double summ = 0;
            param = intensity * time;
            double p1, p0 = Math.Exp(-1 * param);
            tmp_kek.left = 0;
            tmp_kek.right = 1;
            summ = tmp_kek.p = p0;
            temp_list.Add(tmp_kek);
            int i = 1;
            while (1.0 - summ > decision)
            {
                p1 = (param / i) * p0;
                p0 = p1;
                summ += p0;
                tmp_kek.left = i;
                tmp_kek.right = ++i;
                tmp_kek.p = summ;
                temp_list.Add(tmp_kek);
            }
            real_Fn = new Fn[temp_list.Count];
            for (int j = 0; j < temp_list.Count; j++)
            {
                real_Fn[j] = temp_list[j];
            }
            temp_list.Clear();
        }

        private void CalcD_outrun() // считаем меру расхождения
        {
            double max = -1;// = Math.Abs(real_Fn[0].p - select_Fn[0].p);
            double max_iterator = Math.Max(real_count, real_Fn.Count());
            int j = 0;
            for (int i = 0; i < max_iterator; i++)
            {

                if (j == Math.Min(real_count, real_Fn.Count())) break;
                if (select_Fn[j].left == real_Fn[i].left)
                {
                    if (max < Math.Abs(real_Fn[i].p - select_Fn[j].p)) max = Math.Abs(real_Fn[i].p - select_Fn[j].p);
                    ++j;
                };
            }
            D_outrun = max;
        }

        private void CalcProb_disc_max() // поиск максимальной невязки вероятности и частоты
        {
            double max = work_yi[0].prob_disc;
            for (int i = 1; i < real_count; i++)
            {
                if (max < work_yi[i].prob_disc) max = work_yi[i].prob_disc;
            }
            prob_disc_max = max;
        }

    }
}
