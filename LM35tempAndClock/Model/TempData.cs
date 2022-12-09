using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM35tempAndClock.Model
{
    [ObservableObject]
    public partial class TempData
    {
        //debugging variable
        [ObservableProperty]
        string footBug;

        //creates obserable variables that can be used anywhere in the project
        [ObservableProperty]
        string lblTemperature0;
        [ObservableProperty]
        string lblTemperature1;

        public TempData()
        {


        }
    }
}
