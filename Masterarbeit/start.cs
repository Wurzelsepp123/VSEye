using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Statistics;
using SPOCK.Receivers;
using System.Windows;
using System.IO;



namespace Masterarbeit
{


    class start:IEyeMovementListener
    {

        static int window_size = 15;
        public bool removeErrors;
        public bool keep_running;
        double[,] buf;
        GazeReceiver m_gazer;


       
        public start()
        {
            removeErrors = true;
            buf = new double[window_size, 4];
            m_gazer = GazeReceiver.Instance;
         

            GazeReceiver.Instance.AddListener(this);
        }
        ~start()
        {
            GazeReceiver.Instance.RemoveListener(this);
        }


        public void RaiseEyeEvents(Point Position, ulong Timestamp, double Reliability)
        {
           // Console.WriteLine(Timestamp + " " + Position.X + " " + Position.Y + " " + Reliability);
           // buf.Add(Timestamp, Position.X, Position.Y, Reliability);
        }




        public string startTracker()
        {
            m_gazer.Start();
            keep_running = true;
            sp_classifier SP;
            while (keep_running)//abort when tracker dies
            {
                /*Wait for input data*/
                //new_data=getData

                //Find measurement error           
                checkData();            
               // SP.detect_type(buf);
                //SP.verify_type();

            }
            return "";
        }
        void checkData()
        {
            for (int row = 0; row < buf.GetLength(1); row++)
            {
                if (buf[row, 3] != 1)
                {
                    //Console.Write("Input data contains measurement errors!\n");
                      
                    if (removeErrors)
                    {
                        if (row > 2)
                        {
                            //get estimated pos from velocity vector of previous values
                            buf[row, 2] = ((buf[row - 1, 2] - buf[row - 2, 2]) / (buf[row - 1, 1] - buf[row - 2, 1])) * (buf[row, 1] - buf[row - 1, 1]);
                            buf[row, 3] = ((buf[row - 1, 3] - buf[row - 2, 3]) / (buf[row - 1, 1] - buf[row - 2, 1])) * (buf[row, 1] - buf[row - 1, 1]);
                        }
                        else
                        {
                            buf[row, 2] = buf.GetColumn(2).Mean();
                            buf[row, 3] = buf.GetColumn(3).Mean();
                        }
                        return;
                    }

                    return;
                }
            }
            return;
        }




    }

}
