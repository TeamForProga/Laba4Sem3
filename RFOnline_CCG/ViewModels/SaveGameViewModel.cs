using System;

namespace RFOnline_CCG.ViewModels
{
    public class SaveGameViewModel : BaseViewModel
    {
        private string _saveName;
        private string _date;
        private string _factions;
        private bool _isSelected;

        public string SaveName
        {
            get => _saveName;
            set => SetField(ref _saveName, value);
        }

        public string Date
        {
            get => _date;
            set => SetField(ref _date, value);
        }

        public string Factions
        {
            get => _factions;
            set => SetField(ref _factions, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public string FilePath { get; set; }
    }
}