using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TV_Lab
{
    
    public class RV
    {


        public double intensity; // интенсивность
        public double time; // время
        public int count; // количество экспериментов

        public double par;

        public int sort_filt_count;  // количество найденных значений СВ

        private bool IsSorted;

        public Y[] raw_yi; // вычислительная структура СВ (несорт, нефильтр)
        public Y[] sort_filt_yi; // рабочая структура полученных и сортированных СВ
        public Fn[] sort_filt_Fn; // хранение кусков функции распределения
        public Fn[] Fn_ex; // хранение кусков выборочной функции распределения

      
        public int n_ex;
 
        public double x_medium; // среднее значение СВ
        public double prob_disc_max; // максимальное значение невязки вероятности

        public double En; // матожидание = intensity * time
        public double En_ex; // матожидание посчитанное экспериментально

        public double Dn; // дисперсия = intensity * time
        public double Dn_ex; // дисперсия посчитанная экспериментально
        public double Dn_medium; // выборочная дисперсия

        public double Me; // медиана
        public double R; // размах выборки
        public double D_outrun; // мера расхождения // max(abs(Fn_real - Fn_select))

        ////3 part

        public struct Den // хранение интервального разбиения
        {
            public int l; // левая граница
            public int r; // правая граница
            public double n; // частота
            public double p; // вероятность
            public int count; // количество попавших значений СВ из выборки в границу
        }

        public int size_segs; // k интервалов разбиения
        public double a_level; // уровень значимости alpha
        public double F_R0; // статистика Хи^2
        public double R0=0;
        public Den[] segments; // разбитая выборка с вероятностью


        public struct Y
        {
            public int size; // размер вектора вероятностей 
            public int res; // случайная величина
            public int ni; // сколько раз встретили случайную величину
            public int number; // номер испытания
            public bool original; // уникальность
            public double prob_disc; // невязка вероятности
            public double pi; //найденная вероятность 
        }
        public struct Fn // хранение кусков функции выборочной функции распределения
        {
            public double left; // левая граница
            public double right; // правая граница
            public double p; // вероятность
        }

        public RV() { }

        public RV(double _intensity, double _time, int _count) // конструктор по входным параметрам
        {
            intensity = _intensity; // интенсивность а/м
            time = _time; // время
            count = _count; // количество испытаний

            IsSorted = false;

            raw_yi = new Y[count];
        }

        public RV(RV enter)
        {
            intensity = enter.intensity;
            time = enter.time;
            count = enter.count;
            raw_yi = new Y[count];
            IsSorted = enter.IsSorted;
            for (int i = 0; i < count; i++)
            {
                raw_yi[i].pi = enter.raw_yi[i].pi;
                raw_yi[i].res = enter.raw_yi[i].res;
                raw_yi[i].size = enter.raw_yi[i].size;
                raw_yi[i].number = enter.raw_yi[i].number;
                raw_yi[i].original = enter.raw_yi[i].original;
            }
        }

        public void Set(double _intensity, double _time, int _count) 
        {
            if(count!=0)
            {
                for (int i = 0; i < count; i++)
                {
                    raw_yi[i].original = true;
                }
                raw_yi = null;
            }
            IsSorted = false;
            intensity = _intensity; // интенсивность а/м
            time = _time; // время
            count = _count; // количество испытаний

            par = time * intensity;



          
            raw_yi = new Y[count];// массив для y i-ых.
        }

        public void finalize()
        {
            for (int i = 0; i < count; i++)
            {
                raw_yi[i].res = 0;
                raw_yi[i].pi = 0;
                raw_yi[i].size = 0;
                raw_yi[i].number = 0;
                raw_yi[i].original = true;
            }
            raw_yi = null;
            sort_filt_yi = null;
            Fn_ex = null;
            sort_filt_Fn = null;
            intensity = 0;
            time = 0;
            count = 0;
        }

        public void GenModel()
        {
            Random rand = new Random();//инициализация класса генератора случайных чисел
            for (int i = 0; i < count; i++){
                double u = rand.NextDouble();// генерация случайного вещественного числа на отрезке [0,1]
                //моделирование 
                double p0 = 1/Math.Exp(par);
                double p1;
                double p2 = p0;
                raw_yi[i].pi = p2;
                int r = 0;
                raw_yi[i].size = r;
                while (u > p2) {//пока не доберемся до сгенерированной вероятности
                    r++;
                    //рекуррентное соотношение
                    p1 = (par / r) * p0;
                    p2 += p1;
                    raw_yi[i].pi = p1;
                    p0 = p1;
                }
                    raw_yi[i].res = r;
                    raw_yi[i].ni = 1;
                    raw_yi[i].number = i;
                    raw_yi[i].original = true;
                raw_yi[i].size = r;
            }
            //подсчёт числа повторений каждого значения св
            for (int i = 0; i < count; i++){
                for (int j = 0; j < count; j++){
                    if ((raw_yi[i].res == raw_yi[j].res) && (i != j))
                    {
                        raw_yi[i].ni++;
                    }
                }
            }
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
                    if (raw_yi[i].res < raw_yi[j].res)
                    {
                        tmp_res = raw_yi[i].res;
                        tmp_size = raw_yi[i].size;
                        tmp_ni = raw_yi[i].ni;
                        tmp_number = raw_yi[i].number;
                        tmpi = raw_yi[i].pi;
                        tmp_prob = raw_yi[i].prob_disc;

                        raw_yi[i].res = raw_yi[j].res;
                        raw_yi[i].size = raw_yi[j].size;
                        raw_yi[i].ni = raw_yi[j].ni;
                        raw_yi[i].number = raw_yi[j].number;
                        raw_yi[i].pi = raw_yi[j].pi;
                        raw_yi[i].prob_disc = raw_yi[j].prob_disc;

                        raw_yi[j].res = tmp_res;
                        raw_yi[j].size = tmp_size;
                        raw_yi[j].ni = tmp_ni;
                        raw_yi[j].number = tmp_number;
                        raw_yi[j].pi = tmpi;
                        raw_yi[j].prob_disc = tmp_prob;
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
                if (!raw_yi[i].original) continue;
                for (int j = 0; j < count; j++)
                {
                    if ((raw_yi[i].res == raw_yi[j].res) && i != j)
                    {
                        raw_yi[j].original = false;
                        ++new_size;
                    }
                       
                }    
            }
            sort_filt_count = count - new_size;

            sort_filt_yi = new Y[sort_filt_count];

            n_ex = 0;

            // создание новой структуры для оригинальных СВ
            for (int i = 0, j = 0; i < count; i++) 
            {
                if (!raw_yi[i].original) continue;
                sort_filt_yi[j].ni = raw_yi[i].ni;
                n_ex += sort_filt_yi[j].ni;
                sort_filt_yi[j].res = raw_yi[i].res;
                sort_filt_yi[j].number = raw_yi[i].number;
                sort_filt_yi[j].original = true;
                sort_filt_yi[j].pi = raw_yi[i].pi;
                sort_filt_yi[j].size = raw_yi[i].size;
                ++j;
            }
            
            
           
            // ищем невязку вероятности
            for (int i = 0; i < sort_filt_count; i++)
            {
                sort_filt_yi[i].prob_disc = Math.Abs(((double)sort_filt_yi[i].ni / (double)count) - sort_filt_yi[i].pi);
            }
            // ищем максимум
            prob_disc_max = sort_filt_yi[0].prob_disc;
            for (int i = 1; i < sort_filt_count; i++)
            {
                if (prob_disc_max < sort_filt_yi[i].prob_disc) prob_disc_max = sort_filt_yi[i].prob_disc;
            }
        }

        public int FindMaxres()
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                if (value < raw_yi[i].res)
                    value = raw_yi[i].res;
            }
            return value;
        }
        public void CalcRealDn()
        {
            Dn_ex = 0;
            double first = 0, second = 0;

            for (int i = 0; i < sort_filt_count; i++)
            {
                first += Math.Pow(sort_filt_yi[i].res, 2) * sort_filt_yi[i].pi;
                second += sort_filt_yi[i].res * sort_filt_yi[i].pi;
            }
            second *= second;
            Dn_ex = first - second;//M(ksi^2 )- (Mksi)^2
            Dn = intensity * time;
        }
     
        public void FindXMedium()
        {
            double sum = 0;
            for (int i = 0; i < count; i++) 
            {
                sum += raw_yi[i].res;
            }
            x_medium = sum / count;
        }

        public void FnEx() 
        {
            Fn_ex = new Fn[sort_filt_count];
            double s = 0;
            for (int i = 0; i < sort_filt_count-1; i++)
            {
                s += sort_filt_yi[i].ni;
                Fn_ex[i].left = sort_filt_yi[i].res;
                    Fn_ex[i].right = sort_filt_yi[i + 1].res;
                Fn_ex[i].p = s /n_ex;
            }
            s += sort_filt_yi[sort_filt_count - 1].ni;
            Fn_ex[sort_filt_count - 1].left = sort_filt_yi[sort_filt_count - 1].res;
            Fn_ex[sort_filt_count - 1].right = sort_filt_yi[sort_filt_count - 1].res + 3;
            Fn_ex[sort_filt_count - 1].p = s / n_ex;
        }

        public void FnReal(double eps) // считаем функцию распределения
        {
            List<Fn> temp_list = new List<Fn>();
            Fn tmp_ni = new Fn();
            double p = 0;
            par = intensity * time;
            double p1, p0 = Math.Exp(-1 * par);
            tmp_ni.left = 0;
            tmp_ni.right = 1;
            p = tmp_ni.p = p0;
            temp_list.Add(tmp_ni);
            int i = 1;
            while (1 - p > eps)
            {
                p1 = (par / i) * p0;
                p0 = p1;
                p += p0;
                tmp_ni.left = i;
                tmp_ni.right = ++i;
                tmp_ni.p = p;
                temp_list.Add(tmp_ni);
            }
            sort_filt_Fn = new Fn[temp_list.Count];
            for (int j = 0; j < temp_list.Count; j++)
            {
                sort_filt_Fn[j] = temp_list[j];
            }
            temp_list.Clear();
        }

        public void CalcD_outrun() // считаем меру расхождения
        {
            double max = -1;
            double max_iterator = Math.Max(sort_filt_count, sort_filt_Fn.Count());
            int j = 0;
            for (int i = 0; i < max_iterator; i++)
            {

                if (j == Math.Min(sort_filt_count, sort_filt_Fn.Count())) break;
                if (Fn_ex[j].left == sort_filt_Fn[i].left)
                {
                    if (max < Math.Abs(sort_filt_Fn[i].p - Fn_ex[j].p)) max = Math.Abs(sort_filt_Fn[i].p - Fn_ex[j].p);
                    ++j;
                };
            }
            D_outrun = max;
        }

        public void CalcProb_disc_max() // поиск максимальной невязки вероятности и частоты
        {
            double max = sort_filt_yi[0].prob_disc;
            for (int i = 1; i < sort_filt_count; i++)
            {
                if (max < sort_filt_yi[i].prob_disc) max = sort_filt_yi[i].prob_disc;
            }
            prob_disc_max = max;
        }

        public long factorial(long x)
        {
            return (x == 0) ? 1 : x * factorial(x - 1);
        }

        public void SearchCh()
        {
            //матожидание
            En_ex = 0;
            for (int i = 0; i < sort_filt_count; i++)
            {
                En_ex += sort_filt_yi[i].res * sort_filt_yi[i].pi;//из выборки
            }
            En = intensity * time;// теоретическое
            //дисперсия
            Dn_ex = 0;
            double left = 0;
            double right = 0;
            for (int i = 0; i < sort_filt_count; i++)
            {
                left += Math.Pow(sort_filt_yi[i].res, 2) * sort_filt_yi[i].pi;
                right += sort_filt_yi[i].res * sort_filt_yi[i].pi;
            }
            right *= right;
            Dn_ex = left - right;//из выборки
            Dn = intensity * time;// теоретическое
            //выборочная дисперсия
            double sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += Math.Pow(raw_yi[i].res - x_medium, 2);
            }
            Dn_medium = sum / count;
            //медиана
            int k=1;
            if (count % 2 > 0)//нечетный
            {
                k = count / 2;
                Me = raw_yi[k].res;
            }
            else//четный
            {
                k = count / 2;
                Me = (raw_yi[k - 1].res + raw_yi[k].res) / 2;
            }
            //разброс выборки
            R = sort_filt_yi[sort_filt_count - 1].res - sort_filt_yi[0].res;
        }

        /////////////////////////////////////////////////////////// 3 PART //////////////////////////////////////////////////////////////////////

        public bool H0()
        {
            //Разбитие на полуинтервалы и подсчёт данных в каждом
            segments = new Den[size_segs];//инициализация объекта - совокупность отрезков
            int div_size = (Fn_ex.Length - 1) / size_segs;
            segments[0].l = Int32.MinValue;//самая левая граница первого отрезка, соответствует минус бесконечности
            segments[0].r = sort_filt_yi[div_size].res;
            //подсчёт всех данных для первого отрезка
            for (int j = 0; j <= div_size; j++)
            {
                segments[0].count += sort_filt_yi[j].ni;//число значений
            }
            if (size_segs == 1)//если задан только один отрезок
            {
                segments[0].p = sort_filt_Fn[sort_filt_Fn.Length - 1].p;//вероятность как последнее значение функции распределения
            }
            else//если несколько отрезков
            {
                for (int j = 1; j < sort_filt_Fn[segments[0].r].right; j++)
                {
                    segments[0].p += sort_filt_Fn[j].p - sort_filt_Fn[j - 1].p;//вероятность для первого
                }
            //подсчёт данных для всех остальных отрезков
                for (int i = 1; i < size_segs; i++)
                {
                    segments[i].l = segments[i - 1].r;
                    if (i == size_segs - 1)
                    {
                        segments[i].r = sort_filt_yi[sort_filt_yi.Length - 1].res;
                        for (int j = (i * div_size) + 1; j < sort_filt_yi.Length; j++)
                        {
                            segments[i].count += sort_filt_yi[j].ni;//число значений
                        }

                        if ((Fn_ex.Length - 1) % size_segs == 0)
                        {
                            if ((i * div_size) + 1 == sort_filt_yi.Length)
                            {
                                segments[i].count += sort_filt_yi[sort_filt_yi.Length - 1].ni;//число значений
                            }
                            segments[i].r += 4;
                        }

                        for (int j = segments[i].l + 1; j < sort_filt_Fn.Length; j++)
                        {
                            segments[i].p += sort_filt_Fn[j].p - sort_filt_Fn[j - 1].p;//вероятность
                        }

                    }
                    else
                    {
                        if ((i + 1) * div_size == sort_filt_yi.Length)
                        {
                            segments[i].r = sort_filt_yi[(i + 1) * div_size - 1].res;
                            for (int j = (i * div_size) + 1; j < (i + 1) * div_size; j++)
                            {
                                segments[i].count += sort_filt_yi[j].ni;//число значений
                            }
                        }
                        else
                        {
                            segments[i].r = sort_filt_yi[(i + 1) * div_size].res;
                            for (int j = (i * div_size) + 1; j <= (i + 1) * div_size; j++)
                            {
                                segments[i].count += sort_filt_yi[j].ni;//число значений
                            }
                        }
                        for (int j = segments[i].l + 1; j < sort_filt_Fn[segments[i].r].right; j++)
                        {
                            segments[i].p += sort_filt_Fn[j].p - sort_filt_Fn[j - 1].p;//вероятность
                        }
                    }
                    segments[size_segs-1].r = Int32.MaxValue;//правая граница последнего отрезка соответствует плюс бесконечности
                }
            }

            //Процесс принятия/отвержения гипотезы
            R0 = 0;
            for (int i = 0; i < size_segs; i++)//вычисление R0
            {
                R0 += (Math.Pow(segments[i].count - count * segments[i].p, 2.0)) / (count * segments[i].p);
            }
            double res = 0;
            int steps = 3000;
            //вычисление F(R0)
            for (int k = 1; k <= steps; k++)
            {
                res += (density(R0 * ((k - 1) / (double)steps)) + density(R0 * (k / (double)steps))) * (R0 / (2 * steps));
            }
            F_R0 = 1 - res;//где F_R0- это F c чертой
            //сравнение с уровнем значимости
            if (F_R0 < a_level)
            { return false; }
            else
            { return true; }
        }
        private double density(double x)
        {
            double fx2 = 0;
            if (x <= 0)
            {
                fx2 = 0;
            }
            else
            {
                fx2 = (Math.Pow(2, -(size_segs) / 2.0) / SpecialFunction.gamma(size_segs / 2.0)) * Math.Pow(x, (size_segs) / 2.0 - 1) * Math.Exp(-x / 2.0);
            }
            return fx2;
        }
    }






}

