using RestSharp;
using System.Collections;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Data.Common;
using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;


class Program
{

    static void Main(string[] args)
    {

        getData();

    }


    public static Rootobject getData()
    {

        DateTime fecha = DateTime.Now;
        Rootobject result = new Rootobject();

        ArrayList jsonFormato = new ArrayList();
        jsonFormato.Add("fecha;hora;fof2");

        string station = "tuj2o";
        string date_template = "{0}-{1}-{2}";

        //fechas introducidas de entrada por el usuario
        Console.WriteLine("Ingrese el Año: ");
        int since_year = Int32.Parse(Console.ReadLine());
        Console.WriteLine("Ingrese el Mes: ");
        int since_month = Int32.Parse(Console.ReadLine());


        string fecha2;
        int dias = 0;

        //inicializa la variable que sirve para saber si hay pozos en la pagina eswua, false si no hay en una fecha determinada
        bool bandera = false;
        bool bandera2 = false;
        bool bandera_mod = false;

        fecha.AddYears(since_year);

        //añade un cero a las fechas ingresadas por el susuario menores a 10
        if (since_month >= 1 && since_month < 10)
        {
            fecha2 = since_year + "/" + "0" + since_month + "/" + "01";
        }
        else
        {
            fecha2 = since_year + "/" + since_month + "/" + "01";
        }


        DateTime today = DateTime.MinValue;

        int minutos = 0;


        DateTime answer = today;
        //agrega el año introducido de entrada por el usuario
        answer = today.AddYears(since_year - 1);

        //crea la variable fecha que se utilizara como fecha imaginaria
        DateTime answer2 = answer;


        //agrega el mes introducido de entrada por el usuario
        answer2 = answer.AddMonths(since_month - 1);

        Console.WriteLine("Ingrese la ruta donde se guardaran los archivos de texto: ");
        string ruta = (Console.ReadLine());
        //archivo txt que se guarda en la pc
        StreamWriter sw = new StreamWriter(ruta + since_year + since_month + ".txt", true, Encoding.ASCII);
        // C:\Users\Alvaro\Documents\test_ionosfera\


        int año = since_year;
        int mes = since_month;


        if (mes == 1 || mes == 3 || mes == 5 || mes == 7 || mes == 8 || mes == 10 || mes == 12)
        {
            dias = 31;
        }
        if (mes == 2)
        {
            if (año % 4 == 0)
            {
                dias = 29;
            }
            else
            {
                dias = 28;
            }
        }
        if (mes == 4 || mes == 6 || mes == 9 || mes == 11)
        {
            dias = 30;
        }


        for (int dia = 1; dia <= dias; dia++)
        {

            //formatea las fechas desde y hasta introducidas por el usuario en año mes y dia
            string since_date = String.Format(date_template, año, mes, dia);//YYYY-MM-DD

            string since_hour = "00:00:00";//HH:MM:SS
            string until_hour = "23:50:00";
            //busca las fechas de entrada hasta y desde que ingreso el usuario en la pagina de eswua                 
            string url = String.Format("http://ws-eswua.rm.ingv.it/ais.php/records/{0}_auto?filter=dt,bt,{1}%20{2},{3}%20{4}&include+dt,{5}&order=dt", station, since_date, since_hour, since_date, until_hour, station);
            Console.WriteLine("Consultando...");
            var client = new RestClient(url);
            var request = new RestRequest();
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string rawResponse = response.Content;
                result = JsonConvert.DeserializeObject<Rootobject>(rawResponse);


                if (result.records.Length == 0)
                {

                    bandera = true;

                    while (bandera)
                    {

                        if (bandera == true)
                        {

                            string nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

                            sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");
                        }

                        if (answer2.Hour == 23 && (answer2.Minute == 45 || answer2.Minute == 50))
                        {

                            bandera = false;
                        }



                        //incrementa minutos a la fecha imaginaria
                        if (bandera2 == false)
                        {

                            answer2 = answer2.AddMinutes(10);
                        }
                        if (bandera2 == true)
                        {

                            answer2 = answer2.AddMinutes(15);
                        }


                    }//cierra while


                }
                else
                {

                    try
                    {

                        //comprueba si los minutos se incrementan en 10 o 15
                        int minute_record1 = int.Parse(result.records[1].dt.Substring(14, 2));

                        if (minute_record1 == 15)
                        {
                            bandera2 = true;
                        }

                        //recorre los datos por minutos (10 o 15)
                        foreach (var record in result.records)
                        {


                            //fecha de eswua en años meses y dias y hora
                            int year_record = int.Parse(record.dt.Substring(0, 4));
                            int month_record = int.Parse(record.dt.Substring(5, 2));
                            int day_record = int.Parse(record.dt.Substring(8, 2));
                            int hour_record = int.Parse(record.dt.Substring(11, 2));
                            int minute_record = int.Parse(record.dt.Substring(14, 2));
                            int second_record = int.Parse(record.dt.Substring(17, 2));

                            //comprueba si hay numeros que se incrementan en un valor distinto a 10 o 15 minutos
                            if ((minute_record % 5) != 0)
                            {
                                bandera_mod = true;
                            }

                            //compara si la fecha bandera coincide con la fecha de eswua para saber si hay pozos de datos
                            if (year_record == answer2.Year && month_record == answer2.Month && day_record == answer2.Day && hour_record == answer2.Hour && minute_record == answer2.Minute)
                            {

                          

                                if (record.fof2 == null)
                                {

                                    sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + "NULL" + " " + "\n");

                                }
                                else
                                {

                                    sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + record.fof2 + " " + "\n");

                                }
                                //incrementa minutos a la fecha imaginaria
                                if (bandera2 == false)
                                {

                                    answer2 = answer2.AddMinutes(10);
                                }
                                if (bandera2 == true)
                                {

                                    answer2 = answer2.AddMinutes(15);
                                }

                            }

                            //comprueba si hay un error en los minutos en la pagina de eswua
                            else if (bandera_mod == true && year_record == answer2.Year && month_record == answer2.Month && day_record == answer2.Day && hour_record == answer2.Hour)
                            {
                                if (record.fof2 == null)
                                {

                                    sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + "NULL" + " " + "\n");

                                }
                                else
                                {

                                    sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + record.fof2 + " " + "\n");

                                }
                                //incrementa minutos a la fecha imaginaria
                                if (bandera2 == false)
                                {

                                    answer2 = answer2.AddMinutes(10);
                                }
                                if (bandera2 == true)
                                {

                                    answer2 = answer2.AddMinutes(15);
                                }
                            }

                            //si la fecha de bandera no coincide con la fecha de eswua
                            else
                            {

                                bandera = true;

                                while (bandera)
                                {

                                    if (year_record == answer2.Year && month_record == answer2.Month && day_record == answer2.Day && hour_record == answer2.Hour && minute_record == answer2.Minute)
                                    {

                                        bandera = false;
                                    }
                                    else if (bandera_mod == true && year_record == answer2.Year && month_record == answer2.Month && day_record == answer2.Day && hour_record == answer2.Hour)
                                    {

                                        bandera = false;

                                    }


                                    if (bandera == true)
                                    {

                                        string nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

                                        sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");


                                    }
                                    else
                                    {

                                        if (record.fof2 == null)
                                        {

                                            sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + "NULL" + " " + "\n");

                                        }
                                        else
                                        {

                                            sw.Write(record.dt.Replace("-", "/").Substring(0, 10) + ";" + record.dt.Substring(11) + ";" + record.fof2 + " " + "\n");

                                        }
                                    }

                                    //incrementa minutos a la fecha imaginaria
                                    if (bandera2 == false)
                                    {

                                        answer2 = answer2.AddMinutes(10);
                                    }

                                    if (bandera2 == true)
                                    {

                                        answer2 = answer2.AddMinutes(15);
                                    }

                                }//cierra while


                            }//cierra else



                        }//cierra foreach



                    }//cierra try




                    catch (Exception e)
                    {

                        Console.WriteLine("Exception: " + e.Message);
                    }
                    finally
                    {

                        Console.WriteLine("Executing finally block.");
                    }

                }//cierra else
            }

        }//cierra for dia




        while (answer2.Day != dias && answer2.Year == since_year && answer2.Month == since_month)
        {
            string nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

            sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");

            //incrementa minutos a la fecha imaginaria
            if (bandera2 == false)
            {

                answer2 = answer2.AddMinutes(10);
            }
            if (bandera2 == true)
            {

                answer2 = answer2.AddMinutes(15);
            }
        }

        if (answer2.Day == dias && answer2.Year == since_year)
        {
            while (answer2.Hour != 23)
            {

                string nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

                sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");

                //incrementa minutos a la fecha imaginaria
                if (bandera2 == false)
                {

                    answer2 = answer2.AddMinutes(10);
                }
                if (bandera2 == true)
                {

                    answer2 = answer2.AddMinutes(15);
                }


                if (answer2.Hour == 23)
                {
                    while (answer2.Minute != 45 && answer2.Minute != 50)
                    {

                        nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

                        sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");

                        //incrementa minutos a la fecha imaginaria
                        if (bandera2 == false)
                        {

                            answer2 = answer2.AddMinutes(10);
                        }
                        if (bandera2 == true)
                        {

                            answer2 = answer2.AddMinutes(15);
                        }
                    }
                    nuevaFechaStr = answer2.ToString("yyyy/MM/dd;HH:mm:ss");

                    sw.Write(nuevaFechaStr + ";" + "C" + " " + "\n");
                }
            }

        }

        //close the file
        sw.Close();


        return result;
    }

}



public class Rootobject
{
    public Record[] records { get; set; }
}

public class Record
{
    public string dt { get; set; }
    public string station { get; set; }
    public string fromfile { get; set; }
    public string producer { get; set; }
    public object evaluated { get; set; }
    public string? fof2 { get; set; }
    public bool fof2_eval { get; set; }
    public string muf3000f2 { get; set; }
    public bool muf3000f2_eval { get; set; }
    public string m3000f2 { get; set; }
    public bool m3000f2_eval { get; set; }
    public string fxi { get; set; }
    public bool fxi_eval { get; set; }
    public string fof1 { get; set; }
    public bool fof1_eval { get; set; }
    public string ftes { get; set; }
    public bool ftes_eval { get; set; }
    public int? h_es { get; set; }
    public bool h_es_eval { get; set; }
    public object aip_hmf2 { get; set; }
    public object aip_fof2 { get; set; }
    public object aip_fof1 { get; set; }
    public object aip_hmf1 { get; set; }
    public object aip_d1 { get; set; }
    public object aip_foe { get; set; }
    public object aip_hme { get; set; }
    public object aip_yme { get; set; }
    public object aip_h_ve { get; set; }
    public object aip_ewidth { get; set; }
    public object aip_deln_ve { get; set; }
    public object aip_b0 { get; set; }
    public object aip_b1 { get; set; }
    public object tec_bottom { get; set; }
    public object tec_top { get; set; }
    public object profile { get; set; }
    public object trace { get; set; }
    public string modified { get; set; }
    public string fof2_med_27_days { get; set; }
}


