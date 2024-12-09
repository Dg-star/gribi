using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm;

namespace WpfApp1.Models
{
    public class mushroom
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _color;
        public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }

        private bool _edible;
        public bool Edible { get => _edible; set { _edible = value; OnPropertyChanged(); } }

        private double _weight;
        public double Weight { get => _weight; set { _weight = value; OnPropertyChanged(); } }

        private double _height;
        public double Height { get => _height; set { _height = value; OnPropertyChanged(); } }

        private double _capRadius;
        public double CapRadius { get => _capRadius; set { _capRadius = value; OnPropertyChanged(); } }

        PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
