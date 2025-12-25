using RFCardGame.Core;
using System.Windows.Media;

namespace RFOnline_CCG.ViewModels
{
    public class CreatureViewModel : BaseViewModel
    {
        private ICreatureCard _creature;
        private bool _canAttack;
        private bool _isSelected;

        public string Name => _creature.Name;
        public int Attack => _creature.Attack;
        public int Health => _creature.CurrentHealth;
        public int MaxHealth => _creature.MaxHealth;
        public string HealthText => $"{Health}/{MaxHealth}";
        public CreatureState State => _creature.State;

        public bool IsAlive => _creature.IsAlive;
        public bool CanAttack => _creature.IsAlive && _creature.State == CreatureState.Active;

        public Brush HealthColor
        {
            get
            {
                if (!IsAlive) return Brushes.Gray;
                double percentage = (double)Health / MaxHealth;
                if (percentage > 0.5) return Brushes.LimeGreen;
                if (percentage > 0.25) return Brushes.Yellow;
                return Brushes.Red;
            }
        }

        public string StateText => _creature.State.ToString();

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public ICreatureCard GetCreature() => _creature;

        public CreatureViewModel(ICreatureCard creature)
        {
            _creature = creature;
        }

        public void Update()
        {
            OnPropertyChanged(nameof(Health));
            OnPropertyChanged(nameof(HealthText));
            OnPropertyChanged(nameof(IsAlive));
            OnPropertyChanged(nameof(CanAttack));
            OnPropertyChanged(nameof(HealthColor));
            OnPropertyChanged(nameof(StateText));
        }
    }
}