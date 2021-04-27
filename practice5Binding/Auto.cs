using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace practice5Binding
{
    [Serializable]
    public class Automobile : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

         public enum AutoHPCategory
        {
            LowPerformance,
            MiddlePerformance,
            HighPerformance
        };

        [Browsable(false)]private string mark;
        [DisplayName("Марка")] public string Mark { get => mark;
            set => mark = value;
        }
        
        [Browsable(false)]private string model;
        [DisplayName("Модель")] public string Model { get => model; set => model = value; }
        
        [Browsable(false)]private int yearOfRelease;
        [DisplayName("Год выпуска")] public int YearOfRelease { get => yearOfRelease; set => yearOfRelease = value; }

        [Browsable(false)]private int horsePower;
        [DisplayName("Кол-во ЛС")]
        public int HorsePower
        {
            get => horsePower;
            set
            {
                if (horsePower != value)
                {
                    horsePower = value;
                    category = SetCategory();
                }
            }
        }

        [Browsable(false)] private AutoHPCategory category;

        [DisplayName("Категория ТС")]
        public AutoHPCategory AutoCategory => category;

        private AutoHPCategory SetCategory()
        {
            if (this.HorsePower < 250) return AutoHPCategory.LowPerformance;
            else if (this.HorsePower < 500 && this.HorsePower > 250) return AutoHPCategory.MiddlePerformance;
            else  return AutoHPCategory.HighPerformance;
        }

        [Browsable(false)] private string imageFile;

        public string ImageFile
        {
            get => imageFile;
            set
            {
                if (imageFile != value)
                {
                    imageFile = value;
                    if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ImageFile"));
                }
            }
        }
        
        public Automobile()
        {
            mark = "";
            model = "";
            yearOfRelease = 0;
            horsePower = 0;
            imageFile = "";
        }

        public Automobile(string mark, string model, int year, int hp, string imageFile)
        {
            Mark = mark;
            Model = model;
            YearOfRelease = year;
            HorsePower = hp;
            ImageFile = imageFile;
            category = SetCategory();
        }

        public Automobile(string mark, string model, int year, int hp)
        {
            Mark = mark;
            Model = model;
            YearOfRelease = year;
            HorsePower = hp;
            category = SetCategory();
        }
    }
}